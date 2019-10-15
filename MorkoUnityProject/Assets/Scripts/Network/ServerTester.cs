using UnityEngine;

using Morko.Network;

public class ServerTester : MonoBehaviour
{
	public enum StatusType {Idle, Broadcasting, RunningGame};
	public StatusType Status { get; private set; }

	public ServerStartInfo serverInfo;
	private Server server;

	public string [] players;

	public void CreateServer()
	{
		serverInfo.logFunction = Debug.Log;
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