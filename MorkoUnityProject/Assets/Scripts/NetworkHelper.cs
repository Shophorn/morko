using UnityEngine;

public class NetworkHelper : MonoBehaviour
{
	private NetworkTester2 joinConnection;
	private ServerTester hostConnection;

	private bool HasAction => (joinConnection != null) || (hostConnection != null);

	public void Host()
	{
		if (HasAction)
		{
			Debug.LogError("NetworkHelper is already hosting");
			return;
		}

		hostConnection = gameObject.AddComponent<ServerTester>();
	}

	public void CancelHost()
	{
		if (HasAction == false)
		{
			Debug.LogError("NetworkHelper is not hosting");
			return;
		}

		Destroy(hostConnection);
		hostConnection = null;
	}

	public void Join()
	{
		if (HasAction)
		{
			Debug.LogError("NetworkHelper is already joining");
			return;
		}

		joinConnection = gameObject.AddComponent<NetworkTester2>();
	}

	public void CancelJoin()
	{
		if (HasAction == false)
		{
			Debug.LogError("NetworkHelper is not joining");
			return;
		}

		Destroy(joinConnection);
		joinConnection = null;
	}
}