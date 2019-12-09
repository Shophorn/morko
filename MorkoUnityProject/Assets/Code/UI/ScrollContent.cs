using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScrollContent : MonoBehaviour
{
	public ListItem listItem;
	public ListItem[] listElements;

	public ListItem currentItem;

	public float radius;
	public float angle;

	public int SelectedIndex { get => selectedItemIndex; }
	public event Action<int> OnSelectionChanged;

	[SerializeField]
	private int selectedItemIndex;

	private void InstantiateContentCircular(string[] names, GameObject[] objects)
	{
		angle = 360f / (float)listElements.Length;
		if (listElements.Length != 0)
		{
			for(int i = 0; i < listElements.Length; i++)
			{
				Quaternion rotation = Quaternion.AngleAxis(i * angle, Vector3.up);
				Vector3 direction = rotation * Vector3.back;

				Vector3 position = transform.position + (direction * radius);
				listElements[i] = Instantiate(listItem, position,rotation);
				listElements[i].transform.SetParent(transform);
				listElements[i].listItemId = i;
				listElements[i].listItemName = names[i];
				Instantiate(objects[i], listElements[i].transform.position, rotation).transform.SetParent(listElements[i].transform);
				listElements[i].transform.Rotate(0f, listElements[i].transform.rotation.y+180-(i*angle), 0f, 0f);
			}
			CheckCurrentItem();
		}
	}

	public ListItem CheckCurrentItem()
	{
		Vector3 selectedPosition = new Vector3(0,transform.position.y,-190);
		float shortestDistance = 1000;
		float distance = 0;
		ListItem shortest = null;
		foreach (var item in listElements)
		{
			distance = Vector3.Distance(item.transform.position, selectedPosition);
			if (distance < shortestDistance)
			{
				shortestDistance = distance;
				shortest = item;
			}
		}
		currentItem = shortest;
		Debug.Log(currentItem);
		int tempIndex = selectedItemIndex;
		selectedItemIndex = currentItem.listItemId;
		if (tempIndex != selectedItemIndex)
			OnSelectionChanged?.Invoke(selectedItemIndex);

		return currentItem;
	}

	private void SetCurrentSelectionIndex(int itemIndex)
	{
		ListItem currentListItem = listElements[itemIndex];
		selectedItemIndex = currentListItem.listItemId;
	}


	public void SetOptions(string[] names, GameObject[] objects)
	{
		Debug.Log("Options set");

		if (names.Length != objects.Length)
		{
			Debug.LogError("Lengths of passed arrays do not match.");
			return;
		}
		ClearScrollingList();

		listElements = new ListItem[names.Length];
		InstantiateContentCircular(names,objects);

		OnSelectionChanged += SetCurrentSelectionIndex;
		currentItem = CheckCurrentItem();
	}

	private void ClearScrollingList()
	{
		transform.DestroyAllChildren();
	}
}