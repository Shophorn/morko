using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class UIController
{
	[Serializable]
	private struct ClientLobbyView : IMenuLayout
	{
		public MenuView view;

		MenuView IMenuLayout.View => view;
		bool IMenuLayout.BelongsToMainMenu => false;

		public Button readyButton;
		public Button cancelButton;

		public CircularScroll characterSelectionList;
	}
	[SerializeField] private ClientLobbyView clientLobbyView;

	private void InitializeClientLobbyView()
	{
		clientLobbyView.view.OnShow += () => clientLobbyView.readyButton.enabled = true;
		clientLobbyView.view.OnShow += () => clientLobbyView.characterSelectionList.listItemContainer.SetActive(true);

		clientLobbyView.readyButton.onClick.AddListener(() => 
		{
			clientControls.OnClientReady();
			clientLobbyView.readyButton.enabled = false;
		});

		clientLobbyView.cancelButton.onClick.AddListener(() =>
		{
			EventSystem.current.SetSelectedGameObject(mainView.hostViewButton.gameObject);
			SetMainView();
		});
	}
}