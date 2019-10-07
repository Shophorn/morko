// using UnityEngine;

// using System.Threading;
// using System.Collections.Generic;
// using System.Collections.Concurrent;
// using System.Net;
// using System.Net.Sockets;
// using System.Text;

// using Morko.Network;

// public class NetworkTester : MonoBehaviour
// {
// 	[System.Serializable]
// 	public struct ServerInfo
// 	{
// 		public string name;
// 		public IPEndPoint endPoint;
// 	}

// 	public bool IsSearchingServers { get; private set; }
// 	public int broadcastDelayMilliseconds = 500;

// 	[SerializeField]	
// 	private List<ServerInfo> servers = new List<ServerInfo>();

// 	private const int serverBroadcastPort = 11000;

// 	private UdpClient client;
// 	private IPEndPoint broadcastEndPoint;

// 	[SerializeField] private string playerName;

// 	// Note(Leo): This is updated from update only, but it
// 	// is probably accurate enough to begin with
// 	private float timeForThreads;

// 	private bool threadsAlive;
// 	private float timeToKillThreads;

// 	public int selectedServerIndex;
// 	public int assignedPlayerId;
// 	public void JoinSelectedServer(int serverIndex)
// 	{
// 		var data = Commands.MakeCommand(NetworkCommand.JoinRequest, $"{playerName}");
// 		client.Send(data, data.Length, servers[serverIndex].endPoint);

// 		Debug.Log($"Joining {servers[serverIndex].name}");
// 	}

// 	public void ConfirmJoin()
// 	{
// 		int serverIndex = selectedServerIndex;
// 		int playerId = assignedPlayerId;

// 		var data = Commands.MakeCommand(NetworkCommand.JoinComplete, $"{playerId}");
// 		client.Send(data, data.Length, servers[serverIndex].endPoint);

// 		Debug.Log("Confirm joining to server");
// 	}

// 	public void StartGame()
// 	{
// 		var data = Commands.MakeCommand(NetworkCommand.StartGame);
// 		client.Send(data, data.Length, servers[selectedServerIndex].endPoint);
// 	}

// 	void ServerBroadcastThread()
// 	{
// 		var data = Commands.MakeCommand(NetworkCommand.GetServers);

// 		while (threadsAlive)
// 		{
// 			client.Send(data, data.Length, broadcastEndPoint);
//   			Thread.Sleep(broadcastDelayMilliseconds);
// 		}
// 	}

// 	void ServerReceiveThread()
// 	{
// 		while (threadsAlive)
// 		{
// 			IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, 0);
// 			var data = client.Receive(ref serverEndPoint);

// 			var command = Commands.GetCommand(data);
// 			if (command == NetworkCommand.IntroduceServer)
// 			{
// 				var newServer = new ServerInfo
// 				{
// 					name = Commands.GetArguments(data),
// 					endPoint = serverEndPoint
// 				};

// 				if (servers.Contains(newServer) == false)
// 				{
// 					Debug.Log($"Found {newServer.name}");
// 					servers.Add(newServer);
// 				}
// 			}
// 		}
// 	}

// 	public void StartSearchingServers()
// 	{
// 		threadsAlive = true;
// 		new Thread(ServerBroadcastThread).Start();
// 		new Thread(ServerReceiveThread).Start();

// 		IsSearchingServers = true;
// 	}

// 	public void StopSearchingServers()
// 	{
// 		IsSearchingServers = false;
// 		threadsAlive = false;
// 	}

// 	private void Start()
// 	{
// 		client = new UdpClient(0);	
// 		broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, serverBroadcastPort);
// 	}	

// 	private void OnDisable()
// 	{
// 		// Note(Leo): we need to terminate threads before closing, otherwise they will keep running indefinitely
// 		threadsAlive = false;
// 	}
// }