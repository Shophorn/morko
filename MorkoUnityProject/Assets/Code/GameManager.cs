using System;
using System.Runtime.InteropServices;
using UnityEngine;

using Morko.Network;

public class GameManager : MonoBehaviour
{
	public UIController uiController;
	public ServerController serverController;
	public ClientController clientController;

	private bool isJoining = false;

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

		uiController.OnEnterJoinWindow += StartJoin;
		uiController.OnExitJoinWindow += StopJoin;

		// ---------------------------------------------------------


	}

	private void StartServer(ServerInfo info)
	{
		serverController = gameObject.AddComponent<ServerController>();
		var createInfo = new ServerCreateInfo
		{
			serverName = info.name,
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

	private void StartJoin()
	{
		if (isJoining)
		{
			Debug.LogError("Trying to start joining when already joining.");
			return;
		}

		isJoining = true;

		clientController.StartListen();
		clientController.OnServerListChanged 	+= uiController.SetServerList;
		clientController.OnServerStartGame 		+= StartGame;
	}

	private void StopJoin()
	{
		if (isJoining == false)
		{
			Debug.LogError("Trying to stop joining when not joining.");
			return;
		}

		isJoining = false;

		clientController.StopListen();
		clientController.OnServerListChanged 	-= uiController.SetServerList;
		clientController.OnServerStartGame 		-= StartGame;
	}
}