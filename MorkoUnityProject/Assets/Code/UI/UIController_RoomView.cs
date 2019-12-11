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

    public int HAXOR_BAD_CODE_GetSelectedCharacterIndex()
    {
        return roomView.characterSelectionList.SelectedIndex;
    }

    private void InitializeRoomView()
    {
        roomView.view.OnShow += () =>
        {
            EventSystem.current.SetSelectedGameObject(roomView.characterSelectionList.scrollLeft.gameObject);
        };

        roomView.hostStartGameButton.onClick.AddListener(() =>
        {
            netControls.StartGame();
        });

        roomView.readyButton.onClick.AddListener(() => 
        {
            netControls.OnPlayerReady();
        });

        roomView.cancelButton.onClick.AddListener(() =>
        {
            netControls.LeaveRoom();
        });
    }

    private void SetRoomViewHost(bool isHost)
    {
        roomView.IsHost = isHost;
        roomView.hostStartGameButton.gameObject.SetActive(isHost);
    }
}