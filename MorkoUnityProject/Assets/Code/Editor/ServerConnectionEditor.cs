using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ServerConnection))]
public class ServerConnectionEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		var tester = target as ServerConnection;


		EditorGUILayout.LabelField($"Status: {tester.Status}");

		if (GUILayout.Button("Create Server"))
		{
			tester.CreateServer();
		}

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

		if (GUILayout.Button("Stop Game"))
		{
			tester.StopGame();
		}
	}
}