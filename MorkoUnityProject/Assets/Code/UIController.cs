using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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
	[Serializable] 
	private struct MainView
	{
		public Button hostViewButton;
		public Button joinViewButton;
		public Button creditsButton;
		public Button optionsButton;
		public Button quitButton;
	}
	[SerializeField] private MainView mainView;

	[Serializable]
	private struct HostView
	{
		public const string defaultServerName = "Default Server";
		public const string defaultPlayerName = "Hosting Player";

		public Button cancelButton;
		public Button createRoomButton;
		public InputField playerNameField;
		public InputField serverNameField;
		public DiscreteInputField playerCountField;
		public DiscreteInputField gameDurationField;
	}
	[SerializeField] private HostView hostView;

	[Serializable]
	private struct HostLobbyView
	{
		public InputField playerNameField;
		public Button startGameButton;
		public Button cancelButton;
	}
	[SerializeField] private HostLobbyView hostLobbyView;

	[Serializable]
	private struct JoinView
	{
		public ToggleGroup 			availableServersToggleGroup;
		public Transform 			availableServersToggleParent;
		public ServerToggleListItem availableServersTogglePrefab;
		public int 					selectedServerIndex;

		public Text hostingPlayerNameText;
		public Text mapNameText;
		public Text joinedPlayersCountText;
		public Text gameDurationText;

		public InputField playerNameField;

		public Button requestJoinButton;
		public Button cancelButton;

	}
	[SerializeField] private JoinView joinView;

	[Header("Uncategorized")]
	[SerializeField] private GameObject mainGameObject;

	[SerializeField] private GameObject mainMenuWindow;
	[SerializeField] private GameObject joinServerWindow;
	[SerializeField] private GameObject serverCreationWindow;
	[SerializeField] private GameObject hostLobbyWindow;
	[SerializeField] private GameObject playerLobbyWindow;
	[SerializeField] private GameObject optionsWindow;
	[SerializeField] private GameObject creditsWindow;
	[SerializeField] private GameObject loadingWindow;


	[SerializeField] private ServerToggleListItem toggle;
	[SerializeField] private ToggleGroup toggleGroup;
	[SerializeField] private GameObject toggleContainer;
	[SerializeField] private RoomInfoPanel roomInfo;
	[SerializeField] private Text levelName;
	[SerializeField] private Text playerAmount;
	[SerializeField] private Text roundLength;

	[SerializeField] private Button playerLobbyWindowCancelButton;

	[SerializeField] private Button optionsWindowCancelButton;
	[SerializeField] private Button creditsWindowCancelButton;

	[SerializeField] private InfiniteScroll mapSelectionList;
	[SerializeField] private InfiniteScroll characterSelectionListHost;
	[SerializeField] private InfiniteScroll characterSelectionListPlayer;
	[SerializeField] private ScrollContent mapScrollContent;
	[SerializeField] private ScrollContent characterScrollContentHost;
	[SerializeField] private ScrollContent characterScrollContentPlayer;

	[SerializeField] private Text playerName;

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

	private void Start()
	{
		mainMenuWindow.SetActive(true);

		/// MAIN VIEW
		mainView.hostViewButton.onClick.AddListener(() => MoveToServerCreationWindow());
		mainView.joinViewButton.onClick.AddListener(
			() => {
				MoveToServerJoiningWindow();
				OnEnterJoinView?.Invoke();
			});
		mainView.optionsButton.onClick.AddListener(() => MoveToOptionsWindow());
		mainView.creditsButton.onClick.AddListener(() => MoveToCreditsWindow());
		mainView.quitButton.onClick.AddListener(() => OnQuit?.Invoke());


		/// HOST VIEW
		hostView.createRoomButton.onClick.AddListener(() =>
		{
			var info = new ServerInfo
			{
				serverName			= hostView.serverNameField.text,
				mapIndex 			= 0,
				maxPlayers 			= hostView.playerCountField.IntValue,
				gameDurationSeconds = hostView.gameDurationField.IntValue, 	
			};
			OnStartHosting?.Invoke(info);
			MoveToHostLobbyWindow();
		});
		hostView.cancelButton.onClick.AddListener(BackToMainMenu);
		hostView.playerNameField.text = HostView.defaultPlayerName;
		hostView.serverNameField.text = HostView.defaultServerName;


		/// HOST LOBBY VIEW
		hostLobbyView.startGameButton.onClick.AddListener(() =>
		{
			OnHostStartGame?.Invoke();
		});

		hostLobbyView.cancelButton.onClick.AddListener(() =>
		{
			OnHostAbortGame?.Invoke();
			BackToMainMenu();
		});

		/// JOIN VIEW
		joinView.requestJoinButton.onClick.AddListener (() => 
		{
			var info = new JoinInfo
			{
				playerName = joinView.playerNameField.text,
				selectedServerIndex = joinView.selectedServerIndex
			};
			OnRequestJoin?.Invoke(info);
		});

		joinView.cancelButton.onClick.AddListener(() =>
		{
			BackToMainMenu();
			OnExitJoinView?.Invoke();
		});




		playerLobbyWindowCancelButton.onClick.AddListener(() =>
		{
			BackToMainMenu();
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