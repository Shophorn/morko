using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public UIController uiController;
	public NetworkController netController;

	public void Awake()
	{
		uiController.OnRequestJoin += (info) =>
		{
			Debug.Log($"Joined server as {info.playerName}");
		};

		uiController.OnStartHosting += netController.CreateServer;

		uiController.OnEnterJoinWindow += netController.StartListenBroadcast;
		uiController.OnExitJoinWindow += netController.StopListenBroadcast;

		// ---------------------------------------------------------

		netController.OnServersUpdated += () => 
		{
			Debug.Log("OnServersUpdated called");
			uiController.SetServerList(netController.GetServerList());
		};


	}
}