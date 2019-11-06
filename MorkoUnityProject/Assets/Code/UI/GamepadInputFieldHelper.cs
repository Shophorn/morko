using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class GamepadInputFieldHelper : EventTrigger
{
	[SerializeField] private InputField inputField;
	private bool isActive = false;

	private void Update()
	{
		if(inputField.name == EventSystem.current.currentSelectedGameObject.name)
		{
			if (Input.GetButtonUp("Submit") || Input.GetButtonUp("Cancel"))
				inputField.DeactivateInputField();
		}
	}
}