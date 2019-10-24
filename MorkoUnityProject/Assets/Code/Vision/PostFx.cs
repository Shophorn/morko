using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostFx : MonoBehaviour
{
    public Material visionEffectMaterial;
    public Material visionEffectMaterial2;

    public Camera camMain;
    public Camera camMask;
    public Camera camMaskFull;
    public Camera camMorko;

    RenderTexture maskColor;
    RenderTexture maskDepth;

    RenderTexture maskColorFull;
    RenderTexture maskDepthFull;

    RenderTexture morkoColor;
    RenderTexture morkoDepth;

    RenderTexture originColor;
    RenderTexture originDepth;

    public bool restart = false;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Debug.Log("RENDER IMAGE");

        Graphics.Blit(source, destination, visionEffectMaterial);
        //Graphics.Blit(maskColorFull, destination, visionEffectMaterial2);
    }

    private void Update()
    {
        if(restart)
        {
            Start2();
            restart = false;
        }
    }

    private void Start2()
    {
        Debug.Log("START POST FX");

        visionEffectMaterial.EnableKeyword("_OriginColor");
        visionEffectMaterial.EnableKeyword("_OriginDepth");
        visionEffectMaterial.EnableKeyword("_MorkoColor");
        visionEffectMaterial.EnableKeyword("_MorkoDepth");
        visionEffectMaterial.EnableKeyword("_MaskColor");
        visionEffectMaterial.EnableKeyword("_MaskDepth");
        visionEffectMaterial.EnableKeyword("_MaskColorFull");
        visionEffectMaterial.EnableKeyword("_MaskDepthFull");

        if (camMask.targetTexture != null)
            camMask.targetTexture.Release();
        maskColor = new RenderTexture(Screen.width, Screen.height, 0);
        maskDepth = new RenderTexture(Screen.width, Screen.height, 24,RenderTextureFormat.Depth);

        camMask.SetTargetBuffers(maskColor.colorBuffer, maskDepth.depthBuffer);

        if (camMaskFull.targetTexture != null)
            camMaskFull.targetTexture.Release();
        maskColorFull = new RenderTexture(Screen.width, Screen.height, 0);
        maskDepthFull = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth);

        camMaskFull.SetTargetBuffers(maskColorFull.colorBuffer, maskDepthFull.depthBuffer);

        if (camMorko.targetTexture != null)
            camMorko.targetTexture.Release();
        morkoColor = new RenderTexture(Screen.width, Screen.height, 24);
        morkoDepth = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth);

        camMorko.SetTargetBuffers(morkoColor.colorBuffer, morkoDepth.depthBuffer);

        if (camMain.targetTexture != null)
            camMain.targetTexture.Release();
        originColor = new RenderTexture(Screen.width, Screen.height, 0);
        originDepth = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth);

        camMain.SetTargetBuffers(originColor.colorBuffer, originDepth.depthBuffer);

        visionEffectMaterial.SetTexture("_OriginColor",originColor);
        visionEffectMaterial.SetTexture("_OriginDepth",originDepth);
        visionEffectMaterial.SetTexture("_MaskColor",maskColor);
        visionEffectMaterial.SetTexture("_MaskDepth",maskDepth);
        visionEffectMaterial.SetTexture("_MorkoColor",morkoColor);
        visionEffectMaterial.SetTexture("_MorkoDepth",morkoDepth);
        visionEffectMaterial.SetTexture("_MaskColorFull",maskColorFull);
        visionEffectMaterial.SetTexture("_MaskDepthFull",maskDepthFull);

    }
}
