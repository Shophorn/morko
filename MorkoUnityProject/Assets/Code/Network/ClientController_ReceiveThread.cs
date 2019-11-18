// using System;
// using System.Linq;
// using System.Net;
// using System.Net.Sockets;

// using UnityEngine;

// using Morko.Network;
// using Morko.Threading;

// public partial class ClientController
// {	
// 	private class ReceiveThread : IThreadRunner
// 	{
// 		public ClientController connection;

// 		void IThreadRunner.Run ()
// 		{
// 			var receiveEndPoint = new IPEndPoint(IPAddress.Any, 0);
// 			while(true)
// 			{
// 				var receivedData = connection.udpClient.Receive(ref receiveEndPoint);
// 				bool isCommand = ProtocolFormat.TryParseCommand(receivedData,
// 																out NetworkCommand command,
// 																out byte [] argumentsData);

// 				if (isCommand == false)
// 					continue;

// 				switch (command)
// 				{
// 					case NetworkCommand.ServerIntroduce:
// 						HandleServerIntroduceMessage(argumentsData, receiveEndPoint);
// 						break;

// 					case NetworkCommand.ServerStartGame:
// 						HandleServerStartGameMessage(argumentsData);
// 						break;

// 					case NetworkCommand.ServerGameUpdate:
// 						HandleServerGameUpdateMessage(argumentsData);
// 						break;

// 					case NetworkCommand.ServerConfirmJoin:
// 						HandleServerConfirmJoinMessage(argumentsData);
// 						break;
// 				}
// 			}
// 		}

// 		// Note(Leo): This is part of interface now, but we do not need it
// 		void IThreadRunner.CleanUp(){}

// 		private void HandleServerIntroduceMessage(byte [] argumentsData, IPEndPoint receiveEndPoint)
// 		{
// 			receiveEndPoint.Port = Constants.serverReceivePort;
// 			var existingServer = connection.servers
// 									.Find(server => IPEndPoint.Equals(	server.endPoint,
// 																		receiveEndPoint));
// 			if (existingServer != null)
// 			{
// 				existingServer.lastConnectionTime = DateTime.Now;
// 			}
// 			else
// 			{
// 				var arguments = argumentsData.ToStructure<ServerIntroduceArgs>();
// 				connection.servers.Add(new ServerConnectionInfo
// 				{
// 					serverInfo = new ServerInfo
// 					{ 
// 						serverName = arguments.serverName
// 					},
// 					endPoint 			= receiveEndPoint,
// 					lastConnectionTime 	= DateTime.Now
// 				});

// 				if (connection.selectedServerIndex < 0)
// 				{
// 					connection.selectedServerIndex = 0;
// 				}

// 				connection.netControls.UpdateServersList(connection.GetServers());
// 			}
// 		}

// 		private void HandleServerStartGameMessage(byte [] argumentsData)
// 		{
// 			var arguments 			= argumentsData.ToStructure<ServerStartGameArgs>(out byte [] packageData);
// 			int playerCount 		= arguments.playerCount;
// 			var playerStartInfos 	= packageData.ToArray<PlayerStartInfo>(playerCount);

// 			Debug.Log($"Server called to start game, arguments parsed, my index = {connection.ClientId}");

// 			var gameStartInfo = new GameStartInfo
// 			{
// 				mapIndex = 0,
// 				localPlayer = playerStartInfos
// 									.Where(info => info.playerId == connection.ClientId)
// 									.First(),
// 				remotePlayers = playerStartInfos
// 									.Where(info => info.playerId != connection.ClientId)
// 									.ToArray()
// 			};
// 			connection.netControls.StartGame(gameStartInfo);
// 		}

// 		private void HandleServerGameUpdateMessage(byte [] argumentsData)
// 		{
// 			var arguments = argumentsData.ToStructure<ServerGameUpdateArgs>(out byte [] packageData);
// 			Debug.Log($"Received update from servers, id {arguments.playerId} {((arguments.playerId == connection.ClientId) ? "(skipping own update)" : "")}");

// 			if (connection.receivers != null)
// 			{
// 				if (arguments.playerId != connection.ClientId)
// 				{
// 					var package = packageData.ToStructure<PlayerGameUpdatePackage>();
// 					// connection.receivedPositions[arguments.playerId].Value = package.position;
// 					connection.receivers[arguments.playerId].Receive(package);
// 					Debug.Log(package.position);
// 				}
// 			}
// 			else
// 			{
// 				Debug.LogError("Receivers not yet created");
// 			}
// 		}

// 		private void HandleServerConfirmJoinMessage(byte [] argumentsData)
// 		{
// 			var arguments = argumentsData.ToStructure<ServerConfirmJoinArgs>();

// 			if (arguments.accepted)
// 			{
// 				connection.ClientId = arguments.playerId;
// 				connection.joinedServer = connection.requestedServer;
// 				Debug.Log($"Server accepted request, my id is {connection.ClientId}");
// 			}
// 			else
// 			{
// 				Debug.Log("Server declined request");
// 			}

// 			connection.OnJoinedServer?.Invoke();
// 			connection.requestedServer = null;
// 		}
// 	}
// }