using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CircularScroll : Selectable ,IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
{
	public enum Direction { Left = -1, Right = 1}

	public GameObject listItemContainer;
	public ListItem currentItem;
	public GameObject centerObject;

	public ListItem[] listElements;

	public ScrollContent content;

	public Button scrollLeft;
	public Button scrollRight;

	public Direction currentDirection;

	private Vector2 lastDragPosition;

	protected override void Awake()
	{
		listElements = content.listElements;

		scrollLeft.onClick.AddListener(delegate { StartCoroutine(Rotate(Direction.Left)); });
		scrollRight.onClick.AddListener(delegate { StartCoroutine(Rotate(Direction.Right)); });
	}

	private void Update()
	{
		if (Input.GetAxis("Horizontal") > 0)
		{
			Debug.Log("JööpJööp");
			currentDirection = Direction.Right;
		}
		else if (Input.GetAxis("Horizontal") < 0)
			currentDirection = Direction.Left;
	}

	private IEnumerator Rotate(Direction direction)
	{
		float factor = 5 / listElements.Length;

		float startAngle = 0f;
		while (startAngle < content.angle)
		{
			for (int i = 0; i < listElements.Length; i++)
			{
				listElements[i].transform.RotateAround(centerObject.transform.position, Vector3.up, factor * (int)direction);
			}
			startAngle += factor;
			yield return null;
		}
		currentItem = content.CheckCurrentItem();
	}
	public void OnBeginDrag(PointerEventData eventData)
	{
		lastDragPosition = eventData.position;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		StartCoroutine(Rotate(currentDirection));
	}
	public void OnDrag(PointerEventData eventData)
	{
		currentDirection = eventData.position.x < lastDragPosition.x ? Direction.Right : Direction.Left;
		lastDragPosition = eventData.position;
	}

	public void OnScroll(PointerEventData eventData)
	{
		currentDirection = eventData.scrollDelta.y > 0 ?Direction.Right : Direction.Left;
		StartCoroutine(Rotate(currentDirection));
	}
}
