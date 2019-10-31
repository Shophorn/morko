using System.Numerics;
using UnityEditor;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using Vector4 = UnityEngine.Vector4;

public class LocalCameraController : MonoBehaviour
{
	public Character character;
	public Camera camera;
	public Transform target;
	public float distance = 10;
	public float angle = 45;
	public float speed = 5;

	public float pushDistance = 3f;
	public float pushForwardTime = 5f;
	public float pullBackTime = 1f;
	private float pushForwardTimer = 0f;
	private float pullBackTimer = 0f;

	public float minMouseDistance = 3f;
	private LayerMask groundMask = 1 << 9;
	private float previousTargetDistance = 0f;
	
	Quaternion quaternion = Quaternion.Euler(new Vector3(60, 0, 0));

	private Vector3 pos = Vector3.zero;
	private Vector3 orgPos = Vector3.zero;

	private Quaternion prevQ;
	private Quaternion currQ;
	
	private void Start()
	{
		character = GetComponentInParent<Character>();
		camera = GetComponent<Camera>();

		prevQ = target.rotation;
		currQ = prevQ;
	}

	private void LateUpdate()
	{
		currQ = target.rotation;
		
		transform.rotation = quaternion;

		float y = Mathf.Sin(angle) * distance;
		float z = -Mathf.Cos(angle) * distance;
		Vector3 targetPosition = target.position;
		
		Vector3 localPosition = new Vector3(0, y, z);
		Vector3 cameraNormalPosition = targetPosition + localPosition;

		orgPos = cameraNormalPosition;
		
		LocalPlayerController.MovementState targetMovementState = character.localController.currentMovementState;
		float targetStateInterpolation = character.localController.movementStateInterpolator;
		float ang = Quaternion.Angle(currQ, prevQ);

		var moveCameraBy = CalculateCameraPosition(cameraNormalPosition, targetMovementState, targetStateInterpolation, ang);
		transform.position = moveCameraBy;

		prevQ = currQ;
	}

	private Vector3 CalculateCameraPosition(Vector3 cameraPosition, LocalPlayerController.MovementState state, float interpolate, float angle)
	{

		var rotationFactor = Mathf.Lerp(1f, 0f, Mathf.Clamp(angle / 3f, 0f, 1f));
		
		Debug.Log(rotationFactor);
		
		var desiredPosition = cameraPosition;
		var targetMaxPositionRun = cameraPosition + target.forward * pushDistance;
		
		bool running = state == LocalPlayerController.MovementState.Run ||
		               state == LocalPlayerController.MovementState.SidewaysRun ||
		               state == LocalPlayerController.MovementState.BackwardsRun;

		if (running)
			desiredPosition = Vector3.Lerp(cameraPosition, targetMaxPositionRun, Mathf.SmoothStep(0.0f, 1.0f, interpolate) * rotationFactor);

		pos = desiredPosition;
		
		return desiredPosition;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(orgPos, 0.1f);
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(pos, 0.1f);
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