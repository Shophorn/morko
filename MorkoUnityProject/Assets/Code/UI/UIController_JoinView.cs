using System;
using UnityEngine;
using UnityEngine.UI;
using Morko;

public partial class UIController
{
	[Serializable]
	private struct JoinView : IMenuLayout
	{
		public MenuView view;

		MenuView IMenuLayout.View => view;
		bool IMenuLayout.BelongsToMainMenu => true;

		public const string defaultPlayerName = "Client Player";

		public ToggleGroup 			availableServersToggleGroup;
		public Transform 			availableServersToggleParent;
		public ServerToggleListItem availableServersTogglePrefab;
		public int 					selectedServerIndex;

		public Text hostingPlayerNameText;
		public Text mapNameText;
		public Text joinedPlayersCountText;
		public Text gameDurationText;

		public InputField playerNameField;

		public Button requestJoinButton;
		public Button cancelButton;
	}
	[SerializeField] private JoinView joinView;

	private void InitializeJoinView()
	{
		joinView.view.OnShow += clientControls.BeginJoin;
		// joinView.view.OnHide += clientControls.EndJoin;
		
		joinView.requestJoinButton.onClick.AddListener (() => 
		{
			var info = new JoinInfo
			{
				playerName = joinView.playerNameField.text,
				selectedServerIndex = joinView.selectedServerIndex
			};
			clientControls.RequestJoin(info);

			SetView(clientLobbyView);
		});
		joinView.playerNameField.text = JoinView.defaultPlayerName;

		joinView.cancelButton.onClick.AddListener(() =>
		{
			clientControls.EndJoin();
			SetMainView();
		});
	}
}