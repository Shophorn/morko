using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


/* Note(Leo): This was stupid long namespace and also there is
hashtable also in System.Collections. */
using Hashtable = ExitGames.Client.Photon.Hashtable;

using Player = Photon.Realtime.Player;

[RequireComponent(typeof(AudioController))]
public partial class GameManager : 	MonoBehaviourPunCallbacks,
									INetUIControllable,
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
	public string mapSceneName;

	private float gameEndTime;

	private Dictionary<int, Character> connectedCharacters;
	private int currentMorkoActorNumber;
	private int localCharacterActorNumber;
	private bool localCharacterSpawned;

	[SerializeField] private ParticleSystem morkoChangeParticlesPrefab;

	[SerializeField] private TrackTransform maskTrackerPrefab;
	private TrackTransform maskTracker;


	private enum SceneState { Menu, Map, End }
	private SceneState sceneState;

	private static readonly string menuSceneName = "EmptyScene";
	private static readonly string endSceneName = "EndScene";


	[PunRPC]
	private void LoadEndSceneRPC()
	{
		LoadScene(SceneLoader.Photon, endSceneName, OnEndSceneLoaded);
	}

	private void Awake()
	{
		this.MakeMonoBehaviourSingleton();

		DontDestroyOnLoad(this);
		PhotonNetwork.ConnectUsingSettings();

		uiController.Configure(this, this, GetComponent<AudioController>());
		uiController.SetConnectingScreen();
	}

	private void Update()
	{
		if (sceneState == SceneState.Menu)
			return;

		if (Input.GetButtonDown("Cancel"))
		{
			uiController.ToggleNotPauseMenu();
		}

		if (PhotonNetwork.IsMasterClient 
			&& sceneState == SceneState.Map
			&& gameEndTime < Time.time)
		{
			photonView.RPC(nameof(EndGameRPC), RpcTarget.All);
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

	public override void OnJoinedRoom()
	{
		foreach (var player in PhotonNetwork.PlayerList)
		{
			if (player.IsLocal)
			{
				var properties = new Hashtable ();
				properties.Add(PlayerProperty.Status, (int)PlayerNetworkStatus.Waiting);
				player.SetCustomProperties(properties);
				uiController.AddPlayer(player.ActorNumber, player.NickName, PlayerNetworkStatus.Waiting);
			}
			else
			{
				var status = PlayerNetworkStatus.Waiting;
				if (player.CustomProperties.ContainsKey(PlayerProperty.Status))
				{
					status = (PlayerNetworkStatus)player.CustomProperties[PlayerProperty.Status];
				}
				uiController.AddPlayer(player.ActorNumber, player.NickName, status);
			}
		}
		uiController.SetRoomView();
	}

	public override void OnJoinRoomFailed(short returnCode, string message)
	{
		// Todo(Leo): Inform user that connection failed
		Debug.LogError($"Failed to join room ({returnCode}, {message})");
		uiController.SetMainView();
	}


	public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable properties)
	{
		if (properties.ContainsKey(PlayerProperty.Status))
		{
			uiController.UpdatePlayerNetworkStatus(	targetPlayer.ActorNumber,
													(PlayerNetworkStatus)properties[PlayerProperty.Status]);
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

	void INetUIControllable.RequestJoin(JoinInfo joinInfo)
	{
		PhotonNetwork.NickName = joinInfo.playerName;
		PhotonNetwork.JoinRoom(joinInfo.selectedRoomInfo.Name);
		uiController.SetJoiningScreen();
	}

	void INetUIControllable.OnPlayerReady()
	{
		var properties = new Hashtable();
		properties.Add(PlayerProperty.Status, PlayerNetworkStatus.Ready);
		PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
	}

	void INetUIControllable.CreateRoom(RoomCreateInfo createInfo)
	{
		PhotonNetwork.NickName = createInfo.hostingPlayerName;
		var options = new RoomOptions
		{
			MaxPlayers = (byte)createInfo.maxPlayers,
			CustomRoomPropertiesForLobby = new string [] {RoomProperty.MapId, RoomProperty.GameDuration},
			CustomRoomProperties = new Hashtable
			{
				{RoomProperty.MapId, createInfo.mapIndex},
				{RoomProperty.GameDuration, createInfo.gameDurationSeconds}
			}
		};
		PhotonNetwork.CreateRoom(createInfo.roomName, options);
	}

	void INetUIControllable.StartGame()
	{
		this.photonView.RPC(nameof(StartGameRPC), RpcTarget.All);
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

	[PunRPC]
	void StartGameRPC()
	{
		uiController.SetLoadingScreen();
		connectedCharacters = new Dictionary<int, Character>();
		currentMorkoActorNumber = -1;

		LoadScene(SceneLoader.Photon, mapSceneName, OnMapSceneLoaded);
	}

	[PunRPC]
	void EndGameRPC()
	{
		// Todo(Leo): Any other functionality, like sound effects, animation triggers, etc
		LoadScene(SceneLoader.Photon, endSceneName, OnEndSceneLoaded);
	}

	void INetUIControllable.LeaveRoom()
	{
		PhotonNetwork.LeaveRoom();
		uiController.SetMainView();
	}

	public void ExitCurrentMatch()
	{
		Debug.Log("Exited the current match");
		PhotonNetwork.LeaveRoom();
		uiController.Show();
		uiController.SetMainView();

		LoadScene(SceneLoader.Photon, menuSceneName, OnMenuSceneLoaded);

		// Todo(Leo): If we were master client remove or host migrate room
	}

	///////////////////////////////////////////////////////////////////////////
	///                SCENE LOAD CALLBACKS                                 ///
	///////////////////////////////////////////////////////////////////////////
	private enum SceneLoader { Photon, UnityEngine }

	private void LoadScene(SceneLoader sceneLoader, string sceneName, UnityAction<Scene, LoadSceneMode> callback)
	{
		SceneManager.sceneLoaded += callback;
		switch (sceneLoader)
		{
			case SceneLoader.Photon: 
				PhotonNetwork.AutomaticallySyncScene = true;
				PhotonNetwork.LoadLevel(sceneName);
				break;

			case SceneLoader.UnityEngine:
				SceneManager.LoadScene(sceneName);
				break;

			default:
				Debug.LogError("Invalid scene loader");
				SceneManager.sceneLoaded -= callback;
				break;
		}
	}

	private void OnMenuSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		SceneManager.sceneLoaded -= OnMenuSceneLoaded;
		sceneState = SceneState.Menu;
	}

	private void OnMapSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		SceneManager.sceneLoaded -= OnMapSceneLoaded;
		sceneState = SceneState.Map;

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
        gameEndTime = Time.time + (int)PhotonNetwork.CurrentRoom.CustomProperties[RoomProperty.GameDuration];

		uiController.Hide();
	}

	private void OnEndSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		SceneManager.sceneLoaded -= OnEndSceneLoaded;
		sceneState = SceneState.End;
	}

	///---------------------------------------------------------------------///

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
		// Todo(Leo): Make different settings for when sceneState is End
		if (instance.sceneState != SceneState.Map)
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

/*	
Note(Leo): Photon uses strings to identify custom properties and
also encourages the use of short words. Using these enum-like class
we get short strings and also compile errors if these do not match
unlike using actual string literals.

When adding new ones, just make sure they are different from previous
ones, and if we run out of sensible 1 character strings, just use two
or more characters.
*/
public static class RoomProperty
{
	public const string GameDuration = "d";
	public const string MapId = "m";
}

public static class PlayerProperty
{
	public const string Status = "s";
}
