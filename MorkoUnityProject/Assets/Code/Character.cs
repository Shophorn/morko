using System;
using Morko;
using UnityEngine;

public class Character : MonoBehaviour
{
	public CharacterController characterController;
	public AnimatorController animatorController;
	[HideInInspector]
	public LocalPlayerController localController;


	private LocalPlayerController lc
	{
		get => localController;
		set => localController = value;
	}

	private void Start()
	{
		characterController = GetComponent<CharacterController>();
		animatorController = GetComponent<AnimatorController>();
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
}
