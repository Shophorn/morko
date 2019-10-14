using UnityEngine;
using Morko.Network;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Morko
{
	public class LocalController
	{
		private Character character;
		private AvatarPackage package;
		private Camera camera;
		private Vector3 lastMousePosition = Input.mousePosition;
		private LayerMask groundMask = 1 << 9;
		
		// mouseControl = True == control with mouse
		// mouseControl = False == control with joystick
		bool mouseControl = true;
		
		private PlayerSettings playerSettings;
		private float currentMovementSpeed = 0f;
		private float previousSpeed = 0f;
		private float maxSpeed;
		private float sneakSpeed;
		private float runSpeed;
		private float accelerationTime;
		private float decelerationTime;
		private bool isMorko = false;

		private Vector3 moveDirection;
		private Vector3 moveDirectionGamepad;
		private Vector3 oldDirection = Vector3.zero;
		private bool givingMovementInput = false;
		private float decelerationTimer = 0f;
		private float accelerationTimer = 0f;

		public void changeState(bool toMorko)
		{
			if (toMorko)
			{
				isMorko = true;
				maxSpeed = playerSettings.morkoMaxSpeed;
				sneakSpeed = playerSettings.morkoMaxSpeed * playerSettings.morkoSneakMultiplier;
				runSpeed = playerSettings.morkoMaxSpeed * playerSettings.morkoRunMultiplier;
			}
			else
			{
				isMorko = false;
				maxSpeed = playerSettings.maxSpeed;
				sneakSpeed = playerSettings.maxSpeed * playerSettings.sneakMultiplier;
				runSpeed = playerSettings.maxSpeed * playerSettings.runMultiplier;
			}
		}

		public static LocalController Create(Character character, PlayerSettings settings)
		{
			var result = new LocalController();
			result.package = new AvatarPackage();

			result.package.id = 0;
			result.package.position = Vector3.zero;
			result.package.rotation = Quaternion.identity;
			result.package.velocity = Vector3.zero;
			result.playerSettings = settings;
			result.maxSpeed = result.playerSettings.maxSpeed;
			result.sneakSpeed = result.playerSettings.maxSpeed * result.playerSettings.sneakMultiplier;
			result.runSpeed = result.playerSettings.maxSpeed * result.playerSettings.runMultiplier;
			result.accelerationTime = result.playerSettings.accelerationTime;
			result.decelerationTime = result.playerSettings.decelerationTime;

			result.character = character;
			result.camera = character.GetComponentInChildren<Camera>();
			return result;
		}

		// Todo(Sampo): Input support for multiple platforms (Mac, Linux)

		public AvatarPackage Update()
		{
			Vector3 lastPosition = character.gameObject.transform.position;
			
			// Move direction with keyboard
			moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical")).normalized;
			// Move direction with gamepad
			moveDirectionGamepad = new Vector3(Input.GetAxis("GamepadHorizontal"), 0.0f, Input.GetAxis("GamepadVertical"));

			if (moveDirection.x + moveDirection.z > moveDirectionGamepad.x + moveDirectionGamepad.z)
			{
				// Moving with KB
				//Debug.Log("KB");
			}
			else
			{
				// Moving with GP
				//Debug.Log("KB");
			}

			float joystickRotateX = Input.GetAxis("RotateX");
			float joystickRotateY = Input.GetAxis("RotateY");

			Vector3 currentMousePosition = Input.mousePosition;
			Vector3 mouseDelta = currentMousePosition - lastMousePosition;
			
			// Rotate
			// Mouse moved, use mouse
			if (Mathf.Abs(mouseDelta.x) > 0 || Mathf.Abs(mouseDelta.y) > 0)
			{
				mouseControl = true;
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
				mouseControl = false;
				Vector3 lookDirectionJoystick = new Vector3(Input.GetAxis("RotateX"), 0f, Input.GetAxis("RotateY"));
				Quaternion lookRotation = Quaternion.LookRotation(lookDirectionJoystick, Vector3.up);
       
				float step = maxSpeed * Time.deltaTime;
				character.transform.rotation = Quaternion.RotateTowards(lookRotation, character.transform.rotation, step);
			}
			// Controller being used, right joystick not being used, look towards player forward
			if (moveDirection != Vector3.zero && joystickRotateX == 0 && joystickRotateY == 0 && mouseDelta.x == 0 && mouseDelta.y == 0 && mouseControl == false)
			{
				character.transform.rotation = Quaternion.LookRotation(moveDirection);
			}
			// Mouse being used, keep old rotation
			if (joystickRotateX == 0 && joystickRotateY == 0 && mouseDelta.x == 0 && mouseDelta.y == 0 && mouseControl == true)
			{
				character.transform.rotation = package.rotation;
			}
			
			CalculateMovementSpeed();
			
			// Save direction when not moving
			// Because direction is required even when not giving input for deceleration
			if (moveDirection != Vector3.zero)
				oldDirection = moveDirection;
			
			if (currentMovementSpeed > 0)
				moveDirection = oldDirection.normalized;
			else
				moveDirection = Vector3.zero;
			
			// Move
			character.characterController.Move(moveDirection * currentMovementSpeed * Time.deltaTime);
			
			// Update package data
			package.position = character.gameObject.transform.position;
			package.rotation = character.gameObject.transform.rotation;
			package.velocity = (character.transform.position - lastPosition) / Time.deltaTime;
			
			return package;
		}

		// Acceleration/Deceleration
		public void CalculateMovementSpeed()
		{
			
			if (moveDirection != Vector3.zero && currentMovementSpeed < maxSpeed)
			{
				if (accelerationTimer < 0f)
					accelerationTimer = 0f;
				
				accelerationTimer += Time.deltaTime;
				decelerationTimer += Time.deltaTime;
				
				currentMovementSpeed = maxSpeed * (accelerationTimer / accelerationTime);
				previousSpeed = currentMovementSpeed;
			}
			else if (moveDirection == Vector3.zero && currentMovementSpeed > 0f)
			{
				if (decelerationTimer > decelerationTime)
					decelerationTimer = decelerationTime;
				
				decelerationTimer -= Time.deltaTime;
				accelerationTimer -= Time.deltaTime;
				
				currentMovementSpeed = previousSpeed * (decelerationTimer / decelerationTime);
			}
			
			else if (moveDirection != Vector3.zero && currentMovementSpeed >= maxSpeed)
				currentMovementSpeed = maxSpeed;
			else
			{
				accelerationTimer = 0f;
				decelerationTimer = decelerationTime;
				
				currentMovementSpeed = 0;
			}
		}
	}
}
