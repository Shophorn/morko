using UnityEngine;

public class LocalCameraController : MonoBehaviour
{
	public Transform target;
	public Vector3 offset;
	public float distance;
	public float defaultAngle;
	public float maxAngle = 90;
	[Range(0, 0.3f)] public float angleIncrement;

	private Vector3 targetWithOffset => target.position + offset;
	private float currentAngle;

	private Vector3 defaultCameraPosition;
	private Vector3 newCameraPosition;

	private float offsetEnd = 0.4f;
	private float offsetStart = 0.2f;

	private void Start()
	{
		currentAngle = defaultAngle;
		defaultCameraPosition = GetCameraPositionWithDistanceAndAngle(targetWithOffset, distance, defaultAngle);
		transform.position = defaultCameraPosition;
	}

	private void LateUpdate()
	{
		defaultCameraPosition = GetCameraPositionWithDistanceAndAngle(targetWithOffset, distance, defaultAngle);
		newCameraPosition = MoveCameraIfCharacterNotVisible(newCameraPosition, defaultAngle, maxAngle);

		transform.position = newCameraPosition;
		transform.LookAt(targetWithOffset);
	}

	private Vector3 GetCameraPositionWithDistanceAndAngle(Vector3 from, float distance, float angle)
	{
		var radians = angle * Mathf.Deg2Rad;
		var z = distance * Mathf.Cos(radians);
		var y = distance * Mathf.Sin(radians);
		var localPosition = new Vector3(0, y, -z);

		var position = from + localPosition;

		return position;
	}

	private Vector3 MoveCameraIfCharacterNotVisible(Vector3 currentPosition, float minAngle, float maxAngle)
	{
		Vector3 pointUnderDefault = new Vector3(currentPosition.x, currentPosition.y - offsetStart, currentPosition.z - offsetStart);
		Vector3 pointUnderTarget = targetWithOffset - new Vector3(0, offsetEnd, 0);

		bool targetVisibleFromDefault = IsTargetVisible(currentPosition, targetWithOffset);
		bool targetVisibleFromCheckUpRay = IsTargetVisible(pointUnderDefault, pointUnderTarget);

		if (targetVisibleFromCheckUpRay)
			currentAngle -= angleIncrement;
		else if (!targetVisibleFromDefault)
			currentAngle += angleIncrement;

		currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);
		Vector3 newPosition = GetCameraPositionWithDistanceAndAngle(targetWithOffset, distance, currentAngle);

		return newPosition;
	}

	private bool IsTargetVisible(Vector3 from, Vector3 target, string hitTag = "Wall")
	{
		var forward = target - from;
		RaycastHit hit;

		if (Physics.Raycast(from, forward, out hit))
			if (hit.collider.gameObject.CompareTag(hitTag))
				return false;

		return true;
	}
}
