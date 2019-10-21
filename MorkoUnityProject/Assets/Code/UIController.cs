using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Morko;
using Morko.Network;

public class JoinInfo
{
	public string playerName;
	public int selectedServerIndex;
}

public partial class UIController : MonoBehaviour
{
	UIController_public uIController_public;

	[SerializeField] private GameObject mainMenuWindow;
	[SerializeField] private GameObject joinServerWindow;
	[SerializeField] private GameObject serverCreationWindow;
	[SerializeField] private GameObject hostLobbyWindow;
	[SerializeField] private GameObject playerLobbyWindow;
	[SerializeField] private GameObject optionsWindow;
	[SerializeField] private GameObject creditsWindow;
	[SerializeField] private GameObject loadingWindow;

	[SerializeField] private Text serverName;

	[SerializeField] private ServerToggleListItem toggle;
	[SerializeField] private ToggleGroup toggleGroup;
	[SerializeField] private GameObject toggleContainer;
	[SerializeField] private RoomInfoPanel roomInfo;
	[SerializeField] private Text hostName;
	[SerializeField] private Text levelName;
	[SerializeField] private Text playerAmount;
	[SerializeField] private Text roundLength;

	[SerializeField] private Button mainMenuHostWindowButton;
	[SerializeField] private Button hostWindowCancelButton;

	[SerializeField] private Button mainMenuJoinWindowButton;
	[SerializeField] private Button joinViewCancelButton;

	[SerializeField] private Button hostWindowCreateRoomButton;
	[SerializeField] private Button hostLobbyWindowCancelButton;

	[SerializeField] private Button joinWindowRequestJoinButton;
	[SerializeField] private Button joinWindowCancelButton;
	[SerializeField] private Button playerLobbyWindowCancelButton;

	[SerializeField] private Button mainMenuOptionsButton;
	[SerializeField] private Button mainMenuCreditsButton;
	[SerializeField] private Button optionsWindowCancelButton;
	[SerializeField] private Button creditsWindowCancelButton;

	[SerializeField] private InfiniteScroll mapSelectionList;
	[SerializeField] private InfiniteScroll characterSelectionListHost;
	[SerializeField] private InfiniteScroll characterSelectionListPlayer;
	[SerializeField] private ScrollContent mapScrollContent;
	[SerializeField] private ScrollContent characterScrollContentHost;
	[SerializeField] private ScrollContent characterScrollContentPlayer;

	[SerializeField] private Text playerName;
	[SerializeField] private int selectedServerId;

	private void BackToMainMenu()
	{
		serverCreationWindow.SetActive(false);
		joinServerWindow.SetActive(false);
		hostLobbyWindow.SetActive(false);
		playerLobbyWindow.SetActive(false);
		optionsWindow.SetActive(false);
		creditsWindow.SetActive(false);
		mainMenuWindow.SetActive(true);
	}
	private void MoveToServerCreationWindow()
	{
		joinServerWindow.SetActive(false);
		hostLobbyWindow.SetActive(false);
		playerLobbyWindow.SetActive(false);
		optionsWindow.SetActive(false);
		creditsWindow.SetActive(false);
		mainMenuWindow.SetActive(true);
		serverCreationWindow.SetActive(true);
	}
	private void MoveToServerJoiningWindow()
	{
		serverCreationWindow.SetActive(false);
		hostLobbyWindow.SetActive(false);
		playerLobbyWindow.SetActive(false);
		optionsWindow.SetActive(false);
		creditsWindow.SetActive(false);
		mainMenuWindow.SetActive(true);
		joinServerWindow.SetActive(true);
	}
	private void MoveToHostLobbyWindow()
	{
		serverCreationWindow.SetActive(false);
		hostLobbyWindow.SetActive(true);
		playerLobbyWindow.SetActive(false);
		optionsWindow.SetActive(false);
		creditsWindow.SetActive(false);
		mainMenuWindow.SetActive(false);
		joinServerWindow.SetActive(false);
	}
	private void MoveToPlayerLobbyWindow()
	{
		serverCreationWindow.SetActive(false);
		hostLobbyWindow.SetActive(false);
		playerLobbyWindow.SetActive(true);
		optionsWindow.SetActive(false);
		creditsWindow.SetActive(false);
		mainMenuWindow.SetActive(false);
		joinServerWindow.SetActive(false);
	}
	private void MoveToOptionsWindow()
	{
		joinServerWindow.SetActive(false);
		serverCreationWindow.SetActive(false);
		hostLobbyWindow.SetActive(false);
		playerLobbyWindow.SetActive(false);
		creditsWindow.SetActive(false);
		optionsWindow.SetActive(true);
		mainMenuWindow.SetActive(true);
	}
	private void MoveToCreditsWindow()
	{
		joinServerWindow.SetActive(false);
		serverCreationWindow.SetActive(false);
		hostLobbyWindow.SetActive(false);
		playerLobbyWindow.SetActive(false);
		optionsWindow.SetActive(false);
		creditsWindow.SetActive(true);
		mainMenuWindow.SetActive(true);
	}

	public string PlayerName => playerName.text;
	public int SelectedServerID => selectedServerId;
	public string ServerName => serverName.text;

	private void Start()
	{
		joinWindowRequestJoinButton.onClick.AddListener (() => 
		{
			var info = new JoinInfo
			{
				playerName = PlayerName,
				selectedServerIndex = SelectedServerID
			};
			//OnRequestJoin?.Invoke(info);
			//OnExitJoinWindow?.Invoke();
		});

		hostWindowCreateRoomButton.onClick.AddListener(() =>
		{
			var info = new ServerInfo
			{
				name 				= ServerName,
				mapIndex 			= 0,
				maxPlayers 			= 4,
				gameDurationSeconds = 300, 	
			};
			//OnStartHosting(info);
			MoveToHostLobbyWindow();
		});

		hostLobbyWindowCancelButton.onClick.AddListener(() =>
		{
			Debug.Log("Stopped hosting");
			//OnStopHosting();
			BackToMainMenu();
		});

		mainMenuJoinWindowButton.onClick.AddListener(() => 
		{
			MoveToServerJoiningWindow();
			//OnEnterJoinWindow?.Invoke();
		});

		joinViewCancelButton.onClick.AddListener(() =>
		{
			BackToMainMenu();
			//OnExitJoinWindow?.Invoke();
		});

		mainMenuHostWindowButton.onClick.AddListener(() =>
		{
			MoveToServerCreationWindow();
		});

		hostWindowCancelButton.onClick.AddListener(() =>
		{
			BackToMainMenu();
		});

		playerLobbyWindowCancelButton.onClick.AddListener(() =>
		{
			BackToMainMenu();
		});

		mainMenuOptionsButton.onClick.AddListener(() =>
		{
			MoveToOptionsWindow();
		});

		mainMenuCreditsButton.onClick.AddListener(() =>
		{
			MoveToCreditsWindow();
		});

		optionsWindowCancelButton.onClick.AddListener(() =>
		{
			BackToMainMenu();
		});

		creditsWindowCancelButton.onClick.AddListener(() =>
		{
			BackToMainMenu();
		});
	}
}