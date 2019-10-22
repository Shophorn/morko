// using UnityEngine;

// public class NetworkHelper : MonoBehaviour
// {
// 	public bool autoStartHosting;

// 	public bool autoStartJoining;
// 	public GameObject testAvatarPrefab;

// 	private ClientController clientConnection;
// 	private ServerController serverConnection;

// 	private bool HasAction => (clientConnection != null) || (serverConnection != null);

// 	public void Host()
// 	{
// 		if (HasAction)
// 		{
// 			Debug.LogError("NetworkHelper is already hosting");
// 			return;
// 		}

// 		serverConnection = gameObject.AddComponent<ServerController>();
// 		// serverConnection.AutoStart = autoStartHosting;
// 	}

// 	public void CancelHost()
// 	{
// 		if (HasAction == false)
// 		{
// 			Debug.LogError("NetworkHelper is not hosting");
// 			return;
// 		}

// 		Destroy(serverConnection);
// 		serverConnection = null;
// 	}
// }