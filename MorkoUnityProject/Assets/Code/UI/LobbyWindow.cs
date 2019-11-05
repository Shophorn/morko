//using UnityEngine;

//namespace Morko
//{
//	public class LobbyWindow : MonoBehaviour
//	{
//		public HostRoomContainer hostRooms;
//		public GameObject playerListContainer;
//		public PlayerListItem playerListItem;
//		public InfiniteScroll characterSelectionList;

//		public int selectedRoomId;

//		private void OnEnable()
//		{
//			CreatePlayerList();
//		}
//		private void OnDisable()
//		{
//			ClearPlayerList();
//		}

//		public void CreatePlayerList()
//		{
//			int iteration = 0;
//			foreach(Player player in hostRooms.rooms[selectedRoomId].Players)
//			{

//				PlayerListItem playerInstance = Instantiate(playerListItem, playerListContainer.transform.position - (new Vector3(0,35,0)*iteration), Quaternion.identity);
//				playerInstance.transform.SetParent(playerListContainer.transform);
//				playerInstance.playerName.text = player.Name;
//				playerInstance.characterName.text = player.Character;
//				playerInstance.id = player.ID;
//				iteration++;
//			}
//		}

//		public void ClearPlayerList()
//		{
//			foreach (Transform child in playerListContainer.transform)
//				GameObject.Destroy(child.gameObject);
//		}
//	}
//}