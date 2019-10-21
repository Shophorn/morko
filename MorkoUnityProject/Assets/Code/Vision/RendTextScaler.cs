using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Camera))]
public class RendTextScaler : MonoBehaviour
{
    private Camera cam;


    void Start()
    {
        cam = GetComponent<Camera>();
        if(cam.targetTexture != null)
        {
            cam.targetTexture.Release();
        }
        cam.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
