using System; 

[Serializable]
public class ServerInfo
{
	public string serverName;
	public string hostingPlayerName;
	public int mapIndex;
	public int maxPlayers;
	public JoinInfo[] joinedPlayers;
	public int gameDurationSeconds;
}