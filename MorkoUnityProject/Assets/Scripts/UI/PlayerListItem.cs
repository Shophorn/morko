using UnityEngine;
using UnityEngine.UI;

namespace Morko
{
	public class PlayerListItem : MonoBehaviour
	{
		public int id;
		public Text playerName;
		public Text characterName;
		public Image checkMark;

		private void Start()
		{
			playerName = GameObject.Find("Player").GetComponent<Text>();
			characterName = GameObject.Find("Character").GetComponent<Text>();
			checkMark = GameObject.Find("Checkmark").GetComponent<Image>();
		}
	}
}
