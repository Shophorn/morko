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
		public Action<string> logFunction;
	}

	public class Server
	{
		private string name;

		private int broadcastDelayMs = 100;
		private int gameUpdateThreadDelayMs = 20;

		private readonly ThreadControl broadcastControl = new ThreadControl ();
		private readonly ThreadControl broadcastReceiveControl = new ThreadControl ();
		private readonly ThreadControl gameUpdateThreadControl = new ThreadControl ();
		private readonly ThreadControl gameUpdateReceiveThreadControl = new ThreadControl ();

		private UdpClient broadcastClient;
		private UdpClient responseClient;

		private List<ClientInfo> players;
		private int clientUpdatePackageSize;

		public event Action OnPlayerAdded;

		private Action<string> Log;

		// Note(Leo): this disables the use of constructor outside class
		private Server() {}

		public static Server Create(ServerCreateInfo info)
		{
			var server = new Server
			{
				name 					= info.serverName,
				clientUpdatePackageSize = info.clientUpdatePackageSize,
				Log 					= info.logFunction ??  Morko.Logging.Logger.Log,

				broadcastClient 		= new UdpClient(0),
				responseClient 			= new UdpClient(Constants.serverReceivePort),
				players 				= new List<ClientInfo>(),
			};

			server.Log($"Created '{server.name}'");

			return server;
		}

		private class BroadcastThread : IThreadRunner
		{
			public Server server;

			public void Run()
			{
				server.Log($"[{server.name}]: Start broadcasting");
				var broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, Constants.broadcastPort);
				var arguments = new ServerIntroduceArgs
				{
					serverName = server.name,
					mapIndex = 52
				};
				var data = ProtocolFormat.MakeCommand(arguments);
				while(true)
				{
					server.broadcastClient.Send(data, data.Length, broadcastEndPoint);
					Thread.Sleep(server.broadcastDelayMs);
				}
			}

			public void CleanUp()
			{
				server.Log($"[{server.name}]: Stop broadcasting");
			}
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
			// TODO(Leo): Remove questionmarksP????
			broadcastControl?.Stop();
			broadcastReceiveControl?.Stop();
		}

		public void AddHostingPlayer(string name, IPEndPoint endPoint)
		{
			Log($"Added hosting player: {name} ({endPoint})");
			players.Add(new ClientInfo
			{
				endPoint 			= endPoint,
				name 				= name,
				lastConnectionTime 	= DateTime.Now
			});
		}

		public void StartGame()
		{	
			InitializePlayers();

			Log($"[{name}]: Start Game");
			gameUpdateThreadControl.Start(new GameUpdateThread { server = this });

			Log($"[{name}]: Start receiving player updates");
			gameUpdateReceiveThreadControl.Start(new ReceiveUpdateFromPlayersThread { server = this });
		}

		public void AbortGame()
		{
			// TODO(Leo): Remove questionmarksP????
			Log($"[{name}]: Stop Game");
			gameUpdateThreadControl?.Stop();

			Log($"[{name}]: Stop receiving player updates");
			gameUpdateReceiveThreadControl?.Stop();
		}

		public int PlayerCount => players.Count;
		public string [] PlayersNames => players.Select(player => player.name).ToArray();
	 	
	 	public void Close()
		{
			broadcastClient?.Close();
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
				server.broadcastClient.JoinMulticastGroup(Constants.multicastAddress);

				int playerCount = server.players.Count;
				// Send gameStartInfo
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
					
					server.broadcastClient.Send(data, data.Length, endPoint);

					server.Log("[SERVER] Sent start info to players");
				}

				while(true)
				{
					for (int playerId = 0; playerId < playerCount; playerId++)
					{
						var data = ProtocolFormat.MakeCommand(
										new ServerGameUpdateArgs {playerId = playerId},
										server.players[playerId].lastReceivedPackage);

						server.broadcastClient.Send(data, data.Length, endPoint);
					}

					Thread.Sleep(server.gameUpdateThreadDelayMs);
				}
			}

			public void CleanUp()
			{
				server.broadcastClient.DropMulticastGroup(Constants.multicastAddress);
			}
		}

		private class ReceiveUpdateFromPlayersThread : IThreadRunner
		{
			public Server server;

			public void Run()
			{
				server.Log($"[{server.name}]: Run '{nameof(ReceiveUpdateFromPlayersThread)}'");
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
							server.Log($"Received update from '{server.players[arguments.playerId].name}'");
							server.players[arguments.playerId].lastReceivedPackage = package;

							break;
						}
					}
				}
			}

			public void CleanUp()
			{
				server.Log($"[{server.name}]: Stop '{nameof(ReceiveUpdateFromPlayersThread)}'");
			}
		}
	}
}