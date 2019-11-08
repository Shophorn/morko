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
using System.IO;
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
	public NetworkStream tcpStream;
	public TcpClient tcpClient;
	public IPEndPoint udpEndPoint;
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

		private ThreadControl broadcastControl;// = new ThreadControl ();
		private ThreadControl broadcastReceiveControl;// = new ThreadControl ();
		private ThreadControl gameUpdateThreadControl;// = new ThreadControl ();
		private ThreadControl gameUpdateReceiveThreadControl;// = new ThreadControl ();

		private UdpClient senderClient;
		private UdpClient responseClient;

		/* Todo(Leo) IMPORTANT: THIS MUST BE THREAD SAFE. Or maybe not, if we only add from broadcast
		listen thread and only loop all in gameupdate thread, which are not run simultaneously. */
		private List<ClientInfo> players;
		private int clientUpdatePackageSize;

		public event Action OnPlayerAdded;

		private Action<string> Log;

		public int PlayerCount => players.Count;
		public string [] PlayersNames => players.Select(player => player.name).ToArray();
	 	

		// Note(Leo): this disables the use of constructor outside class
		private Server() {}

		private static IPAddress GetIPAddress()
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(Environment.MachineName);

            foreach (IPAddress address in hostEntry.AddressList)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                    return address;
            }

            return null;
        }

		public static Server Create(ServerCreateInfo info)
		{
			var server = new Server
			{
				name 					= info.serverName,
				clientUpdatePackageSize = info.clientUpdatePackageSize,
				broadcastDelayMs		= info.broadcastDelayMs,
				gameUpdateThreadDelayMs	= info.gameUpdateThreadDelayMs,
				Log 					= info.logFunction ?? Morko.Logging.Logger.Log,

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

		private class BroadcastListenThread : IThreadRunner
		{
			public Server server;

			public void Run()
			{
				var listener = new TcpListener(new IPEndPoint(IPAddress.Any, Constants.serverTcpListenPort));
				listener.Start();
				server.Log($"TCP Listener created {listener.LocalEndpoint}");

				while(true)
				{
					var newPlayerClient = listener.AcceptTcpClient();
					var newPlayerStream = newPlayerClient.GetStream();

					var command = ProtocolFormat.ReadTcpMessage(newPlayerStream, out byte [] argumentsData);
					if (command == NetworkCommand.ClientRequestJoin)
					{
						var arguments = argumentsData.ToStructure<ClientRequestJoinArgs>();

						int playerIndex = server.players.Count;
						server.players.Add(new ClientInfo
						{
							name 		= arguments.playerName,
							tcpClient 	= newPlayerClient,
							tcpStream 	= newPlayerStream
						});
						server.players[playerIndex].tcpStream.WriteTcpMessage(new ServerConfirmJoinArgs { playerId = 0, accepted = true });

						server.Log($"New player connected: {newPlayerClient.Client.LocalEndPoint} \"{arguments.playerName}\"");						
					}
				}
			}

			public void CleanUp() {}
		}

		public void StartBroadcasting()
		{
			broadcastControl = ThreadControl.Start(new BroadcastThread {server = this});
			broadcastReceiveControl = ThreadControl.Start(new BroadcastListenThread {server = this});
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

			gameUpdateThreadControl 		= ThreadControl.Start(new GameUpdateThread { server = this });
			gameUpdateReceiveThreadControl 	= ThreadControl.Start(new ReceiveUpdateFromPlayersThread { server = this });
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

			foreach(var player in players)
			{
				player.tcpClient.Close();
			}
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

					var arguments = new ServerStartGameArgs
					{
						playerCount = playerCount,
						mapId 		= 0
					};
					var data = playerStartInfos.ToBinary();
					for (int playerId = 0; playerId < playerCount; playerId++)
					{
						server.players[playerId].tcpStream.WriteTcpMessage(arguments, data);
					}
				}

				while(true)
				{
					for (int playerId = 0; playerId < playerCount; playerId++)
					{
						var data = ProtocolFormat.MakeCommand(
										new ServerGameUpdateArgs
										{
											playerId = playerId
										},
										server.players[playerId].lastReceivedPackage);

						server.senderClient.Send(data, data.Length, endPoint);
						// server.Log($"first float = {BitConverter.ToSingle(server.players[playerId].lastReceivedPackage, 0)}");

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
					server.Log("Waiting data");

					byte [] data = server.responseClient.Receive(ref receiveEndPoint);

					if (ProtocolFormat.TryParseCommand(data, out NetworkCommand command, out byte [] contents))
					{
						server.Log("Got data");
						switch (command)
						{
						case NetworkCommand.ClientGameUpdate:
							var arguments = contents.ToStructure<ClientGameUpdateArgs>(out byte [] package);
							server.players[arguments.playerId].lastReceivedPackage = package;
							// server.Log($"first float = {BitConverter.ToSingle(server.players[arguments.playerId].lastReceivedPackage, 0)}");

							break;
						}
					}
				}
			}

			public void CleanUp() {}
		}
	}
}