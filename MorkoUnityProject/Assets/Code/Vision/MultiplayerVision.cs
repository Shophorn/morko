using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerVision : MonoBehaviour
{
    public Camera baseCamera;
    public Camera maskCamera;

    private RenderTexture maskTexture;
    public Texture MaskTexture => maskTexture;

    public void CreateMask()
    {
        maskTexture = new RenderTexture(Screen.width, Screen.height ,0);
        maskTexture.name = "VisibilityMask";
        maskCamera.targetTexture = maskTexture;
    }
}
