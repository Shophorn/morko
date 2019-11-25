using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Photon.Realtime;

public interface IClientUIControllable
{
	void RequestJoin(JoinInfo joinInfo);
	void OnPlayerReady();
}

public interface IServerUIControllable
{
	void CreateServer(ServerInfo serverInfo);
	void DestroyServer();

	// Note(Leo): These are called when hosting player starts or stops game
	void StartGame();
	void AbortGame();
}

public interface IAppUIControllable
{
	void Quit();
}

public class JoinInfo 
{
	public string playerName;
	public int selectedServerIndex;
	public RoomInfo selectedRoomInfo;
}

/* Note(Leo): For clarity, public interface and MonoBehaviour internals
are separated. Users only must depend on this public side. */
public partial class UIController
{
	public event Action OnExitHostWindow;
	public event Action OnEnterHostLobbyWindow;
	public event Action OnExitHostLobbyWindow;
	public event Action OnEnterPlayerLobbyWindow;
	public event Action OnExitPlayerLobbyWindow;
	public event Action OnStartGame;
	public event Action OnAbortGame;

	public void Show()
	{
		uiMainGameObject.SetActive(true);
	}

	public void Hide()
	{
		uiMainGameObject.SetActive(false);	
	}

	private ServerInfo [] availableServers;
	private List<RoomInfo> availableRooms;

	private string MapNameFromIndex(int mapIndex)
	{
		// Todo(Leo): Obviously this is not correct, please fix
		return "Somber Bomber Suburbinator";
	}

	public void AddPlayer(int uniqueId, string name, PlayerNetworkStatus status)
		=> roomView.playerNameList.AddPlayer(uniqueId, name, status);

	public void RemovePlayer(int uniqueId)
		=> roomView.playerNameList.RemovePlayer(uniqueId);

	public void UpdatePlayerNetworkStatus(int uniqueId, PlayerNetworkStatus status)
		=> roomView.playerNameList.SetStatus(uniqueId, status);

	private void SetCurrentServer(int serverIndex)
	{
		ServerInfo selectedServer = availableServers[serverIndex];

		joinView.selectedServerIndex = serverIndex;

		joinView.hostingPlayerNameText.text = selectedServer.hostingPlayerName;
		joinView.mapNameText.text = MapNameFromIndex(selectedServer.mapIndex);
		joinView.joinedPlayersCountText.text = selectedServer.maxPlayers.ToString();
		joinView.gameDurationText.text = TimeFormat.ToTimeFormat(selectedServer.gameDurationSeconds);
	}

	public void SetRooms(List<RoomInfo> rooms)
	{
		availableRooms = rooms;

		string[] names = rooms.Select(room => room.Name).ToArray();
		joinView.availableServersSelector.SetOptions(names);
	}

	public void SetServerList(ServerInfo [] servers)
	{
		availableServers = servers;

		string[] names = servers.Select(info => info.serverName).ToArray();
		joinView.availableServersSelector.OnServerListUpdated += SetServerListNavigation;
		joinView.availableServersSelector.SetOptions(names);
		joinView.availableServersSelector.OnSelectionChanged += SetCurrentServer;
		joinView.availableServersSelector.OnSelectionChanged?.Invoke(0);
	}
}