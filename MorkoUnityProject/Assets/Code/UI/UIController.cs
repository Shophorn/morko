using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public interface IAudioUIControllable
{
	void SetMasterVolume(float value);
	void SetMusicVolume(float value);
	void SetCharacterVolume(float value);
	void SetSfxVolume(float value);
}

public partial class UIController : MonoBehaviour
{

	private interface IMenuLayout
	{
		MenuView View { get; }
		bool BelongsToMainMenu { get; }
	}

	private IClientUIControllable clientControls;
	private IServerUIControllable serverControls;
	private IAppUIControllable appControls;
	public IAudioUIControllable soundControllable;


	[SerializeField] private GameObject uiMainGameObject;

	[SerializeField] private GameObject connectingScreen;
	[SerializeField] private GameObject loadingScreen;

	[SerializeField] private GameObject notPauseWindow;
	[SerializeField] private Button exitMatchButton;

	[SerializeField] private GameObject background;

	private bool notPauseMenuActive = false;
	private bool hidden = false;

	private MenuView currentView = null;

	public void SetMainView()
	{
		loadingScreen.SetActive(false);
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

		loadingScreen.SetActive(false);
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
	public void Configure(	IClientUIControllable clientControls,
							IServerUIControllable serverControls,
							IAppUIControllable appControls,
							IAudioUIControllable soundControllable)
	{
		this.clientControls = clientControls;
		this.serverControls = serverControls;
		this.appControls = appControls;
		this.soundControllable = soundControllable;

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

	public void SetConnectingScreen()
	{
		connectingScreen.SetActive(true);
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
}
