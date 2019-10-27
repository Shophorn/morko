using UnityEngine;
using UnityEngine.UI;

namespace Morko
{
	public class JoinRoomWindow : MonoBehaviour
	{
		public HostRoomContainer hostRoomContainer;
		public GameObject toggleContainer;
		public RoomListItem toggleItem;
		public LobbyWindow lobby;

		public int roomToBeJoined;

		public Text nameText;
		public string PlayerName => nameText.text;
		public int SelectedServerId => roomToBeJoined;

		public Button requestJoinButton;
		public Button cancelButton;

		private void OnEnable()
		{
			// CreateRoomToggleList();
		}

		private void OnDisable()
		{
			foreach (Transform child in toggleContainer.transform)
				GameObject.Destroy(child.gameObject);
		}

		public void CreateRoomToggleList()
		{
			int iteration = 0;
			foreach(Room room in hostRoomContainer.rooms)
			{
				RoomListItem roomInstance = Instantiate(toggleItem, toggleContainer.transform.position - new Vector3(0,20,0)*iteration, Quaternion.identity);
				roomInstance.ID = iteration;
				roomInstance.label.text = hostRoomContainer.rooms[iteration].RoomName;
				Debug.Log(hostRoomContainer.rooms[iteration].RoomName);
				roomInstance.transform.parent = toggleContainer.transform;
				iteration++;
			}
			toggleContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(280,20*iteration);
		}

		public void JoinRoom()
		{
			Player player = new Player();
			player.ID = hostRoomContainer.rooms[roomToBeJoined].Players.Count;
			player.Name = nameText.text;
			player.Character = "";
			player.Host = false;

			lobby.selectedRoomId =roomToBeJoined;
			hostRoomContainer.rooms[roomToBeJoined].Players.Add(player);
		}
	}
}