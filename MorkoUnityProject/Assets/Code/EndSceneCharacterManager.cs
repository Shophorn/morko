using UnityEngine;

public class EndSceneCharacterManager : MonoBehaviour
{
	public GameObject characterPrefab;
	public float characterOffset = 3f;

	private void Start()
	{
		var endResult = GameManager.GetEndResult();

		float fullCharacterLineLength = (endResult.characterCount - 1) * characterOffset;
		Vector3 lineStartPosition = Vector3.left * (fullCharacterLineLength / 2);

		for (int i = 0; i < endResult.characterCount; i++)
		{
			Vector3 position = lineStartPosition + Vector3.right * i * characterOffset;
			if (i == endResult.winningCharacterIndex)
			{
				position += new Vector3(0, 0.5f, 0);
			}

			var character = Instantiate(characterPrefab, transform, false);
			character.transform.position = position;
			Destroy(character.GetComponent<PlayerController>());
		}
	}
}

public struct GameEndResult
{
	public int characterCount;
	public int winningCharacterIndex;
}