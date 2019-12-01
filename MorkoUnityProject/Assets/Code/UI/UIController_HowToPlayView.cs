using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class UIController
{
    [Serializable]
	private struct HowToPlayView : IMenuLayout
	{
		public MenuView view;
		MenuView IMenuLayout.View => view;
		bool IMenuLayout.BelongsToMainMenu => true;

		public Button cancelButton;
	}
	[SerializeField] private HowToPlayView howToPlayView;

	private void InitializeHowToPlayView()
	{
		howToPlayView.cancelButton.onClick.AddListener(() =>
		{
			EventSystem.current.SetSelectedGameObject(mainView.hostViewButton.gameObject);
			SetMainView();
		});
	}
}
