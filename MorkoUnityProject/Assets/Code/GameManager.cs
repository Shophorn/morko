using System;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

using Morko;

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


	private void Awake()
	{
		DontDestroyOnLoad(this);

		PhotonNetwork.ConnectUsingSettings();
	}

	public override void OnDisconnected (DisconnectCause cause)
	{
		Debug.Log($"[PHOTON]: Disconnect {cause}");
	}

	public override void OnConnectedToMaster()
	{
		Debug.Log("[PHOTON]: Connect");
		PhotonNetwork.JoinLobby();
	}

	public override void OnRoomListUpdate(List<RoomInfo> rooms)
	{
		Debug.Log("[PHOTON]: Roomlist updated");
	}

	void IClientUIControllable.OnClientReady()
	{
	}

	void IClientUIControllable.RequestJoin(JoinInfo joinInfo)
	{
	}

	void IServerUIControllable.CreateServer(ServerInfo serverInfo)
	{
	}

	void IServerUIControllable.DestroyServer()
	{
	}

	void IServerUIControllable.StartGame()
	{
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