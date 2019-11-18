using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemFocusRegain : MonoBehaviour
{
	private GameObject lastSelectedUIElemenent;

    void Start()
    {
		lastSelectedUIElemenent = new GameObject();
    }

    void Update()
    {
		if (EventSystem.current.currentSelectedGameObject == null)
			EventSystem.current.SetSelectedGameObject(lastSelectedUIElemenent);
		else
			lastSelectedUIElemenent = EventSystem.current.currentSelectedGameObject;
    }
}
