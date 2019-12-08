using System.Linq;
using UnityEngine;
using Photon.Pun;

public struct PositionAndRotation
{
	public Vector3 position;
	public float rotation;
}

public class SpawnPoint : MonoBehaviour
{
	[UnityEditor.MenuItem("Morko/Spawn Point/Confirm Spawn Point")]
	private static void ConfrimSpawnPointInScene()
	{
		if (GetInstance() != null)
		{
			Debug.Log("[SPAWN POINT]: Found!");
		}
		else
		{
			Debug.LogWarning("[SPAWN POINT]: Not found! If this is a map scene, you should add SpawnPoint component to a GameObject in scene.");
		}
	}


	public System.Collections.Generic.List<Vector3> yieldedPositions = new System.Collections.Generic.List<Vector3>();
	public float radius;

	private static SpawnPoint _instance;
	private static SpawnPoint GetInstance()
	{
		if (_instance == null)
			_instance = FindObjectOfType<SpawnPoint>();

		if (_instance == null)
			Debug.LogError("[SPAWN POINT]: No SpawnPoint in scene!"); 

		return _instance;
	}

	public static PositionAndRotation GetCharacterPlacement(int photonActorNumber)
		=> GetInstance().ImplGetCharacterSpawnPosition(photonActorNumber);

	private PositionAndRotation ImplGetCharacterSpawnPosition(int photonActorNumber)
	{
		if (GetActorIndexInPhotonRoom(photonActorNumber, out int index, out int playerCount))
		{
			float angle = index * (360f / playerCount);
			var localPosition = new Vector3
			{
				x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius,
				z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius
			};

			return new PositionAndRotation
			{
				position = transform.position + localPosition,
				rotation = (angle + 180f) % 360f
			};
		}

		return new PositionAndRotation();	
	}

	public static PositionAndRotation GetMaskPlacement()
	{
		var result = new PositionAndRotation
		{
			position = GetInstance().transform.position,
			rotation = 0,
		};
		return result;
	}

	private static bool GetActorIndexInPhotonRoom(int actorNumber, out int index, out int playerCount)
	{
		playerCount = PhotonNetwork.CurrentRoom.PlayerCount;

		var sortedPlayers = PhotonNetwork.CurrentRoom.Players.OrderBy(entry => entry.Key);
		int runningIndex = 0;
		foreach (var entry in sortedPlayers)
		{
			if (entry.Value.ActorNumber == actorNumber)
			{
				index = runningIndex;
				return true;
			}	
			runningIndex++;
		}
		index = -1;
		return false;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = new Color(1, 0, 0.7f, 0.5f);
		Gizmos.DrawSphere(transform.position, radius + 0.5f);
	}
}