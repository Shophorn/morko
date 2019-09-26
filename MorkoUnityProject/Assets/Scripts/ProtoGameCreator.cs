using System;
using System.Collections;
using System.Collections.Generic;
using Morko;
using UnityEngine;

public class ProtoGameCreator : MonoBehaviour
{
    public Character characterPrefab;
    private LocalController localController;
    
    // Start is called before the first frame update
    void Start()
    {
        var character = Instantiate(characterPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        localController = LocalController.Create(character);
    }

    private void Update()
    {
        localController.Update();
    }
}
