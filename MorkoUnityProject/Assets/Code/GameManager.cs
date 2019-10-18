using System;
using System.Runtime.InteropServices;
using UnityEngine;

using Morko.Network;

public class GameManager : MonoBehaviour
{
	public UIController uiController;
	public ServerController serverController;
	public ClientController clientController;

	public void Awake()
	{
		uiController.OnRequestJoin += (info) =>
		{
			clientController.selectedServerIndex = info.selectedServerIndex;
			clientController.playerName = info.playerName;
			Debug.Log($"Joined server as {info.playerName}, server index = {info.selectedServerIndex}");
			clientController.JoinSelectedServer();
		};

		uiController.OnStartHosting += StartServer;

		uiController.OnEnterJoinWindow += clientController.StartListen;
		uiController.OnExitJoinWindow += clientController.StopListen;

		// ---------------------------------------------------------

		clientController.OnServerListChanged += 
			() => uiController.SetServerList(clientController.GetServers());
		clientController.OnServerStartGame += StartGame;
	}

	private void StartServer(HostInfo info)
	{
		serverController = gameObject.AddComponent<ServerController>();
		var createInfo = new ServerCreateInfo
		{
			serverName = info.serverName,
			clientUpdatePackageType = typeof(PlayerGameUpdatePackage),
			clientUpdatePackageSize = Marshal.SizeOf(default(PlayerGameUpdatePackage)),
			logFunction = Debug.Log
		};
		serverController.CreateServer(createInfo);
		serverController.StartBroadcast();

		uiController.OnStartGame += serverController.StartGame;
		uiController.OnAbortGame += serverController.AbortGame;
	}

	private void StartGame()
	{
		// Load characters
		// Load map
		Debug.Log("Client says server starts the game :)");
	}
}