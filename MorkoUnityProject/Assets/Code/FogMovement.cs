using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogMovement : MonoBehaviour
{
    public float noiseSpeedXZ = 1f;
    private float currentXZ;

    public float noiseSpeedY = 1f;
    private float currentY;

    public Renderer rend;
    public Material fogMat;

    private float startXZ = 1;
    private float startY = 1;

    // Start is called before the first frame update
    void Start()
    {
        if (rend == null)
        {
            rend = GetComponent<Renderer>();
            rend.material = fogMat;
        }
            
        else
        {
            rend.material.SetFloat("_NoiseOffset", startXZ);
            rend.material.SetFloat("_NoiseMorph", startY);
        }
    }

    // Update is called once per frame
    void Update()
    {
        currentXZ = rend.material.GetFloat("_NoiseOffset");
        currentY = rend.material.GetFloat("_NoiseMorph");

        currentXZ += Time.deltaTime * noiseSpeedXZ;
        currentY += Time.deltaTime * noiseSpeedY;

        if (currentXZ > 100000 || currentXZ < -100000)
            currentXZ = 1;
        if (currentY > 100000 || currentY < -100000)
            currentY = 1;

        rend.material.SetFloat("_NoiseOffset", currentXZ);
        rend.material.SetFloat("_NoiseMorph", currentY);
    }
}
