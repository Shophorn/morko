using System;
using System.Runtime.InteropServices;

using UnityEngine;

using Morko.Network;

public class NetworkController : MonoBehaviour
{
	public ServerConnection server;
	public ClientConnection client;

	public event Action OnServersUpdated;



	private void Awake()
	{
		client.OnServersUpdated += () => 
		{
			Debug.Log("Hello from event!!!!!!!!!!!1");
			OnServersUpdated();
		};
	}


	public ServerInfo [] GetServerList ()
	{
		return new ServerInfo [0];
	}


	// public event Action<bool> ServerConfirmJoinRequest;

	public void CreateServer(HostInfo info)
	{
		server.serverInfo = new ServerStartInfo
		{
			serverName = info.serverName,
			clientUpdatePackageSize = Marshal.SizeOf(default(PlayerGameUpdatePackage)),
			logFunction = Debug.Log,
		};
		server.CreateServer();

		Debug.Log($"Create server {info.serverName}");
	}

	public void RequestJoin(JoinInfo info)
	{
		client.JoinSelectedServer();
	}

	public void StartListenBroadcast()
	{
		client.StartListen();
		Debug.Log("Start listen broadcast");
	}

	public void StopListenBroadcast()
	{
		client.StopListen();
		Debug.Log("Stop listen broadcast");
	}
}