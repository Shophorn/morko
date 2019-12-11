using TMPro;
using UnityEngine;

public class EndSceneCharacterManager : MonoBehaviour
{
	public float characterOffset = 3f;
	public GameObject namePrefab;
	public Vector3 nameOffset = new Vector3(0f, 2.5f, 0f);

	private void Start()
	{
		var endResult = GameManager.GetEndResult();
		bool winner = false;

		float fullCharacterLineLength = (endResult.characterCount - 1) * characterOffset;
		Vector3 lineStartPosition = Vector3.left * (fullCharacterLineLength / 2);

		for (int i = 0; i < endResult.characterCount; i++)
		{
			Vector3 position = lineStartPosition + Vector3.right * i * characterOffset;
			if (i == endResult.winningCharacterIndex)
			{
				position += new Vector3(0, 0.5f, -3f);
				winner = true;
			}
			else
				winner = false;

			int avatarIndex = endResult.playerAvatarIds[i];
			var character = Instantiate(GameManager.GetCharacterPrefabs[avatarIndex], transform, false);
			var name = Instantiate(namePrefab, character.transform, false);
			
			name.GetComponent<TextMeshPro>().text = "Sampo";
			
			character.GetComponent<Character>().EnableFlashlight(false);
			if (winner)
				character.GetComponent<Animator>().SetTrigger("Win");
			else
				character.GetComponent<Animator>().SetTrigger("Lose");
			
			character.transform.position = position;
			name.transform.position = nameOffset;

			Destroy(character.GetComponent<PlayerController>());
		}
	}
}

public struct GameEndResult
{
	public int characterCount;
	public int winningCharacterIndex;
	public int [] playerAvatarIds;
}