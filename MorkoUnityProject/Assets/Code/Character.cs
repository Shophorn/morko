using UnityEngine;

namespace Morko
{
	public class Character : MonoBehaviour
	{
		public CharacterController characterController;
		public LocalController lc;

		private LocalController localController
		{
			get => lc;
			set => lc = value;
		}

		private void Start()
		{
			characterController = GetComponent<CharacterController>();
		}

		private void OnControllerColliderHit(ControllerColliderHit hit)
		{
			if (hit.collider.CompareTag("Avatar"))
			{
				Debug.Log("COLLISION BETWEEN CHARACTERS");
				LocalController hitCharacterLocalController = hit.collider.GetComponent<Character>().localController;

				if (!lc.isMorko && !hitCharacterLocalController.isMorko) return;
				
				lc.ChangeState();
				hitCharacterLocalController.ChangeState();
			}
				
		}
	}
}
