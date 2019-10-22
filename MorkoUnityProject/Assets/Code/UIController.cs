using System;
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
	[Header("Main View Elements")]
	[SerializeField] private Button mainMenuHostWindowButton;
	[SerializeField] private Button mainMenuJoinWindowButton;
	[SerializeField] private Button mainMenuCreditsButton;
	[SerializeField] private Button mainMenuOptionsButton;
	[SerializeField] private Button mainMenuQuitButton;

	[Header("Host View Elements")]
	[SerializeField] private Button hostWindowCancelButton;
	[SerializeField] private Button hostWindowCreateRoomButton;
	[SerializeField] private Text hostViewPlayerNameField;
	[SerializeField] private Text hostViewServerNameField;
	[SerializeField] private DiscreteInputField hostViewPlayerCountField;
	[SerializeField] private DiscreteInputField hostViewGameDurationField;

	[Header("Uncategorized")]
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


	[SerializeField] private Button joinViewCancelButton;
	[SerializeField] private Button hostLobbyWindowCancelButton;


	[SerializeField] private Button joinWindowRequestJoinButton;
	[SerializeField] private Button joinWindowCancelButton;
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

	private string PlayerName => playerName.text;
	private int SelectedServerID => selectedServerId;

	private void Start()
	{
		mainMenuWindow.SetActive(true);

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
				name 				= hostViewServerNameField.text,
				mapIndex 			= 0,
				maxPlayers 			= hostViewPlayerCountField.IntValue,
				gameDurationSeconds = hostViewGameDurationField.IntValue, 	
			};
			CallEvent(OnStartHosting, info, nameof(OnStartHosting));

			//OnStartHosting(info);
			MoveToHostLobbyWindow();
		});

		hostLobbyWindowCancelButton.onClick.AddListener(() =>
		{
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

		mainMenuQuitButton.onClick.AddListener(() => CallEvent(OnQuit, nameof(OnQuit)));
	}

	private void CallEvent(Action action, string name)
	{
		if (action == null)
		{
			Debug.LogError($"Trying to call event '{name}', but it is null.");
		}
		else
		{
			action.Invoke();
		}
	}

	private void CallEvent<T>(Action<T> action, T argument, string name)
	{
		if (action == null)
		{
			Debug.LogError($"Trying to call event '{name}', but it is null.");
		}
		else
		{
			action.Invoke(argument);
		}	
	}
}