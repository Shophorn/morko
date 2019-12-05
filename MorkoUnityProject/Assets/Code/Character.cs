using Photon.Pun;
using UnityEngine;
using System.Collections;

public class Character : MonoBehaviourPun
{
	public bool Frozen { get; private set; }
	private float freezeEndTime;

	public void FreezeForSeconds(float seconds)
	{
		freezeEndTime = Time.time + seconds;

		StopAllCoroutines();
		StartCoroutine(FreezingRoutine());
	}

	private IEnumerator FreezingRoutine()
	{
		Frozen = true;
		while(Time.time < freezeEndTime)
		{
			yield return null;
		}
		Frozen = false;
	}

	public void SetMorkoLevel(float level)
	{
		transform.localScale = Vector3.one * (1.0f + level);
	}
}