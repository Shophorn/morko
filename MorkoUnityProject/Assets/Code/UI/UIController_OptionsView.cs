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

		public Button cancelButton;
	}
	[SerializeField] private OptionsView optionsView;

	private void InitializeOptionsView()
	{
		optionsView.cancelButton.onClick.AddListener (() =>
		{
			EventSystem.current.SetSelectedGameObject(mainView.hostViewButton.gameObject);
			SetMainView();
		}); 
	}
}