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

	public int lerpAmount = 10;
	public float parabolaHeigh = 2f;

	private Vector3 defaultPoss;
	private Vector3[] points = new Vector3[10];
	private Vector3 point;
	private Vector3 previousPosition;

	private void Start()
	{
		character = GetComponentInParent<Character>();
		camera = GetComponent<Camera>();
	}

	private void LateUpdate()
	{
		float y = Mathf.Sin(angle) * distance;
		float z = -Mathf.Cos(angle) * distance;

		Vector3 localPosition = new Vector3(0, y, z);
		Vector3 defaultPosition = target.position + localPosition;
		defaultPoss = defaultPosition;

		transform.position = previousPosition;

		var cameraPos = MoveCameraIfPlayerNotVisible(defaultPosition);
		point = cameraPos;

		transform.position = cameraPos;
		previousPosition = cameraPos;
		
		transform.LookAt(target.position);
	}

	private Vector3 MoveCameraIfPlayerNotVisible(Vector3 defaultPosition)
	{
		var cameraCurrentPosition = transform.position;
		var defaultForward = target.position - defaultPosition;
		var cameraMaxPosition = new Vector3(target.transform.position.x, distance, target.transform.position.z);
		var defaultToMaxVector = cameraMaxPosition - defaultPosition;
		
		Vector3[] vectorPoints = new Vector3[lerpAmount];
		float iterator = 0f;
		float iteratorAmount = 1f / lerpAmount;

		for (int i = 0; i < lerpAmount; i++)
		{
			iterator += iteratorAmount;
			iterator = Mathf.Clamp(iterator, 0, 1);
			
			vectorPoints[i] = defaultPosition + defaultToMaxVector * iterator;
			vectorPoints[i].y += Mathf.Sin( iterator * Mathf.PI ) * parabolaHeigh;
		}

		points = vectorPoints;
		RaycastHit hit;

		if(Physics.Raycast(defaultPosition, defaultForward, out hit)) {

			if (hit.collider.gameObject.CompareTag("Wall"))
			{
				foreach (var p in vectorPoints)
				{
					var direction = target.position - p;
					if (Physics.Raycast(p, direction, out hit))
					{
						if (!hit.collider.gameObject.CompareTag("Wall"))
						{
							transform.position = Vector3.MoveTowards(cameraCurrentPosition, p, speed * Time.deltaTime);
							return transform.position;
						}
					}
				}
			}
		}
		
		return defaultPosition;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawLine(defaultPoss, new Vector3(target.transform.position.x, distance, target.transform.position.z));
		Gizmos.color = Color.blue;
		foreach (var p in points)
		{
			Gizmos.DrawSphere(p, 0.05f);
		}
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(point, 0.15f);
	}

	private Vector3 CalculateCameraPosition(Vector3 cameraPosition, LocalPlayerController.MovementState state, float interpolate, float angle)
	{

		var rotationFactor = Mathf.Lerp(1f, 0f, Mathf.Clamp(angle / 3f, 0f, 1f));
		
		var desiredPosition = cameraPosition;
		var targetMaxPositionRun = cameraPosition + target.forward * pushDistance;
		
		bool running = state == LocalPlayerController.MovementState.Run ||
		               state == LocalPlayerController.MovementState.SidewaysRun ||
		               state == LocalPlayerController.MovementState.BackwardsRun;

		if (running)
			desiredPosition = Vector3.Lerp(cameraPosition, targetMaxPositionRun, Mathf.SmoothStep(0.0f, 1.0f, interpolate));

		pos = desiredPosition;
		
		return desiredPosition;
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