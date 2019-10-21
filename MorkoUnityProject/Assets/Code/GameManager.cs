using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

using Morko;
using Morko.Network;

public class GameManager : MonoBehaviour
{
	public UIController uiController;
	public ServerController serverController;
	public ClientController clientController;

	private bool isRunningServer = false;
	private bool isListeningBroadcasts = false;

	public PlayerSettings normalSettings;
	public PlayerSettings morkoSettings;

	public LocalCameraController cameraControllerPrefab;
	public PostFx gameCameraPrefab;
	public GameObject visibilityEffectPrefab;

	public void Awake()
	{
		uiController.OnRequestJoin += (info) =>
		{
			if (info == null)
			{
				// Todo(Leo): this is debug path
				StartGame(null);
			}
			else
			{
				clientController.selectedServerIndex = info.selectedServerIndex;
				clientController.playerName = info.playerName;
				Debug.Log($"Joined server as {info.playerName}, server index = {info.selectedServerIndex}");
				clientController.JoinSelectedServer();
			}
		};

		uiController.OnStartHosting += StartServer;
		uiController.OnStopHosting += StopServer;

		uiController.OnEnterJoinWindow += StartListenBroadcast;
		uiController.OnExitJoinWindow += StopListenBroadcast;
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

	private void StopServer()
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

	private void StartGame(GameStartInfo startInfo)
	{
		Debug.Log("Client says server starts the game :)");


		uiController.Hide();
		SceneManager.LoadScene("Map01", LoadSceneMode.Additive);
		// TODO(Leo): clientController.SendSceneLoadedMessage();

		var localPlayer = AvatarInstantiator.Instantiate(new int [] {0})[0];
		var avatar = localPlayer.GetComponent<Character>();
		var localController = LocalController.Create(avatar, normalSettings, morkoSettings);

		var visibilityObject = Instantiate(visibilityEffectPrefab);
		visibilityObject.transform.SetParent(avatar.transform.root);
		visibilityObject.transform.localPosition = Vector3.up * 0.5f;
		visibilityObject.transform.rotation = Quaternion.identity;

		LocalCameraController cameraController = Instantiate(cameraControllerPrefab);
		cameraController.target = localPlayer.transform;

		PostFx gameCamera = Instantiate(gameCameraPrefab);
		gameCamera.transform.SetParent(cameraController.transform);
		gameCamera.transform.localPosition = Vector3.zero;
		gameCamera.transform.rotation = Quaternion.identity;
		localController.TEMPORARYSetCamera(gameCamera.camMain);

		// Load characters
		// Load map

		// Todo(Leo): Definetly not like this
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
		clientController.OnServerListChanged 	+= uiController.SetServerList;
		clientController.OnServerStartGame 		+= StartGame;
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
		clientController.OnServerListChanged 	-= uiController.SetServerList;
		clientController.OnServerStartGame 		-= StartGame;
	}
}