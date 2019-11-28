using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerVision : MonoBehaviour
{
    public Material visionEffectMaterial;

    public Material visionTESTMAT;

    public Camera baseCamera;
    public Camera maskCamera;
    public Camera multiplayerCamera;

    RenderTexture maskColor;
    RenderTexture maskDepth;

    RenderTexture morkoColor;
    RenderTexture morkoDepth;

    RenderTexture baseColor;
    RenderTexture baseDepth;

    /*
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, visionEffectMaterial);
    }
    */

    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();

        Debug.Log("START POST FX");

        /*
        visionEffectMaterial.EnableKeyword("_BaseColor");
        visionEffectMaterial.EnableKeyword("_BaseDepth");
        visionEffectMaterial.EnableKeyword("_MultiplayerColor");
        visionEffectMaterial.EnableKeyword("_MultiplayerDepth");
        visionEffectMaterial.EnableKeyword("_MaskColor");
        visionEffectMaterial.EnableKeyword("_MaskDepth");
        */
        if (maskCamera.targetTexture != null)
            maskCamera.targetTexture.Release();
        
        maskColor = new RenderTexture(Screen.width, Screen.height ,0);
        //maskDepth = new RenderTexture(Screen.width, Screen.height, 24,RenderTextureFormat.Depth);
        maskCamera.targetTexture = maskColor;
        
        //maskCamera.SetTargetBuffers(maskColor.colorBuffer, maskDepth.depthBuffer);
        /*
        if (multiplayerCamera.targetTexture != null)
            multiplayerCamera.targetTexture.Release();
        morkoColor = new RenderTexture(Screen.width, Screen.height, 24);
        morkoDepth = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth);

        multiplayerCamera.SetTargetBuffers(morkoColor.colorBuffer, morkoDepth.depthBuffer);

        if (baseCamera.targetTexture != null)
            baseCamera.targetTexture.Release();

        baseColor = new RenderTexture(Screen.width, Screen.height, 0);
        baseDepth = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth);

        baseCamera.SetTargetBuffers(baseColor.colorBuffer, baseDepth.depthBuffer);


        visionEffectMaterial.SetTexture("_BaseColor",baseColor);
        visionEffectMaterial.SetTexture("_BaseDepth",baseDepth);
        visionEffectMaterial.SetTexture("_MaskColor",maskColor);
        visionEffectMaterial.SetTexture("_MaskDepth",maskDepth);
        visionEffectMaterial.SetTexture("_MultiplayerColor",morkoColor);
        visionEffectMaterial.SetTexture("_MultiplayerDepth",morkoDepth);
        */
        visionTESTMAT.SetTexture("_MaskTex", maskColor);

    }
}
