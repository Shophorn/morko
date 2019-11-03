using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ToggleGroup))]
public class ToggleContainer : Selectable
{
	[SerializeField] private ServerToggleListItem toggleItem;
	[SerializeField] private Transform toggleParent;
	private ToggleGroup toggleGroup;

	public Transform ToggleParent { get => toggleParent; }
	public int SelectedIndex { get; private set; }


	public Action<int> OnSelectionChanged;

	public void Awake()
	{
		toggleGroup = GetComponent<ToggleGroup>();
	}

	public void SetOptions(string[] serverNames)
	{
		ClearToggleList();

		/* Todo(Leo): keep track of selected server, as index is likely to change
	 	For example, get current selected servers name, and in the end find if it 
		is in new ones, and set it as active */

		int serverCount = serverNames.Length;
		int toggleHeight = 20;
		for (int serverIndex = 0; serverIndex < serverCount; serverIndex++)
		{
			/* Note(Leo): this is done because in c# for loop keeps index as
			reference (or something), so it would keep increasing, and any calls
			after loop would point to value of last iteration (aka count - 1) */
			int selectedIndex = serverIndex;

			var toggleInstance = Instantiate(toggleItem, toggleParent);

			float yPosition = toggleHeight * selectedIndex;
			toggleInstance.transform.localPosition = new Vector3(0, -yPosition, 0);

			toggleInstance.Label.text = serverNames[selectedIndex];
			toggleInstance.Toggle.group = toggleGroup;

			void SetIndex(bool ignored)
			{
				if (toggleInstance.Toggle.isOn)
				{
					SelectedIndex = selectedIndex;
					OnSelectionChanged?.Invoke(selectedIndex);
				}
				else  {/* nothing */} 
			}
			toggleInstance.Toggle.onValueChanged.AddListener(SetIndex);
		}
		ToggleNavigation();
	}

	private void ClearToggleList()
	{
		toggleParent.DestroyAllChildren();
	}

	public void ToggleNavigation()
	{
		//for (int i = 0; i < ToggleParent.childCount; i++)
		//{
		//	Navigation nav = toggleParent.GetChild(i).GetComponent<Toggle>().navigation;
		//	if (i == 0)
		//	{
		//		nav.selectOnUp = joinView.playerNameField;
		//		nav.selectOnDown = joinView.availableServersToggleParent.GetChild(i + 1).GetComponent<Toggle>();
		//	}
		//	else
		//	{
		//		nav.selectOnUp = joinView.availableServersToggleParent.GetChild(i - 1).GetComponent<Toggle>();
		//		if (i == joinView.availableServersToggleParent.childCount - 1)
		//		{
		//			nav.selectOnDown = joinView.requestJoinButton;
		//		}
		//		else
		//		{
		//			nav.selectOnDown = joinView.availableServersToggleParent.GetChild(i + 1).GetComponent<Toggle>();
		//		}
		//	}
		//	nav.selectOnLeft = nav.selectOnRight = joinView.availableServersToggleParent.GetChild(i).GetComponent<Toggle>();
		//	joinView.availableServersToggleParent.GetChild(i).GetComponent<Toggle>().navigation = nav;
		//}
		//Navigation nameNav = joinView.playerNameField.navigation;
		//nameNav.selectOnDown = joinView.availableServersToggleParent.GetChild(joinView.availableServersToggleParent.childCount - 1).GetComponent<Toggle>();
		//joinView.playerNameField.navigation = nameNav;
		//Navigation cancelNav = joinView.cancelButton.navigation;
		//cancelNav.selectOnUp = joinView.availableServersToggleParent.GetChild(0).GetComponent<Toggle>();
		//joinView.cancelButton.navigation = cancelNav;
		//Navigation joinNav = joinView.requestJoinButton.navigation;
		//joinNav.selectOnUp = joinView.availableServersToggleParent.GetChild(0).GetComponent<Toggle>();
		//joinView.requestJoinButton.navigation = joinNav;
	}
}
