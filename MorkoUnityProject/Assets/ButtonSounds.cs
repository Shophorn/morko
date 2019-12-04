using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ButtonSounds : MonoBehaviour, ISelectHandler
{
    public UnityEvent onSelect;

    // Start is called before the first frame update
    void Awake()
    {
        onSelect.AddListener(delegate { GetComponentInParent<AudioController>().PlayButtonSelect(); });
        GetComponent<Button>().onClick.AddListener(delegate { GetComponentInParent<AudioController>().PlayButtonClick(); });
        
    }

    public void OnSelect(BaseEventData eventData)
    {
        onSelect.Invoke();
    }

}
