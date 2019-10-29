using System;
using System.Collections.Generic;
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
	void OnRequestJoin(JoinInfo joinInfo);
}

public interface IServerUIControllable
{
	void CreateServer(ServerInfo serverInfo);
	void DestroyServer();

	// Note(Leo): These are called when hosting player starts or stops game
	void StartGame();
	void AbortGame();
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

	public event Action OnQuit;

	public void Show()
	{
		mainGameObject.SetActive(true);	
	}

	public void Hide()
	{
		mainGameObject.SetActive(false);	
	}

	private string MapNameFromIndex(int mapIndex)
	{
		// Todo(Leo): Obviously this is not correct, please fix
		return ServerNameGenerator.GetRandom();
	}

	public void SetServerList(ServerInfo [] infos)
	{
		/* Todo(Leo): keep track of selected server, as index is likely to change
	 	For example, get current selected servers name, and in the end find if it 
		is in new ones, and set it as active */
		
		Debug.Log("Server info updated");
		joinView.availableServersToggleParent.DestroyAllChildren();

		int serverCount = infos.Length;
		int toggleHeight = 20;
		for (int serverIndex = 0; serverIndex < serverCount; serverIndex++)
		{
			/* Note(Leo): this is done because in c# for loop keeps index as
			reference (or something), so it would keep increasing, and any calls
			after loop would point to value of last iteration (aka count - 1) */
			int selectedIndex = serverIndex;

			var toggleInstance = Instantiate(
									joinView.availableServersTogglePrefab,
									joinView.availableServersToggleParent);

			float yPosition = toggleHeight * selectedIndex;
			toggleInstance.transform.localPosition = new Vector3(0, yPosition, 0);

			toggleInstance.label.text = infos[selectedIndex].serverName;
			toggleInstance.toggle.group = joinView.availableServersToggleGroup;

			/* Note(Leo): Unity documentation on Toggle.onValueChanged was unclear
			about what does the bool argument represent, so it is ignored here. */
			void SetSelectedIndex (bool ignored)
			{
				if (toggleInstance.toggle.isOn)
				{
					joinView.selectedServerIndex = selectedIndex;

					joinView.hostingPlayerNameText.text = "Hosting Player";
					joinView.mapNameText.text = MapNameFromIndex(infos[selectedIndex].mapIndex);
					joinView.joinedPlayersCountText.text = infos[selectedIndex].maxPlayers.ToString(); 
					joinView.gameDurationText.text = TimeFormat.ToTimeFormat(infos[selectedIndex].gameDurationSeconds);
				}
			}

			toggleInstance.toggle.onValueChanged.AddListener(SetSelectedIndex);
		}
	}
}