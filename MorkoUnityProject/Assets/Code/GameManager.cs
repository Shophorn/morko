using System;

using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

using Morko;
using Morko.Network;

public class GameManager : 	MonoBehaviour,
							IClientUIControllable,
							IClientNetControllable,
							IServerUIControllable
{
	public UIController uiController;
	public ServerController serverController;
	public ClientController clientController;

	private bool isRunningServer 		= false;
	private bool isListeningBroadcasts 	= false;
	private bool isConnectedToServer 	= false;

	public PlayerSettings normalSettings;
	public PlayerSettings morkoSettings;

	public LocalCameraController cameraControllerPrefab;
	public PostFx gameCameraPrefab;
	public GameObject visibilityEffectPrefab;

	public int localAvatarLayer;
	public int remoteAvatarLayer;

	public void Awake()
	{
		uiController.OnQuit += ApplicationQuit;
	}

	void IClientUIControllable.OnClientReady()
	{
		Debug.Log("Player is ready");
		MainThreadWorker.AddJob(clientController.StartUpdate);
	}

	void IClientUIControllable.OnRequestJoin(JoinInfo joinInfo)
	{
		if (joinInfo == null)
		{
			Debug.LogError("No request join info provided");
			return;
		}

		clientController.selectedServerIndex = joinInfo.selectedServerIndex;
		clientController.playerName = joinInfo.playerName;
		Debug.Log($"Joined server as {joinInfo.playerName}, server index = {joinInfo.selectedServerIndex}");
		clientController.JoinSelectedServer();
	}

	void IClientNetControllable.OnServerStartGame(GameStartInfo gameStartInfo)
	{
		Debug.Log("Game manager starting game");
		MainThreadWorker.AddJob(() => StartGame(gameStartInfo));
	}

	void IClientNetControllable.OnServerListChanged(ServerInfo [] servers)
	{
		MainThreadWorker.AddJob(() => uiController.SetServerList(servers));
	}

	void IServerUIControllable.CreateServer(ServerInfo serverInfo)
	{
		if (isRunningServer)
		{
			Debug.LogError("Trying to start server while already hosting");
			return;
		}

		isRunningServer = true;

		serverController = gameObject.AddComponent<ServerController>();
		var createInfo = new ServerCreateInfo
		{
			serverName = serverInfo.serverName,
			clientUpdatePackageType = typeof(PlayerGameUpdatePackage),
			clientUpdatePackageSize = Marshal.SizeOf(default(PlayerGameUpdatePackage)),
			logFunction = Debug.Log
		};
		serverController.CreateServer(createInfo);
		serverController.StartBroadcast();

		 // Todo(Leo): Join itself to server
		clientController.CreateHostingPlayerConnection();
		int hostingPlayerId = serverController.AddHostingPlayer("Local player", clientController.CurrentEndPoint);
		clientController.ClientId = hostingPlayerId;
	}

	void IServerUIControllable.DestroyServer()
	{
		if (isRunningServer == false)
		{
			Debug.LogError("Trying to stop server while not hosting");
			return;
		}

		isRunningServer = false;
		serverController.CloseServer();
		Destroy(serverController);
		serverController = null;

	}

	void IServerUIControllable.StartGame()
	{
		Debug.Log("[GAMEMANAGER]: Starting game as hosting player");
		clientController.StartUpdateAsHostingPlayer();
		serverController.StartGame();
	}

	void IServerUIControllable.AbortGame()
	{
		serverController.AbortGame();
	}

	void IClientUIControllable.BeginJoin()
	{
		Debug.Log("[UI]: Begin join");
		clientController.StartListenBroadcast();
	}

	void IClientUIControllable.EndJoin()
	{
		Debug.Log("[UI]: End join");	
		clientController.StopListenBroadcast();
	}

	private void StartGame(GameStartInfo startInfo)
	{
		Debug.Log("Client says server starts the game :)");

		uiController.Hide();
		SceneManager.LoadScene("Map01", LoadSceneMode.Additive);
		// TODO(Leo): clientController.SendSceneLoadedMessage();

		int localPlayerId = clientController.ClientId;

		var localPlayerInfo = startInfo.localPlayer;
		var localPlayer 	= AvatarInstantiator.Instantiate(new int [] {localPlayerInfo.avatarId})[0];
		var localAvatar 	= localPlayer.GetComponent<Character>();
		var localController = LocalController.Create(localAvatar, normalSettings, morkoSettings);

		clientController.SetSender(localAvatar.transform);

		var visibilityObject = Instantiate(	visibilityEffectPrefab,
											Vector3.up * 1.0f,
											Quaternion.identity,
											localAvatar.transform.root);

		LocalCameraController cameraController = Instantiate(cameraControllerPrefab);
		cameraController.target = localPlayer.transform;

		PostFx gameCamera = Instantiate(	gameCameraPrefab,
											Vector3.zero,
											Quaternion.identity,
											cameraController.transform);

		localController.TEMPORARYSetCamera(gameCamera.camMain);

		clientController.InitializeReceivers();
		int remotePlayerCount = startInfo.remotePlayers.Length;
		for (int remotePlayerIndex = 0; remotePlayerIndex < remotePlayerCount; remotePlayerIndex++)
		{
			var info = startInfo.remotePlayers[remotePlayerIndex];
			var remotePlayer = AvatarInstantiator.Instantiate(new int [] { info.avatarId })[0];
			var remoteAvatar = remotePlayer.GetComponent<Character>();
			clientController.SetReceiver(info.playerId, remoteAvatar.transform);

			// Todo(Leo): Only get renderer from avatar and set its layer
			remoteAvatar.gameObject.SetLayerRecursively(remoteAvatarLayer);
			
			// Todo(Leo): RemoteAvatarContoller
		}

		clientController.StartNetworkUpdate();

		// Todo(Leo): Most definetly not like this
		StartCoroutine(UpdateLocalCharacter(localController));
	}

	private IEnumerator UpdateLocalCharacter(LocalController localController)
	{
		while(true)
		{
			localController.Update();
			yield return null;
		}
	}

	private void ApplicationQuit()
	{
		#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
		#else
			Application.Quit();
		#endif
	}
}