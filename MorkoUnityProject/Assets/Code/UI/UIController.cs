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
	[SerializeField] private ToggleContainer toggleContainer;
	//[SerializeField] private RoomInfoPanel roomInfo;
	[SerializeField] private Text levelName;
	[SerializeField] private Text playerAmount;
	[SerializeField] private Text roundLength;

	[SerializeField] private Button playerLobbyWindowCancelButton;

	[SerializeField] private Button optionsWindowCancelButton;
	[SerializeField] private Button creditsWindowCancelButton;

	[SerializeField] private Text playerName;

	[SerializeField] private GameObject listItemContainer;
	[SerializeField] private ScrollContent scrollContent;

	private Selectable currentUIObject;
	private bool isAxisInUse = false;

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
		currentUIObject = mainView.hostViewButton;
	}
	private void MoveToServerCreationWindow()
	{
		DisableAll();
		mainMenuWindow.SetActive(true);
		serverCreationWindow.SetActive(true);
		listItemContainer.SetActive(true);
		currentUIObject = hostView.serverNameField;
	}
	private void MoveToServerJoiningWindow()
	{
		DisableAll();
		mainMenuWindow.SetActive(true);
		joinServerWindow.SetActive(true);
		currentUIObject = joinView.playerNameField;
	}
	private void MoveToHostLobbyWindow()
	{
		DisableAll();
		hostLobbyWindow.SetActive(true);
		listItemContainer.SetActive(true);
		currentUIObject = hostLobbyView.startGameButton;
	}
	private void MoveToClientLobbyWindow()
	{
		DisableAll();
		playerLobbyWindow.SetActive(true);
		listItemContainer.SetActive(true);
		currentUIObject = clientLobbyView.readyButton;
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
		currentUIObject = mainView.hostViewButton;
	}

	private void Update()
	{
		MenuNavigation();
	}
	private void MenuNavigation()
	{

		if (Input.GetAxisRaw("Horizontal") < 0)
		{
			if (!isAxisInUse)
			{
				isAxisInUse = true;

				currentUIObject.GetComponent<Image>().color = currentUIObject.colors.normalColor;
				currentUIObject = currentUIObject.FindSelectableOnLeft();
				currentUIObject.GetComponent<Image>().color = currentUIObject.colors.highlightedColor;
			}
		}
		else if (Input.GetAxisRaw("Horizontal") > 0)
		{
			if (!isAxisInUse)
			{
				isAxisInUse = true;
				currentUIObject.GetComponent<Image>().color = currentUIObject.colors.normalColor;
				currentUIObject = currentUIObject.FindSelectableOnRight();
				currentUIObject.GetComponent<Image>().color = currentUIObject.colors.highlightedColor;
			}
		}
		else if (Input.GetAxisRaw("Vertical") > 0)
		{
			if (!isAxisInUse)
			{
				isAxisInUse = true;
				currentUIObject.GetComponent<Image>().color = currentUIObject.colors.normalColor;
				currentUIObject = currentUIObject.FindSelectableOnUp();
				currentUIObject.GetComponent<Image>().color = currentUIObject.colors.highlightedColor;
			}
		}
		else if (Input.GetAxisRaw("Vertical") < 0)
		{
			if (!isAxisInUse)
			{
				isAxisInUse = true;
				currentUIObject.GetComponent<Image>().color = currentUIObject.colors.normalColor;
				currentUIObject = currentUIObject.FindSelectableOnDown();
				currentUIObject.GetComponent<Image>().color = currentUIObject.colors.highlightedColor;
			}
		}
		else
			isAxisInUse = false;

		currentUIObject.Select();
	}

	public void ToggleNavigation()
	{
		for(int i = 0; i < toggleContainer.ToggleParent.childCount; i++)
		{
			Navigation nav = joinView.availableServersToggleParent.GetChild(i).GetComponent<Toggle>().navigation;
			if(i == 0)
			{
				nav.selectOnUp = joinView.playerNameField;
				nav.selectOnDown = joinView.availableServersToggleParent.GetChild(i + 1).GetComponent<Toggle>();
			}
			else
			{
				nav.selectOnUp = joinView.availableServersToggleParent.GetChild(i - 1).GetComponent<Toggle>();
				if(i == joinView.availableServersToggleParent.childCount-1)
				{
					nav.selectOnDown = joinView.requestJoinButton;
				}
				else
				{
					nav.selectOnDown = joinView.availableServersToggleParent.GetChild(i + 1).GetComponent<Toggle>();
				}
			}
			nav.selectOnLeft = nav.selectOnRight = joinView.availableServersToggleParent.GetChild(i).GetComponent<Toggle>();
			joinView.availableServersToggleParent.GetChild(i).GetComponent<Toggle>().navigation = nav;
		}
		Navigation nameNav = joinView.playerNameField.navigation;
		nameNav.selectOnDown = joinView.availableServersToggleParent.GetChild(joinView.availableServersToggleParent.childCount - 1).GetComponent<Toggle>();
		joinView.playerNameField.navigation = nameNav;
		Navigation cancelNav = joinView.cancelButton.navigation;
		cancelNav.selectOnUp = joinView.availableServersToggleParent.GetChild(0).GetComponent<Toggle>();
		joinView.cancelButton.navigation = cancelNav;
		Navigation joinNav = joinView.requestJoinButton.navigation;
		joinNav.selectOnUp = joinView.availableServersToggleParent.GetChild(0).GetComponent<Toggle>();
		joinView.requestJoinButton.navigation = joinNav;
	}
}