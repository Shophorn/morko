using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class UIController
{
    [Serializable]
    private struct RoomView : IMenuLayout
    {
        public MenuView view;

        MenuView IMenuLayout.View => view;
        bool IMenuLayout.BelongsToMainMenu => false;

        public PlayerNameList playerNameList;

        public Button hostStartGameButton;

        public Button readyButton;
        public Button cancelButton;
		public CircularScroll characterSelectionList;

		public bool IsHost { get; set; }
    }
    [SerializeField] private RoomView roomView;

    private void InitializeRoomView()
    {
        roomView.hostStartGameButton.onClick.AddListener(() =>
        {
            serverControls.StartGame();
        });

        roomView.readyButton.onClick.AddListener(() => 
        {
            clientControls.OnPlayerReady();
        });

        roomView.cancelButton.onClick.AddListener(() =>
        {
			EventSystem.current.SetSelectedGameObject(mainView.hostViewButton.gameObject);
			// Todo(Leo): Call some cancel callback, we need to cancel photon
			SetMainView();
        });
    }

    private void SetRoomViewHost(bool isHost)
    {
        roomView.IsHost = isHost;
        roomView.hostStartGameButton.gameObject.SetActive(isHost);
    }
}