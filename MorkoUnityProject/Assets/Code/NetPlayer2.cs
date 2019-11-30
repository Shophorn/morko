using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class NetPlayer2 : MonoBehaviourPunCallbacks
{
	// private void Start()
	// {
	// 	PhotonNetwork.ConnectUsingSettings();
	// }

	// public override void OnDisconnected (DisconnectCause cause)
	// {
	// 	Debug.Log($"[PHOTON] Disconnect {cause}");
	// }

	// public override void OnConnectedToMaster()
	// {
	// 	Debug.Log("[PHOTON] Connect");
	// 	PhotonNetwork.JoinLobby();
	// 	connected = true;
	// }

	// public override void OnRoomListUpdate(List<RoomInfo> rooms)
	// {
	// 	Debug.Log("[PHOTON] Roomlist updated");
	// 	availableRooms = rooms;
	// }

	private bool connected = false;
	private bool joinedRoom = false;

	private string roomName = "Room Name";
	private List<RoomInfo> availableRooms = new List<RoomInfo>();

	public GameObject playerPrefab;

	// public void OnGUI()
	// {
	// 	if (connected == false)
	// 	{
	// 		GUI.Label(new Rect(20, 20, 200, 20), "Connecting");
	// 	}
	// 	else if (joinedRoom == false)
	// 	{
	// 		Debug.Log(PhotonNetwork.IsConnected);

	// 		roomName = GUI.TextField(new Rect(20, 20, 200, 20), roomName);
	// 		if (GUI.Button(new Rect(330, 20, 100, 20), "Create"))
	// 		{
	// 			CreateRoom();			
	// 		}

	// 		for (int roomIndex = 0; roomIndex < availableRooms.Count; roomIndex++)
	// 		{
	// 			if (GUI.Button(new Rect(20, 60 + roomIndex * 25, 100, 20), availableRooms[roomIndex].Name))
	// 			{
	// 				PhotonNetwork.JoinRoom(availableRooms[roomIndex].Name);
	// 			}
	// 		}
	// 	}

	// 	else if (connected && joinedRoom)
	// 	{
	// 		GUI.Label(new Rect(20, 20, 200, 20), "In Room");
	// 	}
	// }

	// public void CreateRoom()
	// {
	// 	Debug.Log($"Creating room {roomName}");
	// 	PhotonNetwork.CreateRoom(roomName);
	// }

	// public void JoinRoom()
	// {
	// 	Debug.Log("Joining room");
	// }

	// public override void OnJoinedRoom()
	// {
	// 	Debug.Log("Joined room");
	// 	joinedRoom = true;

	// 	PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity);
	// }
}
