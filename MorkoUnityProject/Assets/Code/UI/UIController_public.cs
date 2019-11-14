using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Morko;
using Morko.Network;

public interface IClientUIControllable
{
	void BeginJoin();
	void EndJoin();

	void OnClientReady();
	void RequestJoin(JoinInfo joinInfo);
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

	private string MapNameFromIndex(int mapIndex)
	{
		// Todo(Leo): Obviously this is not correct, please fix
		return "Somber Bomber Suburbinator";
	}

	private void SetCurrentServer(int serverIndex)
	{
		ServerInfo selectedServer = availableServers[serverIndex];

		joinView.selectedServerIndex = serverIndex;

		joinView.hostingPlayerNameText.text = selectedServer.hostingPlayerName;
		joinView.mapNameText.text = MapNameFromIndex(selectedServer.mapIndex);
		joinView.joinedPlayersCountText.text = selectedServer.maxPlayers.ToString();
		joinView.gameDurationText.text = TimeFormat.ToTimeFormat(selectedServer.gameDurationSeconds);
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

	public void UpdateJoinedPlayersList(JoinInfo info)
	{
		int index = availableServers[info.selectedServerIndex].joinedPlayers.Length;

		availableServers[info.selectedServerIndex].joinedPlayers[index] = info;
		DisplayJoinedPlayersList(availableServers[info.selectedServerIndex]);
		hostLobbyView.joinedPlayers[index].playerName.text = info.playerName;
		clientLobbyView.joinedPlayers[index].playerName.text = info.playerName;
	}

	public void DisplayJoinedPlayersList(ServerInfo server)
	{
		hostLobbyView.joinedPlayers = hostLobbyView.joinedPlayersList.InitializeJoinedPlayersList(server.maxPlayers);
		hostLobbyView.joinedPlayers[0].playerName.text = server.hostingPlayerName;
		clientLobbyView.joinedPlayers = hostLobbyView.joinedPlayersList.InitializeJoinedPlayersList(server.maxPlayers);
		clientLobbyView.joinedPlayers[0].playerName.text = server.hostingPlayerName;
	}
}