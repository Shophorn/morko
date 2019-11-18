using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class GamepadInputFieldHelper : MonoBehaviour
{
	public InputField inputField;

	private void Update()
	{
		if(inputField.name == EventSystem.current.currentSelectedGameObject.name)
		{
			if (Input.GetButtonUp("Submit") || Input.GetButtonUp("Cancel"))
				inputField.DeactivateInputField();
		}
	}
}