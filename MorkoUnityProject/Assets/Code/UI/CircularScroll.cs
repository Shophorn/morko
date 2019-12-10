using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CircularScroll : Selectable /*,IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler*/
{
	public enum Direction { Left = -1, Right = 1}

	public GameObject listItemContainer;
	public ListItem currentItem;
	public ListItem[] listElements;

	public ScrollContent content;

	public Button scrollLeft;
	public Button scrollRight;

	private Vector2 lastDragPosition;

	public Text nameLabel;

	[SerializeField] private float baseSpeed;

	protected override void Awake()
	{
		scrollLeft.onClick.AddListener(delegate { StartCoroutine(Rotate(Direction.Left)); });
		scrollRight.onClick.AddListener(delegate { StartCoroutine(Rotate(Direction.Right)); });
	}

	private IEnumerator Rotate(Direction direction)
	{
		listElements = content.listElements;
		Debug.Log(listElements.Length);
		float startAngle = 0f;
		float factor;
		if(listElements.Length > 1)
		{
			factor = baseSpeed / listElements.Length;
			while (startAngle < content.angle)
			{
				for (int i = 0; i < listElements.Length; i++)
				{
					listElements[i].transform.RotateAround(content.transform.position, Vector3.up, factor * (int)direction);
					listElements[i].transform.Rotate(0f,-factor*(int)direction,0f);
				}
				Debug.Log(factor);
				startAngle += factor;
				yield return null;
			}
			currentItem = content.CheckCurrentItem();
			nameLabel.text = currentItem.listItemName;
		}
	}

	public void SetLabel()
	{
		currentItem = content.CheckCurrentItem();
		nameLabel.text = currentItem.listItemName;
	}

	//Events suddenly stopped working for some reason.
	//TODO (Joonas): Fix if necessary

	//public void OnBeginDrag(PointerEventData eventData)
	//{
	//	lastDragPosition = eventData.position;
	//}

	//public void OnEndDrag(PointerEventData eventData)
	//{
	//	StartCoroutine(Rotate(currentDirection));
	//}
	//public void OnDrag(PointerEventData eventData)
	//{
	//	currentDirection = eventData.position.x < lastDragPosition.x ? Direction.Right : Direction.Left;
	//	lastDragPosition = eventData.position;
	//}

	//public void OnScroll(PointerEventData eventData)
	//{
	//	currentDirection = eventData.scrollDelta.y > 0 ?Direction.Right : Direction.Left;
	//	StartCoroutine(Rotate(currentDirection));
	//}
}
