using UnityEditor;
using UnityEngine;

public class ReplaceTool
{
	private static string TrimToFolder(string folderName, string path)
	{
		string trimmedPath = path;

		char[] delims = new [] {'/', '\\'};
		while(true)
		{
			var parts = trimmedPath.Split(delims, 2);
			if (parts.Length == 1)
			{
				break;
			}


			if (parts[0] == folderName)
			{
				return trimmedPath;
			}

			trimmedPath = parts [1];
		}
		return path;
	}

	[MenuItem("Tools/Replace All Selected Objects")]
	private static void ReplaceObject()
	{
		foreach (var obj in Selection.objects)
		{
			if ((obj is GameObject) == false)
			{
				Debug.LogWarning("Only replace if all selected objects are GameObjects. No objects replaced");
				return;
			}
		}

		string path = EditorUtility.OpenFilePanel("","Assets", "prefab");
		path = TrimToFolder("Assets", path);
		GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));

		Debug.Log($"Loaded prefabs name: {prefab.name}");

		Undo.RecordObject(null, "Base Undo");
		int undoID = Undo.GetCurrentGroup();
		foreach (var obj in Selection.objects)
		{
			GameObject current = obj as GameObject;
			
			Transform parent = current.transform.parent;
			Vector3 localPosition = current.transform.localPosition;
			Quaternion localRotation = current.transform.localRotation;


			Undo.DestroyObjectImmediate(current);
			Undo.CollapseUndoOperations(undoID);
			

			GameObject newGameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
			Undo.RegisterCreatedObjectUndo(newGameObject, "Replace with new");
			Undo.CollapseUndoOperations(undoID);

			newGameObject.transform.SetParent(parent);
			newGameObject.transform.localPosition = localPosition;
			newGameObject.transform.localRotation = localRotation;



		}

		Debug.Log("Its working!!!");
	}
	
}