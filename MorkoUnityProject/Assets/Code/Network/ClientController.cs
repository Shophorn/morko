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
	public JoinInfo[] joinedPlayers;
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

	private UdpClient udpClient;
	public IPEndPoint CurrentEndPoint => udpClient.Client.LocalEndPoint as IPEndPoint;

	// Todo(Leo): Please just make these normal variables, so we can like check against null... 
	private readonly ThreadControl detectServersThread = new ThreadControl();
	private readonly ThreadControl receiveUpdateThread = new ThreadControl();
	private readonly ThreadControl sendUpdateThread = new ThreadControl();
	private SendUpdateThread sendUpdateThreadRunner;

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

	public void StartNetworkUpdate()
	{
		doNetworkUpdate = true;
	}

	private ServerInfo [] GetServers()
	{
		var result = servers.Select(connection => connection.serverInfo).ToArray();
		return result;	
	}

	public void StartUpdate()
	{
		var localEndPoint = new IPEndPoint(IPAddress.Any, multicastPort);
		udpClient = new UdpClient(localEndPoint);
		udpClient.JoinMulticastGroup(multicastAddress);

		Debug.Log($"Joined server = {joinedServer}");

		sendUpdateThreadRunner = new SendUpdateThread
		{
			sendDelayMs = netUpdateIntervalMs,
			udpClient 	= udpClient,
			endPoint 	= joinedServer.endPoint,
			controller 	= this
		};
		sendUpdateThread.Start(sendUpdateThreadRunner);
		
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
		sendUpdateThreadRunner = new SendUpdateThread
		{
			sendDelayMs = netUpdateIntervalMs,
			udpClient 	= udpClient,
			endPoint 	= serverEndPoint,
			controller 	= this
		};
		sendUpdateThread.Start(sendUpdateThreadRunner);
		
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
				netControls.UpdateServersList(GetServers());
			}
		}

		// if (doNetworkUpdate)
		// {
		// 	// Note(Leo): Lol, no accessing transform from threads??
		// 	if (sender != null && sendUpdateThread.IsRunning)
		// 		sendUpdateThreadRunner.playerPosition = sender.position;
		// }
	}

	public void StartListenBroadcast()
	{
		udpClient = new UdpClient(broadcastPort);
		detectServersThread.Start(new ReceiveThread {connection = this});
	}

	public void StopListenBroadcast()
	{
		detectServersThread.StopAndWait();
		udpClient?.Close();
	}

	private void OnDisable()
	{
		detectServersThread.StopAndWait();
		sendUpdateThread.StopAndWait();
		receiveUpdateThread.StopAndWait();

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