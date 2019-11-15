using System;
using UnityEngine;
using UnityEngine.UI;

using Photon.Realtime;

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


		public ToggleContainer availableServersSelector;

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
		joinView.availableServersSelector.OnSelectionChanged += (index) =>
		{
			joinView.selectedServerIndex = index;

			RoomInfo selectedRoom = availableRooms[index];
			// joinView.hostingPlayerNameText.text 	= selectedRoom.hostingPlayerName;

			joinView.mapNameText.text 				= MapNameFromIndex((int)selectedRoom.CustomProperties["map"]);
			joinView.joinedPlayersCountText.text 	= selectedRoom.MaxPlayers.ToString(); 
			joinView.gameDurationText.text 			= TimeFormat.ToTimeFormat((int)selectedRoom.CustomProperties["time"]);			
		};

		joinView.requestJoinButton.onClick.AddListener (() => 
		{
			var info = new JoinInfo
			{
				playerName = joinView.playerNameField.text,
				selectedServerIndex = joinView.selectedServerIndex,
				selectedRoomInfo = availableRooms[joinView.selectedServerIndex]
			};
			clientControls.RequestJoin(info);

			SetRoomViewHost(false);
			SetView(roomView);
		});
		joinView.playerNameField.text = JoinView.defaultPlayerName;

		joinView.cancelButton.onClick.AddListener(() =>
		{
			SetMainView();
		});
	}
}