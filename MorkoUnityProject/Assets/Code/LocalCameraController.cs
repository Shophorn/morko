using System;
using UnityEngine;

public class LocalCameraController : MonoBehaviour
{
	public Transform target;
	public Vector3 cameraDefaultPosition = new Vector3(0, 10, -10);
	public float rotateCameraSpeed = 3;
	public float pullCameraBackSpeed = 15;

	[SerializeField]
	private float minCameraXRotation;
	public float maxCameraXRotation = 90f;

	private Vector3 localPosition;
	private Vector3 defaultPosition;
	private Vector3 previousPosition;
	private void Start()
	{
		localPosition = new Vector3(cameraDefaultPosition.x, cameraDefaultPosition.y, cameraDefaultPosition.z);
		defaultPosition = target.position + localPosition;
		previousPosition = defaultPosition;
		minCameraXRotation = Vector3.Angle(target.position - defaultPosition, Vector3.forward);
	}

	private void LateUpdate()
	{
		transform.position = previousPosition;

		localPosition = new Vector3(cameraDefaultPosition.x, cameraDefaultPosition.y, cameraDefaultPosition.z);
		defaultPosition = target.position + localPosition;
		minCameraXRotation = Vector3.Angle(target.position - defaultPosition, Vector3.forward);
		
		var cameraPos = MoveCameraIfPlayerNotVisible(defaultPosition, minCameraXRotation, maxCameraXRotation);
		transform.position = cameraPos;
		transform.position = new Vector3(target.position.x, transform.position.y, transform.position.z);
		previousPosition = transform.position;
		
		transform.LookAt(target.position);
	}

	private Vector3 MoveCameraIfPlayerNotVisible(Vector3 defaultPosition, float minRotation, float maxRotation)
	{
		var defaultForward = target.position - defaultPosition;
		RaycastHit hit;

		if(Physics.Raycast(defaultPosition, defaultForward, out hit)) {

			if (hit.collider.gameObject.CompareTag("Wall"))
			{
				int rotateAmount = 0;
				bool positiveAngleFound = false;
				bool negativeAngleFound = false;
				var currentRotation = transform.eulerAngles.x;
				
				for (int angle = 0; currentRotation + angle <= maxRotation; angle++)
				{
					Vector3 positive = Quaternion.AngleAxis(angle, Vector3.right) * (transform.position - target.position);
					Vector3 negative = Quaternion.AngleAxis(-angle, Vector3.right) * (transform.position - target.position);
					
					Vector3 positionPositive = target.position + positive;
					Vector3 positionNegative = target.position + negative;
					
					var directionPositive = target.position - positionPositive;
					var directionNegative = target.position - positionNegative;
					
					if (((currentRotation - angle) > minRotation) && Physics.Raycast(positionNegative, directionNegative, out hit))
					{
						if (!hit.collider.gameObject.CompareTag("Wall"))
						{
							rotateAmount = -angle;
							negativeAngleFound = true;
						}
					}
					
					if (!negativeAngleFound && !positiveAngleFound && Physics.Raycast(positionPositive, directionPositive, out hit))
					{
						if (!hit.collider.gameObject.CompareTag("Wall"))
						{
							rotateAmount = angle;
							positiveAngleFound = true;
						}
					}
				}

				transform.RotateAround(target.position, Vector3.right, rotateAmount * rotateCameraSpeed * Time.deltaTime);
				return transform.position;
			}
		}
		transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(minRotation, 0f, 0f), rotateCameraSpeed * Time.deltaTime);
		transform.position = Vector3.MoveTowards(transform.position, defaultPosition, pullCameraBackSpeed * Time.deltaTime);
		return transform.position;
	}

	//private Vector3 CalculateCameraPosition(Vector3 cameraPosition, LocalPlayerController.MovementState state, float interpolate, float angle)
	//{
//
	//	var rotationFactor = Mathf.Lerp(1f, 0f, Mathf.Clamp(angle / 3f, 0f, 1f));
	//	
	//	var desiredPosition = cameraPosition;
	//	var targetMaxPositionRun = cameraPosition + target.forward * pushDistance;
	//	
	//	bool running = state == LocalPlayerController.MovementState.Run ||
	//	               state == LocalPlayerController.MovementState.SidewaysRun ||
	//	               state == LocalPlayerController.MovementState.BackwardsRun;
//
	//	if (running)
	//		desiredPosition = Vector3.Lerp(cameraPosition, targetMaxPositionRun, Mathf.SmoothStep(0.0f, 1.0f, interpolate));
//
	//	return desiredPosition;
	//}
//
	//
//
	//private Vector3 CalculateCameraLookAtPosition(Vector3 target.position, LocalPlayerController.MovementState state, float interpolate)
	//{
	//	var mouseDistanceFromTarget = CalculateMouseDistanceFromTarget(target.position);
	//	bool mouseTooCloseToTarget = mouseDistanceFromTarget <= minMouseDistance;
	//	var targetMaxPositionRun = target.position + target.forward * pushDistance;
	//	var previousLookAtPosition = target.position + target.forward * previousTargetDistance;
	//	bool running =  state == LocalPlayerController.MovementState.Run ||
	//	                state == LocalPlayerController.MovementState.SidewaysRun ||
	//	                state == LocalPlayerController.MovementState.BackwardsRun;
//
	//	var lookAtPosition = target.position;
//
	//	//if (running && mouseTooCloseToTarget)
	//	//{
	//	//	
	//	//}
	//	if (state == LocalPlayerController.MovementState.Run || state == LocalPlayerController.MovementState.SidewaysRun || state == LocalPlayerController.MovementState.BackwardsRun) //&& !mouseTooCloseToTarget)
	//	{
	//		lookAtPosition = Vector3.Lerp(target.position, targetMaxPositionRun, Mathf.SmoothStep(0.0f, 1.0f, interpolate));
	//	}
	//	//else if (hasVelocity && mouseTooCloseToTarget)
	//	//{
	//	//	var targetMaxPosition2 = target.position + target.forward * minMouseDistance;
	//	//	var interpolate = Mathf.Clamp(mouseDistanceFromTarget / minMouseDistance, 0f, 1f);
	//	//	lookAtPosition = Vector3.Lerp(target.position, targetMaxPosition2, Mathf.SmoothStep(0.0f, 1.0f, interpolate));
	//	//}
	//	//else if(state == LocalPlayerController.MovementState.Walk || state == LocalPlayerController.MovementState.Sideways || state == LocalPlayerController.MovementState.Backwards)
	//	//{
	//	//	lookAtPosition = Vector3.Lerp(previousLookAtPosition, target.position, Mathf.SmoothStep(0.0f, 1.0f, interpolate));
	//	//}
	//	
	//	//Debug.Log(previousTargetDistance);
	//	return lookAtPosition;
	//}
	//private float CalculateMouseDistanceFromTarget(Vector3 target.position)
	//{
	//	Ray mouseRay = camera.ScreenPointToRay(Input.mousePosition);
	//	RaycastHit hit;
	//	Physics.Raycast(mouseRay, out hit, groundMask);
	//	float mouseDistanceFromTarget = Vector3.Distance(target.position, hit.point);
//
	//	return mouseDistanceFromTarget;
	//}
}