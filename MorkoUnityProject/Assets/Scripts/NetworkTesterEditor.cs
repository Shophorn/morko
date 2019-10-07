// using UnityEditor;
// using UnityEngine;

// [CustomEditor(typeof(NetworkTester))]
// public class NetworkTesterEditor : Editor
// {
// 	public override void OnInspectorGUI()
// 	{
// 		DrawDefaultInspector();

// 		var tester = target as NetworkTester;

// 		if (tester.IsSearchingServers)
// 		{
// 			if (GUILayout.Button("Stop Serching Servers"))
// 			{
// 				tester.StopSearchingServers();
// 			}
// 		}
// 		else
// 		{
// 			if (GUILayout.Button("Start Searching Servers"))
// 			{
// 				tester.StartSearchingServers();
// 			}
// 		}

// 		if (GUILayout.Button("Join selected server"))
// 		{
// 			tester.JoinSelectedServer(tester.selectedServerIndex);
// 		}

// 		if (GUILayout.Button("Confirm join"))
// 		{
// 			tester.ConfirmJoin();
// 		}

// 		if (GUILayout.Button("Start Game"))
// 		{
// 			tester.StartGame();
// 		}
// 	}
// }