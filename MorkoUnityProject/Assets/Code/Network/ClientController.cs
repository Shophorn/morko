using System; 
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
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
	void StartGame(GameStartInfo gameStartInfo);
	void UpdateServersList(ServerInfo [] servers);
}

[Serializable]
public class ServerInfo
{
	public string serverName;
	public string hostingPlayerName;
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
	public Vector3 position;
	public float rotation;
}

public interface INetworkReceiver
{
	void Receive(PlayerGameUpdatePackage package);
}

public interface INetworkSender
{
	PlayerGameUpdatePackage GetPackageToSend();
}

public partial class ClientController : MonoBehaviour
{
	[Header("Network Configuration")]
	public int netUpdateIntervalMs = 50;
	public int connectionRetryTimeMs = 500;

	private float netUpdateInterval => netUpdateIntervalMs / 1000f;
	private float nextNetUpdateTime;
	private float connectionRetryTime => connectionRetryTimeMs / 1000f;
	
	[Header("Player info")]
	[HideInInspector] public string playerName = "Default Player";
	[HideInInspector] public int ClientId { get; set; } = -1;

	[HideInInspector] public int selectedServerIndex;

	public List<ServerConnectionInfo> servers { get; }= new List<ServerConnectionInfo>();
	private ServerConnectionInfo requestedServer;
	private ServerConnectionInfo joinedServer;
	
	private INetworkSender sender;
	private Dictionary<int, INetworkReceiver> receivers;

	private TcpClient tcpClient;
	private NetworkStream tcpStream;

	private UdpClient udpClient;
	public IPEndPoint CurrentEndPoint => udpClient.Client.LocalEndPoint as IPEndPoint;

	// Todo(Leo): Please just make these normal variables, so we can like check against null... 
	private ThreadControl receiveUpdateThread;
	private ThreadControl sendUpdateThread;
	private SendUpdateThread sendUpdateThreadRunner;
	private ThreadControl tcpReceiveThread;
	private ThreadControl listenServersThread;

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

	public void SetSender(INetworkSender sender)
	{
		this.sender = sender;
		Debug.Log($"[CLIENT CONTROLLER]: sender set {sender}");
	}

	public void InitializeReceivers()
	{
		receivers = new Dictionary<int, INetworkReceiver>();
	}

	public void SetReceiver(int index, INetworkReceiver receivingPlayer)
	{
		receivers.Add(index, receivingPlayer);
	}

	private ServerInfo [] GetServers()
	{
		var result = servers.Select(connection => connection.serverInfo).ToArray();
		return result;	
	}

	public void StartUpdate()
	{
		Debug.Log("[CLIENT CONTROLLER]: Started game!");

		var localEndPoint 	= new IPEndPoint(IPAddress.Any, multicastPort);
		udpClient 			= new UdpClient(localEndPoint);
		udpClient.JoinMulticastGroup(multicastAddress);

		sendUpdateThreadRunner = new SendUpdateThread
		{
			sendDelayMs = netUpdateIntervalMs,
			udpClient 	= udpClient,
			endPoint 	= joinedServer.endPoint,
			controller 	= this
		};
		sendUpdateThread = ThreadControl.Start(sendUpdateThreadRunner);
		
		receiveUpdateThread = ThreadControl.Start(new ReceiveUpdateThread { controller = this });
	}


	public void StopUpdate()
	{
		receiveUpdateThread.Stop();
		udpClient.Close();
	}

	#region MONOBEHAVIOUR METHODS
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
				netControls.UpdateServersList(GetServers());
			}
		}
	}

	private void OnDisable()
	{
		// Todo(Leo): Can we do and should we do StopAndWait also in a separate thread??
		sendUpdateThread.StopAndWait();
		receiveUpdateThread.StopAndWait();
		tcpReceiveThread?.StopAndWait();
		listenServersThread?.StopAndWait();

		udpClient?.Close();
		tcpClient?.Close();
	}
	#endregion

	public void StartListenBroadcast()
	{
		udpClient = new UdpClient(Constants.broadcastPort);
		listenServersThread = ThreadControl.Start(new ListenServerBroadcastThread { controller = this });
	}

	public void StopListenBroadcast()
	{
		listenServersThread.StopAndWait();
		listenServersThread = null;
		udpClient.Close();
	}


	public void JoinSelectedServer()
	{
		requestedServer = servers[selectedServerIndex];

		tcpClient = new TcpClient();
		tcpClient.Connect(new IPEndPoint(requestedServer.endPoint.Address, Constants.serverTcpListenPort));

		tcpStream 			= tcpClient.GetStream();
		tcpReceiveThread 	= ThreadControl.Start(new TcpReceiveThread { controller = this });
		var arguments 		= new ClientRequestJoinArgs{ playerName = playerName };
		tcpStream.WriteTcpMessage(arguments);
	}

	public void JoinLocalServer()
	{
		requestedServer = new ServerConnectionInfo { endPoint = new IPEndPoint(IPAddress.Loopback, Constants.serverReceivePort)};

		tcpClient = new TcpClient();
		tcpClient.Connect(IPAddress.Loopback, Constants.serverTcpListenPort);

		tcpStream 			= tcpClient.GetStream();
		tcpReceiveThread 	= ThreadControl.Start(new TcpReceiveThread { controller = this });
		var arguments 		= new ClientRequestJoinArgs {playerName = playerName};
		tcpStream.WriteTcpMessage(arguments);
	}

}