using UnityEngine;
using UInt8 = System.Byte;

namespace Morko.Network
{
	[System.Flags]
	enum AvatarFlags
	{
		Attack = 1 << 0,
		Parry = 1 << 1,
	}

	public class AvatarPackage
	{
		public UInt8 id;
		public Vector3 position;
		public Quaternion rotation;
		public Vector3 velocity;
		AvatarFlags flags;
	}

	public class Client : MonoBehaviour
	{
		LocalController localController = null;
		NetworkController [] networkControllers;

		void StartGame()
		{
			// Create characters and controllers
			
		}

		void Update()
		{
			localController.Update();

			bool weHaveNewPackage = false;
			AvatarPackage package = null;

			foreach(var controller in networkControllers)
			{
				if (weHaveNewPackage)
				{
					controller.Update(package);
				}
				else
				{
					controller.Update();
				}

			}
		}
	}
}