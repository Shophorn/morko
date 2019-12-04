using System.Linq;
using UnityEngine;

public class LocalCameraController : MonoBehaviour
{
	public Transform target;
	public Vector3 offset;
	public float distance;
	public float defaultAngle;
	public float maxAngle = 90;
	[Range(0, 0.3f)] public float angleIncrement;
	public string[] tagsThatTriggerCameraPositionChange;

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
		newCameraPosition = GetNewCameraPositionIfCharacterNotVisible(newCameraPosition, defaultAngle, maxAngle);

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

	private Vector3 GetNewCameraPositionIfCharacterNotVisible(Vector3 currentPosition, float minAngle, float maxAngle)
	{
		Vector3 pointUnderDefault = new Vector3(currentPosition.x, currentPosition.y - offsetStart, currentPosition.z - offsetStart);
		Vector3 pointUnderTarget = targetWithOffset - new Vector3(0, offsetEnd, 0);

		bool targetVisibleFromDefaultPosition = IsTargetVisible(currentPosition, targetWithOffset, tagsThatTriggerCameraPositionChange);
		bool targetVisibleFromCheckUpRay = IsTargetVisible(pointUnderDefault, pointUnderTarget, tagsThatTriggerCameraPositionChange);

		if (targetVisibleFromCheckUpRay)
			currentAngle -= angleIncrement;
		else if (!targetVisibleFromDefaultPosition)
			currentAngle += angleIncrement;

		currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);
		Vector3 newPosition = GetCameraPositionWithDistanceAndAngle(targetWithOffset, distance, currentAngle);

		return newPosition;
	}

	private bool IsTargetVisible(Vector3 from, Vector3 target, string[] hitTags)
	{
		var direction = target - from;
		var distance = Vector3.Distance(from, target);
		
		RaycastHit[] hits;
		hits = Physics.RaycastAll(from, direction, distance);

		foreach (var hit in hits)
			if (hitTags.Contains(hit.collider.tag))
				return false;
		
		return true;
	}
}
