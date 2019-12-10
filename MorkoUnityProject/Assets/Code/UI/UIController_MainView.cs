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
		public Button howToPlayButton;
	}
	[SerializeField] private MainView mainView;

	private void InitializeMainView()
	{
		mainView.view.OnShow += () =>
		{
			EventSystem.current.SetSelectedGameObject(mainView.hostViewButton.gameObject);
		};

		mainView.hostViewButton.onClick.AddListener(() =>
		{
			EventSystem.current.SetSelectedGameObject(hostView.mapSelectionList.scrollLeft.gameObject);
			SetView(hostView);
			scrollContent.SetOptions(GameManager.GetMapNames, GameManager.GetMapPrefabsForSelection());
			hostView.mapSelectionList.SetLabel(); 
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
		mainView.howToPlayButton.onClick.AddListener(() =>
		{
			EventSystem.current.SetSelectedGameObject(howToPlayView.cancelButton.gameObject);
			SetView(howToPlayView);
		});

		mainView.quitButton.onClick.AddListener(() => appControls.Quit());
	}
}