using System.Reflection;
using UnityEngine;

public static class UnityExtensions
{
	public static void DestroyAllChildren(this Transform parent)
	{
		foreach (Transform child in parent)
			GameObject.Destroy(child.gameObject);
	}

	public static void MakeMonoBehaviourSingleton<T>(this T instance) where T : MonoBehaviour
	{
		var fieldInfo = typeof(T).GetField(
							"instance", 
							BindingFlags.NonPublic | BindingFlags.Static);

		if (fieldInfo == null)
		{
			Debug.LogError($"Cannot make instance of type {typeof(T)} singleton, because it does not have private static member 'instance' of type {typeof(T)}.");
		}


		T value = (T)fieldInfo.GetValue(instance);
		if (value == null)
		{
			fieldInfo.SetValue(null, instance);
		}
		else if (value != instance)
		{
			MonoBehaviour.Destroy(instance);
		}
	}

	public static void SetLayerRecursively(this GameObject gameObject, int layer)
	{
		if (gameObject == null)
		{
			Debug.LogError("Cannot set layer of 'null' GameObject");
		}

		gameObject.layer = layer;

		foreach (Transform child in gameObject.transform)
		{
			child.gameObject.SetLayerRecursively(layer);
		}
	}
}