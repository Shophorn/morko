using UnityEngine;
using UnityEngine.UI;

public class JoinedPlayersList : MonoBehaviour
{
	public JoinedPlayer joinedPlayersPrefab;

	public JoinedPlayer[] joinedPlayers;

	public JoinedPlayer[] InitializeJoinedPlayersList(int playerCount)
	{
		joinedPlayers = new JoinedPlayer[playerCount];

		ClearJoinedPlayersList();
		for (int i = 0; i < playerCount; i++)
		{
			joinedPlayers[i]= Instantiate(joinedPlayersPrefab, transform);
			joinedPlayers[i].transform.SetParent(transform);
			
			joinedPlayers[i].playerName.text = "Vacant spot";
			joinedPlayers[i].characterName.text = " - ";
		}
		return joinedPlayers;
	}
	private void ClearJoinedPlayersList()
	{
		transform.DestroyAllChildren();
	}
}
