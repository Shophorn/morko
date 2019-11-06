using System;
using System.Numerics;
using UnityEngine;
using Morko.Network;
using Plane = UnityEngine.Plane;
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
		bool hasMoved = (moveDirection.sqrMagnitude > joystickMinDeadzone);
		
		bool runningInput = (Input.GetButton("Sprint") || Input.GetKey(KeyCode.LeftShift));
		bool runningSpeed = currentMovementSpeed >= walkSpeed;
		bool accelerateAndRun = runningInput && runningSpeed ? true : false;
		
		bool dive = Input.GetKeyDown(KeyCode.Space);
		
		Vector3 lookDirectionJoystick = new Vector3(Input.GetAxis("RotateX"), 0f, Input.GetAxis("RotateZ"));
		Vector3 currentMousePosition = Input.mousePosition;
		Vector3 mouseDelta = currentMousePosition - lastMousePosition;
		RotationWith rotationWith = CheckGamepadOrMouse(lookDirectionJoystick, currentMousePosition, mouseDelta, hasMoved);

		Move(moveDirection, accelerateAndRun, hasMoved);
		Rotate(rotationWith, currentMousePosition, lookDirectionJoystick);

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
			currentMovementSpeed = sneakSpeed;
		else if (accelerateWalk)
			currentMovementSpeed += accelerationWalk * Time.deltaTime;
		else if (decelerateWalk)
			currentMovementSpeed -= decelerationWalk * Time.deltaTime;
		else if (maxRunSpeed)
		{
			ran = true;
			currentMovementSpeed = runSpeed;
		}
		else if (accelerateRun)
		{
			ran = true;
			currentMovementSpeed += accelerationRun * Time.deltaTime;
		}
		else if (decelerateRun)
		{
			currentMovementSpeed -= decelerationRun * Time.deltaTime;
			if (currentMovementSpeed <= walkSpeed)
			{
				ran = false;
				currentMovementSpeed = walkSpeed;
			}
		}
		else if (maxWalkSpeed)
			currentMovementSpeed = walkSpeed;
		else
			currentMovementSpeed = 0f;
		
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
		
		bool movingSideways = moveLookDotProduct >= 0 && moveLookDotProduct < 1f;
		bool movingBackwards = moveLookDotProduct < 0 && moveLookDotProduct >= -1f;
		
		if (movingSideways)
		{
			// Walk side multiplier
			if (currentMovementSpeed <= walkSpeed)
				decrease = Mathf.Lerp(sideMultiplier, 1f, moveLookDotProduct);
			// Run side multiplier
			else
				decrease = Mathf.Lerp(sideRunMultiplier, 1f, moveLookDotProduct);
		}
		else if (movingBackwards)
		{
			// Walk backwards multiplier
			if (currentMovementSpeed <= walkSpeed)
				decrease = Mathf.Lerp(sideMultiplier, backwardMultiplier, Mathf.Abs(moveLookDotProduct));
			// Run backwards multiplier
			else
				decrease = Mathf.Lerp(sideRunMultiplier, backwardRunMultiplier, Mathf.Abs(moveLookDotProduct));
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

	private void Rotate(RotationWith rotationWith, Vector3 currentMousePosition, Vector3 lookDirectionJoystick)
	{
		var targetRotation = character.transform.rotation;

		switch (rotationWith)
		{
			case RotationWith.mouse:
				
				Plane characterPlane = new Plane(Vector3.up, character.transform.position);
				Ray ray = camera.ScreenPointToRay (currentMousePosition);
         
				float distance = 0.0f;
         
				if (characterPlane.Raycast (ray, out distance)) 
				{
					Vector3 targetPoint = ray.GetPoint(distance);
					targetRotation = Quaternion.LookRotation(targetPoint - character.transform.position);
				}
				break;
			case RotationWith.leftStick:
				targetRotation = Quaternion.LookRotation(moveDirection);
				break;
			case RotationWith.rightStick:
				targetRotation = Quaternion.LookRotation(lookDirectionJoystick, Vector3.up);
				break;
			case RotationWith.currentRotation:
				targetRotation = character.transform.rotation;
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}

		character.transform.rotation = targetRotation;
		lastRotation = targetRotation;
	}

	private RotationWith CheckGamepadOrMouse(Vector3 lookDirectionJoystick, Vector3 currentMousePosition, Vector3 mouseDelta, bool hasMoved)
	{
		bool mouseRotated = (Mathf.Abs(mouseDelta.x) > minMouseDelta) || (Mathf.Abs(mouseDelta.y) > minMouseDelta);
		bool rightJoystickRotated = lookDirectionJoystick.sqrMagnitude > joystickMinDeadzone;
		
		bool mouseOrLeftJoystickRotated = (mouseRotated && !rightJoystickRotated) || (!mouseRotated && rightJoystickRotated);
		bool mouseAndJoystickRotated = mouseRotated && rightJoystickRotated;
		
		bool mouseForRotation = false;
		bool rightJoystickForRotation = false;
		bool onlyLeftJoystickUsed = false;

		if (mouseOrLeftJoystickRotated)
		{
			if (mouseRotated)
			{
				mouseRotatedLast = true;
				return RotationWith.mouse;
			}

			mouseRotatedLast = false;
			return RotationWith.rightStick;
		}

		if (mouseAndJoystickRotated)
		{
			lastMousePosition = currentMousePosition;
			
			Plane playerPlane = new Plane(Vector3.up, character.transform.position);
			Ray ray = camera.ScreenPointToRay (currentMousePosition);
         
			float distance = 0.0f;
			var mouseRotation = character.transform.rotation;
         
			if (playerPlane.Raycast (ray, out distance)) 
			{
				Vector3 targetPoint = ray.GetPoint(distance);
				mouseRotation = Quaternion.LookRotation(targetPoint - character.transform.position);
			}
			
			// check which one is larger
			float mouseAngle = Quaternion.Angle(mouseRotation, lastRotation);
			float joystickAngle = Quaternion.Angle(Quaternion.Euler(lookDirectionJoystick),lastRotation);

			if (mouseAngle >= joystickAngle)
			{
				mouseRotatedLast = true;
				return RotationWith.mouse;
			}
			else if (mouseAngle < joystickAngle)
			{
				mouseRotatedLast = false;
				return RotationWith.rightStick;
			}
		}
		else if (hasMoved && currentSettings.rotateTowardsMove)
			return RotationWith.leftStick;
		
		return RotationWith.currentRotation;
	}

	enum RotationWith
	{
		mouse,
		leftStick,
		rightStick,
		currentRotation
	}
}
