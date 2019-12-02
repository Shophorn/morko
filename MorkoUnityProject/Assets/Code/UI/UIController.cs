using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

	private void Awake()
	{
		// Todo(Leo): These must be injected, since they might actually not be present here
		clientControls 	= GetComponent<IClientUIControllable>();
		serverControls 	= GetComponent<IServerUIControllable>();
		appControls 	= GetComponent<IAppUIControllable>();

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
