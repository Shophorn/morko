using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Morko
{
	public class HostRoomContainer : MonoBehaviour
	{
		public GameObject hostWindow;
		public GameObject joinWindow;
		public GameObject lobbyHost;
		public GameObject lobbyPlayer;

		public Text hostNameText;
		public Text roomNameText;

		public Text playerAmount;
		public Text timerValue;

		public InfiniteScroll levelSelectionList;

		public List<Room> rooms;

		public void Start()
		{
			rooms = new List<Room>();
		}

		public void SaveHostInfo()
		{
			Room room = new Room();
			room.ID = rooms.Count;
			room.HostName = hostNameText.text;
			room.RoomName = roomNameText.text;
			room.LevelName = levelSelectionList.currentItem.listItemName;
			room.MaxPlayers = playerAmount.text;
			room.RoundLength = timerValue.text;

			rooms.Add(room);
		}
	}
}