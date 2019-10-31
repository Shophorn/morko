using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleContainer : Selectable
{
	[SerializeField] private ServerToggleListItem toggleItem;
	[SerializeField] private Transform toggleParent;
	[SerializeField] private ToggleGroup toggleGroup;
	[SerializeField] private string serverName;
	[SerializeField] private int selectedIndex;

	[SerializeField] private UIController uiController;

	public Transform ToggleParent { get => toggleParent; }
	public int SelectedIndex { get => selectedIndex; }


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

			/* Note(Leo): Unity documentation on Toggle.onValueChanged was unclear
			about what does the bool argument represent, so it is ignored here. */
			//void SetSelectedIndex(bool ignored)
			//{
			//	if (toggleInstance.Toggle.isOn)
			//	{
			//		joinView.selectedServerIndex = selectedIndex;

			//		joinView.hostingPlayerNameText.text = "Hosting Player";
			//		joinView.mapNameText.text = MapNameFromIndex(infos[selectedIndex].mapIndex);
			//		joinView.joinedPlayersCountText.text = infos[selectedIndex].maxPlayers.ToString();
			//		joinView.gameDurationText.text = TimeFormat.ToTimeFormat(infos[selectedIndex].gameDurationSeconds);
			//	}
			//}

			//toggleInstance.Toggle.onValueChanged.AddListener(SetSelectedIndex);
		}
		uiController.ToggleNavigation();
	}

	private void ClearToggleList()
	{
		toggleParent.DestroyAllChildren();
	}
}
