using System;
using System.Numerics;
using UnityEngine;
using Morko.Network;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;

public class LocalPlayerController : INetworkSender
{
	private Character character;
	public bool isMorko = false;
	private Camera camera;
	private Vector3 lastMousePosition = Input.mousePosition;
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
	
	PlayerSettings normalSettings;
	PlayerSettings morkoSettings;
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
	private Vector3 lastRotation = Vector3.zero;
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
	
	
	public static LocalPlayerController Create(
		Character character,
		Camera camera,
		PlayerSettings normalSettings,
		PlayerSettings morkoSettings
	){
		var controller = new LocalPlayerController();

		controller.normalSettings = normalSettings;
		controller.morkoSettings = morkoSettings;
		
		controller.character = character;
		controller.camera = camera;

		return controller;
	}

	public void Update()
	{
		moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical"));
		
		bool runningInput = (Input.GetButton("Sprint") || Input.GetKey(KeyCode.LeftShift));
		bool runningSpeed = currentMovementSpeed >= walkSpeed;
		bool accelerateAndRun = runningInput && runningSpeed ? true : false;
		
		bool dive = Input.GetKeyDown(KeyCode.Space);
		
		Vector3 lookDirectionJoystick = new Vector3(Input.GetAxis("RotateX"), 0f, Input.GetAxis("RotateZ"));

		Vector3 currentMousePosition = Input.mousePosition;
		Vector3 mouseDelta = currentMousePosition - lastMousePosition;

		Move(moveDirection, accelerateAndRun);
		Rotate(lookDirectionJoystick, currentMousePosition, mouseDelta);
		currentMovementState = dive ? MovementState.Dive : currentMovementState;

		float interpolate = CurrentStateInterpolation(currentMovementState, character.characterController.velocity.magnitude);
		UpdateAnimator(currentMovementState, interpolate);

		positionForNetwork.Value = character.transform.position;
		rotationForNetwork.Value = Vector3.SignedAngle(Vector3.forward, character.transform.forward, Vector3.up);
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
	private void Move(Vector3 moveDirection, bool accelerateRun)
	{
		lastPosition = character.gameObject.transform.position;
		character.transform.position = new Vector3(character.transform.position.x, 0f, character.transform.position.z);
		
		bool joystickMaxed = moveDirection.magnitude >= joystickMaxThreshold;
		bool hasMoved = (moveDirection.sqrMagnitude > joystickMinDeadzone);
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

	private void Rotate(Vector3 lookDirectionJoystick, Vector3 currentMousePosition, Vector3 mouseDelta)
	{
		Vector3 rotationVector = Vector3.zero;
			
		bool mouseRotated = (Mathf.Abs(mouseDelta.x) > minMouseDelta) || (Mathf.Abs(mouseDelta.y) > minMouseDelta);
		bool rightJoystickRotated = lookDirectionJoystick.sqrMagnitude > joystickMinDeadzone;
		bool mouseAndJoystickRotated = mouseRotated && rightJoystickRotated;
		bool mouseOrRightJoystickRotated = (mouseRotated && !rightJoystickRotated) || (!mouseRotated && rightJoystickRotated);
		
		bool hasMoved = (moveDirection.sqrMagnitude > joystickMinDeadzone);
		bool mouseForRotation = false;
		bool rightJoystickForRotation = false;
		bool onlyLeftJoystickUsed = false;
		
		if (mouseOrRightJoystickRotated)
		{
			mouseForRotation = mouseRotated;
			mouseRotatedLast = mouseForRotation;
			rightJoystickForRotation = rightJoystickRotated;
			// Check rotation amount compared to last frame
		}
		else if (mouseAndJoystickRotated)
		{
			lastMousePosition = currentMousePosition;
			Ray mouseRay = camera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			Physics.Raycast(mouseRay, out hit, groundMask);
			Vector3 lookDirection = (hit.point - character.transform.position).normalized;
			Vector3 lookDirectionLevel = new Vector3(lookDirection.x, character.transform.position.y, lookDirection.z);
			// check which one is larger
			float mouseAngle = Vector3.Angle(lookDirectionLevel, lastRotation);
			float joystickAngle = Vector3.Angle(lookDirectionJoystick, lastRotation);
			

			if (mouseAngle > joystickAngle)
			{
				mouseForRotation = true;
				mouseRotatedLast = true;
				rightJoystickForRotation = false;
			}
			else if (mouseAngle < joystickAngle)
			{
				mouseForRotation = false;
				mouseRotatedLast = false;
				rightJoystickForRotation = true;
			}
		}
		else if (hasMoved)
			onlyLeftJoystickUsed = true;
		
		if (onlyLeftJoystickUsed && !mouseRotatedLast && currentSettings.rotateTowardsMove)
		{
			character.transform.rotation = Quaternion.LookRotation(moveDirection);
		}
		
		else if (mouseForRotation)
		{
			lastMousePosition = currentMousePosition;
			Ray mouseRay = camera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			Physics.Raycast(mouseRay, out hit, groundMask);
			Vector3 lookDirection = (hit.point - character.transform.position).normalized;
			Vector3 lookDirectionLevel = new Vector3(lookDirection.x, character.transform.position.y, lookDirection.z);
			character.transform.rotation = Quaternion.LookRotation(lookDirectionLevel);
		}
		else if (rightJoystickForRotation)
		{
			Quaternion lookRotation = Quaternion.LookRotation(lookDirectionJoystick, Vector3.up);
			character.transform.rotation = Quaternion.RotateTowards(lookRotation, character.transform.rotation, Time.deltaTime);
		}
		
		// If rotation amount > threshold, slowdown character
		float angle = Vector3.Angle(character.transform.forward, lastRotation);
		lastRotation = character.transform.forward;

		if ((mouseOrRightJoystickRotated || mouseAndJoystickRotated) && character.characterController.velocity.magnitude <= 0.1f)
		{
			currentMovementState = MovementState.Rotate;
		}
		else if (!mouseOrRightJoystickRotated && !mouseAndJoystickRotated && character.characterController.velocity.magnitude <= 0.1f)
		{
			currentMovementState = MovementState.Idle;
		}
	}
	/*
	 Will be replaced by animation
	private void Dash(bool dive)
	{
		if (!isMorko || !dive) return;

		Vector3 currentPosition = character.transform.position;
		Vector3 targetPosition = currentPosition + character.transform.forward * dashDistance;
		
		long currentMillis = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
		bool pastCooldown = currentMillis - lastMillis >= dashCooldown * 1000;
			
		if (!pastCooldown)
			return;
		
		var time = 0f;
		while(time < 1)
		{
			time += Time.deltaTime / dashDuration;
			Debug.Log(time);
			character.transform.position = Vector3.Lerp(currentPosition, targetPosition, time);
			
			// If collision with other player
			// changeState(false);
			// Change other player to morko
		}
		
		lastMillis = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
		
		currentMovementSpeed = 0f;
	}
	*/
}
