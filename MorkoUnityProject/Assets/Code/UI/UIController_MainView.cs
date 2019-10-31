using System;
using UnityEngine;
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
		mainView.hostViewButton.onClick.AddListener(() => SetView(hostView));
		mainView.joinViewButton.onClick.AddListener(() => SetView(joinView));
		mainView.optionsButton.onClick.AddListener(() => SetView(optionsView));
		mainView.creditsButton.onClick.AddListener(() => SetView(creditsView));

		mainView.quitButton.onClick.AddListener(() => appControls.Quit());
	}
}