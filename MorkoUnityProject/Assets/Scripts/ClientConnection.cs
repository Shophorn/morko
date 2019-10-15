using System; 
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using UnityEngine;

using Morko.Network;
using Morko.Threading;

// Todo(Leo): probably a bad idea
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

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PlayerGameUpdatePackage
{
	public int playerId;
	public Vector3 position;
}

public class ClientConnection : MonoBehaviour
{
	public bool AutoStart { get; set; }

	public int netUpdateIntervalMs = 50;
	private float netUpdateInterval => netUpdateIntervalMs / 1000f;
	private float nextNetUpdateTime;

	public string playerName = "Default Player";

	public int selectedServerIndex;
	public string selectedServerName;

	public int connectionRetryTimeMs = 500;
	private float connectionRetryTime => connectionRetryTimeMs / 1000f;
	
	public List<ServerInfo> servers = new List<ServerInfo>();
	private ServerInfo requestedServer;
	private ServerInfo joinedServer;
	public GameObject avatarPrefab;
	public Transform senderTransform;

	private Dictionary<int, Transform> receiverTransforms;
	private Dictionary<int, Synchronized<Vector3>> receivedPositions;

	private UdpClient udpClient;
	private bool listenBroadcast = false;

	private readonly ThreadControl detectServersThread = new ThreadControl();
	private readonly ThreadControl receiveUpdateThread = new ThreadControl();
	private readonly ThreadControl<SendUpdateThread> sendUpdateThread 
		= new ThreadControl<SendUpdateThread>();

	public int myClientId;

	private static readonly ConcurrentQueue<Action> mainThreadSyncQueue = new ConcurrentQueue<Action>();

	[Serializable]
	public struct EndPointDisplay
	{	
		public string address;
		public int port;

		public static EndPointDisplay FromEndPoint(IPEndPoint endPoint)
		{
			return new EndPointDisplay
			{
				address = endPoint.Address.ToString(),
				port = endPoint.Port
			};
		}
	}

	private void Start()
	{
		if (AutoStart)
		{
			StartListen();
		}
	}

	public EndPointDisplay serverEndpoint;
	public EndPointDisplay myEndPoint;

	public void CreateGameInstance(PlayerStartInfo [] playerStartInfos)
	{
		if (playerStartInfos.Length == 0)
		{
			Debug.LogError("Cannot start a game with 0 players");
			return;
		}

		Debug.Log("Game session created");


		senderTransform = Instantiate(avatarPrefab, Vector3.zero, Quaternion.identity).transform;
		senderTransform.gameObject.AddComponent<ControlsTest>();

		// Todo(Leo): Load proper level etc.

		receiverTransforms = new Dictionary<int, Transform>();
		receivedPositions = new Dictionary<int, Synchronized<Vector3>>();

		// int netPlayerCount = playerStartInfos.Length - 1;
		foreach (var item in playerStartInfos)
		{
			if (item.playerId != myClientId)
			{
				Vector3 startPosition = Vector3.zero;
				
				receiverTransforms.Add(	item.playerId,
										Instantiate(avatarPrefab, startPosition, Quaternion.identity).transform);

				receivedPositions.Add(	item.playerId,
										new Synchronized<Vector3>(startPosition));
			}
		}

		// Todo(Leo): Start update here 
		doNetworkUpdate = true;
	}

	private class ReceiveThread : IThreadRunner
	{
		public ClientConnection connection;

		public void Run ()
		{
			var receiveEndPoint = new IPEndPoint(IPAddress.Any, 0);
			while(true)
			{
				if (ProtocolFormat.TryParseCommand(	connection.udpClient.Receive(ref receiveEndPoint),
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
						var existingServer = connection.servers
												.Find(server => IPEndPoint.Equals(	server.endPoint,
																					receiveEndPoint));
						if (existingServer != null)
						{
							existingServer.lastConnectionTime = DateTime.Now;
						}
						else
						{
							var arguments = contents.ToStructure<ServerIntroduceArgs>();
							connection.servers.Add(new ServerInfo
							{
								endPoint 			= receiveEndPoint,
								name 				= arguments.name,
								lastConnectionTime 	= DateTime.Now
							});

							// Todo(Leo): set default, so we get faster to play. Remove when testing is done
							if (connection.selectedServerIndex < 0)
							{
								connection.selectedServerIndex = 0;
								connection.OnValidate();
							}
						}
					} break;

					case NetworkCommand.ServerStartGame:
					{
						var arguments = contents.ToStructure<ServerStartGameArgs>(out byte [] packageData);
						int playerCount = arguments.playerCount;
						var playerStartInfos = packageData.ToArray<PlayerStartInfo>(playerCount);
						
						mainThreadSyncQueue.Enqueue(() => connection.CreateGameInstance(playerStartInfos));

					} break;

					case NetworkCommand.ServerGameUpdate:
					{
						var arguments = contents.ToStructure<ServerGameUpdateArgs>(out byte [] packageData);
						Debug.Log($"Received update from servers, id {arguments.playerId}");

						if (connection.receivedPositions != null)
						{
							// if (packageData.Length == Marshal.SizeOf(default(PlayerGameUpdatePackage)))
							if (arguments.playerId != connection.myClientId)
							{
								var package = packageData.ToStructure<PlayerGameUpdatePackage>();
								connection.receivedPositions[arguments.playerId].Write(package.position);
							}
						}
						else
						{
							Debug.Log("Receivers not yet created");
						}


					} break;

					case NetworkCommand.ServerConfirmJoin:
					{
						var arguments = contents.ToStructure<ServerConfirmJoinArgs>();

						if (arguments.accepted)
						{
							connection.myClientId = arguments.playerId;
							connection.joinedServer = connection.requestedServer;
							Debug.Log($"Server accepted request, my id is {connection.myClientId}");
						}
						else
						{
							Debug.Log("Server declined request");
						}

						connection.requestedServer = null;
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
		receiveUpdateThread.Start(new ReceiveThread { connection = this });

		// doNetworkUpdate = true;
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

		while(mainThreadSyncQueue.TryDequeue(out Action action))
		{
			action.Invoke();
		}

		if (doNetworkUpdate)
		{
			foreach (var item in receivedPositions)
			{
				int key = item.Key;
				receiverTransforms[key].position = item.Value.Read();
			}

			// Note(Leo): Lol, no accessing transform from threads??
			if (senderTransform != null && sendUpdateThread.IsRunning)
				sendUpdateThread.Runner.playerPosition = senderTransform.position;
		}
	}

	public void StartListen()
	{
		Debug.Log("Start listening broadcast");
		udpClient = new UdpClient(broadcastPort);

		detectServersThread.Start(new ReceiveThread {connection = this});
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

		serverEndpoint = EndPointDisplay.FromEndPoint(requestedServer.endPoint);

		var arguments 	= new PlayerRequestJoinArgs{ playerName = playerName };
		var data 		= Morko.Network.ProtocolFormat.MakeCommand(arguments);
		var endPoint 	= requestedServer.endPoint;

		int sentBytes = udpClient.Send(data, data.Length, endPoint);
		Debug.Log($"Sent {sentBytes} to {requestedServer.endPoint}");
	}
}