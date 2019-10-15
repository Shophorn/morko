using System;
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
		
		// mouseRotate = True == rotate with mouse
		// mouseRotate = False == rotate with joystick
		// mouseMove = True == move with KB
		// mouseMove = False == move with GP
		bool mouseRotate = true;
		bool keyboardMove = true;
		
		private PlayerSettings playerSettings;
		private float currentMovementSpeed = 0f;
		private float previousSpeed = 0f;
		private float maxSpeed;
		private float sneakSpeed;
		private float runSpeed;
		private float accelerationTime;
		private float decelerationTime;
		private float accelerationRunTime;
		private float decelerationRunTime;
		private bool isMorko = false;

		private Vector3 moveDirectionKeyboard;
		private Vector3 moveDirectionGamepad;
		private Vector3 lastPosition;
		private Vector3 oldDirectionKeyboard = Vector3.zero;
		private Vector3 oldDirectionGamepad = Vector3.zero;
		private Vector3 gamepadMovementSpeed;
		private Vector3 gamepadPreviousMovementSpeed;
		private bool givingMovementInput = false;
		private float decelerationTimer = 0f;
		private float accelerationTimer = 0f;
		private float decelerationRunTimer = 0f;
		private float accelerationRunTimer = 0f;

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
			result.accelerationRunTime = result.playerSettings.accelerationRunTime;
			result.decelerationRunTime = result.playerSettings.decelerationRunTime;

			result.character = character;
			result.camera = character.GetComponentInChildren<Camera>();
			return result;
		}

		// Todo(Sampo): Input support for multiple platforms (Mac, Linux)

		public AvatarPackage Update()
		{
			lastPosition = character.gameObject.transform.position;
			
			// Move direction with keyboard
			moveDirectionKeyboard = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
			// Move direction with gamepad
			moveDirectionGamepad = new Vector3(Input.GetAxisRaw("GamepadHorizontal"), 0.0f, Input.GetAxisRaw("GamepadVertical"));
			
			if (Mathf.Abs(moveDirectionKeyboard.x) + Mathf.Abs(moveDirectionKeyboard.z) > Mathf.Abs(moveDirectionGamepad.x) + Mathf.Abs(moveDirectionGamepad.z))
			{
				// Moving with KB
				Debug.Log("KB");
				keyboardMove = true;
			}
			else if (moveDirectionKeyboard == Vector3.zero && moveDirectionGamepad == Vector3.zero)
			{
				// Neither is being used
			}
			else
			{
				// Moving with GP
				Debug.Log("GP");
				keyboardMove = false;
			}

			moveDirectionKeyboard = moveDirectionKeyboard.normalized;

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
			if (moveDirectionKeyboard != Vector3.zero && joystickRotateX == 0 && joystickRotateY == 0 && mouseDelta.x == 0 && mouseDelta.y == 0 && mouseRotate == false)
			{
				character.transform.rotation = Quaternion.LookRotation(moveDirectionKeyboard);
			}
			// Mouse being used, keep old rotation
			if (joystickRotateX == 0 && joystickRotateY == 0 && mouseDelta.x == 0 && mouseDelta.y == 0 && mouseRotate == true)
			{
				character.transform.rotation = package.rotation;
			}
			
			CalculateMovementSpeed();
			//Debug.Log(currentMovementSpeed);
			
			// Save direction when not moving
			// Because direction is required even when not giving input for deceleration
			if (moveDirectionKeyboard != Vector3.zero)
				oldDirectionKeyboard = moveDirectionKeyboard;
			if (moveDirectionGamepad != Vector3.zero)
				oldDirectionGamepad = moveDirectionGamepad;
			
			moveDirectionKeyboard = currentMovementSpeed > 0 ? oldDirectionKeyboard.normalized : Vector3.zero;
			moveDirectionGamepad = currentMovementSpeed > 0 ? oldDirectionGamepad.normalized : Vector3.zero;
			
			// Move
			if (keyboardMove)
				character.characterController.Move(moveDirectionKeyboard * currentMovementSpeed * Time.deltaTime);
			else
				character.characterController.Move(new Vector3(moveDirectionGamepad.x * gamepadMovementSpeed.x, 0f,moveDirectionGamepad.z * gamepadMovementSpeed.z) * Time.deltaTime);

			// Update package data
			package.position = character.gameObject.transform.position;
			package.rotation = character.gameObject.transform.rotation;
			package.velocity = (character.transform.position - lastPosition) / Time.deltaTime;
			
			return package;
		}

		// Acceleration/Deceleration
		public void CalculateMovementSpeed()
		{
			// Moving with keyboard
			if (keyboardMove)
			{
				// Accelerate walking
				if (moveDirectionKeyboard != Vector3.zero && currentMovementSpeed < maxSpeed)
				{
					if (accelerationTimer < 0f)
						accelerationTimer = 0f;
				
					accelerationTimer += Time.deltaTime;
					decelerationTimer += Time.deltaTime;
				
					currentMovementSpeed = maxSpeed * (accelerationTimer / accelerationTime);
					previousSpeed = currentMovementSpeed;
				}
				// Decelerate walking
				else if (moveDirectionKeyboard == Vector3.zero && currentMovementSpeed > 0f)
				{
					if (decelerationTimer > decelerationTime)
						decelerationTimer = decelerationTime;
				
					decelerationTimer -= Time.deltaTime;
					accelerationTimer -= Time.deltaTime;
				
					currentMovementSpeed = previousSpeed * (decelerationTimer / decelerationTime);
				}
				else if (moveDirectionKeyboard != Vector3.zero && currentMovementSpeed >= maxSpeed)
				{
					// Accelerate running
					if (Input.GetKey(KeyCode.LeftShift) == true && currentMovementSpeed < runSpeed)
					{
						if (accelerationRunTimer < 0f)
							accelerationRunTimer = 0f;
						if (decelerationRunTimer <= 0)
							decelerationRunTimer = 0f;
						
						accelerationRunTimer += Time.deltaTime;
						decelerationRunTimer += Time.deltaTime;

						float acceleratedSpeed = runSpeed * (accelerationRunTimer / accelerationRunTime);

						if (acceleratedSpeed > maxSpeed)
							currentMovementSpeed = runSpeed * (accelerationRunTimer / accelerationRunTime);
						else
							currentMovementSpeed = maxSpeed + acceleratedSpeed;

						previousSpeed = currentMovementSpeed;
					}
					// Maximum running speed
					else if (Input.GetKey(KeyCode.LeftShift) == true && currentMovementSpeed >= runSpeed)
					{
						currentMovementSpeed = runSpeed;
						decelerationRunTimer = decelerationRunTime;
						previousSpeed = currentMovementSpeed;
					}
					// Decelerate running
					else if (Input.GetKey(KeyCode.LeftShift) == false && currentMovementSpeed > maxSpeed)
					{
						if (decelerationRunTimer > decelerationRunTime)
							decelerationRunTimer = decelerationRunTime;

						decelerationRunTimer -= Time.deltaTime;
						accelerationRunTimer -= Time.deltaTime;

						currentMovementSpeed = previousSpeed * (decelerationRunTimer / decelerationRunTime);
						if (currentMovementSpeed < maxSpeed)
							currentMovementSpeed = maxSpeed;
					}
					// Max walking speed
					else
						currentMovementSpeed = maxSpeed;
				}
				else
				{
					accelerationTimer = 0f;
					decelerationTimer = decelerationTime;
					currentMovementSpeed = 0;
				}
			}
			// Moving with gamepad
			else
			{
				if (moveDirectionGamepad != Vector3.zero && currentMovementSpeed < maxSpeed)
				{
					gamepadMovementSpeed = new Vector3(x:maxSpeed, y:0, z:maxSpeed);
					gamepadPreviousMovementSpeed = new Vector3(moveDirectionGamepad.x * gamepadMovementSpeed.x, 0f,
						moveDirectionGamepad.z * gamepadMovementSpeed.z) * Time.deltaTime;
				}
				else if ((moveDirectionGamepad.x == 1 || moveDirectionGamepad.z == 1) && currentMovementSpeed < maxSpeed)
				{
					gamepadMovementSpeed = new Vector3(x:maxSpeed, y:0, z:maxSpeed);
					gamepadPreviousMovementSpeed = new Vector3(moveDirectionGamepad.x * gamepadMovementSpeed.x, 0f,
						moveDirectionGamepad.z * gamepadMovementSpeed.z) * Time.deltaTime;
					Debug.Log("AC");
				}
				else if (moveDirectionGamepad == Vector3.zero && currentMovementSpeed > 0f)
				{
					Debug.Log("DC");
					if (decelerationTimer > decelerationTime)
						decelerationTimer = decelerationTime;
				
					decelerationTimer -= Time.deltaTime;
					accelerationTimer -= Time.deltaTime;
				
					gamepadMovementSpeed = gamepadPreviousMovementSpeed * (decelerationTimer / decelerationTime);
				}
				else
				{
					Debug.Log("STILL");
					gamepadMovementSpeed = Vector3.zero;
				}
			}
		}
	}
}
