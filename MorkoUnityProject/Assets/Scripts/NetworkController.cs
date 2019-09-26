using Unity;
using UnityEngine;
using Morko.Network;
	
namespace Morko
{
	public class NetworkController
	{
		private Character character;

		public static NetworkController Create(Character character)
		{
			var result = new NetworkController();
			result.character = character;
			return result;
		}

		public void Update()
		{

		}

		public void Update(AvatarPackage package)
		{
			 // Todo(Iiro, Leo): Implement and control character
		}
	}
}
