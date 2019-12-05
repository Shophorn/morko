using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskControllerMethods : MonoBehaviour
{
    public MaskController maskController;

    public void MaskOn()
    {
        maskController.MaskOnNewMorko();
    }
    
    public void MaskOff()
    {
        maskController.MaskOffMorko();
    }
}
