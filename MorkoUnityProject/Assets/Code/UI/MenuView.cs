using System;
using UnityEngine;

public class MenuView : MonoBehaviour
{
	public event Action OnShow;
	public event Action OnHide;

	public void Show()
	{
		if (gameObject.activeInHierarchy == false)
		{
			gameObject.SetActive(true);
			OnShow?.Invoke();
		}
	}

	public void Hide()
	{
		if (gameObject.activeInHierarchy)
		{
			gameObject.SetActive(false);
			OnHide?.Invoke();
		}
	}
}