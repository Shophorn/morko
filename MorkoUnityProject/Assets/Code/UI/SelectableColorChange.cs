using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectableColorChange : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public Text text;
	public Text secondaryText; //If there are multiple text elements which colors need to be changed

	public Color defaultColor;
	public Color highlightColor;

	public void OnPointerEnter(PointerEventData eventData)
	{
		text.color = highlightColor;
		if (secondaryText != null)
			secondaryText.color = highlightColor;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		text.color = defaultColor;
		if (secondaryText != null)
			secondaryText.color = defaultColor;
	}
}
