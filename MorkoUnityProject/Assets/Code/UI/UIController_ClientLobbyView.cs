// using System;
// using UnityEngine;
// using UnityEngine.UI;

// public partial class UIController
// {
// 	[Serializable]
// 	private struct ClientLobbyView : IMenuLayout
// 	{
// 		public MenuView view;

// 		MenuView IMenuLayout.View => view;
// 		bool IMenuLayout.BelongsToMainMenu => false;

// 		public PlayerNameList playerNameList;

// 		public Button readyButton;
// 		public Button cancelButton;
// 	}
// 	[SerializeField] private ClientLobbyView clientLobbyView;

// 	private void InitializeClientLobbyView()
// 	{
// 		clientLobbyView.view.OnShow += () => clientLobbyView.readyButton.enabled = true;
// 		clientLobbyView.view.OnShow += () => listItemContainer.SetActive(true);

// 		clientLobbyView.readyButton.onClick.AddListener(() => 
// 		{
// 			clientControls.OnClientReady();
// 			clientLobbyView.readyButton.enabled = false;
// 		});

// 		clientLobbyView.cancelButton.onClick.AddListener(() =>
// 		{
// 			SetMainView();
// 		});
// 	}
// }