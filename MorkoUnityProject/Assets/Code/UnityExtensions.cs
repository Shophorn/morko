using UnityEngine;

public static class UnityExtensions
{
	public static void DestroyAllChildren(this Transform parent)
	{
		foreach (Transform child in parent)
			GameObject.Destroy(child.gameObject);
	}
}