using System; 
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

using UnityEngine;

using Morko.Network;

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

public class NetworkTester2 : MonoBehaviour
{
	public string playerName;

	public int selectedServerIndex;
	public string selectedServerName;

	public int connectionRetryTimeMs = 500;
	private float connectionRetryTime => connectionRetryTimeMs / 1000f;
	
	public List<ServerInfo> servers = new List<ServerInfo>();

	private UdpClient udpClient;
	private bool listenBroadcast = false;

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
/*
		bool found = false;
		foreach (var server in servers)
		{
			if (IPEndPoint.Equals(server.endPoint, endPoint))
			{
				found = true;
				server.lastConnectionTime = DateTime.Now;
			}
		}

		if (found == false)
		{
			if (ProtocolFormat.TryGetServerName(data, out string serverName))
			{
				servers.Add(new ServerInfo
				{
					endPoint = endPoint,
					name = serverName,
					lastConnectionTime = DateTime.Now
				});
			}
		}
*/
		// if (listenBroadcast)
		udpClient.BeginReceive(ReceiveCallback, state);
	}

	private void Start()
	{
		udpClient = new UdpClient(11000);
		var state = new UdpState
		{
			client = udpClient,
			endPoint = new IPEndPoint(IPAddress.Any, 0)
		};
		udpClient.BeginReceive(ReceiveCallback, state);
	}


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
	}

	private void OnDisable()
	{
		listenBroadcast = false;
		udpClient.Close();
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
		var sendData = Morko.Network.ProtocolFormat.MakeCommand(NetworkCommand.PlayerJoin, playerName);
		var endPoint = servers[selectedServerIndex].endPoint;
		udpClient.Send(sendData, sendData.Length, endPoint);
	}
}