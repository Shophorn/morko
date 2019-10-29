using System;
using System.Numerics;
using UnityEngine;
using Morko.Network;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;

public class LocalPlayerController
{
	/* Todo(Leo): controller only uses camera for one ray, so it should be
	modified to only depend on more limited interface. */
	public void TEMPORARYSetCamera(Camera camera)
	{	
		this.camera = camera;
	}

	private Character character;
	public bool isMorko = false;
	private AvatarPackage package;
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
	private Vector3 lastRotation = Vector3.zero;
	private Vector3 previousVelocityVector = Vector3.zero;
	private const float minVectorVelocityAngleChange = 160f;
	private bool ran = false;
	private bool disableMovement = false;
	private long lastMillis = 0;

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
	
	
	public static LocalPlayerController Create(Character character, PlayerSettings normalSettings, PlayerSettings morkoSettings)
	{
		var result = new LocalPlayerController();
		result.package = new AvatarPackage();

		result.package.id = 0;
		result.package.position = Vector3.zero;
		result.package.rotation = Quaternion.identity;
		result.package.velocity = Vector3.zero;
		
		result.normalSettings = normalSettings;
		result.morkoSettings = morkoSettings;
		
		result.character = character;
		result.camera = character.GetComponentInChildren<Camera>();
		return result;
	}

	public AvatarPackage Update()
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
		Dash(dive);

		// Update package data
		package.position = character.gameObject.transform.position;
		package.rotation = character.gameObject.transform.rotation;
		package.velocity = (character.transform.position - lastPosition) / Time.deltaTime;
		
		return package;
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

	private void Rotate(Vector3 lookDirectionJoystick, Vector3 currentMousePosition, Vector3 mouseDelta)
	{
		Vector3 rotationVector = Vector3.zero;
		
		bool mouseRotated = (Mathf.Abs(mouseDelta.x) > minMouseDelta) || (Mathf.Abs(mouseDelta.y) > minMouseDelta);
		bool rightJoystickRotated = lookDirectionJoystick.sqrMagnitude > joystickMinDeadzone;
		bool mouseAndJoystickRotated = mouseRotated && rightJoystickRotated;
		bool mouseOrLeftJoystickRotated = (mouseRotated && !rightJoystickRotated) || (!mouseRotated && rightJoystickRotated);
		
		bool hasMoved = (moveDirection.sqrMagnitude > joystickMinDeadzone);
		bool mouseForRotation = false;
		bool rightJoystickForRotation = false;
		bool onlyLeftJoystickUsed = false;
		
		if (mouseOrLeftJoystickRotated)
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
	}
	
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
}
