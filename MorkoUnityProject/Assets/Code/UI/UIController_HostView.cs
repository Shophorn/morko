using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class UIController
{
	[Serializable]
	private struct HostView : IMenuLayout
	{
		public int serverIndex;

		public MenuView view;

		MenuView IMenuLayout.View => view;
		bool IMenuLayout.BelongsToMainMenu => true;

		public const string defaultServerName = "ROOM NAME";
		public const string defaultPlayerName = "PLAYER";

		public Button cancelButton;
		public Button createRoomButton;
		public InputField playerNameField;
		public InputField serverNameField;
		public DiscreteInputField playerCountField;
		public DiscreteInputField gameDurationField;
		public CircularScroll mapSelectionList;
	}
	[SerializeField] private HostView hostView;

	private void InitializeHostView()
	{
		hostView.view.OnShow += () => hostView.mapSelectionList.listItemContainer.SetActive(true);

		hostView.createRoomButton.onClick.AddListener(() =>
		{
			var info = new RoomCreateInfo
			{
				roomName			= hostView.serverNameField.text,
				hostingPlayerName	= hostView.playerNameField.text,
				mapIndex 			= 0,
				maxPlayers 			= hostView.playerCountField.IntValue,
				gameDurationSeconds = hostView.gameDurationField.IntValue, 	
			};

			netControls.CreateRoom(info);

			SetRoomViewHost(true);
			EventSystem.current.SetSelectedGameObject(roomView.characterSelectionList.scrollLeft.gameObject);
			SetView(roomView);
			scrollContent.SetOptions(GameManager.GetCharacterNames, GameManager.GetCharacterModelsForSelection());
			roomView.characterSelectionList.SetLabel();
		});
		hostView.cancelButton.onClick.AddListener(() =>
		{
			EventSystem.current.SetSelectedGameObject(mainView.hostViewButton.gameObject);
			SetMainView();
		});

		hostView.playerNameField.text = HostView.defaultPlayerName;
		hostView.serverNameField.text = HostView.defaultServerName;
	}
}