// using System.Linq;

// using UnityEditor;
// using UnityEngine;

// [CustomEditor(typeof(ClientController))]
// public class ClientControllerEditor : Editor
// {
// 	public override void OnInspectorGUI ()
// 	{
// 		DrawDefaultInspector();

// 		// var connection = target as ClientController;


// 		// GUILayout.Label("Player Info", EditorStyles.boldLabel);
// 		// // Note(Leo): These do not need to be serialized, this is just a mock ui demo
// 		// EditorGUILayout.BeginHorizontal();
// 		// 	GUILayout.Label("Player Name:", GUILayout.Width(EditorGUIUtility.labelWidth));
// 		// 	connection.playerName = EditorGUILayout.TextField(connection.playerName);
// 		// EditorGUILayout.EndHorizontal();

// 		// EditorGUILayout.BeginHorizontal();
// 		// 	GUILayout.Label("Client Id:", GUILayout.Width(EditorGUIUtility.labelWidth));
// 		// 	GUILayout.Label(connection.ClientId >= 0 ? connection.ClientId.ToString() : " - ");
// 		// EditorGUILayout.EndHorizontal();


// 		// GUILayout.Label("Connection", EditorStyles.boldLabel);
// 		// GUILayout.Label(connection.servers.Count == 0 ?
// 		// 				"No Servers Available" :
// 		// 				"Servers Available ");

// 		// var serverNames = connection.servers.Select(server => server.serverInfo.serverName).ToArray();
// 		// connection.selectedServerIndex = EditorGUILayout.Popup("Servers", connection.selectedServerIndex, serverNames);
// 	}
// }