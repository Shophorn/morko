using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScrollContent : MonoBehaviour
{
	public ListItem listItem;
	public ListItem[] listElements;

	public string[] nimet;
	public GameObject[] objektit;

	public ListItem currentItem;

	public GameObject center;
	public float radius;
	public float angle;

	public TextMesh nameLabel;

	public int SelectedIndex { get => selectedItemIndex; }

	public event Action<int> OnSelectionChanged;

	[SerializeField]
	private int selectedItemIndex;

	private void OnEnable()
	{

		listElements = new ListItem[nimet.Length];


		InstantiateContentCircular(nimet, objektit);
	}
	private void OnDisable()
	{
		ClearSelectionList();
	}

	private void ClearSelectionList()
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			Destroy(transform.GetChild(i).gameObject);
		}
	}

	private void InstantiateContentCircular(string[] names, GameObject[] objects)
	{
		angle = 360f / (float)listElements.Length;
		for(int i = 0; i < listElements.Length; i++)
		{
			Quaternion rotation = Quaternion.AngleAxis(i * angle, Vector3.up);
			Vector3 direction = rotation * Vector3.back;

			Vector3 position = center.transform.position - (direction * radius);
			listElements[i] = Instantiate(listItem, position,rotation);
			listElements[i].transform.SetParent(transform);
			listElements[i].listItemId = i;
			listElements[i].listItemName = names[i];
			objects[i].transform.localScale = new Vector3(30,30,30);
			Instantiate(objects[i], listElements[i].transform.position, rotation).transform.SetParent(listElements[i].transform);
		}
		CheckCurrentItem();
	}

	public ListItem CheckCurrentItem()
	{
		Vector3 selectedPosition = new Vector3(0,center.transform.position.y,-180);
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
		int tempIndex = selectedItemIndex;
		selectedItemIndex = currentItem.listItemId;
		if (tempIndex != selectedItemIndex)
			OnSelectionChanged?.Invoke(selectedItemIndex);

		nameLabel.text = currentItem.listItemName;

		return currentItem;
	}

	private void SetCurrentSelectionIndex(int itemIndex)
	{
		ListItem currentListItem = listElements[itemIndex];
		selectedItemIndex = currentListItem.listItemId;
	}


	public void SetOptions(string[] names, GameObject[] objects)
	{
		if (names.Length != objects.Length)
		{
			Debug.LogError("Lengths of passed arrays do not match.");
			return;
		}
		ClearScrollingList();

		listElements = new ListItem[nimet.Length];
		InstantiateContentCircular(nimet,objektit);

		OnSelectionChanged += SetCurrentSelectionIndex;
		currentItem = CheckCurrentItem();
	}

	private void ClearScrollingList()
	{
		transform.DestroyAllChildren();
	}
}