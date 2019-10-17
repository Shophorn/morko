using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtoMapScaler : MonoBehaviour
{
    public Transform[] pavement;
    public float currentScale = 1f;
    public float baseScale = 3f;
    private void OnValidate()
    {
        foreach(var pave in pavement)
        {
            pave.localScale = new Vector3(baseScale * currentScale, pave.localScale.y, pave.localScale.z);
            
        }
    }
}
