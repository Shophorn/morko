using UnityEngine;
public static class AvatarInstantiator
{
    public static GameObject[] Instantiate(int[] avatarModelIds, string folderName = "AvatarPrefabs")
    {
        int count = avatarModelIds.Length;
        
        var prefabs = Resources.LoadAll<GameObject>(folderName);
        var results = new GameObject[count];

        for (int avatarIndex = 0; avatarIndex < count; avatarIndex++)
            results[avatarIndex] = MonoBehaviour.Instantiate(prefabs[avatarIndex]);
        
        return results;
    }
}

// Note(Leo): Maybe we need to it like this anyway, because we need to instantiate characters in multiple places
public class AvatarInstantiator2
{
	private GameObject [] prefabs;

	private AvatarInstantiator2 (){}

	public static AvatarInstantiator2 Load(string folderName)
	{
		var instantiator = new AvatarInstantiator2
		{
	        prefabs = Resources.LoadAll<GameObject>(folderName)
		};
		return instantiator;
	}

	public GameObject InstantiateOne (int avatarIndex)
	{
		return MonoBehaviour.Instantiate(prefabs[avatarIndex]);
	}

    public GameObject[] InstantiateMany(int[] avatarModelIds)
    {
        int count = avatarModelIds.Length;
        var results = new GameObject[count];

        for (int avatarIndex = 0; avatarIndex < count; avatarIndex++)
            results[avatarIndex] = MonoBehaviour.Instantiate(prefabs[avatarIndex]);
        
        return results;
    }
}
