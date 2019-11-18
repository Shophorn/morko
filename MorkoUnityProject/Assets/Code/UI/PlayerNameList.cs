using System.Collections.Generic;
using UnityEngine;

public class PlayerNameList : MonoBehaviour
{
    [SerializeField] private NameListEntry nameListEntryPrefab;
    [SerializeField] private Transform nameListEntryParent;

    private Dictionary<int, NameListEntry> listedNames = new Dictionary<int, NameListEntry>();

    public void AddPlayer(  int uniqueId,
                            string playerName,
                            PlayerNetworkStatus status = PlayerNetworkStatus.Waiting)
    {
        if (listedNames.ContainsKey(uniqueId) == false)
        {
            var entry = Instantiate(nameListEntryPrefab, nameListEntryParent);
            entry.Name = playerName;
            entry.Status = status;
            listedNames.Add(uniqueId, entry);
        }
    }

    public void RemovePlayer(int uniqueId)
    {
        if (listedNames.ContainsKey(uniqueId))
        {
            Destroy(listedNames[uniqueId].gameObject);
            listedNames.Remove(uniqueId);
        }
    }

    public void SetStatus(int uniqueId, PlayerNetworkStatus status)
    {
        if (listedNames.ContainsKey(uniqueId))
        {
            listedNames[uniqueId].Status = status;
        }
    }
}