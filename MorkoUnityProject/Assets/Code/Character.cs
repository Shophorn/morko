using System;
using UnityEngine;

namespace Morko
{
	public class Character : MonoBehaviour
	{
		public CharacterController characterController;
		public AnimatorController animatorController;
		[HideInInspector]
		public LocalController localController;

		private DisableMovement disableMovement;

		private LocalController lc
		{
			get => localController;
			set => localController = value;
		}

		private void Start()
		{
			characterController = GetComponent<CharacterController>();
			animatorController = GetComponent<AnimatorController>();
			disableMovement = GetComponent<DisableMovement>();
		}

		private void OnControllerColliderHit(ControllerColliderHit hit)
		{
			if (hit.collider.CompareTag("Avatar"))
			{
				/*
				LocalController hitCharacterLocalController = hit.collider.GetComponent<Character>().localController;
				
				if (!localController.isMorko && !hitCharacterLocalController.isMorko) return;
				
				// TODO (Sampo/Leo): Both characters are morko, conflict
				if (localController.isMorko && hitCharacterLocalController.isMorko)
					throw new NotImplementedException();

				localController.ChangeState();
				hitCharacterLocalController.ChangeState();
				*/
				localController.ChangeState();
			}
		}
	
		public void EnableDisableMovementScript()
		{
			disableMovement.enabled = true;
		}
		
		public void DisableDisableMovementScript()
		{
			disableMovement.enabled = false;
		}
	}
}
