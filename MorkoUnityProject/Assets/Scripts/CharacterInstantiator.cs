using UnityEngine;

[CreateAssetMenu(fileName = "CharacterInstantiator")]
public class CharacterInstantiator : ScriptableObject
{
    private static GameObject[] characterPrefabs;

    private void OnEnable()
    {
        characterPrefabs = Resources.LoadAll<GameObject>("CharacterPrefabs");
    }

    public GameObject InstantiateCharacter(int id, Vector3 position)
    {
        GameObject instantiatedCharacter = Instantiate(characterPrefabs[id], position, Quaternion.identity);
        return instantiatedCharacter;
    }
}
