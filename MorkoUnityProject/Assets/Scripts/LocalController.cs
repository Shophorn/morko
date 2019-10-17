using UnityEngine;
using Morko.Network;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Morko
{
	public class LocalController
	{
		private Character character;
		private bool isMorko = false;
		private AvatarPackage package;
		private Camera camera;
		private Vector3 lastMousePosition = Input.mousePosition;
		private LayerMask groundMask = 1 << 9;
		
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
			
			moveDirection = moveDirection.normalized;

			float joystickRotateX = Input.GetAxis("RotateX");
			float joystickRotateY = Input.GetAxis("RotateY");

			Vector3 currentMousePosition = Input.mousePosition;
			Vector3 mouseDelta = currentMousePosition - lastMousePosition;
			
			// Rotate
			// Mouse moved, use mouse
			if (Mathf.Abs(mouseDelta.x) > 0 || Mathf.Abs(mouseDelta.y) > 0)
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
			// If joystick is used for rotation, use controller
			if (Mathf.Abs(joystickRotateX) > 0f || Mathf.Abs(joystickRotateY) > 0f )
			{
				mouseRotate = false;
				Vector3 lookDirectionJoystick = new Vector3(Input.GetAxis("RotateX"), 0f, Input.GetAxis("RotateY"));
				Quaternion lookRotation = Quaternion.LookRotation(lookDirectionJoystick, Vector3.up);
       
				character.transform.rotation = Quaternion.RotateTowards(lookRotation, character.transform.rotation, Time.deltaTime);
			}
			// Controller being used, right joystick not being used, look towards player forward
			if (moveDirection != Vector3.zero && joystickRotateX == 0 && joystickRotateY == 0 && mouseDelta.x == 0 && mouseDelta.y == 0 && mouseRotate == false)
			{
				character.transform.rotation = Quaternion.LookRotation(moveDirection);
			}
			// Mouse being used, keep old rotation
			if (joystickRotateX == 0 && joystickRotateY == 0 && mouseDelta.x == 0 && mouseDelta.y == 0 && mouseRotate == true)
			{
				character.transform.rotation = package.rotation;
			}
			
			// Hardcoded min magnitude value where joystick is maxed out
			bool joystickMaxed = moveDirection.magnitude >= 0.8;
			
			// Sneak
			if (moveDirection != Vector3.zero && joystickMaxed == false && currentMovementSpeed <= sneakSpeed)
				currentMovementSpeed = sneakSpeed;
			// Accelerate walk
			else if (joystickMaxed == true && currentMovementSpeed < walkSpeed)
				currentMovementSpeed += accelerationWalk * Time.deltaTime;
			// Decelerate walk
			else if ((moveDirection == Vector3.zero || joystickMaxed == false) && currentMovementSpeed <= walkSpeed && currentMovementSpeed > 0f)
				currentMovementSpeed -= decelerationWalk * Time.deltaTime;
			// Maximum running speed
			else if ((Input.GetButton("Sprint") || Input.GetKey(KeyCode.LeftShift) == true) && joystickMaxed && currentMovementSpeed >= runSpeed)
			{
				ran = true;
				currentMovementSpeed = runSpeed;
			}
			// Accelerate run
			else if ((Input.GetButton("Sprint") || Input.GetKey(KeyCode.LeftShift) == true) && joystickMaxed && currentMovementSpeed >= walkSpeed)
			{
				ran = true;
				currentMovementSpeed += accelerationRun * Time.deltaTime;
			}
			// Decelerate run
			else if (Input.GetButton("Sprint") == false && Input.GetKey(KeyCode.LeftShift) == false && currentMovementSpeed > walkSpeed && ran)
			{
				currentMovementSpeed -= decelerationRun * Time.deltaTime;
				if (currentMovementSpeed <= walkSpeed)
				{
					ran = false;
					currentMovementSpeed = walkSpeed;
				}
			}
			// Maximum walking speed
			else if (joystickMaxed == true && currentMovementSpeed >= walkSpeed && ran == false)
				currentMovementSpeed = walkSpeed;
			// Still
			else
				currentMovementSpeed = 0f;
			
			// Save direction when not moving
			// Because direction is required even when not giving input for deceleration
			if (moveDirection != Vector3.zero)
				oldDirection = moveDirection;
			
			moveDirection = currentMovementSpeed > 0 ? oldDirection.normalized : Vector3.zero;
			
			// Dot product between character facing direction and character moving direction
			// Parallel == 1
			// Perpendicular == 0
			// Opposite == -1
			float moveLookDotProduct = Vector3.Dot(moveDirection, character.transform.forward);
			
			float decrease = 1f;
			// Apply sideways multiplier lerp
			if (moveLookDotProduct >= 0)
			{
				// Walk side multiplier
				if (currentMovementSpeed <= walkSpeed)
					decrease = Mathf.Lerp(sideMultiplier, 1f, moveLookDotProduct);
				// Run side multiplier
				else
					decrease = Mathf.Lerp(sideRunMultiplier, 1f, moveLookDotProduct);
			}
			// Apply backwards multiplier lerp
			else
			{
				// Walk backwards multiplier
				if (currentMovementSpeed <= walkSpeed)
					decrease = Mathf.Lerp(sideMultiplier, backwardMultiplier, Mathf.Abs(moveLookDotProduct));
				// Run backwards multiplier
				else
					decrease = Mathf.Lerp(sideRunMultiplier, backwardRunMultiplier, Mathf.Abs(moveLookDotProduct));
			}
			
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
