using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Morko
{
	public class ScrollContent : MonoBehaviour
	{
		#region Public Properties

		public ListItem listElement;
		public ListItem[] listElements;
		private GameObject listGrid;

		public InfiniteScroll levelSelectionList;

		public ListItem currentItem;

		public Sprite[] levelImages;

		public GameObject gridCenter;

		private int numberOfScenes;
		private string[] levelNames;


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

		private void Start()
		{
			listGrid = GameObject.Find("ScrollContent");
			CreateList();

			rectTransform = GetComponent<RectTransform>();

			// Subtract the margin from both sides.
			width = rectTransform.rect.width - (2 * horizontalMargin);

			// Subtract the margin from the top and bottom.
			height = rectTransform.rect.height - (2 * verticalMargin);

			horizontal = !vertical;
			if (vertical)
				InitializeContentVertical();
			else
				InitializeContentHorizontal();

			levelSelectionList.listElements = listElements;
		}

		private void CreateList()
		{
			numberOfScenes = SceneManager.sceneCountInBuildSettings;
			listElements = new ListItem[numberOfScenes - 1];
			levelNames = new string[numberOfScenes - 1];

			for (int i = 0; i < numberOfScenes - 1; i++)
			{
				string path = SceneUtility.GetScenePathByBuildIndex(i + 1);
				string levelName = System.IO.Path.GetFileNameWithoutExtension(path);
				levelNames[i] = levelName;
			}

			if (listGrid != null)
			{
				for (int i = 0; i < numberOfScenes - 1; i++)
				{
					listElements[i] = Instantiate(listElement, new Vector3(listGrid.transform.position.x, listGrid.transform.position.y, 0), Quaternion.identity);
					listElements[i].id = i;
					listElements[i].transform.Find("Level Name").GetComponent<Text>().text = levelNames[i];
					listElements[i].transform.Find("Level Image").GetComponent<Image>().sprite = levelImages[i];
					listElements[i].transform.parent = listGrid.transform;
				}
				currentItem = listElements[0];

				for (int i = 0; i < numberOfScenes - 1; i++)
				{
					if (listElements[i].id != listElements[listElements.Length - 1].id)
						listElements[i].nextItem = listElements[i + 1];
					else
						listElements[i].nextItem = listElements[0];
					if (listElements[i].id != listElements[0].id)
						listElements[i].lastItem = listElements[i - 1];
					else
						listElements[i].lastItem = listElements[listElements.Length - 1];
				}
			}
		}

		/// <summary>
		/// Initializes the scroll content if the scroll view is oriented horizontally.
		/// </summary>
		private void InitializeContentHorizontal()
		{
			float originX = 0 - (width * 0.25f);
			float posOffset = width * 0.25f;
			for (int i = 0; i < numberOfScenes - 1; i++)
			{
				Vector3 childPos = listElements[i].transform.localPosition;
				childPos.x = originX + posOffset + i * (width + itemSpacing);
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
			for (int i = 0; i < numberOfScenes - 1; i++)
			{
				Vector2 childPos = listElements[i].transform.localPosition;
				childPos.y = originY + posOffset + i * (height + itemSpacing);
				listElements[i].transform.localPosition = childPos;
			}
		}
	}
}