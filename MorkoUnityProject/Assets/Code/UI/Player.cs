using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Morko
{
	public class Player
	{
		private int id;
		private string name;
		private string character;
		private bool isHost;

		public int ID { get => id; set => id = value; }
		public string Name{ get => name; set => name = value; }
		public string Character { get => character; set => character = value; }
		public bool Host { get => isHost; set => isHost = value; }

		public Player() { }
	}
}