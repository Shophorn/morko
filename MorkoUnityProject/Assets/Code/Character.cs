using System;
using UnityEngine;

public class Character : MonoBehaviour
{
	public CharacterController characterController;
	[HideInInspector]
	public LocalPlayerController localController;

	private DisableMovement disableMovement;

	private LocalPlayerController lc
	{
		get => localController;
		set => localController = value;
	}

	private void Start()
	{
		characterController = GetComponent<CharacterController>();
		disableMovement = GetComponent<DisableMovement>();
	}

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (hit.collider.CompareTag("Avatar"))
		{
			/*
			LocalPlayerController hitCharacterLocalPlayerController = hit.collider.GetComponent<Character>().localController;
			
			if (!localController.isMorko && !hitCharacterLocalPlayerController.isMorko) return;
			
			// TODO (Sampo/Leo): Both characters are morko, conflict
			if (localController.isMorko && hitCharacterLocalPlayerController.isMorko)
				throw new NotImplementedException();

			localController.ChangeState();
			hitCharacterLocalPlayerController.ChangeState();
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
