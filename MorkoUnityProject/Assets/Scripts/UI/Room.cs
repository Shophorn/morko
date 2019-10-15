using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Morko
{
	public class Room
	{
		[SerializeField]
		private int id;
		[SerializeField]
		private string hostName;
		[SerializeField]
		private string roomName;
		[SerializeField]
		private string levelName;
		[SerializeField]
		private string maxPlayers;
		[SerializeField]
		private string roundLength;

		[SerializeField]
		private Player[] players;


		public int ID { get => id; set => id = value; }
		public string HostName { get => hostName; set => hostName = value; }
		public string RoomName { get => roomName; set => roomName = value; }
		public string LevelName { get => levelName; set => levelName = value; }
		public string MaxPlayers { get => maxPlayers; set => maxPlayers = value; }
		public string RoundLength { get => roundLength; set => roundLength = value; }

		public Room() { }
	}
}