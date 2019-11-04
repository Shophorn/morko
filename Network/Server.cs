/*
Leo Tamminen

Resources:
	https://docs.microsoft.com/en-us/dotnet/framework/network-programming/using-udp-services
	https://gist.github.com/darkguy2008/413a6fea3a5b4e67e5e0d96f750088a9
	https://stackoverflow.com/questions/22852781/how-to-do-network-discovery-using-udp-broadcast

How to make forms application:
	https://docs.microsoft.com/en-us/dotnet/framework/winforms/how-to-create-a-windows-forms-application-from-the-command-line
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text;

using Morko.Network;
using Morko.Threading;

internal class ClientInfo
{
	public string name;
	public IPEndPoint endPoint;
	public DateTime lastConnectionTime;
	public byte [] lastReceivedPackage;
}

namespace Morko.Network
{
	[Serializable]
	public class ServerCreateInfo
	{
		public string serverName = "Default Server";
		public int clientUpdatePackageSize;
		public Type clientUpdatePackageType;

		public int broadcastDelayMs = 100;
		public int gameUpdateThreadDelayMs = 50;

		public Action<string> logFunction;
	}

	public class Server
	{
		private string name;

		private int broadcastDelayMs;
		private int gameUpdateThreadDelayMs;

		private readonly ThreadControl broadcastControl = new ThreadControl ();
		private readonly ThreadControl broadcastReceiveControl = new ThreadControl ();
		private readonly ThreadControl gameUpdateThreadControl = new ThreadControl ();
		private readonly ThreadControl gameUpdateReceiveThreadControl = new ThreadControl ();

		private UdpClient senderClient;
		private UdpClient responseClient;

		private List<ClientInfo> players;
		private int clientUpdatePackageSize;

		public event Action OnPlayerAdded;

		private Action<string> Log;

		public int PlayerCount => players.Count;
		public string [] PlayersNames => players.Select(player => player.name).ToArray();
	 	

		// Note(Leo): this disables the use of constructor outside class
		private Server() {}

		public static Server Create(ServerCreateInfo info)
		{
			var server = new Server
			{
				name 					= info.serverName,
				clientUpdatePackageSize = info.clientUpdatePackageSize,
				broadcastDelayMs		= info.broadcastDelayMs,
				gameUpdateThreadDelayMs	= info.gameUpdateThreadDelayMs,
				Log 					= info.logFunction ??  Morko.Logging.Logger.Log,

				senderClient 			= new UdpClient(0),
				responseClient 			= new UdpClient(Constants.serverReceivePort),
				players 				= new List<ClientInfo>(),

			};

			server.senderClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			return server;
		}

		private class BroadcastThread : IThreadRunner
		{
			public Server server;

			public void Run()
			{
				var broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, Constants.broadcastPort);
				var arguments = new ServerIntroduceArgs
				{
					serverName = server.name,
					mapIndex = 0
				};
				var data = ProtocolFormat.MakeCommand(arguments);
				while(true)
				{
					server.senderClient.Send(data, data.Length, broadcastEndPoint);
					Thread.Sleep(server.broadcastDelayMs);
				}
			}

			public void CleanUp() {}
		}

		private class BroadcastReceiveThread : IThreadRunner
		{
			public Server server;

			public void Run()
			{
				var receiveEndPoint = new IPEndPoint(IPAddress.Any, 0);
				while(true)
				{
					var data = server.responseClient.Receive(ref receiveEndPoint);

					if (ProtocolFormat.TryParseCommand(data, out NetworkCommand command, out byte [] contents))
					{
						switch (command)
						{
							case NetworkCommand.ClientRequestJoin:
								var existingPlayer = server.players.Find(player => IPEndPoint.Equals(player.endPoint, receiveEndPoint));
								if (existingPlayer != null)
								{
									existingPlayer.lastConnectionTime = DateTime.Now;
								}
								else
								{
									var arguments = contents.ToStructure<ClientRequestJoinArgs>();
									int playerIndex = server.players.Count;
									if (arguments.isHostingPlayer)
									{
										server.Log("Hosting player joined");
									}
									server.players.Add(new ClientInfo 
									{
										endPoint 			= receiveEndPoint,
										name 				= arguments.playerName,
										lastConnectionTime 	= DateTime.Now
									});
									server.Log($"Added player {arguments.playerName} ({receiveEndPoint})");
									server.OnPlayerAdded?.Invoke();

									var response = ProtocolFormat.MakeCommand(
														new ServerConfirmJoinArgs
														{
															playerId = playerIndex,
															accepted = true
														});

									server.responseClient.Send(	response, response.Length,
																server.players[playerIndex].endPoint);
								}

								break;
						}
					}
				}
			}

			public void CleanUp() {}
		}

		public void StartBroadcasting()
		{
			broadcastControl.Start(new BroadcastThread { server = this });
			broadcastReceiveControl.Start(new BroadcastReceiveThread { server = this });
		}

		public void StopBroadcasting()
		{
			broadcastControl.Stop();
			broadcastReceiveControl.Stop();
		}

		public int AddHostingPlayer(string name, IPEndPoint endPoint)
		{
			int playerId = players.Count;
			players.Add(new ClientInfo
			{
				endPoint 			= endPoint,
				name 				= name,
				lastConnectionTime 	= DateTime.Now
			});
			return playerId;
		}

		public void StartGame()
		{	
			InitializePlayers();

			gameUpdateThreadControl.Start(new GameUpdateThread { server = this });
			gameUpdateReceiveThreadControl.Start(new ReceiveUpdateFromPlayersThread { server = this });
		}

		public void AbortGame()
		{
			gameUpdateThreadControl.Stop();
			gameUpdateReceiveThreadControl.Stop();
		}

	 	public void Close()
		{
			/* Note(Leo): In addition to stopping, we must wait for cleanups also before closing sockets.
			Cleanups may do things with sockets such as dropping multicastgroups */
			broadcastControl.StopAndWait();
			broadcastReceiveControl.StopAndWait();
			gameUpdateThreadControl.StopAndWait();
			gameUpdateReceiveThreadControl.StopAndWait();
			
			senderClient?.Close();
			responseClient?.Close();
		}

		private void InitializePlayers()
		{
			foreach (var player in players)
			{
				player.lastReceivedPackage = new byte[clientUpdatePackageSize];
			}
		}

		private class GameUpdateThread : IThreadRunner
		{
			public Server server;

			public void Run()
			{
				IPEndPoint endPoint = new IPEndPoint(Constants.multicastAddress, Constants.multicastPort);
				server.senderClient.JoinMulticastGroup(Constants.multicastAddress);

				// Send gameStartInfo
				// Todo(Leo): this should not be done in this thread
				int playerCount = server.players.Count;
				{
					var playerStartInfos = new PlayerStartInfo [playerCount];
					for(int playerId = 0; playerId < playerCount; playerId++)
					{
						playerStartInfos[playerId] = new PlayerStartInfo
						{
							name = server.players[playerId].name,
							playerId = playerId,
							avatarId = 0
						};
					}

					var package = playerStartInfos.ToBinary();
					var data = ProtocolFormat.MakeCommand(
									new ServerStartGameArgs { playerCount = server.players.Count },
									package);
					
					server.senderClient.Send(data, data.Length, endPoint);
				}

				while(true)
				{
					for (int playerId = 0; playerId < playerCount; playerId++)
					{
						var data = ProtocolFormat.MakeCommand(
										new ServerGameUpdateArgs {playerId = playerId},
										server.players[playerId].lastReceivedPackage);

						server.senderClient.Send(data, data.Length, endPoint);
					}

					Thread.Sleep(server.gameUpdateThreadDelayMs);
				}
			}

			public void CleanUp()
			{
				server.senderClient.DropMulticastGroup(Constants.multicastAddress);
			}
		}

		private class ReceiveUpdateFromPlayersThread : IThreadRunner
		{
			public Server server;

			public void Run()
			{
				var receiveEndPoint = new IPEndPoint(IPAddress.Any, 0);

				while(true)
				{
					byte [] data = server.responseClient.Receive(ref receiveEndPoint);

					if (ProtocolFormat.TryParseCommand(data, out NetworkCommand command, out byte [] contents))
					{
						switch (command)
						{
						case NetworkCommand.ClientGameUpdate:
							var arguments = contents.ToStructure<ClientGameUpdateArgs>(out byte [] package);
							server.players[arguments.playerId].lastReceivedPackage = package;
							break;
						}
					}
				}
			}

			public void CleanUp() {}
		}
	}
}