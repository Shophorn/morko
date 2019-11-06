using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScrollContent : MonoBehaviour
{
	#region Public Properties
	public ListItem listItem;
	public ListItem[] listElements;
	public GameObject listGrid;

	public ListItem currentItem;
	public float itemWidth;

	public int SelectedIndex { get => selectedItemIndex; }
	public float ItemSpacing { get { return itemSpacing; } }
	public float HorizontalMargin { get { return horizontalMargin; } }
	public float VerticalMargin { get { return verticalMargin; } }
	public bool Horizontal { get { return horizontal; } }
	public bool Vertical { get { return vertical; } }
	public float Width { get { return width; } }
	public float Height { get { return height; } }

	public event Action<int> OnSelectionChanged;
	#endregion

	#region Private Members
	private RectTransform rectTransform;
	private float width, height;
	[SerializeField]
	private int selectedItemIndex;
	[SerializeField]
	private float itemSpacing;
	[SerializeField]
	private float horizontalMargin, verticalMargin;
	[SerializeField]
	private bool horizontal, vertical;
	#endregion

	private void OnEnable()
	{
		itemWidth = listItem.gameObject.GetComponent<RectTransform>().rect.width;

		rectTransform = GetComponent<RectTransform>();
		rectTransform.sizeDelta = new Vector2(listElements.Length * (rectTransform.rect.width + itemSpacing), 100);

		width = rectTransform.rect.width;
		height = rectTransform.rect.height;
	}
	private void OnDisable()
	{
		rectTransform.sizeDelta = new Vector2(100, 100);
		transform.localPosition = Vector3.zero;
		ClearSelectionList();
	}

	private void ClearSelectionList()
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			Destroy(transform.GetChild(i).gameObject);
		}
	}
	private void InitializeContentHorizontal()
	{
		float originX = 0 - (width);
		float posOffset = width * 0.566666666667f;
		for (int i = 0; i < listElements.Length; i++)
		{
			Vector3 childPos = listElements[i].transform.localPosition;
			childPos.x = originX + posOffset + i * (itemWidth + itemSpacing + 12.25f);
			listElements[i].transform.localPosition = childPos;
		}
	}

	IEnumerator LerpTowardsCenter()
	{
		float time = 0;
		currentItem = CheckCurrentItem();

		while (time < 1.0f)
		{
			time += Time.deltaTime;
			transform.localPosition = new Vector3(Mathf.Lerp(transform.localPosition.x, transform.localPosition.x - currentItem.transform.position.x, time), transform.localPosition.y, transform.localPosition.z);
			yield return null;
		}
	}

	private ListItem CheckCurrentItem()
	{
		float shortestDistance = 1000;
		float distance = 0;
		ListItem shortest = null;
		foreach (var item in listElements)
		{
			distance = Vector3.Distance(item.transform.position, listGrid.transform.position);
			if (distance < shortestDistance)
			{
				shortestDistance = distance;
				shortest = item;
			}
		}
		currentItem = shortest;
		int tempIndex = selectedItemIndex;
		selectedItemIndex = currentItem.ID;
		if (tempIndex != selectedItemIndex)
			OnSelectionChanged?.Invoke(selectedItemIndex);

		return currentItem;
	}
	public void SetOptions(string[] names, GameObject[] objects)
	{
		if (names.Length != objects.Length)
		{
			Debug.LogError("Lengths of passed arrays do not match.");
			return;
		}
		listElements = new ListItem[names.Length];

		for (int i = 0; i < listElements.Length; i++)
		{
			listElements[i] = Instantiate(listItem, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.identity);
			listElements[i].ID = i;
			listElements[i].Model = objects[i];
			listElements[i].Name = names[i];

			//TODO (Joonas): Working text element for the scrolling elements
			//listElements[i].transform.Find("NameLabel").GetComponent<Text>().text = names[i];

			listElements[i].transform.parent = transform;
		}

		InitializeContentHorizontal();
		currentItem = CheckCurrentItem();
		transform.Translate(-currentItem.transform.localPosition);
	}
}