using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ServerController))]
public class ServerControllerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		var tester = target as ServerController;

		EditorGUILayout.LabelField($"Status: {tester.Status}");

		if (GUILayout.Button("Start Broadcast"))
		{
			tester.StartBroadcast();
		}

		if (GUILayout.Button("Stop Broadcast"))
		{
			tester.StopBroadcast();
		}

		if (GUILayout.Button("Start Game"))
		{
			tester.StartGame();
		}

		if (GUILayout.Button("Abort Game"))
		{
			tester.AbortGame();
		}
	}
}