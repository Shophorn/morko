using UnityEngine;

namespace Morko
{
	public class Character : MonoBehaviour
	{
		public CharacterController characterController;

		private void Start()
		{
			characterController = GetComponent<CharacterController>();
		}
	}
}
