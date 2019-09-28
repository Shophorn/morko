using UnityEngine;
using Morko.Network;

public class NetworkTester : MonoBehaviour
{
	private void Start()
	{
		var client = Client.CreateLanClient();
	}	
}