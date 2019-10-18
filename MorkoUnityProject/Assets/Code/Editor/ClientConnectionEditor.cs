using System.Linq;

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ClientConnection))]
public class ClientConnectionEditor : Editor
{
	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector();

		var connection = target as ClientConnection;


		GUILayout.Label("Player Info", EditorStyles.boldLabel);
		// Note(Leo): These do not need to be serialized, this is just a mock ui demo
		EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Player Name:", GUILayout.Width(EditorGUIUtility.labelWidth));
			connection.playerName = EditorGUILayout.TextField(connection.playerName);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Client Id:", GUILayout.Width(EditorGUIUtility.labelWidth));
			GUILayout.Label(connection.ClientId >= 0 ? connection.ClientId.ToString() : " - ");
		EditorGUILayout.EndHorizontal();


		GUILayout.Label("Connection", EditorStyles.boldLabel);
		GUILayout.Label(connection.servers.Count == 0 ?
						"No Servers Available" :
						"Servers Available ");

		var serverNames = connection.servers.Select(server => server.name).ToArray();
		connection.selectedServerIndex = EditorGUILayout.Popup("Servers", connection.selectedServerIndex, serverNames);




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