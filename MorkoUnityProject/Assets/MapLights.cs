using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MapLights : MonoBehaviour
{
    public GameObject[] lights;
    public Material defaultMat;
    public bool useDefaultMat;
    public Color color;
    public float emissionPow;

    private void Start()
    {
        UpdateLights();
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        if(EditorApplication.isPlaying)
            UpdateLights();
    }
    #endif

    private void UpdateLights()
    {
        if(useDefaultMat)
        {
            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].GetComponentInChildren<Light>().color = defaultMat.GetColor("_EmissionColor");
                Renderer instance = lights[i].transform.Find("Mesh/glassPart").GetComponent<Renderer>();
                instance.material = defaultMat;
            }
        }
        else
        {
            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].GetComponentInChildren<Light>().color = color;
                Renderer instance = lights[i].transform.Find("Mesh/glassPart").GetComponent<Renderer>();
                instance.material.EnableKeyword("_EMISSION");
                instance.material.SetColor("_EmissionColor", color * (1 + emissionPow));
            }
        }
        
    }
}
