using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ToggleGroup))]
public class AvailableServersSelector : Selectable
{
	[SerializeField] private ServerToggleListItem toggleItem;
	public Transform toggleParent;
	private ToggleGroup toggleGroup;

	public int SelectedIndex { get; private set; }


	public Action<int> OnSelectionChanged;
	public event Action OnServerListUpdated;

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
		OnServerListUpdated?.Invoke();
	}
	private void ClearToggleList()
	{
		toggleParent.DestroyAllChildren();
	}
}
