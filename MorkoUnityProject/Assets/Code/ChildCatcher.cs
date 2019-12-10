using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(MaskController))]
public class ChildCatcher : MonoBehaviourPun
{
	public float angledViewRange;
	public float angledViewAngle;
	public LayerMask characterLayerMask;

	public float radialViewRange = 3;

	private readonly Collider [] overlapColliders = new Collider[20]; 

	private MaskController mask;

	private void Awake()
	{
		mask = GetComponent<MaskController>();
	}

	private void Update()
	{
		if (photonView.IsMine == false)
			return;

		int colliderCount = Physics.OverlapSphereNonAlloc(	transform.position,
															angledViewRange,
															overlapColliders,
															characterLayerMask);

		if (colliderCount > 0)
		{
			Debug.Log("[CHILD CATCHER]: We got a hit");
		}

		for (int colliderIndex = 0; colliderIndex < colliderCount; colliderIndex++)
		{
			// Note(Leo): Skip our own collider
			if (overlapColliders[colliderIndex].gameObject == mask.currentMorko.gameObject)
				continue;

			var hitTransform = overlapColliders[colliderIndex].transform;
			Vector3 hitForward = hitTransform.forward;
			Vector3 fromHitToHere = (transform.position - hitTransform.position).normalized;
			float angle = Vector3.Angle(hitForward, fromHitToHere);

			bool catchChild = false;

			if (angle < angledViewAngle)
			{
				catchChild = true;
			}
			else if (Vector3.Distance(transform.position, hitTransform.position) < radialViewRange)
			{
				catchChild = true;
			}

			if (catchChild)
			{
				mask.SwitchMorko(hitTransform);
			}
		}
	}
}