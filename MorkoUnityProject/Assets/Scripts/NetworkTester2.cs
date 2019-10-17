using System; 
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;


using UnityEngine;

using Morko.Network;
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
public struct CharacterUpdatePackage
{
	public int id;
	public float orientation;
	public Vector3 position;
	public Vector3 speed;
}

public class NetworkTester2 : MonoBehaviour
{
	public CharacterUpdatePackage characterUpdate;

	public int netUpdateIntervalMs;
	private float netUpdateInterval => netUpdateIntervalMs / 1000f;
	private float nextNetUpdateTime;

	public Vector3 [] testPackageData;

	public string playerName;

	public int selectedServerIndex;
	public string selectedServerName;

	public int connectionRetryTimeMs = 500;
	private float connectionRetryTime => connectionRetryTimeMs / 1000f;
	
	public List<ServerInfo> servers = new List<ServerInfo>();
	private ServerInfo joinedServer;

	private UdpClient udpClient;
	private bool listenBroadcast = false;

	public enum UdpListenMode { None, Broadcast, Multicast }
	public UdpListenMode listenMode;

	Vector3 testNetPlayerPosition;

	public bool requestedJoinFromServer;
	public bool serverConfirmedJoin;
	public int myClientId;

	int serverConnectionIndex;

	private void ReceiveCallback(IAsyncResult result)
	{
		var state = (UdpState)result.AsyncState;
		var udpClient = state.client;
		var endPoint = state.endPoint;

		var data = udpClient.EndReceive(result, ref endPoint);
		Debug.Log($"CALLBACK Received{endPoint}: {Encoding.ASCII.GetString(data)}");

		if (ProtocolFormat.TryParseCommand(data, out NetworkCommand command, out string arguments))
		{
			switch (command)
			{
				case NetworkCommand.ServerIntroduce:
					endPoint.Port = Constants.serverReceivePort;
					var existingServer = servers.Find(server => IPEndPoint.Equals(server.endPoint, endPoint));
					if (existingServer != null)
					{
						existingServer.lastConnectionTime = DateTime.Now;
					}
					else
					{
						var serverName = arguments;
						servers.Add(new ServerInfo
						{
							endPoint 			= endPoint,
							name 				= serverName,
							lastConnectionTime 	= DateTime.Now
						});
					}
					break;
			}
		}

		// if (listenBroadcast)
		udpClient.BeginReceive(ReceiveCallback, state);
	}

	private void Start()
	{
		// udpClient = new UdpClient(11000);
		// var state = new UdpState
		// {
		// 	client = udpClient,
		// 	endPoint = new IPEndPoint(IPAddress.Any, 0)
		// };
		// udpClient.BeginReceive(ReceiveCallback, state);
	}

	public void StartUpdate()
	{
		doNetworkUpdate = true;
	}

	public void StopUpdate()
	{
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

		if (doNetworkUpdate && (Time.time > nextNetUpdateTime))
		{
			Debug.Log($"Sending update to {joinedServer.endPoint}");
			var data = EncodePackage(myClientId, testPackageData);
			udpClient.Send(data, data.Length, joinedServer.endPoint);

			nextNetUpdateTime = Time.time + netUpdateInterval;
		}
	}

	private const int Vector3Size = sizeof(float) * 3;	
	public static byte [] EncodePackage(int clientId, Vector3 [] package)
	{
		int packageByteCount = Vector3Size * package.Length;
		int dataByteCount = sizeof(int) + packageByteCount;
		

		byte [] countBytes = BitConverter.GetBytes(package.Length);
		byte [] data = new byte[dataByteCount];
		
		Buffer.BlockCopy(countBytes, 0, data, 0, sizeof(int));

		if (packageByteCount == 0)
			return data;

		unsafe
		{
			// Buffer.BlockCopy(package, 0, data, sizeof(int), packageByteCount);

			fixed (Vector3 * source = &package[0])
			fixed (byte * destination = &data[0])
			{
				Buffer.MemoryCopy(source, destination + sizeof(int), packageByteCount, packageByteCount);
			}		
		}

		return data;
	}


	public static Vector3 [] DecodePackage(byte [] data)
	{
		int count = BitConverter.ToInt32(data, 0);
		int packageByteCount = count * Vector3Size;

		Vector3 [] package = new Vector3[count];
		Buffer.BlockCopy(data, sizeof(int), package, 0, packageByteCount);

		return package;
	}


	public void StartListen()
	{
		if (listenMode == UdpListenMode.None)
			return;

		switch (listenMode)
		{
			case UdpListenMode.Broadcast:
			{
				Debug.Log("Start listening broadcast");
				udpClient = new UdpClient(broadcastPort);
				var state = new UdpState
				{
					client = udpClient,
					endPoint = new IPEndPoint(IPAddress.Any, 0)
				};
				udpClient.BeginReceive(ReceiveCallback, state);	

			} break;

			case UdpListenMode.Multicast:
			{
				Debug.Log("Start listening multicast");
				var localEndPoint = new IPEndPoint(IPAddress.Any, multicastPort);
				udpClient = new UdpClient(localEndPoint);
				udpClient.JoinMulticastGroup(multicastAddress);

				var state = new UdpState
				{
					client = udpClient,
					endPoint = new IPEndPoint(IPAddress.Any, 0)
				};
				udpClient.BeginReceive(ReceiveCallback, state);	
			} break;
		}

	}

	public void StopListen()
	{
		if (listenMode == UdpListenMode.None)
			return;

		udpClient?.Close();
		listenMode = UdpListenMode.None;
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
		// Todo(Leo): this needs to be confirmed from server side
		var sendData = Morko.Network.ProtocolFormat.MakeCommand(NetworkCommand.PlayerJoin, playerName);
		var endPoint = servers[selectedServerIndex].endPoint;
		udpClient.Send(sendData, sendData.Length, endPoint);

		joinedServer = servers[selectedServerIndex];

		requestedJoinFromServer = true;
		waitRequestFromServerThread = new Thread(() => {
			var receiveEndPoint = new IPEndPoint(IPAddress.Any, 0);
			waitRequestThreadRunning = true;
			var data = udpClient.Receive(ref receiveEndPoint);
			if (ProtocolFormat.TryParseCommand(data, out NetworkCommand command, out byte [] content))
			{
				switch (command)
				{
					case NetworkCommand.ServerConfirmJoin:
						int index = BitConverter.ToInt32(content, 0);
						Debug.Log("Server confirmed join");
						serverConfirmedJoin = true;
						myClientId = index;
						break;

					case NetworkCommand.ServerDeclineJoin:
						Debug.Log("Server declined join");
						break;

					default:
						Debug.Log($"Uninteresting message: {command}");
						break;
				}
			}

			waitRequestThreadRunning = false;
		});

		waitRequestFromServerThread.Start();
	}
}