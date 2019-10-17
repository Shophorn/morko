using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NetworkTester2))]
public class NetworkTester2Editor : Editor
{
	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector();

		if (GUILayout.Button("Join Selected Server"))
		{
			(target as NetworkTester2).JoinSelectedServer();
		}

		if (GUILayout.Button("Start Listen"))
		{
			(target as NetworkTester2).StartListen();
		}

		if (GUILayout.Button("Stop Listen"))
		{
			(target as NetworkTester2).StopListen();
		}

		if (GUILayout.Button("Start Update"))
		{
			(target as NetworkTester2).StartUpdate();
		}

		if (GUILayout.Button("Stop Update"))
		{
			(target as NetworkTester2).StopUpdate();
		}
	}
}