using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Photon.Realtime;

public partial class UIController
{
	[Serializable]
	private struct JoinView : IMenuLayout
	{
		public MenuView view;

		MenuView IMenuLayout.View => view;
		bool IMenuLayout.BelongsToMainMenu => true;

		public const string defaultPlayerName = "Client Player";
		public int 	selectedServerIndex;

		public AvailableServersSelector availableServersSelector;

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

			joinView.mapNameText.text 				= MapNameFromIndex((int)selectedRoom.CustomProperties[PhotonPropertyKey.RoomMapId]);
			joinView.joinedPlayersCountText.text 	= selectedRoom.MaxPlayers.ToString(); 
			joinView.gameDurationText.text 			= TimeFormat.ToTimeFormat((int)selectedRoom.CustomProperties[PhotonPropertyKey.RoomGameDuration]);			
		};

		joinView.requestJoinButton.onClick.AddListener (() => 
		{
			var info = new JoinInfo
			{
				playerName = joinView.playerNameField.text,
				selectedRoomIndex = joinView.selectedServerIndex,
				selectedRoomInfo = availableRooms[joinView.selectedServerIndex]
			};
			netControls.RequestJoin(info);

			SetRoomViewHost(false);
			// EventSystem.current.SetSelectedGameObject(roomView.characterSelectionList.scrollLeft.gameObject);
			// SetView(roomView);
		});
		joinView.playerNameField.text = JoinView.defaultPlayerName;

		joinView.cancelButton.onClick.AddListener(() =>
		{
			EventSystem.current.SetSelectedGameObject(mainView.hostViewButton.gameObject);
			SetMainView();
		});
	}

	//TODO (Joonas): Find a more elegant way to do this.
	private void SetServerListNavigation()
	{
		Navigation serverListNav = new Navigation();
		serverListNav.mode = Navigation.Mode.Explicit;
		serverListNav.selectOnDown = serverListNav.selectOnLeft = serverListNav.selectOnRight = serverListNav.selectOnUp
			= joinView.availableServersSelector.toggleParent.GetChild(0).GetComponent<Toggle>();
		joinView.availableServersSelector.navigation = serverListNav;

		int childCount = joinView.availableServersSelector.toggleParent.childCount;

		for (int i = 0; i < childCount; i++)
		{
			Navigation nav = joinView.availableServersSelector.toggleParent.GetChild(i).GetComponent<Toggle>().navigation;
			if (i == 0)
			{
				nav.selectOnUp = joinView.playerNameField;
				if (childCount > 1)
					nav.selectOnDown = joinView.availableServersSelector.toggleParent.GetChild(i + 1).GetComponent<Toggle>();
				else
					nav.selectOnDown = joinView.requestJoinButton;
			}
			else
			{
				nav.selectOnUp = joinView.availableServersSelector.toggleParent.GetChild(i - 1).GetComponent<Toggle>();
				if (i == (childCount - 1))
				{
					nav.selectOnDown = joinView.requestJoinButton;
				}
				else
				{
					nav.selectOnDown = joinView.availableServersSelector.toggleParent.GetChild(i + 1).GetComponent<Toggle>();
				}
			}
			nav.selectOnLeft = nav.selectOnRight = joinView.availableServersSelector.toggleParent.GetChild(i).GetComponent<Toggle>();
			joinView.availableServersSelector.toggleParent.GetChild(i).GetComponent<Toggle>().navigation = nav;
		}
	}
}