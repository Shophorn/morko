using Photon.Pun;
using UnityEngine;

public class HeadShrinker : MonoBehaviour, IPunObservable
{
	void
	IPunObservable.OnPhotonSerializeView(
		PhotonStream stream,
		PhotonMessageInfo messageInfo)
	{
		if (stream.IsReading)
		{
			transform.localScale = new Vector3
			{
				x = (float)stream.ReceiveNext(),
				y = (float)stream.ReceiveNext(),
				z = (float)stream.ReceiveNext(),
			};
		}
		else if (stream.IsWriting)
		{
			stream.SendNext(transform.localScale.x);
			stream.SendNext(transform.localScale.y);
			stream.SendNext(transform.localScale.z);
		}
	}
}