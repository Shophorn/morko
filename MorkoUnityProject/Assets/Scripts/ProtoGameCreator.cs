using System;
using System.Collections;
using System.Collections.Generic;
using Morko;
using UnityEngine;

public class ProtoGameCreator : MonoBehaviour
{
    public bool debug = false;
    public Character characterPrefab;
    private LocalController localController;
    
    private static ProtoGameCreator instance;
    public static ProtoGameCreator Instance { get {return instance;} }
    

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
            instance = this;

        if (debug)
        {
            StartScene();
        }
    }

    public void StartScene()
    {
        var character = Instantiate(characterPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        localController = LocalController.Create(character, new PlayerSettings());
    }

    private void Update()
    {
        localController.Update();
    }
}
