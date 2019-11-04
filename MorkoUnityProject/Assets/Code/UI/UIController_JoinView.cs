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


		public ToggleGroup availableServersToggleGroup;
		public Transform availableServersToggleParent;
		public ServerToggleListItem availableServersTogglePrefab;
		public int 					selectedServerIndex;


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

	private void SetServerListNavigation()
	{
		for (int i = 0; i < joinView.availableServersSelector.toggleParent.childCount; i++)
		{
			Navigation nav = joinView.availableServersSelector.toggleParent.GetChild(i).GetComponent<Toggle>().navigation;
			if (i == 0)
			{
				nav.selectOnUp = joinView.playerNameField;
				nav.selectOnDown = joinView.availableServersSelector.toggleParent.GetChild(i + 1).GetComponent<Toggle>();
			}
			else
			{
				nav.selectOnUp = joinView.availableServersSelector.toggleParent.GetChild(i - 1).GetComponent<Toggle>();
				if (i == joinView.availableServersSelector.toggleParent.childCount - 1)
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
		Navigation nameNav = joinView.playerNameField.navigation;
		nameNav.selectOnDown = joinView.availableServersSelector.toggleParent.GetChild(joinView.availableServersSelector.toggleParent.childCount - 1).GetComponent<Toggle>();
		joinView.playerNameField.navigation = nameNav;
		Navigation cancelNav = joinView.cancelButton.navigation;
		cancelNav.selectOnUp = joinView.availableServersSelector.toggleParent.GetChild(0).GetComponent<Toggle>();
		joinView.cancelButton.navigation = cancelNav;
		Navigation joinNav = joinView.requestJoinButton.navigation;
		joinNav.selectOnUp = joinView.availableServersSelector.toggleParent.GetChild(0).GetComponent<Toggle>();
		joinView.requestJoinButton.navigation = joinNav;
	}
}