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
using System.Runtime.InteropServices;
using System.Text;

using Morko.Network;
using Morko.Threading;

namespace Morko.Network
{
	internal class PlayerConnectionInfo
	{
		public string name;
		public IPEndPoint endPoint;
		public DateTime lastConnectionTime;
		public byte [] lastReceivedPackage;
	}

	/* Document(Leo): This is fixed a fixed size string to be used
	in network structures, where we would like to have consistency for
	easier and speedier content parsing. */
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct NetworkName
	{
		private const int maxLength = 32;
		private unsafe fixed byte data [maxLength];
		private int length;

		public unsafe static implicit operator string(NetworkName name)
		{
			byte [] byteArray = new byte[name.length];
			fixed(byte * dstBytes = &byteArray[0])
			{
				Buffer.MemoryCopy(name.data, dstBytes, name.length, name.length);
			}
			return Encoding.ASCII.GetString(byteArray);
		}

		public unsafe static implicit operator NetworkName (string text)
		{
			NetworkName result = new NetworkName();
			byte [] byteArray = Encoding.ASCII.GetBytes(text);
			result.length = byteArray.Length < maxLength	?
							byteArray.Length :
							maxLength;
	
			fixed(byte * srcBytes = &byteArray[0])
			{
				Buffer.MemoryCopy(srcBytes, result.data, result.length, result.length);
			}
			return result;
		}

		public override string ToString()
		{
			return (string)this;	
		}
	}

	[Serializable]
	public class ServerStartInfo
	{
		public string serverName = "Default Server";
		public int playerUpdatePackageSize;
		public Action<string> logFunction;
	}

	public static class BinaryConverter
	{
		public static byte [] ToBinary<T>(this T value) where T : struct
		{
			int size = Marshal.SizeOf(value);
			var result = new byte [size];

			unsafe
			{
				fixed(byte * resultPtr = result)
				{
					Marshal.StructureToPtr(value, (IntPtr)resultPtr, false);
				}
			}
			return result;
		}

		public static byte[] ToBinary<T>(this T [] array) where T : struct
		{
			int itemCount = array.Length;
			if (itemCount == 0)
				return Array.Empty<byte>();

			int itemSize = Marshal.SizeOf(array[0]);
			int totalSize = itemSize * itemCount;

			var result = new byte[totalSize];

			unsafe
			{
				fixed (byte * resultPtr = result)
				{
					for (int itemId = 0; itemId < itemCount; itemId++)
					{
						IntPtr target = (IntPtr)(resultPtr + itemSize * itemId);
						Marshal.StructureToPtr(	array[itemId], target, false);
					}
				}
			}

			return result;
		}

		public static T ToStructure<T> (this byte[] data, int offset = 0) where T : struct
		{
			int structureSize = Marshal.SizeOf(default(T));
			if (data.Length < structureSize)
			{
				throw new ArgumentException($"Data amount is too little for {typeof(T)}. Required size is {structureSize}, actual size is {data.Length}");
			}

			T result;
			unsafe
			{
				fixed(byte * source = data)
				{
					result = Marshal.PtrToStructure<T>((IntPtr)(source + offset));
				}
			}
			return result;
		}

		public static T ToStructure<T>(this byte [] data, out byte [] leftovers) where T : struct
		{
			T result = data.ToStructure<T>();

			int structureSize 	= Marshal.SizeOf(default(T));
			int leftoverSize 	= data.Length - structureSize;
			leftovers 			= new byte[leftoverSize];

			Buffer.BlockCopy(data, structureSize, leftovers, 0, leftoverSize);

			return result;
		}

		public static T[] ToArray<T> (this byte [] data, int count) where T : struct
		{
			if (count == 0)
				return Array.Empty<T>();

			var result = new T [count];

			int itemSize = Marshal.SizeOf(result[0]);

			int offset = 0;

			for (int itemId = 0; itemId < count; itemId++)
			{
				result [itemId] = data.ToStructure<T>(offset);
				offset += itemSize;
			}

			return result;
		}

		// public static T ToStructure<T>(this byte [] data, int startIndex, out int nextIndex) where T : struct
		// {
		// 	T result = data.ToStructure<T>();

		// 	int structureSize 	= Marshal.SizeOf(default(T));
		// 	int leftoverSize 	= data.Length - structureSize;
		// 	leftovers 			= new byte[leftoverSize];

		// 	Buffer.BlockCopy(data, structureSize, leftovers, 0, leftoverSize);

		// 	return result;
		// }
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

		private List<PlayerConnectionInfo> players;
		private int playerUpdatePackageSize;

		public event Action OnPlayerAdded;

		private Action<string> Log;

		// Note(Leo): this disables the use of constructor outside class
		private Server() {}

		public static Server Create(ServerStartInfo info)
		{

			var server = new Server
			{
				name 					= info.serverName,
				playerUpdatePackageSize = info.playerUpdatePackageSize,
				Log 					= info.logFunction ??  Morko.Logging.Logger.Log,

				broadcastClient 		= new UdpClient(0),
				responseClient 			= new UdpClient(Constants.serverReceivePort),
				players 				= new List<PlayerConnectionInfo>(),
			};

			server.Log($"Created '{server.name}");

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
					name = server.name,
					mapId = 52
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
							case NetworkCommand.PlayerRequestJoin:
								var existingPlayer = server.players.Find(player => IPEndPoint.Equals(player.endPoint, receiveEndPoint));
								if (existingPlayer != null)
								{
									existingPlayer.lastConnectionTime = DateTime.Now;
								}
								else
								{
									var arguments = contents.ToStructure<PlayerRequestJoinArgs>();
									int playerIndex = server.players.Count;
									server.players.Add(new PlayerConnectionInfo 
									{
										endPoint 			= receiveEndPoint,
										name 				= arguments.playerName,
										lastConnectionTime 	= DateTime.Now
									});
									server.Log($"Added player {arguments.playerName}");
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

		public void StartGame()
		{	
			InitializePlayers();

			Log($"[{name}]: Start Game");
			gameUpdateThreadControl.Start(new GameUpdateThread { server = this});

			Log($"[{name}]: Start receiving player updates");
			gameUpdateReceiveThreadControl.Start(new ReceiveUpdateFromPlayersThread{server =this});
		}

		public void StopGame()
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
				player.lastReceivedPackage = new byte[playerUpdatePackageSize];
			}
		}

		private class GameUpdateThread : IThreadRunner
		{
			public Server server;

			public void Run()
			{
				IPEndPoint endPoint = new IPEndPoint(Constants.multicastAddress, Constants.multicastPort);
				server.broadcastClient.JoinMulticastGroup(Constants.multicastAddress);

				// Send gameStartInfo
				{
					int playerCount = server.players.Count;
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
				}


				while(true)
				{
					int playerCount = server.players.Count;
					for (int playerId = 0; playerId < playerCount; playerId++)
					{
						var data = ProtocolFormat.MakeCommand(
										new ServerGameUpdateArgs {playerId = playerId},
										server.players[playerId].lastReceivedPackage);

						server.broadcastClient.Send(data, data.Length, endPoint);
					}


					// int playerCount = server.players.Count;
					// var playerUpdatePackages = new byte [playerCount * playerUpdatePackageSize];

					// for (int playerId = 0; playerId < server.players.Count; playerId++)
					// {
					// 	int offset = playerId * playerUpdatePackageSize;
					// 	Buffer.BlockCopy(	server.players[playerId].lastReceivedPackage, 0,
					// 						playerUpdatePackages, offset,
					// 						playerUpdatePackageSize);
					// }
					// byte [] data = ProtocolFormat.MakeCommand(
					// 					new ServerGameUpdateArgs )

					// server.broadcastClient.Send()

					// for(int playerId = 0; playerId < server.players.Count; playerId++)
					// {
					// 	foreach (var sender in server.players)
					// 	{
					// 		byte [] data = ProtocolFormat.MakeCommand(
					// 							new ServerGameUpdateArgs {playerId = playerId},
					// 							sender.lastReceivedPackage);
					// 		server.broadcastClient.Send(data, data.Length, endPoint);
					// 	}
					// }




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
						case NetworkCommand.PlayerGameUpdate:
							var arguments = contents.ToStructure<PlayerGameUpdateArgs>(out byte [] package);
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