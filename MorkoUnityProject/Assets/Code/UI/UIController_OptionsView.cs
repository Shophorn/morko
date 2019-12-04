using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class UIController
{
	[Serializable]
	private struct OptionsView : IMenuLayout
	{
		public MenuView view;

		MenuView IMenuLayout.View => view;
		bool IMenuLayout.BelongsToMainMenu => true;

		public DiscreteInputField masterVolumeField;
		public DiscreteInputField musicVolumeField;
		public DiscreteInputField characterVolumeField;
		public DiscreteInputField sfxVolumeField;

		public Button cancelButton;
	}
	[SerializeField] private OptionsView optionsView;

	private void InitializeOptionsView()
	{
		optionsView.masterVolumeField.OnValueChanged.AddListener(soundControllable.SetMasterVolume);
		optionsView.musicVolumeField.OnValueChanged.AddListener(soundControllable.SetMusicVolume);
		optionsView.characterVolumeField.OnValueChanged.AddListener(soundControllable.SetCharacterVolume);
		optionsView.sfxVolumeField.OnValueChanged.AddListener(soundControllable.SetSfxVolume);

		optionsView.cancelButton.onClick.AddListener (() =>
		{
			EventSystem.current.SetSelectedGameObject(mainView.hostViewButton.gameObject);
			SetMainView();
		}); 
	}
}