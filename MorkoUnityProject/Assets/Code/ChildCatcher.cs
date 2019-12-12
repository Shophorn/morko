using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(MaskController))]
public class ChildCatcher : MonoBehaviourPun, IPunObservable
{


	public float angledViewRange;
	public float angledViewAngle;
	public LayerMask characterLayerMask;

	public float radialViewRange = 1.5f;
	public float waitBeforeCatchingAgainTime = 5;
	private float canCatchTime = 0;
	private float canCatchCountDown = -1;

	private readonly Collider [] overlapColliders = new Collider[20]; 

	void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(canCatchCountDown);
		}
		else if (stream.IsReading)
		{
			canCatchCountDown = (float)stream.ReceiveNext();
		}
	}
	private MaskController mask;

	private void Awake()
	{
		mask = GetComponent<MaskController>();
	}

	private void Update()
	{
		bool doUpdate = photonView.IsMine

						// Todo(Leo): These should be redundant, but stuff do not work currently
						&& mask.morkoState != MaskController.MorkoState.IdleInBeginning
						&& mask.currentMorko != null
						&& (mask.IsTransferingToOtherCharacter == false);

		if (doUpdate == false)
			return;

		if (canCatchCountDown > 0)
		{
			canCatchCountDown -= Time.deltaTime;
			return;
		}


		int colliderCount = Physics.OverlapSphereNonAlloc(	transform.position,
															angledViewRange,
															overlapColliders,
															characterLayerMask);

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

			bool doCatchChild = false;

			if (Mathf.Abs(angle) < angledViewAngle)
			{
				Debug.Log("[CHILD CATCHER]: Caught by angle");
				doCatchChild = true;
			}
			else if (Vector3.Distance(transform.position, hitTransform.position) < radialViewRange)
			{
				Debug.Log("[CHILD CATCHER]: Caught by radialViewRange");
				doCatchChild = true;
			}

			if (doCatchChild)
			{
				canCatchCountDown = waitBeforeCatchingAgainTime;
				canCatchTime = Time.time + waitBeforeCatchingAgainTime;
				mask.SwitchMorko(hitTransform);
			}
		}
	}
}