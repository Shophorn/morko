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
							IServerUIControllable,
							IAppUIControllable
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

	public int remoteCharacterLayer;

	private LocalPlayerController localPlayerController = null;
 	private RemotePlayerController [] remotePlayerControllers = null;

 	public CharacterCollection characterPrefabs;

	public int broadcastDelayMs = 100;
	public int gameUpdateThreadDelayMs = 50;

	void IClientUIControllable.OnClientReady()
	{
		Debug.Log("Player is ready");
		// Todo(Leo): Implement message to server, and from there to other players
		// MainThreadWorker.AddJob(clientController.StartUpdate);
	}

	void IClientUIControllable.RequestJoin(JoinInfo joinInfo)
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

	void IClientNetControllable.StartGame(GameStartInfo gameStartInfo)
	{
		Debug.Log("Game manager starting game");
		MainThreadWorker.AddJob(() => StartGame(gameStartInfo));
	}

	void IClientNetControllable.UpdateServersList(ServerInfo [] servers)
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
			serverName 				= serverInfo.serverName,
			clientUpdatePackageType = typeof(PlayerGameUpdatePackage),
			clientUpdatePackageSize = Marshal.SizeOf(default(PlayerGameUpdatePackage)),
			broadcastDelayMs 		= broadcastDelayMs,
			gameUpdateThreadDelayMs = gameUpdateThreadDelayMs,
			logFunction 			= Debug.Log
		};
		serverController.CreateServer(createInfo);
		serverController.StartBroadcast();

		clientController.JoinLocalServer();
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
		Debug.Log("[GAMEMANAGER]: Starting game from server");
		// clientController.StartUpdateAsHostingPlayer();
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

		var localPlayerInfo 	= startInfo.localPlayer;
		var localPlayer 		= characterPrefabs.InstantiateOne(localPlayerInfo.avatarId);
		var localCharacter 		= localPlayer.GetComponent<Character>();
		
		LocalCameraController cameraController = Instantiate(cameraControllerPrefab);
		cameraController.target = localCharacter.transform;
		PostFx gameCamera = Instantiate(	gameCameraPrefab,
											Vector3.zero,
											Quaternion.identity,
											cameraController.transform);

		localPlayerController 	= LocalPlayerController.Create(
											localCharacter,
											gameCamera.camMain,
											normalSettings,
											morkoSettings);

		clientController.SetSender(localPlayerController);
		Debug.Log("[GAME MANAGER]: Set sender to client controller");

		var visibilityObject = Instantiate(	visibilityEffectPrefab,
											Vector3.up * 1.0f,
											Quaternion.identity,
											localCharacter.transform.root);

		clientController.InitializeReceivers();
		int remotePlayerCount = startInfo.remotePlayers.Length;
		remotePlayerControllers = new RemotePlayerController [remotePlayerCount];
		for (int remotePlayerIndex = 0; remotePlayerIndex < remotePlayerCount; remotePlayerIndex++)
		{
			var info = startInfo.remotePlayers[remotePlayerIndex];

			var remotePlayer 	= characterPrefabs.InstantiateOne(info.avatarId);
			var remoteCharacter = remotePlayer.GetComponent<Character>();
			remoteCharacter.gameObject.SetLayerRecursively(remoteCharacterLayer);

			var remoteController = RemotePlayerController.Create(remoteCharacter);
			clientController.SetReceiver(info.playerId, remoteController);

			remotePlayerControllers[remotePlayerIndex] = remoteController;			
		}

		clientController.StartUpdate();

		// Todo(Leo): Most definetly not like this
		StartCoroutine(UpdateControllers());
	}

	private IEnumerator UpdateControllers()
	{
		while(true)
		{
			localPlayerController.Update();
			foreach (var remoteController in remotePlayerControllers)
			{
				remoteController.Update();
			}

			yield return null;
		}
	}

	void IAppUIControllable.Quit()
	{
		ApplicationQuit();
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