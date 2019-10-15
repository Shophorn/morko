using UnityEngine;

public class ControlsTest : MonoBehaviour
{
	private void Update()
	{
		float x = Input.GetAxis("Horizontal");
		float z = Input.GetAxis("Vertical");
		float step = Time.deltaTime * 10f;

		Vector3 movement = new Vector3(x * step, 0, z * step);

		transform.position += movement;
	}
}