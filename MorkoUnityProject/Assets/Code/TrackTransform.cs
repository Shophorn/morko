using UnityEngine;

[ExecuteInEditMode]
public class TrackTransform : MonoBehaviour
{
	public Transform target;
	public Vector3 offset;
	public bool trackPosition = true;
	public bool trackRotation = true;


	private void OnValidate() => Move();
	private void Update() => Move();

	private void Move()
	{
		if (target == transform)
		{
			target = null;
		}

		if (target == null)
		{
			return;	
		}

		if (trackPosition)
			transform.position = target.position + offset;
		
		if (trackRotation)
			transform.rotation = target.rotation;
	}
}