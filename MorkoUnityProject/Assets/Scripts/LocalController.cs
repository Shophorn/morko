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
		private float movementSpeed;
		private float sneakSpeed;
		private float runSpeed;
		private bool isMorko = false;

		public void changeState(bool toMorko)
		{
			if (toMorko)
			{
				isMorko = true;
				movementSpeed = playerSettings.morkoMovementSpeed;
				sneakSpeed = playerSettings.morkoMovementSpeed * playerSettings.morkoSneakMultiplier;
				runSpeed = playerSettings.morkoMovementSpeed * playerSettings.morkoRunMultiplier;
			}
			else
			{
				isMorko = false;
				movementSpeed = playerSettings.movementSpeed;
				sneakSpeed = playerSettings.movementSpeed * playerSettings.sneakMultiplier;
				runSpeed = playerSettings.movementSpeed * playerSettings.runMultiplier;
			}
		}

		public static LocalController Create(Character character)
		{
			var result = new LocalController();
			result.package = new AvatarPackage();

			result.package.id = 0;
			result.package.position = Vector3.zero;
			result.package.rotation = Quaternion.identity;
			result.package.velocity = Vector3.zero;

			result.character = character;
			result.camera = character.GetComponentInChildren<Camera>();
			return result;
		}

		// Todo(Sampo): Input support for multiple platforms (Mac, Linux)

		public AvatarPackage Update()
		{
			Vector3 lastPosition = character.gameObject.transform.position;
			// Move direction
			var moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical")) * movementSpeed;
			
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
				character.transform.rotation = Quaternion.LookRotation(lookDirection);
			}
			// If joystick is used for rotation, use controller
			if (Mathf.Abs(joystickRotateX) > 0f || Mathf.Abs(joystickRotateY) > 0f )
			{
				mouseControl = false;
				Vector3 lookDirectionJoystick = new Vector3(Input.GetAxis("RotateX"), 0f, Input.GetAxis("RotateY"));
				Quaternion lookRotation = Quaternion.LookRotation(lookDirectionJoystick, Vector3.up);
       
				float step = movementSpeed * Time.deltaTime;
				character.transform.rotation = Quaternion.RotateTowards(lookRotation, character.transform.rotation, step);
			}
			// Controller being used, right joystick not being used, look towards player forward
			if (joystickRotateX == 0 && joystickRotateY == 0 && mouseDelta.x == 0 && mouseDelta.y == 0 && mouseControl == false)
			{
				character.transform.rotation = Quaternion.LookRotation(moveDirection);
			}
			// Mouse beign used, keep old rotation
			if (joystickRotateX == 0 && joystickRotateY == 0 && mouseDelta.x == 0 && mouseDelta.y == 0 && mouseControl == true)
			{
				character.transform.rotation = package.rotation;
			}

			// Move
			character.characterController.Move(moveDirection * Time.deltaTime);
			
			// Update package data
			package.position = character.gameObject.transform.position;
			package.rotation = character.gameObject.transform.rotation;
			package.velocity = (character.transform.position - lastPosition) / Time.deltaTime;
			
			return package;
		}
	}
}
