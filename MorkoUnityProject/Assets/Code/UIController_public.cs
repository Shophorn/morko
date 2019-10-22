using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Morko;
using Morko.Network;

/* Note(Leo): For clarity, public interface and MonoBehaviour internals
are separated. Users only must depend on this public side. */
public partial class UIController
{
	public event Action<JoinInfo> OnRequestJoin;
	public event Action<ServerInfo> OnStartHosting;
	public event Action OnStopHosting;

	public event Action OnEnterHostWindow;
	public event Action OnExitHostWindow;
	public event Action OnEnterHostLobbyWindow;
	public event Action OnExitHostLobbyWindow;
	public event Action OnEnterPlayerLobbyWindow;
	public event Action OnExitPlayerLobbyWindow;
	public event Action OnEnterJoinWindow;
	public event Action OnExitJoinWindow;
	public event Action OnStartGame;
	public event Action OnAbortGame;

	public event Action OnQuit;

	public void Show()
	{

	}

	public void Hide()
	{
		 
	}

	public void SetServerList(ServerInfo [] infos)
	{
		/*
		Todo(Joonas): Implement....

		Note(Leo): remember to keep track of selected server, as index may change
		*/

		Debug.Log("Server info updated");

		string printout = "";

		foreach (var item in infos)
		{
			printout += $"\t{item.name}\n";
		}
		Debug.Log(printout);
	}
}