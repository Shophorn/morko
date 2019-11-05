using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class UIController
{
	[Serializable] 
	private struct MainView
	{
		public MenuView view;

		public Button hostViewButton;
		public Button joinViewButton;
		public Button creditsButton;
		public Button optionsButton;
		public Button quitButton;
	}
	[SerializeField] private MainView mainView;

	private void InitializeMainView()
	{
		mainView.hostViewButton.onClick.AddListener(() =>
		{
			EventSystem.current.SetSelectedGameObject(hostView.serverNameField.gameObject);
			SetView(hostView);
		});
		mainView.joinViewButton.onClick.AddListener(() =>
		{
			EventSystem.current.SetSelectedGameObject(joinView.playerNameField.gameObject);
			SetView(joinView);
		});
		mainView.optionsButton.onClick.AddListener(() =>
		{
			EventSystem.current.SetSelectedGameObject(optionsView.cancelButton.gameObject);
			SetView(optionsView);
		});
		mainView.creditsButton.onClick.AddListener(() =>
		{
			EventSystem.current.SetSelectedGameObject(creditsView.cancelButton.gameObject);
			SetView(creditsView);
		});

		mainView.quitButton.onClick.AddListener(() => appControls.Quit());
	}
}