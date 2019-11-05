//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//namespace Morko
//{
//	public class HostRoomContainer : MonoBehaviour
//	{
//		public GameObject hostWindow;
//		public GameObject joinWindow;

//		public LobbyWindow lobby;

//		public Text hostNameText;
//		public Text roomNameText;

//		public string ServerName => roomNameText.text;

//		public Text playerAmount;
//		public Text timerValue;

//		public InfiniteScroll levelSelectionList;

//		public List<Room> rooms;

//		public Button hostButton;


//		public void Start()
//		{
//			rooms = new List<Room>();
//		}

//		public void SaveHostInfo()
//		{
//			Room room = new Room();
//			room.ID = rooms.Count;
//			room.HostName = hostNameText.text;
//			room.RoomName = roomNameText.text;
//			room.LevelName = levelSelectionList.currentItem.listItemName;
//			room.MaxPlayers = int.Parse(playerAmount.text);
//			room.PlayersJoined = 1;
//			room.RoundLength = timerValue.text;

//			room.Players.Add(CreateHostPlayer());

//			Debug.Log(rooms.Count);

//			lobby.selectedRoomId = room.ID;
//			rooms.Add(room);			
//		}

//		public Player CreateHostPlayer()
//		{
//			Player player = new Player();
//			player.Name = hostNameText.text;
//			player.Character = "";
//			player.Host = true;

//			return player;
//		}
//	}
//}