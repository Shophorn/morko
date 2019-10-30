using UnityEngine;
using UnityEngine.UI;

namespace Morko
{
	public class ServerToggleListItem : MonoBehaviour
	{
		public Text label;
		public Toggle toggle;

		
		// public int id;
		// public ToggleGroup toggleGroup;
		// public JoinRoomWindow joinWindow;

		// public RoomInfoPanel infoDisplay;
		

		// void Start()
		// {
		// 	toggleGroup = GameObject.Find("Togglegroup").GetComponent<ToggleGroup>();
		// 	GetComponent<Toggle>().group = toggleGroup;
		// 	infoDisplay = GameObject.Find("Info Panel").GetComponent<RoomInfoPanel>();
		// 	joinWindow = GameObject.Find("Join Window").GetComponent<JoinRoomWindow>();
		// }

		// public void DisplayItemInfo()
		// {
		// 	infoDisplay.DisplayRoomInfo(id);
		// }

		// public void JoinRoom()
		// {
		// 	joinWindow.roomToBeJoined = id;
		// }
	}
}