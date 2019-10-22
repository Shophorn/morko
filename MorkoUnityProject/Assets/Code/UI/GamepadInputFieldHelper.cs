using UnityEngine;
using UnityEngine.UI;

namespace Morko
{
	public class GamepadInputFieldHelper : MonoBehaviour
	{
		private InputField inputField;

		private void Start()
		{
			inputField = GetComponent<InputField>();
		}

		private void Update()
		{
			if (Input.GetButtonUp("Cancel") || Input.GetButtonUp("Submit"))
				inputField.DeactivateInputField();
		}
	}
}