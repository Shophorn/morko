using System.Runtime.InteropServices;
using UnityEngine;

using Morko.Network;

public class ServerConnection : MonoBehaviour
{
	public enum StatusType {Idle, Broadcasting, RunningGame};
	public StatusType Status { get; private set; }

	public ServerStartInfo serverInfo = new ServerStartInfo();
	private Server server;

	public string [] players;

	public bool AutoStart { get; set; }

	private void Start()
	{
		if (AutoStart)
		{
			CreateServer();
			StartBroadcast();
		}
	}

	public void CreateServer()
	{
		serverInfo.logFunction = Debug.Log;
		serverInfo.playerUpdatePackageSize = Marshal.SizeOf(default(PlayerGameUpdatePackage));

		server = Server.Create(serverInfo);
		server.OnPlayerAdded += () => players = server.PlayersNames;
	}

	public void StartBroadcast()
	{
		server.StartBroadcasting();
		Status = StatusType.Broadcasting;
	}

	public void StopBroadcast()
	{
		server.StopBroadcasting();
		Status = StatusType.Idle;
	}

	public void StartGame()
	{
		server.StartGame();
		Status = StatusType.RunningGame;
	}

	public void StopGame()
	{
		server.StopGame();
		Status = StatusType.Idle;
	}

	public void OnDisable()
	{
		server?.StopBroadcasting();
		server?.StopGame();
		server?.Close();
	}

}