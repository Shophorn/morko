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
	private interface IMenuLayout
	{
		MenuView View { get; }
		bool BelongsToMainMenu { get; }
	}

	private IClientUIControllable clientControls;
	private IServerUIControllable serverControls;
	private IAppUIControllable appControls;

	[SerializeField] private GameObject uiMainGameObject;

	/* Todo(Leo, Joonas): This does not really belong here, but should be hidden
	 behind ScrollSelector etc. */
	[SerializeField] private GameObject listItemContainer;

	private MenuView currentView = null;

	private void SetMainView()
	{
		currentView?.Hide();
		currentView = null;

		mainView.view.Show();
	}

	private void SetView(IMenuLayout layout)
	{
		if(currentView == layout.View)
			return;

		currentView?.Hide();

		if (layout.BelongsToMainMenu)
			SetMainView();
		else
			mainView.view.Hide();

		currentView = layout.View;
		currentView.Show();
	}

	private void Start()
	{
		// Todo(Leo): These must be injected, since they might actually not be present here
		clientControls 	= GetComponent<IClientUIControllable>();
		serverControls 	= GetComponent<IServerUIControllable>();
		appControls 	= GetComponent<IAppUIControllable>();

		InitializeMainView();
		InitializeHostView();
		InitializeHostLobbyView();
		InitializeJoinView();
		InitializeClientLobbyView();
		InitializeOptionsView();
		InitializeCreditsView();

		mainView.view.Show();
	}
}
