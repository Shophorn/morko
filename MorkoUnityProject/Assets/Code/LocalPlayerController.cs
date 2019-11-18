using Photon.Pun;
using System;
using UnityEngine;

[RequireComponent(typeof(Character))]
public class LocalPlayerController : MonoBehaviourPun
{
	private Character character;
	public bool isMorko = false;
	private Camera camera;
	private Vector3 lastMousePosition;// Cannot call from field initializer = Input.mousePosition;
	private LayerMask groundMask = 1 << 9;
	private const float joystickMaxThreshold = 0.8f;
	private const float joystickMinDeadzone = 0.2f;
	private const float minMouseDelta = 0.05f;
	private const float gravity = -9.81f;
	private float velocityY = 0f;
	// mouseRotate = True == rotate with mouse
	// mouseRotate = False == rotate with joystick
	// mouseMove = True == move with KB
	// mouseMove = False == move with GP
	bool mouseRotatedLast = true;
	bool keyboardMove = true;
	
	public PlayerSettings normalSettings;
	public PlayerSettings morkoSettings;

	PlayerSettings currentSettings => isMorko ? morkoSettings : normalSettings;
	public MovementState currentMovementState = MovementState.Idle;
	public float movementStateInterpolator = 1f;
	private float walkSpeed => currentSettings.walkSpeed;
	private float sneakSpeed => currentSettings.sneakSpeed;
	private float runSpeed => currentSettings.runSpeed;
	private float sideMultiplier => currentSettings.sideMultiplier;
	private float backwardMultiplier => currentSettings.backwardMultiplier;
	private float sideRunMultiplier => currentSettings.sideRunMultiplier;
	private float backwardRunMultiplier => currentSettings.backwardRunMultiplier;
	private float accelerationWalk => currentSettings.accelerationWalk;
	private float accelerateSneak => currentSettings.accelerateSneak;
	private float accelerationRun => currentSettings.accelerationRun;
	private float decelerationWalk => currentSettings.decelerationWalk;
	private float decelerationRun => currentSettings.decelerationRun;
	private float dashDuration => currentSettings.dashDuration;
	private float dashDistance => currentSettings.dashDistance;
	private float dashCooldown => currentSettings.dashCooldown;
	
	private float currentMovementSpeed = 0f;
	private Vector3 moveDirection;
	private Vector3 lastPosition;
	private Vector3 oldDirection = Vector3.zero;
	private Quaternion lastRotation = Quaternion.identity;
	private Vector3 previousVelocityVector = Vector3.zero;
	private const float minVectorVelocityAngleChange = 160f;
	private bool ran = false;
	private bool disableMovement = false;
	private long lastMillis = 0;
	
	public enum MovementState
	{
		Idle,
		Rotate,
		Sneak,
		Walk,
		Sideways,
		Backwards,
		SidewaysRun,
		BackwardsRun,
		Run,
		Dive,
	}
	
	private Interlocked<Vector3> positionForNetwork = new Interlocked<Vector3>();
	private Interlocked<float> rotationForNetwork = new Interlocked<float>();

	public void ChangeStateTo(bool morko)
	{
		isMorko = morko;
	}

	public void ChangeState()
	{
		isMorko = !isMorko;
		
		if (isMorko)
		{
			// Change to morko anims, stuns, mask on, etc
			ToMorko();
		}
		else
		{
			// Change to human, mask off
			ToNormal();
		}
	}

	private void ToMorko()
	{
	}
	private static void ToNormal()
	{
	}

	private void Awake()
	{
		// Note(Leo): Destroy this controller component only if we are not the local player
		if (photonView.IsMine == false)
		{
			Destroy(this);
		}

		character = GetComponent<Character>();
	}

	public void SetCamera(Camera camera)
	{
		this.camera = camera;
	}

	private void Update()
	{
		moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical"));
		bool hasMoved = (moveDirection.sqrMagnitude > joystickMinDeadzone);
		
		bool runningInput = (Input.GetButton("Sprint") || Input.GetKey(KeyCode.LeftShift));
		bool runningSpeed = currentMovementSpeed >= walkSpeed;
		bool accelerateAndRun = runningInput && runningSpeed ? true : false;
		
		bool dive = Input.GetKeyDown(KeyCode.Space);
		
		Vector3 lookDirectionJoystick = new Vector3(Input.GetAxis("RotateX"), 0f, Input.GetAxis("RotateZ"));
		Vector3 currentMousePosition = Input.mousePosition;
		Vector3 mouseDelta = currentMousePosition - lastMousePosition;

		Move(moveDirection, accelerateAndRun, hasMoved);
		Rotate(lookDirectionJoystick, currentMousePosition, mouseDelta, hasMoved);
	}
	
	PlayerGameUpdatePackage INetworkSender.GetPackageToSend()
	{
		var package = new PlayerGameUpdatePackage
		{
			position = positionForNetwork.Value,
			rotation = rotationForNetwork.Value
		};
		return package;
	}

	private float CurrentStateInterpolation(MovementState movementState, float velocityMagnitude)
	{
		movementStateInterpolator = 1f;
		
		switch (movementState)
		{
			case MovementState.Idle:
				break;
			case MovementState.Rotate:
				break;
			case MovementState.Sneak:
				movementStateInterpolator = velocityMagnitude / sneakSpeed;
				break;
			case MovementState.Walk:
				movementStateInterpolator = (velocityMagnitude - sneakSpeed) / (walkSpeed - sneakSpeed);
				break;
			case MovementState.Sideways:
				movementStateInterpolator = velocityMagnitude / walkSpeed;
				break;
			case MovementState.Backwards:
				movementStateInterpolator = velocityMagnitude / walkSpeed;
				break;
			case MovementState.Run:
				movementStateInterpolator = (velocityMagnitude - walkSpeed) / (runSpeed - walkSpeed);
				break;
			case MovementState.SidewaysRun:
				movementStateInterpolator = (velocityMagnitude - walkSpeed) / (runSpeed - walkSpeed);
				break;
			case MovementState.BackwardsRun:
				movementStateInterpolator = (velocityMagnitude - walkSpeed) / (runSpeed - walkSpeed);
				break;
			case MovementState.Dive:
				break;
			
			default:
				throw new ArgumentOutOfRangeException(nameof(movementState), movementState, null);
		}

		movementStateInterpolator = Mathf.Clamp(movementStateInterpolator, 0f, 1f);
		return movementStateInterpolator;
	}
	private void UpdateAnimator(MovementState movementState, float interpolate)
	{
		character.animatorController.SetAnimation(movementState, interpolate);
	}

	// Todo(Sampo): Input support for multiple platforms (Mac, Linux)
	private void Move(Vector3 moveDirection, bool accelerateRun, bool hasMoved)
	{
		lastPosition = character.gameObject.transform.position;
		character.transform.position = new Vector3(character.transform.position.x, 0f, character.transform.position.z);
		
		bool joystickMaxed = moveDirection.magnitude >= joystickMaxThreshold;
		bool sneakingSpeed = currentMovementSpeed <= sneakSpeed;
		bool sneak = hasMoved && !joystickMaxed && sneakingSpeed;
		
		bool accelerateWalk = joystickMaxed && currentMovementSpeed < walkSpeed;
		bool decelerateWalk = !hasMoved && currentMovementSpeed <= walkSpeed && currentMovementSpeed > 0f;
		
		bool maxWalkSpeed = joystickMaxed && currentMovementSpeed >= walkSpeed && ran == false;
		
		bool maxRunSpeed = accelerateRun && currentMovementSpeed >= runSpeed;
		bool decelerateRun = !accelerateRun && currentMovementSpeed > walkSpeed && ran;
		
		
		if (sneak)
		{
			currentMovementState = MovementState.Sneak;
			currentMovementSpeed = sneakSpeed;
		}
		else if (accelerateWalk)
		{
			currentMovementState = MovementState.Walk;
			currentMovementSpeed += accelerationWalk * Time.deltaTime;
			currentMovementSpeed = Mathf.Clamp(currentMovementSpeed, 0f, walkSpeed);
		}
		else if (decelerateWalk)
		{
			currentMovementState = MovementState.Walk;
			currentMovementSpeed -= decelerationWalk * Time.deltaTime;
		}
		else if (maxRunSpeed)
		{
			currentMovementState = MovementState.Run;
			ran = true;
			currentMovementSpeed = runSpeed;
		}
		else if (accelerateRun)
		{
			currentMovementState = MovementState.Run;
			ran = true;
			currentMovementSpeed += accelerationRun * Time.deltaTime;
		}
		else if (decelerateRun)
		{
			currentMovementState = MovementState.Run;
			currentMovementSpeed -= decelerationRun * Time.deltaTime;
			if (currentMovementSpeed <= walkSpeed)
			{
				currentMovementState = MovementState.Walk;
				ran = false;
				currentMovementSpeed = walkSpeed;
			}
		}
		else if (maxWalkSpeed)
		{
			currentMovementState = MovementState.Walk;
			currentMovementSpeed = walkSpeed;
		}
		else
		{
			currentMovementState = MovementState.Idle;
			currentMovementSpeed = 0f;
		}
		
		// Save direction when not moving
		// Because direction is required even when not giving input for deceleration
		if (hasMoved)
			oldDirection = moveDirection;
		
		moveDirection = currentMovementSpeed > 0 ? oldDirection : Vector3.zero;
		
		// Dot product between character facing direction and character moving direction
		// Parallel == 1
		// Perpendicular == 0
		// Opposite == -1
		float moveLookDotProduct = Vector3.Dot(moveDirection.normalized, character.transform.forward);
		
		float decrease = 1f;
		
		bool movingSideways = moveLookDotProduct >= -0.32 && moveLookDotProduct <= 0.66f;
		bool movingBackwards = moveLookDotProduct < -0.32 && moveLookDotProduct >= -1f;
		
		if (movingSideways)
		{
			// Walk side multiplier
			if (currentMovementSpeed <= walkSpeed)
			{
				decrease = Mathf.Lerp(sideMultiplier, 1f, moveLookDotProduct);
				currentMovementState = MovementState.Sideways;
			}
			// Run side multiplier
			else
			{
				decrease = Mathf.Lerp(sideRunMultiplier, 1f, moveLookDotProduct);
				currentMovementState = MovementState.SidewaysRun;
			}

		}
		else if (movingBackwards)
		{
			// Walk backwards multiplier
			if (currentMovementSpeed <= walkSpeed)
			{
				decrease = Mathf.Lerp(sideMultiplier, backwardMultiplier, Mathf.Abs(moveLookDotProduct));
				currentMovementState = MovementState.Backwards;
			}
			// Run backwards multiplier
			else
			{
				decrease = Mathf.Lerp(sideRunMultiplier, backwardRunMultiplier, Mathf.Abs(moveLookDotProduct));
				currentMovementState = MovementState.BackwardsRun;
			}
		}
		else
			decrease = 1f;
		
		float finalSpeed = currentMovementSpeed * decrease;
		velocityY += Time.deltaTime * gravity;

		Vector3 velocity = Vector3.ClampMagnitude(moveDirection, 1f) * finalSpeed + Vector3.up * velocityY;
		
		// Move
		character.characterController.Move(velocity * Time.deltaTime);
		
		float directionAngleChange = Vector3.Angle(velocity, previousVelocityVector);
		float angleClamp = Mathf.Clamp(directionAngleChange, 0f, 180f);
		float decreaseSpeedByAngleMultiplier = Mathf.Lerp(1f, 0f, angleClamp / 180f);

		currentMovementSpeed *= decreaseSpeedByAngleMultiplier;

		previousVelocityVector = velocity;
		
		if (character.characterController.isGrounded)
			velocityY = 0f;
	}

	private void Rotate(Vector3 lookDirectionJoystick, Vector3 currentMousePosition, Vector3 mouseDelta, bool hasMoved)
	{
		bool mouseRotated = (Mathf.Abs(mouseDelta.x) > minMouseDelta) || (Mathf.Abs(mouseDelta.y) > minMouseDelta);
		bool rightJoystickRotated = lookDirectionJoystick.sqrMagnitude > joystickMinDeadzone;
		
		bool mouseOrLeftJoystickRotated = (mouseRotated && !rightJoystickRotated) || (!mouseRotated && rightJoystickRotated);
		bool mouseAndJoystickRotated = mouseRotated && rightJoystickRotated;
		
		var targetRotation = character.transform.rotation;

		if (mouseOrLeftJoystickRotated)
		{
			if (mouseRotated)
			{
				mouseRotatedLast = true;
				targetRotation = GetRotationToCursorPositionRelativeToCameraAndCharacterPosition();
			}
			else
			{
				mouseRotatedLast = false;
				targetRotation = Quaternion.LookRotation(lookDirectionJoystick, Vector3.up);
			}
		}
		else if (mouseAndJoystickRotated)
		{
			lastMousePosition = currentMousePosition;
			var mouseRotation = GetRotationToCursorPositionRelativeToCameraAndCharacterPosition();
			
			// check which one is larger
			float mouseAngle = Quaternion.Angle(mouseRotation, lastRotation);
			float joystickAngle = Quaternion.Angle(Quaternion.Euler(lookDirectionJoystick),lastRotation);

			if (mouseAngle >= joystickAngle)
			{
				mouseRotatedLast = true;
				targetRotation = mouseRotation;
			}
			else if (mouseAngle < joystickAngle)
			{
				mouseRotatedLast = false;
				targetRotation = Quaternion.LookRotation(lookDirectionJoystick, Vector3.up);
			}
		}
		else if (hasMoved && currentSettings.rotateTowardsMove)
			targetRotation = Quaternion.LookRotation(moveDirection);

		character.transform.rotation = targetRotation;
	}

	private Quaternion GetRotationToCursorPositionRelativeToCameraAndCharacterPosition()
	{
		Plane playerPlane = new Plane(Vector3.up, character.transform.position);
		Ray ray = camera.ScreenPointToRay (Input.mousePosition);
         
		float distance = 0.0f;
		var mouseRotation = character.transform.rotation;
         
		if (playerPlane.Raycast (ray, out distance)) 
		{
			Vector3 targetPoint = ray.GetPoint(distance);
			mouseRotation = Quaternion.LookRotation(targetPoint - character.transform.position);
		}

		return mouseRotation;
	}
}
