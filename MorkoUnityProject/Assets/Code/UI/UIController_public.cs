using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Photon.Realtime;

public class RoomCreateInfo
{
	public string roomName;
	public string hostingPlayerName;
	public int mapIndex;
	public int maxPlayers;
	// public JoinInfo[] joinedPlayers;
	public int gameDurationSeconds;
}

public interface IClientUIControllable
{
	void RequestJoin(JoinInfo joinInfo);
	void OnPlayerReady();
}

public interface IServerUIControllable
{
	void CreateRoom(RoomCreateInfo createInfo);
	void StartGame();
}

public interface IAppUIControllable
{
	void ExitMatch();
	void Quit();
}

public class JoinInfo 
{
	public string playerName;
	public int selectedRoomIndex;
	public RoomInfo selectedRoomInfo;
}

/* Note(Leo): For clarity, public interface and MonoBehaviour internals
are separated. Users only must depend on this public side. */
public partial class UIController
{
	public void Show()
	{
		ToggleNotPauseMenu(forceActive: false);
		
		background.SetActive(true);
		hidden = false;

		Debug.Log("[UI]: Shown");
	}

	public void Hide()
	{
		SetView(null);
		connectingScreen.SetActive(false);
		loadingScreen.SetActive(false);
		background.SetActive(false);
		hidden = true;
	}

	private List<RoomInfo> availableRooms;

	private string MapNameFromIndex(int mapIndex)
	{
		Debug.LogError("MapNameFromIndex not properly implemented!!!");

		// Todo(Leo): Obviously this is not correct, please fix
		return "Somber Bomber Suburbinator";
	}

	public void AddPlayer(int uniqueId, string name, PlayerNetworkStatus status)
		=> roomView.playerNameList.AddPlayer(uniqueId, name, status);

	public void RemovePlayer(int uniqueId)
		=> roomView.playerNameList.RemovePlayer(uniqueId);

	public void UpdatePlayerNetworkStatus(int uniqueId, PlayerNetworkStatus status)
		=> roomView.playerNameList.SetStatus(uniqueId, status);

	public void SetRooms(List<RoomInfo> rooms)
	{
		availableRooms = rooms;

		string[] names = rooms.Select(room => room.Name).ToArray();
		joinView.availableServersSelector.SetOptions(names);
	}
}