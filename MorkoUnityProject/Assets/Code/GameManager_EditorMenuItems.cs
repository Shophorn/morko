public partial class GameManager
{
	[UnityEditor.MenuItem("Morko/GameManager/Spawn Mask")]
	private static void SpawnMask()
	{
		if (instance == null)
			return;

		int actorNumber = instance.localCharacterActorNumber;
		instance.photonView.RPC(nameof(SetCharacterMorkoRPC), Photon.Pun.RpcTarget.All, actorNumber);
	}

	[UnityEditor.MenuItem("Morko/GameManager/End Game")]
	private static void EndGame()
	{
		if (instance == null)
			return;

		// instance.photonView.RPC(nameof(LoadEndSceneRPC), Photon.Pun.RpcTarget.All);
		instance.photonView.RPC(nameof(EndGameRPC), Photon.Pun.RpcTarget.All);
	}
}