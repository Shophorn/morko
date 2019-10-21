using UnityEngine;
using Morko.Network;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Morko
{
	public class LocalController
	{
		/* Todo(Leo): controller only uses camera for one ray, so it should be
		modified to only depend on more limited interface. */
		public void TEMPORARYSetCamera(Camera camera)
		{	
			this.camera = camera;
		}


		private Character character;
		private bool isMorko = false;
		private AvatarPackage package;
		private Camera camera;
		private Vector3 lastMousePosition = Input.mousePosition;
		private LayerMask groundMask = 1 << 9;
		private const float joystickMaxThreshold = 0.8f;
		
		// mouseRotate = True == rotate with mouse
		// mouseRotate = False == rotate with joystick
		// mouseMove = True == move with KB
		// mouseMove = False == move with GP
		bool mouseRotate = true;
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
		
		private float currentMovementSpeed = 0f;
		private Vector3 moveDirection;
		private Vector3 lastPosition;
		private Vector3 oldDirection = Vector3.zero;
		private bool ran = false;

		public void changeState(bool toMorko)
		{
			if (toMorko)
				isMorko = true;
			else
				isMorko = false;
		}

		public static LocalController Create(Character character, PlayerSettings normalSettings, PlayerSettings morkoSettings)
		{
			var result = new LocalController();
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

		// Todo(Sampo): Input support for multiple platforms (Mac, Linux)

		public AvatarPackage Update()
		{
			lastPosition = character.gameObject.transform.position;
			
			moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical"));
			
			float joystickRotateX = Input.GetAxis("RotateX");
			float joystickRotateY = Input.GetAxis("RotateY");

			Vector3 currentMousePosition = Input.mousePosition;
			Vector3 mouseDelta = currentMousePosition - lastMousePosition;
			
			bool mouseMoved = Mathf.Abs(mouseDelta.x) > 0 || Mathf.Abs(mouseDelta.y) > 0;
			
			// Rotate
			if (mouseMoved)
			{
				mouseRotate = true;
				lastMousePosition = currentMousePosition;
				Ray mouseRay = camera.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				Physics.Raycast(mouseRay, out hit, groundMask);
				Vector3 lookDirection = (hit.point - character.transform.position).normalized;
				Vector3 lookDirectionLevel = new Vector3(lookDirection.x, character.transform.position.y, lookDirection.z);
				character.transform.rotation = Quaternion.LookRotation(lookDirectionLevel);
			}
			
			bool joystickForRotation = Mathf.Abs(joystickRotateX) > 0f || Mathf.Abs(joystickRotateY) > 0f;
			bool onlyLeftJoystickUsed = moveDirection != Vector3.zero && joystickRotateX == 0 && joystickRotateY == 0 && mouseDelta.x == 0 && mouseDelta.y == 0 && mouseRotate == false;
			bool mouseForRotation = joystickRotateX == 0 && joystickRotateY == 0 && mouseDelta.x == 0 && mouseDelta.y == 0 && mouseRotate == true;
			
			if (joystickForRotation)
			{
				mouseRotate = false;
				Vector3 lookDirectionJoystick = new Vector3(Input.GetAxis("RotateX"), 0f, Input.GetAxis("RotateY"));
				Quaternion lookRotation = Quaternion.LookRotation(lookDirectionJoystick, Vector3.up);
       
				character.transform.rotation = Quaternion.RotateTowards(lookRotation, character.transform.rotation, Time.deltaTime);
			}
			if (onlyLeftJoystickUsed)
				character.transform.rotation = Quaternion.LookRotation(moveDirection);
			if (mouseForRotation)
				character.transform.rotation = package.rotation;
			
			bool joystickMaxed = moveDirection.magnitude >= joystickMaxThreshold;
			
			bool sneak = moveDirection != Vector3.zero && joystickMaxed == false && currentMovementSpeed <= sneakSpeed;
			bool accelerateWalk = joystickMaxed == true && currentMovementSpeed < walkSpeed;
			bool decelerateWalk = (moveDirection == Vector3.zero || joystickMaxed == false) && currentMovementSpeed <= walkSpeed && currentMovementSpeed > 0f;
			bool maxWalkSpeed = joystickMaxed == true && currentMovementSpeed >= walkSpeed && ran == false;
			bool accelerateRun = (Input.GetButton("Sprint") || Input.GetKey(KeyCode.LeftShift) == true) && joystickMaxed && currentMovementSpeed >= walkSpeed;
			bool decelerateRun = Input.GetButton("Sprint") == false && Input.GetKey(KeyCode.LeftShift) == false && currentMovementSpeed > walkSpeed && ran;
			bool maxRunSpeed = (Input.GetButton("Sprint") || Input.GetKey(KeyCode.LeftShift) == true) && joystickMaxed && currentMovementSpeed >= runSpeed;
			
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
			if (moveDirection != Vector3.zero)
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
			
			// TODO (Sampo): When turning backwards and then back forwards, acceleration should be applied
			// Currently it goes straight back to currenMovementSpeed
			// Change speed according to decrease
			float finalSpeed = currentMovementSpeed * decrease;
			
			// Move
			character.characterController.Move(moveDirection * finalSpeed * Time.deltaTime);

			// Update package data
			package.position = character.gameObject.transform.position;
			package.rotation = character.gameObject.transform.rotation;
			package.velocity = (character.transform.position - lastPosition) / Time.deltaTime;
			
			return package;
		}
	}
}
