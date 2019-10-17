using UnityEngine;

using Morko.Network;

public class ServerTester : MonoBehaviour
{
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
	}

	public void StopBroadcast()
	{
		server.StopBroadcasting();
	}

	public void StartGame()
	{
		server.StartGame();
	}

	public void StopGame()
	{
		server.StopGame();
	}

	public void OnDisable()
	{
		server?.StopBroadcasting();
		server?.StopGame();
		server?.Close();
	}

}