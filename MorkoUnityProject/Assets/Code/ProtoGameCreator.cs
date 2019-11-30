using System;
using System.Collections;
using System.Collections.Generic;
//using Morko;
using UnityEngine;

public class ProtoGameCreator : MonoBehaviour
{
    public bool debug = false;
    // public Character characterPrefab;
    public PlayerSettings normalSettings;
    public PlayerSettings morkoSettings;
    private LocalPlayerController localController;

    // Note(Leo): SAMPO LISÄSIN TÄN, pitää kytkee inspectorissa 
    // // public CharacterCollection characterPrefabs;


    // //<body><b>SINGLETON PATTERN ON VÄLIAIKAINEN</b></body>
    // private static ProtoGameCreator instance;
    // public static ProtoGameCreator Instance { get {return instance;} }


    // // Start is called before the first frame update
    // void Awake()
    // {
    //     if (instance == null)
    //         instance = this;

    //     if (debug)
    //     {
    //         StartScene();
    //     }

    //     int[] indexes = {0, 1, 2, 3, 4};
    //     characterPrefabs.InstantiateMany(indexes);
    // }
    // //TÄTÄ KUTSUTAAN LOADERISSA JOTTA HAHMOT INSTANSIOITUVAT OIKEAAN SCENEEN
    // //UNITY INSTANSIOI ASIAT AKTIIVISESN SKENEEN JOKA PITÄÄ ASETTAA SCENEMANAGERISSA
    // //SKENEÄ EI VOIDA ASETTAA AKTIIVISEKSI ENNENKUIN SE ON LOADATTU
    // public void StartScene()
    // {
    //     var character = Instantiate(characterPrefab, new Vector3(0, 0, 0), Quaternion.identity);
    //     character.GetComponent<LocalPlayerController>().SetCamera(character.GetComponentInChildren<Camera>());
    // }
}
