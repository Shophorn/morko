using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

//using Morko;

/* Note(Leo): This was stupid namespace and also there is
hashtable also in System.Collections. */
using Hashtable = ExitGames.Client.Photon.Hashtable;

using Player = Photon.Realtime.Player;

public class GameManager : 	MonoBehaviourPunCallbacks,
							IClientUIControllable,
							IServerUIControllable,
							IAppUIControllable
{
	private static GameManager instance;

	public UIController uiController;

	private bool isRunningServer 		= false;
	private bool isListeningBroadcasts 	= false;
	private bool isConnectedToServer 	= false;

	public PlayerSettings normalSettings;
	public PlayerSettings morkoSettings;

	public LocalCameraController cameraControllerPrefab;
	public MultiplayerVision gameCameraPrefab;
	private MultiplayerVision gameCamera = null;

	public GameObject visibilityEffectPrefab;


 	public CharacterCollection characterPrefabs;

	public int broadcastDelayMs = 100;
	public int gameUpdateThreadDelayMs = 50;

	public int remoteCharacterLayer;
	public GameObject characterPrefab;
	public string levelName;

	private void Awake()
	{
		this.MakeMonoBehaviourSingleton();

		DontDestroyOnLoad(this);
		PhotonNetwork.ConnectUsingSettings();
		uiController.SetConnectingScreen();
	}

	private void Update()
	{
		if (Input.GetButtonDown("Cancel"))
		{
			uiController.ToggleNotPauseMenu();
		}
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

	void IClientUIControllable.RequestJoin(JoinInfo joinInfo)
	{
		PhotonNetwork.NickName = joinInfo.playerName;
		PhotonNetwork.JoinRoom(joinInfo.selectedRoomInfo.Name);
	}

	public class PropertyKey
	{
		/*	
		Note(Leo): Photon uses strings to identify custom properties and
		also encourages the use of short words. Using this enum-like class
		we get short strings and also compile errors if these do not match
		unlike using actual string literals.
		
		When adding new ones, just make sure they are different from previous
		ones, and if we run out of sensible 1 character strings, just use two
		or more characters.
		*/
		public const string PlayerStatus = "s";
		public const string RoomGameDuration = "d";
		public const string RoomMapId = "m";
	}

	public override void OnJoinedRoom()
	{
		foreach (var player in PhotonNetwork.PlayerList)
		{
			if (player.IsLocal)
			{
				var properties = new Hashtable ();
				properties.Add(PropertyKey.PlayerStatus, (int)PlayerNetworkStatus.Waiting);
				player.SetCustomProperties(properties);
				uiController.AddPlayer(player.ActorNumber, player.NickName, PlayerNetworkStatus.Waiting);
			}
			else
			{
				var status = PlayerNetworkStatus.Waiting;
				if (player.CustomProperties.ContainsKey(PropertyKey.PlayerStatus))
				{
					status = (PlayerNetworkStatus)player.CustomProperties[PropertyKey.PlayerStatus];
					Debug.Log($"Status read succesfully ({status})");
				}
				uiController.AddPlayer(player.ActorNumber, player.NickName, status);
			}
		}

		Debug.Log("We are in the room");
	}

	public override void OnJoinRoomFailed(short returnCode, string message)
	{
		Debug.LogError($"Failed to join room ({returnCode}, {message})");
	}

	void IClientUIControllable.OnPlayerReady()
	{
		var properties = new Hashtable();
		properties.Add(PropertyKey.PlayerStatus, PlayerNetworkStatus.Ready);
		PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
	}

	public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable properties)
	{
		if (properties.ContainsKey(PropertyKey.PlayerStatus))
		{
			uiController.UpdatePlayerNetworkStatus(	targetPlayer.ActorNumber,
													(PlayerNetworkStatus)properties[PropertyKey.PlayerStatus]);
		}
	}

	void IServerUIControllable.CreateServer(ServerInfo serverInfo)
	{
		PhotonNetwork.NickName = serverInfo.hostingPlayerName;
		var options = new RoomOptions
		{
			MaxPlayers = (byte)serverInfo.maxPlayers,
			CustomRoomPropertiesForLobby = new string [] {PropertyKey.RoomMapId, PropertyKey.RoomGameDuration},
			CustomRoomProperties = new Hashtable
			{
				{PropertyKey.RoomMapId, serverInfo.mapIndex},
				{PropertyKey.RoomGameDuration, serverInfo.gameDurationSeconds}
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
		uiController.SetLoadingScreen();

		Debug.Log("Start loading scene");
		PhotonNetwork.AutomaticallySyncScene = true;
		PhotonNetwork.LoadLevel(levelName);
		SceneManager.sceneLoaded += OnMapSceneLoaded;
	}

	public void ExitCurrentMatch()
	{
		Debug.Log("Exited the current match");
		PhotonNetwork.LeaveRoom();
		uiController.Show();
		uiController.SetMainView();

		PhotonNetwork.LoadLevel("EmptyScene");

		// Todo(Leo): If we were master client remove or host migrate room
	}

	public override void OnPlayerEnteredRoom(Player enteringPlayer)
	{
		Debug.Log($"{enteringPlayer.NickName} entered room");

		uiController.AddPlayer(	enteringPlayer.ActorNumber,
								enteringPlayer.NickName,
								PlayerNetworkStatus.Waiting);
	}

	public override void OnPlayerLeftRoom(Player leavingPlayer)
	{
		Debug.Log($"{leavingPlayer.NickName} left room");
		uiController.RemovePlayer(leavingPlayer.ActorNumber);
	}

	private void OnMapSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		SceneManager.sceneLoaded -= OnMapSceneLoaded;

		Vector3 startPosition 		= Vector3.zero;
		Quaternion startRotation 	= Quaternion.identity;

		var netCharacter 		= PhotonNetwork.Instantiate(characterPrefab.name,
															startPosition,
															startRotation);
        netCharacter.name 		= "Local Player";
		var cameraController 	= Instantiate(cameraControllerPrefab);
		cameraController.target = netCharacter.transform;
		gameCamera 				= Instantiate(gameCameraPrefab, cameraController.transform);
		gameCamera.CreateMask();
        Instantiate(visibilityEffectPrefab, netCharacter.transform);

		// netCharacter.GetComponent<LocalPlayerController>().SetCamera(gameCamera.baseCamera);

		// var characterPartRenderers = netCharacter.GetComponentsInChildren<Renderer>();
		// foreach(var renderer in characterPartRenderers)
		// {
		// 	renderer.material.SetTexture("_VisibilityMask", gameCamera.MaskTexture);
		// }

		uiController.Hide();
	}

	public static void SetupVisibilityEffect(GameObject character)
	{
		character.GetComponent<LocalPlayerController>().SetCamera(instance.gameCamera.baseCamera);

		var characterPartRenderers = character.GetComponentsInChildren<Renderer>();
		foreach(var renderer in characterPartRenderers)
		{
			renderer.material.SetTexture("_VisibilityMask", instance.gameCamera.MaskTexture);
		}
	}

	void IServerUIControllable.AbortGame()
	{
	}

	void IAppUIControllable.ExitMatch()
	{
		ExitCurrentMatch();
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