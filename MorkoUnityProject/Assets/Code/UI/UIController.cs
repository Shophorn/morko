using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Morko;
using Morko.Network;


public partial class UIController : MonoBehaviour
{
	private IClientUIControllable clientControls;
	private IServerUIControllable serverControls;

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
		public Button startGameButton;
		public Button cancelButton;
	}
	[SerializeField] private HostLobbyView hostLobbyView;

	[Serializable]
	private struct JoinView
	{
		public const string defaultPlayerName = "Client Player";

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

	[Serializable]
	private struct ClientLobbyView
	{
		public Button readyButton;
		public Button cancelButton;
	}
	[SerializeField] private ClientLobbyView clientLobbyView;

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

	[SerializeField] private Text playerName;

	[SerializeField] private GameObject listItemContainer;

	private void DisableAll()
	{
		listItemContainer.SetActive(false);
		serverCreationWindow.SetActive(false);
		joinServerWindow.SetActive(false);
		hostLobbyWindow.SetActive(false);
		playerLobbyWindow.SetActive(false);
		optionsWindow.SetActive(false);
		creditsWindow.SetActive(false);
		mainMenuWindow.SetActive(false);
	}
	private void BackToMainMenu()
	{
		DisableAll();
		mainMenuWindow.SetActive(true);
	}
	private void MoveToServerCreationWindow()
	{
		DisableAll();
		mainMenuWindow.SetActive(true);
		serverCreationWindow.SetActive(true);
		listItemContainer.SetActive(true);
	}
	private void MoveToServerJoiningWindow()
	{
		DisableAll();
		mainMenuWindow.SetActive(true);
		joinServerWindow.SetActive(true);
	}
	private void MoveToHostLobbyWindow()
	{
		DisableAll();
		hostLobbyWindow.SetActive(true);
		listItemContainer.SetActive(true);
	}
	private void MoveToClientLobbyWindow()
	{
		DisableAll();
		playerLobbyWindow.SetActive(true);
		listItemContainer.SetActive(true);
	}
	private void MoveToOptionsWindow()
	{
		DisableAll();
		optionsWindow.SetActive(true);
		mainMenuWindow.SetActive(true);
	}
	private void MoveToCreditsWindow()
	{
		DisableAll();
		creditsWindow.SetActive(true);
		mainMenuWindow.SetActive(true);
	}

	private void Start()
	{
		clientControls = GetComponent<IClientUIControllable>();
		serverControls = GetComponent<IServerUIControllable>();

		mainMenuWindow.SetActive(true);

		/// MAIN VIEW
		mainView.hostViewButton.onClick.AddListener(() => MoveToServerCreationWindow());
		mainView.joinViewButton.onClick.AddListener(
			() => {
				MoveToServerJoiningWindow();
				clientControls.BeginJoin();
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
			serverControls.CreateServer(info);
			MoveToHostLobbyWindow();
		});
		hostView.cancelButton.onClick.AddListener(BackToMainMenu);
		hostView.playerNameField.text = HostView.defaultPlayerName;
		hostView.serverNameField.text = HostView.defaultServerName;


		/// HOST LOBBY VIEW
		hostLobbyView.startGameButton.onClick.AddListener(() =>
		{
			serverControls.StartGame();
		});

		hostLobbyView.cancelButton.onClick.AddListener(() =>
		{
			serverControls.AbortGame();
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
			MoveToClientLobbyWindow();
			clientControls.OnRequestJoin(info);
		});
		joinView.playerNameField.text = JoinView.defaultPlayerName;

		joinView.cancelButton.onClick.AddListener(() =>
		{
			BackToMainMenu();
			clientControls.EndJoin();
		});

		/// CLIENT LOBBY VIEW
		clientLobbyView.readyButton.onClick.AddListener(() => 
		{
			clientControls.OnClientReady();
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