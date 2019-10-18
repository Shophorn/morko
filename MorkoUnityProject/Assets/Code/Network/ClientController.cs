using System; 
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using UnityEngine;

using Morko.Network;
using Morko.Threading;

// Todo(Leo): probably a bad idea to use statically
using static Morko.Network.Constants;

[Serializable]
public class ServerInfo
{
	public string name;
	public int mapIndex;
	public int maxPlayers;
	public int gameDuration;
}

[Serializable]
public class ServerConnectionInfo
{
	public ServerInfo server;	
	public IPEndPoint endPoint;
	public DateTime lastConnectionTime;  
}

public class GameStartInfo
{
	public int netPlayerCount;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PlayerGameUpdatePackage
{
	public int playerId;
	public Vector3 position;
}

public class ClientController : MonoBehaviour
{
	[Header("Network Cofiguration")]
	public int netUpdateIntervalMs = 50;
	public int connectionRetryTimeMs = 500;

	private float netUpdateInterval => netUpdateIntervalMs / 1000f;
	private float nextNetUpdateTime;
	private float connectionRetryTime => connectionRetryTimeMs / 1000f;
	

	public bool AutoStart { get; set; }

	[Header("Player info")]
	[HideInInspector] public string playerName = "Default Player";
	[HideInInspector] public int ClientId { get; private set; } = -1;

	[HideInInspector] public int selectedServerIndex;

	public List<ServerConnectionInfo> servers { get; }= new List<ServerConnectionInfo>();
	private ServerConnectionInfo requestedServer;
	private ServerConnectionInfo joinedServer;
	
	public GameObject AvatarPrefab { get; set; }
	private Transform senderTransform;
	private Dictionary<int, Transform> receiverTransforms;
	private Dictionary<int, Synchronized<Vector3>> receivedPositions;

	private UdpClient udpClient;

	private readonly ThreadControl detectServersThread = new ThreadControl();
	private readonly ThreadControl receiveUpdateThread = new ThreadControl();
	private readonly ThreadControl<SendUpdateThread> sendUpdateThread 
		= new ThreadControl<SendUpdateThread>();

	private bool doNetworkUpdate = false;
	private static readonly ConcurrentQueue<Action> mainThreadSyncQueue = new ConcurrentQueue<Action>();

	public event Action OnServerListChanged;

	public event Action OnServerStartGame;
	public event Action OnServerAbortGame;
	public event Action OnServerFinishGame;

	public ServerInfo [] GetServers()
	{
		var result = servers.Select(connection => connection.server).ToArray();
		return result;	
	}

	// Note(Leo): Debug class only
	// [Serializable]
	// public struct EndPointDisplay
	// {	
	// 	public string address;
	// 	public int port;

	// 	public static EndPointDisplay FromEndPoint(IPEndPoint endPoint)
	// 	{
	// 		return new EndPointDisplay
	// 		{
	// 			address = endPoint.Address.ToString(),
	// 			port = endPoint.Port
	// 		};
	// 	}
	// }

	private void Start()
	{
		if (AutoStart)
		{
			StartListen();
		}
	}

	public void CreateGameInstance(PlayerStartInfo [] playerStartInfos)
	{
		if (playerStartInfos.Length == 0)
		{
			Debug.LogError("Cannot start a game with 0 players");
			return;
		}

		Debug.Log($"Game session created, {playerStartInfos.Length} players");


		senderTransform = Instantiate(AvatarPrefab, Vector3.zero, Quaternion.identity).transform;
		senderTransform.gameObject.AddComponent<ControlsTest>();

		// Todo(Leo): Load proper level etc.

		receiverTransforms = new Dictionary<int, Transform>();
		receivedPositions = new Dictionary<int, Synchronized<Vector3>>();

		// int netPlayerCount = playerStartInfos.Length - 1;
		foreach (var item in playerStartInfos)
		{
			if (item.playerId != ClientId)
			{
				Vector3 startPosition = Vector3.zero;
				
				receiverTransforms.Add(	item.playerId,
										Instantiate(AvatarPrefab, startPosition, Quaternion.identity).transform);

				receivedPositions.Add(	item.playerId,
										new Synchronized<Vector3>(startPosition));
			}
		}

		// Todo(Leo): Start update here 
		doNetworkUpdate = true;
	}

	private class ReceiveThread : IThreadRunner
	{
		public ClientController connection;

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
							connection.servers.Add(new ServerConnectionInfo
							{
								server 				= new ServerInfo
													{ 
														name = arguments.name
													},
								endPoint 			= receiveEndPoint,
								lastConnectionTime 	= DateTime.Now
							});

							if (connection.selectedServerIndex < 0)
							{
								connection.selectedServerIndex = 0;
							}
						}

						connection.OnServerListChanged?.Invoke();
					} break;

					case NetworkCommand.ServerStartGame:
					{
						Debug.Log("Server called to start game");

						var arguments = contents.ToStructure<ServerStartGameArgs>(out byte [] packageData);
						int playerCount = arguments.playerCount;
						var playerStartInfos = packageData.ToArray<PlayerStartInfo>(playerCount);
						
						Debug.Log("Server called to start game, arguments parsed");
						mainThreadSyncQueue.Enqueue(() => connection.CreateGameInstance(playerStartInfos));

					} break;

					case NetworkCommand.ServerGameUpdate:
					{
						var arguments = contents.ToStructure<ServerGameUpdateArgs>(out byte [] packageData);
						Debug.Log($"Received update from servers, id {arguments.playerId}");

						if (connection.receivedPositions != null)
						{
							if (arguments.playerId != connection.ClientId)
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
							connection.ClientId = arguments.playerId;
							connection.joinedServer = connection.requestedServer;
							Debug.Log($"Server accepted request, my id is {connection.ClientId}");
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
				var updateArgs = new ClientGameUpdateArgs
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
			clientId 	= ClientId,
			udpClient 	= udpClient,
			endPoint 	= joinedServer.endPoint
		});
		receiveUpdateThread.Start(new ReceiveThread { connection = this });
	}

	public void StopUpdate()
	{
		receiveUpdateThread.Stop();
		doNetworkUpdate = false;
	}

	private void Update()
	{
		DateTime thresholdTime = DateTime.Now.AddSeconds(-connectionRetryTime);
		for(int serverIndex = 0; serverIndex < servers.Count; serverIndex++)
		{
			Debug.Log($"servers at {serverIndex}: {servers[serverIndex]}");
			if (servers[serverIndex].lastConnectionTime < thresholdTime)
			{
				servers.RemoveAt(serverIndex);
				serverIndex--;
				OnServerListChanged?.Invoke();
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
		udpClient = new UdpClient(broadcastPort);
		detectServersThread.Start(new ReceiveThread {connection = this});
	}

	public void StopListen()
	{
		detectServersThread.Stop();
		udpClient?.Close();
	}

	private void OnDisable()
	{
		udpClient?.Close();
	}

	public void JoinSelectedServer()
	{
		requestedServer = servers[selectedServerIndex];

		var arguments 	= new ClientRequestJoinArgs{ playerName = playerName };
		var data 		= Morko.Network.ProtocolFormat.MakeCommand(arguments);
		var endPoint 	= requestedServer.endPoint;

		int sentBytes = udpClient.Send(data, data.Length, endPoint);
		Debug.Log($"Sent {sentBytes} to {requestedServer.endPoint}");
	}
}