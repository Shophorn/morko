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

		private float speed = 10;
		private LayerMask groundMask = 1 << 9;
		
		// Todo(Sampo): Input support for multiple platforms (Mac, Linux)

		public AvatarPackage Update()
		{
			Vector3 lastPosition = character.gameObject.transform.position;
			// Move direction
			var moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical")) * speed;
			
			float joystickRotateX = Input.GetAxis("RotateX");
			float joystickRotateY = Input.GetAxis("RotateY");

			Vector3 currentMousePosition = Input.mousePosition;
			Vector3 mouseDelta = currentMousePosition - lastMousePosition;
			
			// Rotate
			// Mouse moved, use mouse
			if (Mathf.Abs(mouseDelta.x) > 0 || Mathf.Abs(mouseDelta.y) > 0)
			{
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
				Vector3 lookDirectionJoystick = new Vector3(Input.GetAxis("RotateX"), 0f, Input.GetAxis("RotateY"));
				Quaternion lookRotation = Quaternion.LookRotation(lookDirectionJoystick, Vector3.up);
       
				float step = speed * Time.deltaTime;
				character.transform.rotation = Quaternion.RotateTowards(lookRotation, character.transform.rotation, step);
			}
			// Keep old rotation
			else if (joystickRotateX == 0 && joystickRotateY == 0 && mouseDelta.x == 0 && mouseDelta.y == 0)
			{
				character.transform.rotation = package.rotation;
			}
			
			// Move
			character.characterController.Move(moveDirection * Time.deltaTime);
			
			package.position = character.gameObject.transform.position;
			package.rotation = character.gameObject.transform.rotation;
			package.velocity = (character.transform.position - lastPosition) / Time.deltaTime;
			
			Debug.Log((package.velocity));
            			
			return package;
		}
	}
}
