using UnityEngine;

public class RemotePlayerController
{
	private Interlocked<Vector3> position;
	private Character character;

	// This is called from other threads
	public void SetPosition(Vector3 position)
	{
		Debug.Log("[REMOTE CONTROLLER]: Position set");
		this.position.Value = position;
	}
	
	public void Update()
	{
		// Todo(Leo): Interpolate.
		// Todo(Leo): Actual movement
		character.transform.position = position.Value;
	}

	public static RemotePlayerController Create(Character character)
	{
		RemotePlayerController controller = new RemotePlayerController();
		controller.character = character;
		controller.position = new Interlocked<Vector3>();
		return controller;
	}
}