using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Morko
{
	public class InfiniteScroll : MonoBehaviour, IBeginDragHandler, IDragHandler, IScrollHandler
	{
		#region Private Members

		public float scrollSpeed;
		public ListItem currentItem;
		public GameObject gridCenter;
		public InputField nameInputField;
		public InputField roomInputField;

		public ListItem[] listElements;

		/// <summary>
		/// The ScrollContent component that belongs to the scroll content GameObject.
		/// </summary>
		[SerializeField]
		private ScrollContent scrollContent;

		/// <summary>
		/// How far the items will travel outside of the scroll view before being repositioned.
		/// </summary>
		[SerializeField]
		private float outOfBoundsThreshold;

		/// <summary>
		/// The ScrollRect component for this GameObject.
		/// </summary>
		private ScrollRect scrollRect;

		/// <summary>
		/// The last position where the user has dragged.
		/// </summary>
		private Vector2 lastDragPosition;

		/// <summary>
		/// Is the user dragging in the positive axis or the negative axis?
		/// </summary>
		private bool positiveDrag;

		#endregion

		private void Start()
		{
			scrollRect = GetComponent<ScrollRect>();
			scrollRect.vertical = scrollContent.Vertical;
			scrollRect.horizontal = scrollContent.Horizontal;
			scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
			currentItem = scrollContent.currentItem;
			scrollSpeed = 10.0f;
		}

		private void Update()
		{
			float scrollFactor = Input.GetAxis("Horizontal");
			if (Input.GetAxis("Horizontal") > 0)
				positiveDrag = true;
			else if (Input.GetAxis("Horizontal") < 0)
				positiveDrag = false;

			if (nameInputField != null && nameInputField.isFocused == true)
				scrollFactor = 0;
			if (roomInputField != null && roomInputField.isFocused == true)
				scrollFactor = 0;


			scrollContent.transform.localPosition = new Vector3(scrollContent.transform.localPosition.x + scrollFactor * scrollSpeed, 0, 0);
			
			DetermineCurrentItem();
			scrollContent.currentItem = currentItem;
		}

		/// <summary>
		/// Called when the user starts to drag the scroll view.
		/// </summary>
		/// <param name="eventData">The data related to the drag event.</param>
		public void OnBeginDrag(PointerEventData eventData)
		{
			lastDragPosition = eventData.position;
		}

		/// <summary>
		/// Called while the user is dragging the scroll view.
		/// </summary>
		/// <param name="eventData">The data related to the drag event.</param>
		public void OnDrag(PointerEventData eventData)
		{
			//if (scrollContent.Vertical)
			//{
			//	positiveDrag = eventData.position.y > lastDragPosition.y;
			//}
			//else if (scrollContent.Horizontal)
			//{
			//	positiveDrag = eventData.position.x > lastDragPosition.x;
			//}

			positiveDrag = eventData.position.x > lastDragPosition.x;
			lastDragPosition = eventData.position;
		}

		/// <summary>
		/// Called when the user starts to scroll with their mouse wheel in the scroll view.
		/// </summary>
		/// <param name="eventData">The data related to the scroll event.</param>
		public void OnScroll(PointerEventData eventData)
		{
			if (scrollContent.Vertical)
			{
				positiveDrag = eventData.scrollDelta.y > 0;
			}
			else
			{
				// Scrolling up on the mouse wheel is considered a negative scroll, but I defined
				// scrolling downwards (scrolls right in a horizontal view) as the positive direciton,
				// so I check if the if scrollDelta.y is less than zero to check for a positive drag.
				positiveDrag = eventData.scrollDelta.y < 0;
			}
		}

		/// <summary>
		/// Called when the user is dragging/scrolling the scroll view.
		/// </summary>
		public void OnViewScroll()
		{
			HandleHorizontalScroll();

			//if (scrollContent.Vertical)
			//{
			//	HandleVerticalScroll();
			//}
			//else
			//{
			//	HandleHorizontalScroll();
			//}
		}

		/// <summary>
		/// Called if the scroll view is oriented vertically.
		/// </summary>
		//private void HandleVerticalScroll()
		//{
		//	int currItemIndex = positiveDrag ? scrollRect.content.childCount - 1 : 0;
		//	var currItem = scrollRect.content.GetChild(currItemIndex);

		//	if (!ReachedThreshold(currItem))
		//	{
		//		return;
		//	}

		//	int endItemIndex = positiveDrag ? 0 : scrollRect.content.childCount - 1;
		//	Transform endItem = scrollRect.content.GetChild(endItemIndex);
		//	Vector2 newPos = endItem.position;

		//	if (positiveDrag)
		//	{
		//		newPos.y = endItem.position.y - scrollContent.Height * 1.5f + scrollContent.ItemSpacing;
		//	}
		//	else
		//	{
		//		newPos.y = endItem.position.y + scrollContent.Height * 1.5f - scrollContent.ItemSpacing;
		//	}

		//	currItem.position = newPos;
		//	currItem.SetSiblingIndex(endItemIndex);
		//}

		/// <summary>
		/// Called if the scroll view is oriented horizontally.
		/// </summary>
		private void HandleHorizontalScroll()
		{
			int currItemIndex = positiveDrag ? scrollRect.content.childCount - 1 : 0;
			var currItem = scrollRect.content.GetChild(currItemIndex);
			if (!ReachedThreshold(currItem))
			{
				return;
			}

			int endItemIndex = positiveDrag ? 0 : scrollRect.content.childCount - 1;
			Transform endItem = scrollRect.content.GetChild(endItemIndex);
			Vector2 newPos = endItem.position;

			if (positiveDrag)
			{
				newPos.x = endItem.position.x - scrollContent.Width - scrollContent.ItemSpacing;
			}
			else
			{
				newPos.x = endItem.position.x + scrollContent.Width + scrollContent.ItemSpacing;
			}

			currItem.position = newPos;
			currItem.SetSiblingIndex(endItemIndex);
		}

		/// <summary>
		/// Checks if an item has the reached the out of bounds threshold for the scroll view.
		/// </summary>
		/// <param name="item">The item to be checked.</param>
		/// <returns>True if the item has reached the threshold for either ends of the scroll view, false otherwise.</returns>
		private bool ReachedThreshold(Transform item)
		{
			float posXThreshold = transform.position.x + scrollContent.Width * 0.5f + outOfBoundsThreshold;
			float negXThreshold = transform.position.x - scrollContent.Width * 0.5f - outOfBoundsThreshold;
			return positiveDrag ? item.position.x - scrollContent.Width * 0.5f > posXThreshold :
				item.position.x + scrollContent.Width * 0.5f < negXThreshold;

			//if (scrollContent.Vertical)
			//{
			//	float posYThreshold = transform.position.y + scrollContent.Height * 0.5f + outOfBoundsThreshold;
			//	float negYThreshold = transform.position.y - scrollContent.Height * 0.5f - outOfBoundsThreshold;
			//	return positiveDrag ? item.position.y - scrollContent.Width * 0.5f > posYThreshold :
			//		item.position.y + scrollContent.Width * 0.5f < negYThreshold;
			//}
			//else
			//{
			//	float posXThreshold = transform.position.x + scrollContent.Width * 0.5f + outOfBoundsThreshold;
			//	float negXThreshold = transform.position.x - scrollContent.Width * 0.5f - outOfBoundsThreshold;
			//	return positiveDrag ? item.position.x - scrollContent.Width * 0.5f > posXThreshold :
			//		item.position.x + scrollContent.Width * 0.5f < negXThreshold;
			//}
		}

		private void DetermineCurrentItem()
		{
			float currentItemPositionalValue = (gridCenter.transform.localPosition.x - scrollContent.transform.localPosition.x / 240.0f) % (listElements.Length);
			int positionalIntValue = Mathf.RoundToInt(currentItemPositionalValue);

			if (positionalIntValue < 0)
				positionalIntValue *= -1;

			for (int i = 0; i < listElements.Length; i++)
			{
				if (positionalIntValue == listElements[i].id)
				{
					currentItem = listElements[i];
					currentItem.listItemName = listElements[i].listItemName;
					continue;
				}
				if (positionalIntValue > listElements[listElements.Length - 1].id)
				{
					currentItem = listElements[0];
					currentItem.listItemName = listElements[0].listItemName;
				}
			}
		}
	}
}