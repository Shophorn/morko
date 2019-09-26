using UnityEngine;
using Morko.Network;

namespace Morko
{
	public class LocalController
	{
		private Character character;

		public static LocalController Create(Character character)
		{
			var result = new LocalController();
			result.character = character;
			return result;
		}


		public AvatarPackage Update()
		{
			// Todo(Sampo): Implement like normal MonoBehaviour.Update
			return null;
		}
	}
}
