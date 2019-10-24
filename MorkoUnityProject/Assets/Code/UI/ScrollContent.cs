using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Morko
{
	public class ScrollContent : MonoBehaviour
	{
		#region Public Properties
		public ListItem listItem;
		public ListItem[] listElements;
		public GameObject listGrid;

		public InfiniteScroll[] selectionLists;

		public ListItem currentItem;
		public float itemWidth;

		//public Sprite[] levelImages;

		//private int numberOfScenes;
		public string[] levelNames;
		public string[] characterNames;

		/// <summary>
		/// How far apart each item is in the scroll view.
		/// </summary>
		public float ItemSpacing { get { return itemSpacing; } }

		/// <summary>
		/// How much the items are indented from left and right of the scroll view.
		/// </summary>
		public float HorizontalMargin { get { return horizontalMargin; } }

		/// <summary>
		/// How much the items are indented from top and bottom of the scroll view.
		/// </summary>
		public float VerticalMargin { get { return verticalMargin; } }

		/// <summary>
		/// Is the scroll view oriented horizontally?
		/// </summary>
		public bool Horizontal { get { return horizontal; } }

		/// <summary>
		/// Is the scroll view oriented vertically?
		/// </summary>
		public bool Vertical { get { return vertical; } }

		/// <summary>
		/// The width of the scroll content.
		/// </summary>
		public float Width { get { return width; } }

		/// <summary>
		/// The height of the scroll content.
		/// </summary>
		public float Height { get { return height; } }

		#endregion

		#region Private Members

		/// <summary>
		/// The RectTransform component of the scroll content.
		/// </summary>
		private RectTransform rectTransform;

		/// <summary>
		/// The width and height of the parent.
		/// </summary>
		private float width, height;


		/// <summary>
		/// How far apart each item is in the scroll view.
		/// </summary>
		[SerializeField]
		private float itemSpacing;

		/// <summary>
		/// How much the items are indented from the top/bottom and left/right of the scroll view.
		/// </summary>
		[SerializeField]
		private float horizontalMargin, verticalMargin;

		/// <summary>
		/// Is the scroll view oriented horizontall or vertically?
		/// </summary>
		[SerializeField]
		private bool horizontal, vertical;

		#endregion

		private void OnEnable()
		{
			itemWidth = listItem.gameObject.GetComponent<RectTransform>().rect.width;

			int selectionType = 0;
			foreach(var item in selectionLists)
			{
				if(item.isActiveAndEnabled)
				{
					selectionType = (int)item.type;
				}
			}
			
			Debug.Log("Selection list type = "+selectionType);
			CreateSelectionList(selectionType);

			rectTransform = GetComponent<RectTransform>();
			rectTransform.sizeDelta = new Vector2(listElements.Length * (rectTransform.rect.width + itemSpacing), 100);

			width = rectTransform.rect.width;
			height = rectTransform.rect.height;

			Debug.Log("Width: " + width);
			InitializeContentHorizontal();

			selectionLists[selectionType].listElements = listElements;
		}
		private void OnDisable()
		{
			rectTransform.sizeDelta = new Vector2(100,100);
			transform.localPosition = Vector3.zero;
			ClearSelectionList();
		}

		private void CreateSelectionList(int selectionType)
		{
			switch(selectionType)
			{
				case 0://Map
					listElements = new ListItem[levelNames.Length];
					break;
				case 1://Character
					listElements = new ListItem[characterNames.Length];
					break;
				default:
					Debug.Log("Something went wrong");
					break;
			}

			for (int i = 0; i < listElements.Length; i++)
			{
				listElements[i] = Instantiate(listItem, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.identity);
				SetListElementType(listElements[i], selectionType);
				listElements[i].listItemId = i;

				if(selectionType == 0)
				{
					listElements[i].listItemName = levelNames[i];
					listElements[i].transform.Find("Name").GetComponent<Text>().text = levelNames[i];
				}
				else
				{
					listElements[i].listItemName = characterNames[i];
					listElements[i].transform.Find("Name").GetComponent<Text>().text = characterNames[i];
				}
				listElements[i].transform.parent = transform;
			}
			currentItem = listElements[0];
		}

		private void ClearSelectionList()
		{
			for(int i = 0; i < transform.childCount; i++)
			{
				Destroy(transform.GetChild(i).gameObject);
			}
		}

		private void SetListElementType(ListItem item, int i)
		{
			switch(i)
			{
				case 0://Map
					item.transform.GetChild(i+1).gameObject.SetActive(false);
					item.transform.GetChild(i+2).gameObject.SetActive(true);
					break;
				case 1: //Character
					item.transform.GetChild(i).gameObject.SetActive(true);
					item.transform.GetChild(i+1).gameObject.SetActive(false);
					break;
			}
		}

		/// <summary>
		/// Initializes the scroll content if the scroll view is oriented horizontally.
		/// </summary>
		private void InitializeContentHorizontal()
		{
			float originX = 0 - (width);
			float posOffset = width * 0.566666666667f;
			for (int i = 0; i < listElements.Length; i++)
			{
				Vector3 childPos = listElements[i].transform.localPosition;
				childPos.x = originX + posOffset + i * (itemWidth+itemSpacing+12.25f);
				listElements[i].transform.localPosition = childPos;
			}
		}

		/// <summary>
		/// Initializes the scroll content if the scroll view is oriented vertically.
		/// </summary>
		private void InitializeContentVertical()
		{
			float originY = 0 - (height * 0.5f);
			float posOffset = height * 0.5f;
			for (int i = 0; i < listElements.Length; i++)
			{
				Vector2 childPos = listElements[i].transform.localPosition;
				childPos.y = originY + posOffset + i * (height + itemSpacing);
				listElements[i].transform.localPosition = childPos;
			}
		}

		public void SnapToCenter()
		{
			ListItem shortest = null;
			float shortestDistance = 1000;
			foreach (var item in listElements)
			{
				float distance = Vector3.Distance(item.transform.position, listGrid.transform.position);
				if(distance < shortestDistance)
				{
					shortestDistance = distance;
					shortest = item;
				}
			}

			transform.Translate(-shortest.transform.position);
			currentItem = shortest;
		}
	}
}