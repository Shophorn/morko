using UnityEngine;
using UnityEngine.UI;


public class ServerToggleListItem : MonoBehaviour
{
	[SerializeField] private Text label;
	[SerializeField] private Toggle toggle;


	public Text Label { get => label; set => label = value; }
	public Toggle Toggle { get => toggle; set => toggle = value; }
}