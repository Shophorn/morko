using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class InfiniteScroll : Selectable/*, IBeginDragHandler, IDragHandler, IScrollHandler, IEndDragHandler*/
{
	//	public GameObject listItemContainer;
	//	public ListItem currentItem;
	//	public GameObject gridCenter;
	//	public InputField serverNameField;
	//	public InputField hostNameField;

	//	#region Private Members

	//	public float scrollSpeed;
	//	private bool isScrolling = false;

	//	public ListItem[] listElements;


	//	[SerializeField] private ScrollContent scrollContent;
	//	[SerializeField] private float outOfBoundsThreshold;
	//	private ScrollRect scrollRect;
	//	private Vector2 lastDragPosition;
	//	private bool positiveDrag;

	//	#endregion

	//	private void OnEnable()
	//	{
	//		listElements = scrollContent.listElements;
	//		scrollRect = GetComponent<ScrollRect>();
	//		scrollRect.vertical = scrollContent.Vertical;
	//		scrollRect.horizontal = scrollContent.Horizontal;
	//		scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
	//		currentItem = scrollContent.currentItem;
	//		scrollSpeed = 5.0f;
	//	}


	private void Update()
{
		Debug.Log("JööpJööp");
	//float scrollFactor = Input.GetAxis("Horizontal");
	//if (Input.GetAxis("Horizontal") > 0)
	//	positiveDrag = true;
	//else if (Input.GetAxis("Horizontal") < 0)
	//	positiveDrag = false;

	//if (hostNameField != null && hostNameField.isFocused == true)
	//	scrollFactor = 0;
	//if (serverNameField != null && serverNameField.isFocused == true)
	//	scrollFactor = 0;


	//scrollContent.transform.localPosition = new Vector3(scrollContent.transform.localPosition.x + scrollFactor * scrollSpeed, 0, 0);


	//if (isScrolling && Mathf.Abs(scrollRect.velocity.x) < 10.0f)
	//{
	//	isScrolling = false;
	//	scrollContent.StartCoroutine("LerpTowardsCenter");
	//}
	//currentItem = scrollContent.currentItem;
}

	//	public void OnBeginDrag(PointerEventData eventData)
	//	{
	//		scrollContent.StopCoroutine("LerpTowardsCenter");
	//		isScrolling = false;
	//		lastDragPosition = eventData.position;
	//	}

	//	public void OnEndDrag(PointerEventData eventData)
	//	{
	//		isScrolling = true;
	//	}

	//	public void OnDrag(PointerEventData eventData)
	//	{
	//		positiveDrag = eventData.position.x > lastDragPosition.x;
	//		lastDragPosition = eventData.position;
	//	}

	//	public void OnScroll(PointerEventData eventData)
	//	{
	//		if (scrollContent.Vertical)
	//		{
	//			positiveDrag = eventData.scrollDelta.y > 0;
	//		}
	//		else
	//		{
	//			positiveDrag = eventData.scrollDelta.y < 0;
	//		}
	//	}

	//	public void OnViewScroll()
	//	{
	//		if(gameObject == EventSystem.current.currentSelectedGameObject)
	//			HandleHorizontalScroll();
	//	}

	//	private void HandleHorizontalScroll()
	//	{
	//		int currItemIndex = positiveDrag ? scrollRect.content.childCount - 1 : 0;
	//		var currItem = scrollRect.content.GetChild(currItemIndex);
	//		if (!ReachedThreshold(currItem))
	//		{
	//			return;
	//		}
	//		int endItemIndex = positiveDrag ? 0 : scrollRect.content.childCount - 1;

	//		Transform endItem = scrollRect.content.GetChild(endItemIndex);
	//		Vector2 newPos = endItem.position;

	//		if (positiveDrag)
	//		{
	//			newPos.x = endItem.position.x - scrollContent.itemWidth - scrollContent.ItemSpacing;
	//		}
	//		else
	//		{
	//			newPos.x = endItem.position.x + scrollContent.itemWidth + scrollContent.ItemSpacing;
	//		}

	//		currItem.position = newPos;
	//		currItem.SetSiblingIndex(endItemIndex);
	//	}


	//	private bool ReachedThreshold(Transform item)
	//	{
	//		float posXThreshold = gridCenter.transform.position.x + scrollContent.Width * 0.5f - (scrollContent.itemWidth + outOfBoundsThreshold);
	//		float negXThreshold = gridCenter.transform.position.x - scrollContent.Width * 0.5f + (scrollContent.itemWidth + outOfBoundsThreshold);

	//		return positiveDrag ? item.position.x - scrollContent.Width * 0.5f > posXThreshold :
	//			item.position.x + scrollContent.Width * 0.5f < negXThreshold;
	//	}
}