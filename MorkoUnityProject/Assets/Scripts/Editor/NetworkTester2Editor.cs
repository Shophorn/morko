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
	}
}