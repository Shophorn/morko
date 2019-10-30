using System.Numerics;
using UnityEditor;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class LocalCameraController : MonoBehaviour
{
	public Character character;
	public Camera camera;
	public Transform target;
	public float distance = 10;
	public float angle = 45;

	public float pushDistance = 3f;
	public float pushForwardTime = 5f;
	public float pullBackTime = 1f;
	private float pushForwardTimer = 0f;
	private float pullBackTimer = 0f;

	public float minMouseDistance = 3f;
	private LayerMask groundMask = 1 << 9;
	private float previousTargetDistance = 0f;

	private void Start()
	{
		character = GetComponentInParent<Character>();
		camera = GetComponent<Camera>();
	}

	private void LateUpdate()
	{
		float y = Mathf.Sin(angle) * distance;
		float z = -Mathf.Cos(angle) * distance;
		Vector3 targetPosition = target.position;
		
		Vector3 localPosition = new Vector3(0, y, z);
		Vector3 position = targetPosition + localPosition;
		transform.position = position;

		LocalPlayerController.MovementState targetMovementState = character.localController.currentMovementState;
		float targetStateInterpolation = character.localController.movementStateInterpolator;
		var lookAtPosition = CalculateCameraLookAtPosition(targetPosition, targetMovementState, targetStateInterpolation);
		
		transform.LookAt(lookAtPosition);
	}

	private Vector3 CalculateCameraLookAtPosition(Vector3 targetPosition, LocalPlayerController.MovementState state, float interpolate)
	{
		var mouseDistanceFromTarget = CalculateMouseDistanceFromTarget(targetPosition);
		bool mouseTooCloseToTarget = mouseDistanceFromTarget <= minMouseDistance;
		var targetMaxPositionRun = targetPosition + target.forward * pushDistance;
		var previousLookAtPosition = targetPosition + target.forward * previousTargetDistance;
		bool running =  state == LocalPlayerController.MovementState.Run ||
		                state == LocalPlayerController.MovementState.SidewaysRun ||
		                state == LocalPlayerController.MovementState.BackwardsRun;

		var lookAtPosition = targetPosition;

		//if (running && mouseTooCloseToTarget)
		//{
		//	
		//}
		if (state == LocalPlayerController.MovementState.Run || state == LocalPlayerController.MovementState.SidewaysRun || state == LocalPlayerController.MovementState.BackwardsRun) //&& !mouseTooCloseToTarget)
		{
			lookAtPosition = Vector3.Lerp(targetPosition, targetMaxPositionRun, Mathf.SmoothStep(0.0f, 1.0f, interpolate));
		}
		//else if (hasVelocity && mouseTooCloseToTarget)
		//{
		//	var targetMaxPosition2 = targetPosition + target.forward * minMouseDistance;
		//	var interpolate = Mathf.Clamp(mouseDistanceFromTarget / minMouseDistance, 0f, 1f);
		//	lookAtPosition = Vector3.Lerp(targetPosition, targetMaxPosition2, Mathf.SmoothStep(0.0f, 1.0f, interpolate));
		//}
		//else if(state == LocalPlayerController.MovementState.Walk || state == LocalPlayerController.MovementState.Sideways || state == LocalPlayerController.MovementState.Backwards)
		//{
		//	lookAtPosition = Vector3.Lerp(previousLookAtPosition, targetPosition, Mathf.SmoothStep(0.0f, 1.0f, interpolate));
		//}
		
		//Debug.Log(previousTargetDistance);
		return lookAtPosition;
	}
	private float CalculateMouseDistanceFromTarget(Vector3 targetPosition)
	{
		Ray mouseRay = camera.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		Physics.Raycast(mouseRay, out hit, groundMask);
		float mouseDistanceFromTarget = Vector3.Distance(targetPosition, hit.point);

		return mouseDistanceFromTarget;
	}
}