using UnityEngine;

public class RemotePlayerController : INetworkReceiver
{
	private Interlocked<Vector3> position;
	private Character character;

	void INetworkReceiver.Receive(PlayerGameUpdatePackage package)
	{
		Debug.Log("[RECEIVER INTERFACE]: Package received");
		position.Value = package.position;
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