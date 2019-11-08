using UnityEngine;

public class RemotePlayerController : INetworkReceiver
{
	private Interlocked<Vector3> position;
	private Interlocked<float> rotation;

	private Character character;

	void INetworkReceiver.Receive(PlayerGameUpdatePackage package)
	{
		Debug.Log($"package Received, position {package.position}");

		position.Value = package.position;
		rotation.Value = package.rotation;
	}

	public void Update()
	{
		// Todo(Leo): Interpolate.
		// Todo(Leo): Actual movement
		character.transform.position = position.Value;
		character.transform.rotation = Quaternion.AngleAxis(rotation.Value, Vector3.up);
	}

	public static RemotePlayerController Create(Character character)
	{
		RemotePlayerController controller = new RemotePlayerController();
		controller.character = character;

		controller.position = new Interlocked<Vector3>();
		controller.rotation = new Interlocked<float>();

		return controller;
	}
}