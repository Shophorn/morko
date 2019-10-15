using UnityEngine;

public class NetworkHelper : MonoBehaviour
{
	public bool autoStartHosting;

	public bool autoStartJoining;
	public GameObject testAvatarPrefab;

	private ClientConnection clientConnection;
	private ServerConnection serverConnection;

	private bool HasAction => (clientConnection != null) || (serverConnection != null);

	public void Host()
	{
		if (HasAction)
		{
			Debug.LogError("NetworkHelper is already hosting");
			return;
		}

		serverConnection = gameObject.AddComponent<ServerConnection>();
		serverConnection.AutoStart = autoStartHosting;
	}

	public void CancelHost()
	{
		if (HasAction == false)
		{
			Debug.LogError("NetworkHelper is not hosting");
			return;
		}

		Destroy(serverConnection);
		serverConnection = null;
	}

	public void Join()
	{
		if (HasAction)
		{
			Debug.LogError("NetworkHelper is already joining");
			return;
		}

		clientConnection = gameObject.AddComponent<ClientConnection>();
		clientConnection.avatarPrefab = testAvatarPrefab;
		clientConnection.AutoStart = autoStartJoining;
	}

	public void CancelJoin()
	{
		if (HasAction == false)
		{
			Debug.LogError("NetworkHelper is not joining");
			return;
		}

		Destroy(clientConnection);
		clientConnection = null;
	}
}