using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


/* Note(Leo): This was stupid long namespace and also there is
hashtable also in System.Collections. */
using Hashtable = ExitGames.Client.Photon.Hashtable;

using Player = Photon.Realtime.Player;
using PhotonActorNumber = System.Int32;

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

	public int broadcastDelayMs = 100;
	public int gameUpdateThreadDelayMs = 50;

	public int remoteCharacterLayer;
	public GameObject characterPrefab;
	public string levelName;

	private Dictionary<PhotonActorNumber, Character> connectedCharacters;
	private PhotonActorNumber currentMorkoActorNumber;
	private PhotonActorNumber localCharacterActorNumber;
	private bool localCharacterSpawned;

	[SerializeField] private ParticleSystem morkoChangeParticlesPrefab;

	[SerializeField] private TrackTransform maskTrackerPrefab;
	private TrackTransform maskTracker;

	private bool isEndSceneCurrent = false;

	[UnityEditor.MenuItem("GameManager/Spawn Mask")]
	private static void SpawnMask()
	{
		if (instance == null)
			return;

		int actorNumber = instance.localCharacterActorNumber;
		instance.photonView.RPC(nameof(SetCharacterMorkoRPC), RpcTarget.All, actorNumber);
	}

	[UnityEditor.MenuItem("GameManager/End Game")]
	private static void EndGame()
	{
		if (instance == null)
			return;

		instance.photonView.RPC(nameof(LoadEndScene), RpcTarget.All);
	}

	[PunRPC]
	private void LoadEndScene()
	{
		isEndSceneCurrent = true;

		PhotonNetwork.AutomaticallySyncScene = true;
		PhotonNetwork.LoadLevel("EndScene");
	}

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


	public override void OnJoinedRoom()
	{
		isEndSceneCurrent = false;

		foreach (var player in PhotonNetwork.PlayerList)
		{
			if (player.IsLocal)
			{
				var properties = new Hashtable ();
				properties.Add(PhotonPropertyKey.PlayerStatus, (int)PlayerNetworkStatus.Waiting);
				player.SetCustomProperties(properties);
				uiController.AddPlayer(player.ActorNumber, player.NickName, PlayerNetworkStatus.Waiting);
			}
			else
			{
				var status = PlayerNetworkStatus.Waiting;
				if (player.CustomProperties.ContainsKey(PhotonPropertyKey.PlayerStatus))
				{
					status = (PlayerNetworkStatus)player.CustomProperties[PhotonPropertyKey.PlayerStatus];
				}
				uiController.AddPlayer(player.ActorNumber, player.NickName, status);
			}
		}

	}

	public override void OnJoinRoomFailed(short returnCode, string message)
	{
		Debug.LogError($"Failed to join room ({returnCode}, {message})");
	}

	void IClientUIControllable.OnPlayerReady()
	{
		var properties = new Hashtable();
		properties.Add(PhotonPropertyKey.PlayerStatus, PlayerNetworkStatus.Ready);
		PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
	}

	public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable properties)
	{
		if (properties.ContainsKey(PhotonPropertyKey.PlayerStatus))
		{
			uiController.UpdatePlayerNetworkStatus(	targetPlayer.ActorNumber,
													(PlayerNetworkStatus)properties[PhotonPropertyKey.PlayerStatus]);
		}
	}

	public static GameEndResult GetEndResult()
	{
		var endResult = new GameEndResult
		{
			characterCount = instance.connectedCharacters.Count,
			winningCharacterIndex = 0
		};
		return endResult;
	}

	void IServerUIControllable.CreateRoom(RoomCreateInfo createInfo)
	{
		PhotonNetwork.NickName = createInfo.hostingPlayerName;
		var options = new RoomOptions
		{
			MaxPlayers = (byte)createInfo.maxPlayers,
			CustomRoomPropertiesForLobby = new string [] {PhotonPropertyKey.RoomMapId, PhotonPropertyKey.RoomGameDuration},
			CustomRoomProperties = new Hashtable
			{
				{PhotonPropertyKey.RoomMapId, createInfo.mapIndex},
				{PhotonPropertyKey.RoomGameDuration, createInfo.gameDurationSeconds}
			}
		};
		PhotonNetwork.CreateRoom(createInfo.roomName, options);
	}

	void IServerUIControllable.StartGame()
	{
		this.photonView.RPC("StartGame", RpcTarget.All);
	}

	[PunRPC]
	void StartGame()
	{
		uiController.SetLoadingScreen();
		connectedCharacters = new Dictionary<PhotonActorNumber, Character>();
		currentMorkoActorNumber = -1;

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

		var cameraController 	= Instantiate(cameraControllerPrefab);
		gameCamera 				= Instantiate(gameCameraPrefab, cameraController.transform);
		gameCamera.CreateMask();

		var localPlayer 		= PhotonNetwork.Instantiate(characterPrefab.name,
															startPosition,
															startRotation);
        localPlayer.name 		= "Local Player";
		cameraController.target = localPlayer.transform;
        Instantiate(visibilityEffectPrefab, localPlayer.transform);

        maskTracker = Instantiate(maskTrackerPrefab);

		uiController.Hide();
	}

	public static bool IsCharacterMorko(Character character)
	{
		bool result = character.photonView.Owner.ActorNumber == instance.currentMorkoActorNumber;
		return result;
	}

	public static void SetCharacterMorko(Character character)
	{
		instance.photonView.RPC(nameof(SetCharacterMorkoRPC),
								RpcTarget.All,
								character.photonView.Owner.ActorNumber);
	}

	[PunRPC]
	private void SetCharacterMorkoRPC(int actorNumber)
	{
		if (currentMorkoActorNumber == actorNumber)
			return;

		currentMorkoActorNumber = actorNumber;
		maskTracker.target = connectedCharacters[actorNumber].transform;

		connectedCharacters[actorNumber].FreezeForSeconds(3);
		Vector3 effectPosition = maskTracker.target.transform.position + maskTracker.offset;
		Instantiate(morkoChangeParticlesPrefab, effectPosition, Quaternion.identity);
	}


	public static void RegisterCharactcer(Character character)
	{
		// Todo(Leo): This is a hack
		if (instance.isEndSceneCurrent)
			return;

		if (character.photonView.IsMine)
			instance.localCharacterActorNumber = character.photonView.Owner.ActorNumber;

		var characterPartRenderers = character.GetComponentsInChildren<Renderer>();
		foreach(var renderer in characterPartRenderers)
		{
			renderer.material.SetTexture("_VisibilityMask", instance.gameCamera.MaskTexture);
		}

		int actorNumber = character.photonView.Owner.ActorNumber;
		instance.connectedCharacters.Add(actorNumber, character);
	}

	public static Camera GetPlayerViewCamera()
		=> instance.gameCamera.baseCamera;


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

public static class PhotonPropertyKey
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