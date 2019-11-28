using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogMovement : MonoBehaviour
{
    public float noiseSpeedXZ = 1f;
    private float currentXZ;

    public float noiseSpeedY = 1f;
    private float currentY;

    public Material fogMat;

    private float startXZ = 1;
    private float startY = 1;

    // Start is called before the first frame update
    void Start()
    {
        if (fogMat == null)
            Debug.LogError("FOG MATERIAL MISSING");
        else
        {
            fogMat.SetFloat("_NoiseOffset", startXZ);
            fogMat.SetFloat("_NoiseMorph", startY);
        }
    }

    // Update is called once per frame
    void Update()
    {
        currentXZ = fogMat.GetFloat("_NoiseOffset");
        currentY = fogMat.GetFloat("_NoiseMorph");

        currentXZ += Time.deltaTime * noiseSpeedXZ;
        currentY += Time.deltaTime * noiseSpeedY;

        if (currentXZ > 100000)
            currentXZ = 1;
        if (currentY > 100000)
            currentY = 1;

        fogMat.SetFloat("_NoiseOffset", currentXZ);
        fogMat.SetFloat("_NoiseMorph", currentY);
    }
}
