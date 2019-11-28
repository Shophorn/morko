using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemFocusRegain : MonoBehaviour
{
	[SerializeField] GameObject firstSelectedUIElement;
	private GameObject lastSelectedUIElemenent;

    void Start()
    {
		lastSelectedUIElemenent = firstSelectedUIElement;
    }

    void Update()
    {
		if (EventSystem.current.currentSelectedGameObject == null)
			EventSystem.current.SetSelectedGameObject(lastSelectedUIElemenent);
		else
			lastSelectedUIElemenent = EventSystem.current.currentSelectedGameObject;
    }
}
