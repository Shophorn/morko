using UnityEngine;

namespace Morko.Network
{
	[System.Flags]
	enum AvatarFlags
	{
		Attack = 1
	}

	public class AvatarPackage
	{
		int id;
		Vector3 position;
		Quaternion rotation;
		Vector3 velocity;
		AvatarFlags flags;
	}


	public class Client
	{

	}
}