using UnityEngine;
using UnityEngine.UI;

namespace Morko
{
	public class RoomListItem : MonoBehaviour
	{
		public int id;
		public Text label;
		public ToggleGroup toggleGroup;
		public JoinRoomWindow joinWindow;

		public RoomInfoPanel infoDisplay;

		public int ID { get => id; set => id = value; }
		

		void Start()
		{
			toggleGroup = GameObject.Find("Togglegroup").GetComponent<ToggleGroup>();
			GetComponent<Toggle>().group = toggleGroup;
			infoDisplay = GameObject.Find("Info Panel").GetComponent<RoomInfoPanel>();
			joinWindow = GameObject.Find("Join Window").GetComponent<JoinRoomWindow>();
		}

		public void DisplayItemInfo()
		{
			infoDisplay.DisplayRoomInfo(ID);
		}

		public void JoinRoom()
		{
			joinWindow.roomToBeJoined = ID;
		}
	}
}