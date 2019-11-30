using System;
using System.Collections;
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
			Debug.LogError("Cannot set layer to null GameObject");
		}

		gameObject.layer = layer;

		foreach (Transform child in gameObject.transform)
		{
			child.gameObject.SetLayerRecursively(layer);
		}
	}

	public static void InvokeAfter(this MonoBehaviour host, Action operation, float timeToWait)
	{
		if (operation == null)
		{
			Debug.LogError("Trying to invoke null operation.");
			return;
		}

		IEnumerator Invoker()
		{
			yield return new WaitForSeconds(timeToWait);
			operation();
		}

		host.StartCoroutine(Invoker());
	}
}