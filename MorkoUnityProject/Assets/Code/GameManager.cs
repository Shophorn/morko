using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

using Morko;

/* Note(Leo): This was stupid namespace and also there is
hashtable also in System.Collections. */
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : 	MonoBehaviourPunCallbacks,
							IClientUIControllable,
							IServerUIControllable,
							IAppUIControllable
{
	public UIController uiController;

	private bool isRunningServer 		= false;
	private bool isListeningBroadcasts 	= false;
	private bool isConnectedToServer 	= false;

	public PlayerSettings normalSettings;
	public PlayerSettings morkoSettings;

	public LocalCameraController cameraControllerPrefab;
	public PostFx gameCameraPrefab;
	public GameObject visibilityEffectPrefab;

	public int remoteCharacterLayer;

 	public CharacterCollection characterPrefabs;

	public int broadcastDelayMs = 100;
	public int gameUpdateThreadDelayMs = 50;

	public GameObject characterPrefab;
	public string levelName;

	private void Awake()
	{
		DontDestroyOnLoad(this);
		PhotonNetwork.ConnectUsingSettings();
		uiController.SetConnectingScreen();
	}

	public override void OnDisconnected (DisconnectCause cause)
	{
		Debug.Log($"[PHOTON]: Disconnect {cause}");
	}

	public override void OnConnectedToMaster()
	{
		Debug.Log("[PHOTON]: Connect");
		PhotonNetwork.JoinLobby();
		uiController.SetMainView();
	}

	public override void OnRoomListUpdate(List<RoomInfo> rooms)
	{
		Debug.Log("[PHOTON]: Roomlist updated");
		uiController.SetRooms(rooms);
	}

	void IClientUIControllable.OnClientReady()
	{
	}

	void IClientUIControllable.RequestJoin(JoinInfo joinInfo)
	{
		PhotonNetwork.NickName = joinInfo.playerName;
		PhotonNetwork.JoinRoom(joinInfo.selectedRoomInfo.Name);
	}

	public override void OnJoinedRoom()
	{
		Debug.Log("We are in the room");
	}

	public override void OnJoinRoomFailed(short returnCode, string message)
	{
		Debug.Log($"Failed to join room ({returnCode}, {message})");
	}

	void IServerUIControllable.CreateServer(ServerInfo serverInfo)
	{
		PhotonNetwork.NickName = serverInfo.hostingPlayerName;
		var options = new RoomOptions
		{
			MaxPlayers = (byte)serverInfo.maxPlayers,
			CustomRoomPropertiesForLobby = new string [] {"map", "time"},
			CustomRoomProperties = new Hashtable
			{
				{"map", serverInfo.mapIndex},
				{"time", serverInfo.gameDurationSeconds}
			}
		};
		PhotonNetwork.CreateRoom(serverInfo.serverName, options);
	}

	void IServerUIControllable.DestroyServer()
	{
	}

	void IServerUIControllable.StartGame()
	{
		this.photonView.RPC("StartGame", RpcTarget.All);
	}

	[PunRPC]
	void StartGame()
	{
		Debug.Log("Start loading scene");
		PhotonNetwork.AutomaticallySyncScene = true;
		PhotonNetwork.LoadLevel(levelName);

		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;

		Vector3 startPosition 		= Vector3.zero;
		Quaternion startRotation 	= Quaternion.identity;

		var netCharacter 		= PhotonNetwork.Instantiate(characterPrefab.name,
															startPosition,
															startRotation);

		var cameraController 	= Instantiate(cameraControllerPrefab);
		cameraController.target = netCharacter.transform;
		var camera 				= Instantiate(gameCameraPrefab, cameraController.transform);

		netCharacter.GetComponent<LocalPlayerController>().SetCamera(camera.camMain);

		uiController.Hide();
	}


	void IServerUIControllable.AbortGame()
	{
	}

	void IClientUIControllable.BeginJoin()
	{
	}

	void IClientUIControllable.EndJoin()
	{
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