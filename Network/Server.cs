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
using System.Reflection;

using Morko.Network;

// using static System.Console;
// using static Morko.Logging.Logger;

internal class PlayerInfo
{
	public string name;
	public IPEndPoint endPoint;
	public DateTime lastConnectionTime;
	public byte [] lastReceivedPackage;
}

internal interface IControlledThread
{
	void Run();
	void CleanUp();
}

internal class ThreadControl
{
	private Thread thread;
	private ThreadControl(){}

	// Todo(Leo): Make some assertions that we do not start this more than once

	public static ThreadControl Start(IControlledThread threadRunner)
	{
		/* Note(Leo): We start thread with an infinite loop and/or
		blocking io/network/other function calls. Calling Thread.Abort
		causes ThreadAbortException to be thrown from thread and by
		catching it we are able to exit gracefully. */
		var control = new ThreadControl();
		control.thread = new Thread (() => 
		{
			try { threadRunner.Run(); }
			catch (ThreadAbortException) { threadRunner.CleanUp(); }	
		});
		// Todo(Leo): Check if this is something we want
		// control.thread.IsBackground = true;
		control.thread.Start();
		return control;
	}

	public void Stop()
	{
		thread.Abort();
	}
}

namespace Morko.Network
{
	[Serializable]
	public class ServerStartInfo
	{
		public string serverName;
		public Action<string> logFunction;
	}

	[Serializable]
	public class Server
	{
		static byte [] Encode(string text) => System.Text.Encoding.ASCII.GetBytes(text);
		static string Decode(byte [] data) => System.Text.Encoding.ASCII.GetString(data);

		public string 	Name 			{ get; private set; }

		private int broadcastDelayMs = 500;
		private int gameUpdateThreadDelayMs = 500;

		private ThreadControl broadcastControl;
		private ThreadControl broadcastReceiveControl;

		private ThreadControl gameUpdateThreadControl;
		private ThreadControl gameUpdateReceiveThreadControl;

		private UdpClient broadcastClient;
		private UdpClient responseClient;

		private List<PlayerInfo> players;

		public event Action OnPlayerAdded;

		private Action<string> Log;

		// Note(Leo): this disables the use of constructor outside class
		private Server() {}

		public static Server Create(ServerStartInfo info)
		{
			var server = new Server
			{
				Name 			= info.serverName,
				broadcastClient = new UdpClient(0),
				responseClient 	= new UdpClient(Constants.serverReceivePort),
				players 		= new List<PlayerInfo>(),
				Log 			= info.logFunction ??  Morko.Logging.Logger.Log
			};

			server.Log($"Created '{server.Name}");
			return server;
		}

		private class BroadcastThread : IControlledThread
		{
			public Server server;

			public void Run()
			{
				server.Log($"[{server.Name}]: Start broadcasting");
				var broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, Constants.broadcastPort);
				while(true)
				{
					var data = ProtocolFormat.MakeCommand(NetworkCommand.ServerIntroduce, server.Name);
					server.broadcastClient.Send(data, data.Length, broadcastEndPoint);
					Thread.Sleep(server.broadcastDelayMs);
				}
			}

			public void CleanUp()
			{
				server.Log($"[{server.Name}]: Stop broadcasting");
			}
		}

		private class BroadcastReceiveThread : IControlledThread
		{
			public Server server;

			public void Run()
			{
				var receiveEndPoint = new IPEndPoint(IPAddress.Any, 0);
				while(true)
				{
					var data = server.responseClient.Receive(ref receiveEndPoint);

					if (ProtocolFormat.TryParseCommand(data, out NetworkCommand command, out string arguments))
					{
						switch (command)
						{
							case NetworkCommand.PlayerJoin:
								var existingPlayer = server.players.Find(player => IPEndPoint.Equals(player.endPoint, receiveEndPoint));
								if (existingPlayer != null)
								{
									existingPlayer.lastConnectionTime = DateTime.Now;
								}
								else
								{
									var playerName = arguments;
									int playerIndex = server.players.Count;
									server.players.Add(new PlayerInfo 
									{
										endPoint 			= receiveEndPoint,
										name 				= playerName,
										lastConnectionTime 	= DateTime.Now
									});
									server.Log($"Added player {playerName}");
									server.OnPlayerAdded?.Invoke();

									var response = ProtocolFormat.MakeCommand(NetworkCommand.ServerConfirmJoin, BitConverter.GetBytes(playerIndex));
									server.responseClient.Send(response, response.Length, server.players[playerIndex].endPoint);
								}

								break;
						}
					}
				}
			}

			public void CleanUp()
			{

			}
		}

		public void StartBroadcasting()
		{
			broadcastControl = ThreadControl.Start(
				new BroadcastThread { server = this });

			broadcastReceiveControl = ThreadControl.Start(
				new BroadcastReceiveThread { server = this });
		}

		public void StopBroadcasting()
		{
			broadcastControl.Stop();
			broadcastReceiveControl.Stop();
		}

		public void StartGame()
		{	
			Log($"[{Name}]: Start Game");
			gameUpdateThreadControl = ThreadControl.Start(
				new GameUpdateThread { server = this});

			Log($"[{Name}]: Start receiving player updates");
			gameUpdateReceiveThreadControl = ThreadControl.Start(
				new ReceiveUpdateFromPlayersThread{server =this});
		}

		public void StopGame()
		{
			Log($"[{Name}]: Stop Game");
			gameUpdateThreadControl.Stop();

			Log($"[{Name}]: Stop receiving player updates");
			gameUpdateReceiveThreadControl.Stop();
		}

		public int PlayerCount => players.Count;
		public string [] PlayersNames => players.Select(player => player.name).ToArray();
	 	
	 	public void Close()
		{
			broadcastClient?.Close();
			responseClient?.Close();
		}

		private class GameUpdateThread : IControlledThread
		{
			public Server server;

			public void Run()
			{
				IPEndPoint endPoint = new IPEndPoint(Constants.multicastAddress, Constants.multicastPort);
				server.broadcastClient.JoinMulticastGroup(Constants.multicastAddress);
				while(true)
				{
					// var data = ProtocolFormat.MakeCommand(NetworkCommand.ServerGameUpdate, "Game Update");
					float [] testContents = { 6.3f, 1.2f, 5.6f, 6.2f, 4.5f, 7.9f };
					var testContentData = new byte[testContents.Length * sizeof(float)];
					Buffer.BlockCopy(testContents, 0, testContentData, 0, testContentData.Length);

					var data = ProtocolFormat.MakeCommand(
									NetworkCommand.ServerGameUpdate, 
									testContentData);
					server.broadcastClient.Send(data, data.Length, endPoint);

					Thread.Sleep(server.gameUpdateThreadDelayMs);
				}
			}

			public void CleanUp()
			{
				server.broadcastClient.DropMulticastGroup(Constants.multicastAddress);
			}
		}

		private class ReceiveUpdateFromPlayersThread : IControlledThread
		{
			public Server server;

			public void Run()
			{
				server.Log($"[{server.Name}]: Run '{nameof(ReceiveUpdateFromPlayersThread)}'");
				var receiveEndPoint = new IPEndPoint(IPAddress.Any, 0);
				while(true)
				{
					byte [] data = server.responseClient.Receive(ref receiveEndPoint);
					int count = BitConverter.ToInt32(data, 0);
					if (count > 0)
					{
						float x = BitConverter.ToSingle(data, 4);
						float y = BitConverter.ToSingle(data, 8);
						float z = BitConverter.ToSingle(data, 12);

						server.Log($"Received {count} vectors, first = ({x},{y},{z})");
					}
					else
					{
						server.Log($"Received {count} vectors");
					}

				}
			}

			public void CleanUp()
			{
				server.Log($"[{server.Name}]: Stop '{nameof(ReceiveUpdateFromPlayersThread)}'");
			}
		}
	}
}