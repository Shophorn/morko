using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PPBlender : MonoBehaviour
{
    public PostProcessVolume normal;
    public PostProcessVolume morko;
    [Range(0f,1f)]
    public float blendToMorko = 0;

    void Update()
    {
        blendToMorko = Mathf.Clamp01(blendToMorko);
        normal.weight = 1 - blendToMorko;
        morko.weight = blendToMorko;
    }
}
