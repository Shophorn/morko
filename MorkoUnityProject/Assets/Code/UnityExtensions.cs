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
}