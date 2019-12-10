using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(MaskController))]
public class ChildCatcher : MonoBehaviourPun
{
	public float angledViewRange;
	public float angledViewAngle;
	public LayerMask characterLayerMask;

	public float radialViewRange = 3;
	public float waitBeforeCatchingAgainTime = 5;
	private float canCatchTime = 0;

	private readonly Collider [] overlapColliders = new Collider[20]; 

	private MaskController mask;

	private void Awake()
	{
		mask = GetComponent<MaskController>();
	}

	private void Update()
	{
		bool doUpdate = photonView.IsMine
						&& mask.enabled
						&& mask.currentMorko != null
						&& canCatchTime < Time.time;

		if (doUpdate == false)
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

			var character = overlapColliders[colliderIndex].GetComponent<Character>();
			if (character == null)
				continue;

			var hitTransform = overlapColliders[colliderIndex].transform;
			Vector3 hitForward = hitTransform.forward;
			Vector3 fromHitToHere = (transform.position - hitTransform.position).normalized;
			float angle = Vector3.Angle(hitForward, fromHitToHere);

			bool catchChild = false;

			if (Mathf.Abs(angle) < angledViewAngle)
			{
				Debug.Log("[CHILD CATCHER]: Caught by angle");
				catchChild = true;
			}
			else if (Vector3.Distance(transform.position, hitTransform.position) < radialViewRange)
			{
				Debug.Log("[CHILD CATCHER]: Caught by radialViewRange");
				catchChild = true;
			}

			if (catchChild)
			{
				canCatchTime = Time.time + waitBeforeCatchingAgainTime;
				mask.SwitchMorko(hitTransform);
			}
		}
	}
}