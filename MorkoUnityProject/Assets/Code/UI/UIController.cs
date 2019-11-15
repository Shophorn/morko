using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Morko;

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

	[SerializeField] private GameObject connectingScreen;
	[SerializeField] private GameObject loadingScreen;

	private MenuView currentView = null;

	public void SetMainView()
	{
		loadingScreen.SetActive(false);
		connectingScreen.SetActive(false);

		currentView?.Hide();
		currentView = null;

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

	//private void MenuNavigation()
	//{
	//	if (Input.GetAxisRaw("Horizontal") < 0)
	//	{
	//		if (!isAxisInUse)
	//		{
	//			isAxisInUse = true;

	//			currentUIObject.GetComponent<Image>().color = currentUIObject.colors.normalColor;
	//			currentUIObject = currentUIObject.FindSelectableOnLeft();
	//			currentUIObject.GetComponent<Image>().color = currentUIObject.colors.highlightedColor;
	//		}
	//	}
	//	else if (Input.GetAxisRaw("Horizontal") > 0)
	//	{
	//		if (!isAxisInUse)
	//		{
	//			isAxisInUse = true;
	//			currentUIObject.GetComponent<Image>().color = currentUIObject.colors.normalColor;
	//			currentUIObject = currentUIObject.FindSelectableOnRight();
	//			currentUIObject.GetComponent<Image>().color = currentUIObject.colors.highlightedColor;
	//		}
	//	}
	//	else if (Input.GetAxisRaw("Vertical") > 0)
	//	{
	//		if (!isAxisInUse)
	//		{
	//			isAxisInUse = true;
	//			currentUIObject.GetComponent<Image>().color = currentUIObject.colors.normalColor;
	//			currentUIObject = currentUIObject.FindSelectableOnUp();
	//			currentUIObject.GetComponent<Image>().color = currentUIObject.colors.highlightedColor;
	//		}
	//	}
	//	else if (Input.GetAxisRaw("Vertical") < 0)
	//	{
	//		if (!isAxisInUse)
	//		{
	//			isAxisInUse = true;
	//			currentUIObject.GetComponent<Image>().color = currentUIObject.colors.normalColor;
	//			currentUIObject = currentUIObject.FindSelectableOnDown();
	//			currentUIObject.GetComponent<Image>().color = currentUIObject.colors.highlightedColor;
	//		}
	//	}
	//	else
	//		isAxisInUse = false;

	//	currentUIObject.Select();
	//}
}
