using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ClientConnection))]
public class ClientConnectionEditor : Editor
{
	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector();

		if (GUILayout.Button("Join Selected Server"))
		{
			(target as ClientConnection).JoinSelectedServer();
		}

		if (GUILayout.Button("Start Listen"))
		{
			(target as ClientConnection).StartListen();
		}

		if (GUILayout.Button("Stop Listen"))
		{
			(target as ClientConnection).StopListen();
		}

		if (GUILayout.Button("Start Update"))
		{
			(target as ClientConnection).StartUpdate();
		}

		if (GUILayout.Button("Stop Update"))
		{
			(target as ClientConnection).StopUpdate();
		}
	}
}