using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

using UnityEngine;

using Morko.Network;
using Morko.Threading;

public partial class ClientController
{
	private class ListenServerBroadcastThread : IThreadRunner
	{
		public ClientController controller;

		void IThreadRunner.Run ()
		{
			var receiveEndPoint = new IPEndPoint(IPAddress.Any, 0);
			while(true)
			{
				var receivedData = controller.udpClient.Receive(ref receiveEndPoint);
				bool isCommand = ProtocolFormat.TryParseCommand(receivedData,
																out NetworkCommand command,
																out byte [] argumentsData);

				if (isCommand && command == NetworkCommand.ServerIntroduce)
					controller.HandleServerIntroduceMessage(argumentsData, receiveEndPoint);
			}
		}
	
		// Note(Leo): This is part of interface now, but we do not need it
		void IThreadRunner.CleanUp() {}
	}

	private void HandleServerIntroduceMessage(byte [] argumentsData, IPEndPoint receiveEndPoint)
	{
		receiveEndPoint.Port = Constants.serverReceivePort;
		// TODO(LEO IMPORTANT): Thread safety........!!!!!!!!!! >:(
		var existingServer = servers.Find(server => IPEndPoint.Equals(	server.endPoint,
																		receiveEndPoint));
		if (existingServer != null)
		{
			existingServer.lastConnectionTime = DateTime.Now;
		}
		else
		{
			var arguments = argumentsData.ToStructure<ServerIntroduceArgs>();
			servers.Add(new ServerConnectionInfo
			{
				serverInfo = new ServerInfo
				{ 
					serverName = arguments.serverName
				},
				endPoint 			= receiveEndPoint,
				lastConnectionTime 	= DateTime.Now
			});

			if (selectedServerIndex < 0)
			{
				selectedServerIndex = 0;
			}

			netControls.UpdateServersList(GetServers());
		}
	}
}

public partial class ClientController
{
	private class TcpReceiveThread : IThreadRunner
	{
		public ClientController controller;

		void IThreadRunner.Run ()
		{
			while (true)
			{
				var command = controller.tcpStream.ReadTcpMessage(out byte [] argumentsData);

				switch (command)
				{
					case NetworkCommand.ServerConfirmJoin:
						controller.HandleServerConfirmJoinMessage(argumentsData);
						break;

					case NetworkCommand.ServerStartGame:
						controller.HandleServerStartGameMessage(argumentsData);
						break;
				}
			}
		}

		void IThreadRunner.CleanUp () {}
	}

	private void HandleServerConfirmJoinMessage(byte [] argumentsData)
	{
		var arguments = argumentsData.ToStructure<ServerConfirmJoinArgs>();

		if (arguments.accepted)
		{
			ClientId = arguments.playerId;
			joinedServer = requestedServer;
			Debug.Log($"Server accepted request, my id is {ClientId}");
		}
		else
		{
			// Todo(Leo): What now???
			Debug.Log("Server declined request");
		}

		OnJoinedServer?.Invoke();
		requestedServer = null;
	}

	private void HandleServerStartGameMessage(byte [] argumentsData)
	{
		var arguments 			= argumentsData.ToStructure<ServerStartGameArgs>(out byte [] packageData);
		var playerStartInfos 	= packageData.ToArray<PlayerStartInfo>();

		Debug.Log($"Server called to start game, arguments parsed, my index = {ClientId}, playerCount = {arguments.playerCount}, packageData.Length = {packageData.Length}");

		var gameStartInfo = new GameStartInfo
		{
			mapIndex = 0,
			localPlayer = playerStartInfos
								.Where(info => info.playerId == ClientId)
								.First(),
			remotePlayers = playerStartInfos
								.Where(info => info.playerId != ClientId)
								.ToArray()
		};
		netControls.StartGame(gameStartInfo);
	}
}

public partial class ClientController
{	
	private class ReceiveUpdateThread : IThreadRunner
	{
		public ClientController controller;

		void IThreadRunner.Run ()
		{
			var receiveEndPoint = new IPEndPoint(IPAddress.Any, 0);
			while(true)
			{
				var receivedData = controller.udpClient.Receive(ref receiveEndPoint);
				bool isCommand = ProtocolFormat.TryParseCommand(receivedData,
																out NetworkCommand command,
																out byte [] argumentsData);

				if (isCommand && command == NetworkCommand.ServerGameUpdate)
					controller.HandleServerGameUpdateMessage(argumentsData);
			}
		}

		// Note(Leo): This is part of interface now, but we do not need it
		void IThreadRunner.CleanUp(){}
	}

	private void HandleServerGameUpdateMessage(byte [] argumentsData)
	{
		var arguments = argumentsData.ToStructure<ServerGameUpdateArgs>(out byte [] packageData);
		Debug.Log($"Received update from servers, id {arguments.playerId} {((arguments.playerId == ClientId) ? "(skipping own update)" : "")}");

		if (receivers != null)
		{
			if (arguments.playerId != ClientId)
			{
				var package = packageData.ToStructure<PlayerGameUpdatePackage>();
				receivers[arguments.playerId].Receive(package);
			}
		}
		else
		{
			Debug.LogError("Receivers not yet created");
		}
	}
}