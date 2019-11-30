// using Photon.Pun;
// using UnityEngine;

// [RequireComponent(typeof(CharacterController))]
// public class PlayerController : MonoBehaviourPun
// {
// 	private class Keyboard
// 	{
// 		public static readonly string MoveX = "Horizontal";
// 		public static readonly string MoveY = "Vertical";
// 	}

// 	private static readonly float gravity = -9.81f;
// 	private float ySpeed = 0f;

// 	private float xSpeed = 0f;
// 	private float zSpeed = 0f;

// 	private CharacterController characterController;

// 	public float forwardSpeed = 5f;
// 	public float acceleration = 1f;
// 	public float deceleration = 0.5f;

// 	private void Awake()
// 	{
// 		characterController = GetComponent<CharacterController>();
// 	}

// 	private void Update()
// 	{
// 		if (photonView.IsMine)
// 		{
// 			MoveLocal();
// 		}
// 	}

// 	private static float Vector2Length(float x, float y)
// 	{
// 		float result = Mathf.Sqrt(x * x + y * y);
// 		return result;
// 	}

// 	private void MoveLocal()
// 	{
// 		// Note(Leo): Keyboard input
// 		float xInput = Input.GetAxis(Keyboard.MoveX);
// 		float zInput = Input.GetAxis(Keyboard.MoveY);

// 		float xModifier = xInput * acceleration * Time.deltaTime;


// 		xSpeed += xInput * forwardSpeed * acceleration * Time.deltaTime;
// 		zSpeed += zInput * forwardSpeed * acceleration * Time.deltaTime;

// 		float xAbsSpeed = Mathf.Abs(xSpeed);
// 		float zAbsSpeed = Mathf.Abs(zSpeed);

// 		float xRelativePartOfSpeed = 1.0f;//xAbsSpeed / (xAbsSpeed + zAbsSpeed);
// 		float zRelativePartOfSpeed = 1.0f; //1.0f - xRelativePartOfSpeed;

// 		const float epsilon = 0.0001f;
// 		if(xAbsSpeed > epsilon && xInput < epsilon)
// 		{
// 			float sign = Mathf.Sign(xSpeed);
// 			xSpeed = sign * (xAbsSpeed - xRelativePartOfSpeed * deceleration * Time.deltaTime);
// 		}

// 		if(zAbsSpeed > epsilon && zInput < epsilon)
// 		{
// 			float sign = Mathf.Sign(zSpeed);
// 			zSpeed = sign * (zAbsSpeed - zRelativePartOfSpeed * deceleration * Time.deltaTime);
// 		}


// 		// bool speedIsNotZero = xAbsSpeed > epsilon || zAbsSpeed > epsilon;
// 		// bool inputIsZero = xInput < epsilon && zInput < epsilon;

// 		// if (speedIsNotZero && inputIsZero)
// 		// {

// 		// 	// float speedMagnitude = Vector2Length(xSpeed, zSpeed);
// 		// 	// float deceleratedSpeedMagnitude
// 		// 	// 	= Mathf.Max(0, speedMagnitude - deceleration * forwardSpeed * Time.deltaTime);
// 		// 	// float decelerationRatio;
// 		// 	// if (speedMagnitude > 0)
// 		// 	// {
// 		// 	// 	decelerationRatio = deceleratedSpeedMagnitude / speedMagnitude;
// 		// 	// }
// 		// 	// else 
// 		// 	// {
// 		// 	// 	decelerationRatio = 0;
// 		// 	// }
			
// 		// 	// xSpeed *= decelerationRatio;
// 		// 	// zSpeed *= decelerationRatio;

// 		// 	float xSign = Mathf.Sign(xSpeed);
// 		// 	float zSign = Mathf.Sign(zSpeed);

// 		// 	xSpeed = xSign * (xAbsSpeed - xRelativePartOfSpeed * deceleration * Time.deltaTime);
// 		// 	zSpeed = zSign * (zAbsSpeed - zRelativePartOfSpeed * deceleration * Time.deltaTime);
// 		// }

// 		xSpeed = Mathf.Clamp(xSpeed, -forwardSpeed, forwardSpeed);
// 		zSpeed = Mathf.Clamp(zSpeed, -forwardSpeed, forwardSpeed);

// 		// Gravity
// 		if (characterController.isGrounded)
// 		{
// 			ySpeed = 0;
// 		}
// 		ySpeed += gravity * Time.deltaTime;

// 		Vector3 movement = new Vector3(xSpeed, ySpeed, zSpeed) * Time.deltaTime;
// 		Debug.Log(movement);

// 		characterController.Move(movement);
// 	}

// }