using UnityEngine;

public class RemoteNetworkPlayerController : MonoBehaviour
{
	private Vector3 position;

	public void SetPosition(Vector3 position)
	{
		Debug.Log("[REMOTE CONTROLLER]: Position set");

		// Todo(Leo): Interpolate.
		this.position = position;
	}

	private void Update()
	{
		// Todo(Leo): Actual movement
		transform.position = position;
	}
}