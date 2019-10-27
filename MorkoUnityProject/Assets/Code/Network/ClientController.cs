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

public interface IClientNetControllable
{
	void OnServerStartGame(GameStartInfo gameStartInfo);
	void OnServerListChanged(ServerInfo [] servers);
}

[Serializable]
public class ServerInfo
{
	public string serverName;
	public int mapIndex;
	public int maxPlayers;
	public int gameDurationSeconds;
}

[Serializable]
public class ServerConnectionInfo
{
	public ServerInfo serverInfo;	
	public IPEndPoint endPoint;
	public DateTime lastConnectionTime;  
}

public class GameStartInfo
{
	public int mapIndex;
	public PlayerStartInfo localPlayer;
	public PlayerStartInfo [] remotePlayers;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PlayerGameUpdatePackage
{
	public int playerId;
	public Vector3 position;
}

public class ClientController : MonoBehaviour
{
	[Header("Network Configuration")]
	public int netUpdateIntervalMs = 50;
	public int connectionRetryTimeMs = 500;

	private float netUpdateInterval => netUpdateIntervalMs / 1000f;
	private float nextNetUpdateTime;
	private float connectionRetryTime => connectionRetryTimeMs / 1000f;
	
	public bool AutoStart { get; set; }

	[Header("Player info")]
	[HideInInspector] public string playerName = "Default Player";
	[HideInInspector] public int ClientId { get; set; } = -1;

	[HideInInspector] public int selectedServerIndex;

	public List<ServerConnectionInfo> servers { get; }= new List<ServerConnectionInfo>();
	private ServerConnectionInfo requestedServer;
	private ServerConnectionInfo joinedServer;
	
	public GameObject AvatarPrefab { get; set; }
	private Transform senderTransform;
	private Dictionary<int, Transform> receiverTransforms;
	private Dictionary<int, Atomic<Vector3>> receivedPositions;

	private UdpClient udpClient;
	public IPEndPoint CurrentEndPoint => udpClient.Client.LocalEndPoint as IPEndPoint;

	private readonly ThreadControl detectServersThread = new ThreadControl();
	private readonly ThreadControl receiveUpdateThread = new ThreadControl();
	private readonly ThreadControl<SendUpdateThread> sendUpdateThread 
		= new ThreadControl<SendUpdateThread>();

	private bool doNetworkUpdate = false;

	// Event section
	public event Action OnServerAbortGame;
	public event Action OnServerFinishGame;

	public event Action OnJoinedServer;
	public event Action OnQuitServer;

	IClientNetControllable netControls;

	private void Awake()
	{
		netControls = GetComponent<IClientNetControllable>();
	}

	public void SetSender(Transform transform)
	{
		senderTransform = transform;
	}

	public void InitializeReceivers()
	{
		receiverTransforms = new Dictionary<int, Transform>();
		receivedPositions = new Dictionary<int, Atomic<Vector3>>();
	}

	public void SetReceiver(int index, Transform transform)
	{
		receiverTransforms.Add(index, transform);
		receivedPositions.Add(index, new Atomic<Vector3>());
	}

	public void StartNetworkUpdate()
	{
		doNetworkUpdate = true;
	}

	private ServerInfo [] GetServers()
	{
		var result = servers.Select(connection => connection.serverInfo).ToArray();
		return result;	
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
								serverInfo = new ServerInfo
								{ 
									serverName = arguments.serverName
								},
								endPoint 			= receiveEndPoint,
								lastConnectionTime 	= DateTime.Now
							});

							if (connection.selectedServerIndex < 0)
							{
								connection.selectedServerIndex = 0;
							}
	
							connection.netControls.OnServerListChanged(connection.GetServers());
						}

					} break;

					case NetworkCommand.ServerStartGame:
					{
						var arguments 			= contents.ToStructure<ServerStartGameArgs>(out byte [] packageData);
						int playerCount 		= arguments.playerCount;
						var playerStartInfos 	= packageData.ToArray<PlayerStartInfo>(playerCount);

						Debug.Log($"Server called to start game, arguments parsed, my index = {connection.ClientId}");

						var gameStartInfo = new GameStartInfo
						{
							mapIndex = 0,
							localPlayer = playerStartInfos
												.Where(info => info.playerId == connection.ClientId)
												.First(),
							remotePlayers = playerStartInfos
												.Where(info => info.playerId != connection.ClientId)
												.ToArray()
						};
						connection.netControls.OnServerStartGame(gameStartInfo);
					} break;

					case NetworkCommand.ServerGameUpdate:
					{
						var arguments = contents.ToStructure<ServerGameUpdateArgs>(out byte [] packageData);
						Debug.Log($"Received update from servers, id {arguments.playerId} {((arguments.playerId == connection.ClientId) ? "(skipping own update)" : "")}");

						if (connection.receivedPositions != null)
						{
							if (arguments.playerId != connection.ClientId)
							{
								var package = packageData.ToStructure<PlayerGameUpdatePackage>();
								connection.receivedPositions[arguments.playerId].Write(package.position);
								Debug.Log(package.position);
							}
						}
						else
						{
							Debug.LogError("Receivers not yet created");
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

						connection.OnJoinedServer?.Invoke();
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
			Debug.Log($"[CLIENT]: Start SendUpdateThread, endPoint = {endPoint}");
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

				Debug.Log("[CLIENT]: Send data to server");
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

		Debug.Log($"Joined server = {joinedServer}");

		sendUpdateThread.Start(new SendUpdateThread
		{
			sendDelayMs = netUpdateIntervalMs,
			clientId 	= ClientId,
			udpClient 	= udpClient,
			endPoint 	= joinedServer.endPoint
		});
		receiveUpdateThread.Start(new ReceiveThread { connection = this });
	}

	public void CreateHostingPlayerConnection()
	{
		var localEndPoint = new IPEndPoint(IPAddress.Any, multicastPort);
		udpClient = new UdpClient(localEndPoint);
		udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
		udpClient.JoinMulticastGroup(multicastAddress);
	}

	public void StartUpdateAsHostingPlayer()
	{
		// var serverEndPoint = new IPEndPoint(CurrentEndPoint.Address, Constants.serverReceivePort);
		var serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), Constants.serverReceivePort);
		sendUpdateThread.Start(new SendUpdateThread
		{
			sendDelayMs = netUpdateIntervalMs,
			clientId 	= ClientId,
			udpClient 	= udpClient,
			endPoint 	= serverEndPoint
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
				netControls.OnServerListChanged(GetServers());
			}
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

	public void StartListenBroadcast()
	{
		udpClient = new UdpClient(broadcastPort);
		detectServersThread.Start(new ReceiveThread {connection = this});
	}

	public void StopListenBroadcast()
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

	public void CreateLocalServerConnection()
	{
		StartUpdate();

		// var localEndPoint = new IPEndPoint(IPAddress.Any, multicastPort);
		// udpClient = new UdpClient(localEndPoint);
	}
}