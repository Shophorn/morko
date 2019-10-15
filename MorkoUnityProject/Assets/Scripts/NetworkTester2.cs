using System; 
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using UnityEngine;

using Morko.Network;
using Morko.Threading;

using static Morko.Network.Constants;

[Serializable]
public class ServerInfo
{
	public IPEndPoint endPoint;
	public string name;
	public DateTime lastConnectionTime;  
}

public struct UdpState
{
	public UdpClient client;
	public IPEndPoint endPoint;
}

[Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PlayerGameUpdatePackage
{
	public int playerId;
	public Vector3 position;
}

public class NetworkTester2 : MonoBehaviour
{
	public Transform senderTransform;
	public Transform receiverTransform;

	private Synchronized<Vector3> receivedPosition = new Synchronized<Vector3>();

	public int netUpdateIntervalMs = 50;
	private float netUpdateInterval => netUpdateIntervalMs / 1000f;
	private float nextNetUpdateTime;

	public string playerName;

	public int selectedServerIndex;
	public string selectedServerName;

	public int connectionRetryTimeMs = 500;
	private float connectionRetryTime => connectionRetryTimeMs / 1000f;
	
	public List<ServerInfo> servers = new List<ServerInfo>();
	private ServerInfo requestedServer;
	private ServerInfo joinedServer;

	private List<Synchronized<Vector3>> receivedPositions;

	private UdpClient udpClient;
	private bool listenBroadcast = false;

	private readonly ThreadControl detectServersThread = new ThreadControl();
	private readonly ThreadControl receiveUpdateThread = new ThreadControl();
	private readonly ThreadControl<SendUpdateThread> sendUpdateThread 
		= new ThreadControl<SendUpdateThread>();

	public int myClientId;

	int serverConnectionIndex;

	public void CreateGame(PlayerStartInfo [] playerStartInfos)
	{

		foreach (var item in playerStartInfos)
		{
			Debug.Log($"Player #{item.playerId}: {item.name}");
		}

		Debug.Log($"Game starts with {playerStartInfos.Length} players");
	}

	private class ReceiveThread : IThreadRunner
	{
		public NetworkTester2 tester;

		public void Run ()
		{
			var receiveEndPoint = new IPEndPoint(IPAddress.Any, 0);
			while(true)
			{
				if (ProtocolFormat.TryParseCommand(	tester.udpClient.Receive(ref receiveEndPoint),
													out NetworkCommand command,
													out byte [] contents) == false)
				{
					continue;
				}

				switch (command)
				{
					case NetworkCommand.ServerIntroduce:
					{
						receiveEndPoint.Port = Constants.serverReceivePort;
						var existingServer = tester.servers
												.Find(server => IPEndPoint.Equals(	server.endPoint,
																					receiveEndPoint));
						if (existingServer != null)
						{
							existingServer.lastConnectionTime = DateTime.Now;
						}
						else
						{
							var arguments = contents.ToStructure<ServerIntroduceArgs>();
							tester.servers.Add(new ServerInfo
							{
								endPoint 			= receiveEndPoint,
								name 				= arguments.name,
								lastConnectionTime 	= DateTime.Now
							});
						}
					} break;

					case NetworkCommand.ServerStartGame:
					{
						var arguments = contents.ToStructure<ServerStartGameArgs>(out byte [] packageData);
						int playerCount = arguments.playerCount;
						var playerStartInfos = packageData.ToArray<PlayerStartInfo>(playerCount);
						tester.CreateGame(playerStartInfos);

					} break;

					case NetworkCommand.ServerGameUpdate:
					{
						var arguments = contents.ToStructure<ServerGameUpdateArgs>(out byte [] packageData);
						Debug.Log($"Received player package: {packageData.Length}");

						if (packageData.Length == Marshal.SizeOf(default(PlayerGameUpdatePackage)))
						{
							var package = packageData.ToStructure<PlayerGameUpdatePackage>();
							tester.receivedPosition.Write(package.position);
						}


						Debug.Log($"Received update from servers, id {arguments.playerId}");

					} break;

					case NetworkCommand.ServerConfirmJoin:
					{
						var arguments = contents.ToStructure<ServerConfirmJoinArgs>();

						if (arguments.accepted)
						{
							tester.myClientId = arguments.playerId;
							tester.joinedServer = tester.requestedServer;
							Debug.Log($"Server accepted request, my id is {tester.myClientId}");
						}
						else
						{
							Debug.Log("Server declined request");
						}

						tester.requestedServer = null;
					} break;
				}
			}
		}

		public void CleanUp(){}
	}

	public class SendUpdateThread : IThreadRunner
	{
		public Vector3 		playerPosition;
		public int 			sendDelayMs;
		public int 			clientId;
		public UdpClient 	udpClient;
		public IPEndPoint 	endPoint;

		public void Run()
		{
			Debug.Log($"Start SendUpdateThread, endPoint = {endPoint}");
			while(true)
			{
				var updateArgs = new PlayerGameUpdateArgs
				{
					playerId = clientId
				};

				byte [] updatePackage = new PlayerGameUpdatePackage
				{
					playerId = clientId,
					position = playerPosition
				}.ToBinary();

				byte [] data = ProtocolFormat.MakeCommand (updateArgs, updatePackage);

				udpClient.Send(data, data.Length, endPoint);

				Thread.Sleep(sendDelayMs);
			}
		}

		public void CleanUp() {}
	}

	public void StartUpdate()
	{
		var localEndPoint = new IPEndPoint(IPAddress.Any, multicastPort);
		udpClient = new UdpClient(localEndPoint);
		udpClient.JoinMulticastGroup(multicastAddress);

		sendUpdateThread.Start(new SendUpdateThread
		{
			sendDelayMs = netUpdateIntervalMs,
			clientId 	= myClientId,
			udpClient 	= udpClient,
			endPoint 	= joinedServer.endPoint
		});
		receiveUpdateThread.Start(new ReceiveThread { tester = this });

		doNetworkUpdate = true;
	}

	public void StopUpdate()
	{
		receiveUpdateThread.Stop();
		doNetworkUpdate = false;
	}

	public bool doNetworkUpdate = false;

	private void Update()
	{
		DateTime thresholdTime = DateTime.Now.AddSeconds(-connectionRetryTime);
		for(int serverIndex = 0; serverIndex < servers.Count; serverIndex++)
		{
			if (servers[serverIndex].lastConnectionTime < thresholdTime)
			{
				servers.RemoveAt(serverIndex);
				serverIndex--;
			}
		}

		if (receiverTransform != null)
			receiverTransform.position = receivedPosition.Read();			

		// Note(Leo): Lol, no accessing transform from threads??
		if (senderTransform != null && sendUpdateThread.IsRunning)
			sendUpdateThread.Runner.playerPosition = senderTransform.position;
	}

	public void StartListen()
	{
		Debug.Log("Start listening broadcast");
		udpClient = new UdpClient(broadcastPort);

		detectServersThread.Start(new ReceiveThread {tester = this});
	}

	public void StopListen()
	{
		detectServersThread.Stop();
		udpClient?.Close();
	}

	public bool waitRequestThreadRunning;
	Thread waitRequestFromServerThread;

	private void OnDisable()
	{
		listenBroadcast = false;
		udpClient?.Close();

		if (waitRequestThreadRunning)
		{
			try { waitRequestFromServerThread.Abort();}
			catch (ThreadAbortException) { Debug.Log("Wait join confirm aborted");}
			waitRequestThreadRunning = false;
		}
	}

	private void OnValidate()
	{
		// Note(Leo): These are for debugging only
		// string printout = "";
		// printout += $"sizeof(ServerIntroduceArgs) = {Marshal.SizeOf(default(ServerIntroduceArgs))}\n";
		// printout += $"sizeof(ServerConfirmJoinArgs) = {Marshal.SizeOf(default(ServerConfirmJoinArgs))}\n";
		// printout += $"sizeof(ServerStartGameArgs) = {Marshal.SizeOf(default(ServerStartGameArgs))}\n";
		// printout += $"sizeof(PlayerRequestJoinArgs) = {Marshal.SizeOf(default(PlayerRequestJoinArgs))}\n";
		// Debug.Log(printout);

		if (servers.Count == 0)
		{
			selectedServerIndex = -1;
			selectedServerName = "-";
		}
		else
		{
			selectedServerIndex = Mathf.Clamp(selectedServerIndex, 0, servers.Count - 1);
			selectedServerName = servers[selectedServerIndex].name;
		}
	}

	public void JoinSelectedServer()
	{
		requestedServer = servers[selectedServerIndex];

		var arguments 	= new PlayerRequestJoinArgs{ playerName = playerName };
		var data 		= Morko.Network.ProtocolFormat.MakeCommand(arguments);
		var endPoint 	= requestedServer.endPoint;

		udpClient.Send(data, data.Length, endPoint);
	}
}