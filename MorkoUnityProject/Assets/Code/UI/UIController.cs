using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using RoomInfo = Photon.Realtime.RoomInfo;

public class RoomCreateInfo
{
	public string roomName;
	public string hostingPlayerName;
	public int mapIndex;
	public int maxPlayers;
	public int gameDurationSeconds;
}

public class JoinInfo 
{
	public string playerName;
	public int selectedRoomIndex;
	public RoomInfo selectedRoomInfo;
}

public partial class UIController : MonoBehaviour
{
	private interface IMenuLayout
	{
		MenuView View { get; }
		bool BelongsToMainMenu { get; }
	}

	private INetUIControllable netControls;
	private IAppUIControllable appControls;
	public IAudioUIControllable soundControls;

	[SerializeField] private GameObject uiMainGameObject;

	[SerializeField] private GameObject connectingScreen;
	[SerializeField] private GameObject joiningScreen;
	[SerializeField] private GameObject loadingScreen;

	[SerializeField] private GameObject notPauseWindow;
	[SerializeField] private Button exitMatchButton;

	[SerializeField] private GameObject background;

	[SerializeField] private ScrollContent scrollContent;

	private bool notPauseMenuActive = false;
	private bool hidden = false;

	private MenuView currentView = null;

	public void SetMainView()
	{
		loadingScreen.SetActive(false);
		joiningScreen.SetActive(false);
		connectingScreen.SetActive(false);

		currentView?.Hide();
		currentView = null;
		EventSystem.current.firstSelectedGameObject = mainView.hostViewButton.gameObject;
		mainView.view.Show();
	}

	private void SetView(IMenuLayout layout)
	{
		if (layout == null)
		{
			mainView.view.Hide();
			currentView?.Hide();
			currentView = null;
			return;
		}

		if(currentView == layout.View)
			return;

		// Todo(Leo): Make these IMenuLayout too
		loadingScreen.SetActive(false);
		joiningScreen.SetActive(false);
		connectingScreen.SetActive(false);

		currentView?.Hide();
		currentView = null;


		if (layout.BelongsToMainMenu)
			SetMainView();
		else
			mainView.view.Hide();

		currentView = layout.View;
		currentView.Show();
	}



	// private void Awake()
	public void Configure(	INetUIControllable netControls,
							IAppUIControllable appControls,
							IAudioUIControllable soundControls)
	{
		this.netControls = netControls;
		this.appControls = appControls;
		this.soundControls = soundControls;

		InitializeMainView();
		InitializeHostView();
		InitializeJoinView();
		InitializeRoomView();
		InitializeOptionsView();
		InitializeCreditsView();
		InitializeHowToPlayView();

		exitMatchButton.onClick.AddListener(appControls.ExitMatch);

		hidden = !uiMainGameObject.activeInHierarchy;
	}

	public void SetRoomView ()
	{
		SetView(roomView);
	}

	public void SetConnectingScreen()
	{
		connectingScreen.SetActive(true);
		SetView(null);
	}

	public void SetJoiningScreen()
	{
		joiningScreen.SetActive(true);
		SetView(null);
	}

	public void SetLoadingScreen()
	{
		loadingScreen.SetActive(true);
		SetView(null);
	}

	public void ToggleNotPauseMenu(bool? forceActive = null)
	{
		Debug.Log("Toggle pause menu");

		if (forceActive != null)
			notPauseMenuActive = (bool)forceActive;
		else if (hidden == false)
			notPauseMenuActive = false;
		else
			notPauseMenuActive = !notPauseMenuActive;

		notPauseWindow.SetActive(notPauseMenuActive);
	}

		public void Show()
	{
		ToggleNotPauseMenu(forceActive: false);
		
		background.SetActive(true);
		hidden = false;

		Debug.Log("[UI]: Shown");
	}

	public void Hide()
	{
		SetView(null);
		connectingScreen.SetActive(false);
		loadingScreen.SetActive(false);
		background.SetActive(false);
		hidden = true;
	}

	private List<RoomInfo> availableRooms;

	private string MapNameFromIndex(int mapIndex)
	{
		Debug.LogError("MapNameFromIndex not properly implemented!!!");

		// Todo(Leo): Obviously this is not correct, please fix
		return "Somber Bomber Suburbinator";
	}

	public void AddPlayer(int uniqueId, string name, PlayerNetworkStatus status)
		=> roomView.playerNameList.AddPlayer(uniqueId, name, status);

	public void RemovePlayer(int uniqueId)
		=> roomView.playerNameList.RemovePlayer(uniqueId);

	public void UpdatePlayerNetworkStatus(int uniqueId, PlayerNetworkStatus status)
		=> roomView.playerNameList.SetStatus(uniqueId, status);

	public void SetRooms(List<RoomInfo> rooms)
	{
		availableRooms = rooms;

		string[] names = rooms.Select(room => room.Name).ToArray();
		joinView.availableServersSelector.SetOptions(names);
	}
}
