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

	public GameObject[] characterPrefabs;
	public static GameObject [] GetCharacterPrefabs => instance.characterPrefabs;

	public string mapSceneName;

	private float gameEndTime;

	private Dictionary<int, Character> connectedCharacters;
	private int currentMorkoActorNumber;
	private int localCharacterActorNumber;



	[SerializeField] private ParticleSystem morkoChangeParticlesPrefab;

	[SerializeField] private TrackTransform maskTrackerPrefab;
	private TrackTransform maskTracker;


	private enum SceneState { Menu, Map, End }
	private SceneState sceneState;

	private static readonly string menuSceneName = "EmptyScene";
	private static readonly string endSceneName = "EndScene";


	public static GameObject[] GetCharecterModelsForSelection()
	{
		if (instance = null)
			return new GameObject[0];

		int count = instance.characterPrefabs.Length;
		var results = new GameObject [count];
		for (int i = 0; i < count; i++)
		{
			results[i] = Instantiate(instance.characterPrefabs[i], Vector3.zero, Quaternion.identity);
			Destroy(results[i].GetComponent<PlayerController>());
			Destroy(results[i].GetComponent<Character>());
		}

		return results;
	}

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
		void DoInGameMenu() {
			if (Input.GetButtonDown("Cancel"))
				uiController.ToggleNotPauseMenu();
		}

		switch (sceneState)
		{
			case SceneState.Menu:
				break;

			case SceneState.Map:
			{

				if (PhotonNetwork.IsMasterClient && Input.GetKey(KeyCode.O))
				{
					var properties = PhotonNetwork.LocalPlayer.CustomProperties;
					var currentMorkoLevel = (float)properties[PlayerProperty.MorkoLevel];
					currentMorkoLevel += 0.1f * Time.deltaTime;
					properties[PlayerProperty.MorkoLevel] = currentMorkoLevel;
					PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
				}


				DoInGameMenu();
				if (PhotonNetwork.IsMasterClient && gameEndTime < Time.time)
				{
					photonView.RPC(nameof(EndGameRPC), RpcTarget.All);
				}
			} break;


			case SceneState.End:
				DoInGameMenu();
				break;
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
				// var properties = new Hashtable ();
				var properties = new Hashtable();
				properties.Add(PlayerProperty.Status, (int)PlayerNetworkStatus.Waiting);
				properties.Add(PlayerProperty.MorkoLevel, 0.0f);
				properties.Add(PlayerProperty.AvatarId, UnityEngine.Random.Range(0, characterPrefabs.Length));

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
		string allKeys = "";
		foreach (var pair in properties)
		{
			switch (pair.Key)
			{
				case PlayerProperty.Status:
					uiController.UpdatePlayerNetworkStatus(	targetPlayer.ActorNumber, (PlayerNetworkStatus)pair.Value);
					break;

				case PlayerProperty.AvatarId:
					break;

				case PlayerProperty.MorkoLevel:
					connectedCharacters[targetPlayer.ActorNumber].SetMorkoLevel((float)pair.Value);
					break;
			}
			allKeys += pair.Key + ", ";
		}
		Debug.Log("UPDATED PLAYER PROPERTIES: " + allKeys);
	}

	public static GameEndResult GetEndResult()
	{
		var players = PhotonNetwork.CurrentRoom.Players;

		var avatarIds = new int [players.Count];

		int runningIndex = 0;
		int winningIndex = -1;
		float winningPlayerMorkoLevel = float.MaxValue;
		int winningPlayerActorNumber = int.MaxValue;

		foreach (var entry in players)
		{	
			var player = entry.Value;
			var morkoLevel = (float)player.CustomProperties[PlayerProperty.MorkoLevel];
			if (morkoLevel < winningPlayerMorkoLevel)
			{
				winningIndex = runningIndex;
				winningPlayerMorkoLevel = morkoLevel;
				winningPlayerActorNumber = entry.Key;

				if (entry.Value.ActorNumber != winningPlayerActorNumber)
				{
					Debug.Log($"I've got it all wrong, {winningPlayerActorNumber}, {entry.Value.ActorNumber}");
				}
			}

			avatarIds [runningIndex] = (int)player.CustomProperties[PlayerProperty.AvatarId];
			runningIndex++;
		}

		Debug.Log($"[GAME MANAGER]: Winner index is {winningIndex}, actor number is {winningPlayerActorNumber}");


		var endResult = new GameEndResult
		{
			characterCount = instance.connectedCharacters.Count,
			winningCharacterIndex = winningIndex,
			playerAvatarIds = avatarIds
		};
		return endResult;
	}

	void INetUIControllable.RequestJoin(JoinInfo joinInfo)
	{
		PhotonNetwork.NickName = joinInfo.playerName;
		PhotonNetwork.JoinRoom(joinInfo.selectedRoomInfo.Name);
		uiController.SetJoiningScreen();

		PhotonNetwork.LocalPlayer.CustomProperties.Add(PlayerProperty.MorkoLevel, 10);
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

		int characterIndex 		= (int)PhotonNetwork.LocalPlayer.CustomProperties[PlayerProperty.AvatarId];
		string prefabName 		= characterPrefabs[characterIndex].name;
		var localPlayer 		= PhotonNetwork.Instantiate(prefabName,
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

		// Todo(Leo): Unset current character

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
ones (so that they are unique when both are combined), and if we run
out of sensible 1 character strings, just use two or more characters.
*/
public static class RoomProperty
{
	public const string GameDuration = "d";
	public const string MapId = "m";
}

public static class PlayerProperty
{
	public const string Status = "s";
	public const string AvatarId = "a";
	public const string MorkoLevel = "l";
}
