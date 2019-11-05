using System;
using UnityEngine;
using UnityEngine.UI;

public partial class UIController
{
	[Serializable]
	private struct HostView : IMenuLayout
	{
		public MenuView view;

		MenuView IMenuLayout.View => view;
		bool IMenuLayout.BelongsToMainMenu => true;

		public const string defaultServerName = "Default Server";
		public const string defaultPlayerName = "Hosting Player";

		public Button cancelButton;
		public Button createRoomButton;
		public InputField playerNameField;
		public InputField serverNameField;
		public DiscreteInputField playerCountField;
		public DiscreteInputField gameDurationField;
	}
	[SerializeField] private HostView hostView;

	private void InitializeHostView()
	{
		hostView.view.OnShow += () => listItemContainer.SetActive(true);

		hostView.createRoomButton.onClick.AddListener(() =>
		{
			var info = new ServerInfo
			{
				serverName			= hostView.serverNameField.text,
				mapIndex 			= 0,
				maxPlayers 			= hostView.playerCountField.IntValue,
				gameDurationSeconds = hostView.gameDurationField.IntValue, 	
			};
			serverControls.CreateServer(info);
			SetView(hostLobbyView);
		});
		hostView.cancelButton.onClick.AddListener(() => SetMainView());
		hostView.playerNameField.text = HostView.defaultPlayerName;
		hostView.serverNameField.text = HostView.defaultServerName;
	}
}