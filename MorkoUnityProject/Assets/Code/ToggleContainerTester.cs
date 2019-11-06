using UnityEngine;

public class ToggleContainerTester : MonoBehaviour
{
	public bool button;

	public ServerInfo [] servers;

	private void DoTest()
	{
		var testSubject = GetComponent<IClientNetControllable>();
		testSubject.UpdateServersList(servers);
	// IClientNetControllable.UpdateServersList	
	}

	private void OnValidate()
	{
		if (button)
		{
			button = false;
			DoTest();
		}
	}
}