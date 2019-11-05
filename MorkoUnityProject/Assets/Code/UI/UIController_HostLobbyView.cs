using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class UIController
{
	[Serializable]
	private struct HostLobbyView : IMenuLayout
	{
		public MenuView view;

		MenuView IMenuLayout.View => view;
		bool IMenuLayout.BelongsToMainMenu => false;

		public Button startGameButton;
		public Button cancelButton;
		public InfiniteScroll characterSelectionList;
	}
	[SerializeField] private HostLobbyView hostLobbyView;

	private void InitializeHostLobbyView()
	{
		hostLobbyView.view.OnShow += () => hostLobbyView.characterSelectionList.listItemContainer.SetActive(true);
		
		hostLobbyView.startGameButton.onClick.AddListener(() =>
		{
			serverControls.StartGame();
		});

		hostLobbyView.cancelButton.onClick.AddListener(() =>
		{
			serverControls.AbortGame();
			EventSystem.current.SetSelectedGameObject(mainView.hostViewButton.gameObject);
			SetMainView();
		});
	}
}