using System;
using UnityEngine;
using UnityEngine.UI;

using Morko;

public class JoinInfo
{
	public string playerName;
	public int selectedServerIndex;
}

public class HostInfo
{
	public string serverName;
}

/* Note(Leo): For clarity, public interface and MonoBehaviour internals
are separated. Users only must depend on this public side. */
public partial class UIController
{
	public event Action<JoinInfo> OnRequestJoin;
	public event Action<HostInfo> OnStartHosting;
	public event Action OnEnterJoinWindow;
	public event Action OnExitJoinWindow;
	public event Action OnStartGame;
	public event Action OnAbortGame; 

	public void SetServerList(ServerInfo [] infos)
	{
		/*
		Todo(Joonas): Implement....
		Note(Leo): remember to keep track of selected server, as index may change
		*/
		Debug.Log("Server info updated");
	}
}

public partial class UIController : MonoBehaviour
{
	[SerializeField] private WindowLoadingSystem windowSystem;
	[SerializeField] private JoinRoomWindow joinRoomWindow;
	[SerializeField] private HostRoomContainer hostRoomContainer;

	[SerializeField] private Button mainMenuJoinWindowButton;
	// TODO(Leo): Not most optimal solution
	[SerializeField] private Button joinViewCancelButton;

	private void Start()
	{
		joinRoomWindow.requestJoinButton.onClick.AddListener (() => 
		{
			var info = new JoinInfo
			{
				playerName = joinRoomWindow.PlayerName,
				selectedServerIndex = joinRoomWindow.SelectedServerId
			};
			OnRequestJoin?.Invoke(info);
			OnExitJoinWindow?.Invoke();
		});

		hostRoomContainer.hostButton.onClick.AddListener(() =>
		{
			var info = new HostInfo
			{
				serverName = hostRoomContainer.ServerName
			};
			OnStartHosting(info);
		});

		mainMenuJoinWindowButton.onClick.AddListener(() => 
		{
			OnEnterJoinWindow?.Invoke();
		});

		joinViewCancelButton.onClick.AddListener(() =>
		{
			OnExitJoinWindow?.Invoke();
		});

	}
}