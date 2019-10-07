using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostFx : MonoBehaviour
{
    public Material pp;
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, pp);
    }
}
