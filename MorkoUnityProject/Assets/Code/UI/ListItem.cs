using UnityEngine;


public class ListItem : MonoBehaviour
{
	[SerializeField]private int listItemId;
	[SerializeField]private string listItemName;
	[SerializeField] private GameObject model;

	public int ID { get => listItemId; set => listItemId = value; }
	public string Name { get => listItemName; set => listItemName = value; }
	public GameObject Model { get => model; set => model = value; }
}
