using UnityEngine;

public class LocalCameraController : MonoBehaviour
{
	public Transform target;
	public float distance = 10;
	public float angle = 45;

	private void LateUpdate()
	{
		float y = Mathf.Sin(angle) * distance;
		float z = -Mathf.Cos(angle) * distance;

		Vector3 localPosition = new Vector3(0, y, z);
		Vector3 position = target.position + localPosition;
		transform.position = position;

		transform.LookAt(target.position);
	}
}