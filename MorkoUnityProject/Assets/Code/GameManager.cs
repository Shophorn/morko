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
							IClientNetControllable
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

	public void Awake()
	{
		uiController.OnStartHosting += StartServer;
		uiController.OnStopHosting += StopServer;

		uiController.OnEnterJoinView += StartListenBroadcast;
		uiController.OnExitJoinView += StopListenBroadcast;

		uiController.OnQuit += ApplicationQuit;
	}

	private void StartServer(ServerInfo info)
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
			serverName = info.serverName,
			clientUpdatePackageType = typeof(PlayerGameUpdatePackage),
			clientUpdatePackageSize = Marshal.SizeOf(default(PlayerGameUpdatePackage)),
			logFunction = Debug.Log
		};
		serverController.CreateServer(createInfo);
		serverController.StartBroadcast();

		 // Todo(Leo): Join itself to server
		clientController.CreateHostingPlayerConnection();
		serverController.AddHostingPlayer("Local player", clientController.CurrentEndPoint);

		uiController.OnHostStartGame += HostStartGame;
		uiController.OnHostAbortGame += serverController.AbortGame;
	}

	private void HostStartGame()
	{
		clientController.StartUpdateAsHostingPlayer();
		serverController.StartGame();
	}

	private void StopServer()
	{
		if (isRunningServer == false)
		{
			Debug.LogError("Trying to stop server while not hosting");
			return;
		}

		uiController.OnHostStartGame += serverController.StartGame;
		uiController.OnHostAbortGame += serverController.AbortGame;

		isRunningServer = false;
		serverController.CloseServer();
		Destroy(serverController);
		serverController = null;

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
		MainThreadWorker.AddJob(() => StartGame(gameStartInfo));
	}

	void IClientNetControllable.OnServerListChanged(ServerInfo [] servers)
	{
		MainThreadWorker.AddJob(() => uiController.SetServerList(servers));
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
			// Todo(Leo): RemoteAvatarContoller
		}

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
	
	private void StartListenBroadcast()
	{
		if (isListeningBroadcasts)
		{
			Debug.LogError("Trying to start joining when already joining.");
			return;
		}

		isListeningBroadcasts = true;
		clientController.StartListenBroadcast();
	}

	private void StopListenBroadcast()
	{
		if (isListeningBroadcasts == false)
		{
			Debug.LogError("Trying to stop joining when not joining.");
			return;
		}

		isListeningBroadcasts = false;
		clientController.StopListenBroadcast();
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