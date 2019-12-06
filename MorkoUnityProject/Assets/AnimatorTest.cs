using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorTest : MonoBehaviour
{
    public Animator a;

    private bool running = false;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            a.ResetTrigger("Run");
            a.ResetTrigger("Idle");
            a.SetTrigger("Walk");
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            a.ResetTrigger("Walk");
            a.ResetTrigger("Idle");
            a.SetTrigger("Run");
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            a.ResetTrigger("Walk");
            a.ResetTrigger("Run");
            a.ResetTrigger("Idle");
            a.SetTrigger("Roar");
        }
        
        if (Input.GetKeyDown(KeyCode.I))
        {
            a.ResetTrigger("Walk");
            a.ResetTrigger("Run");
            a.SetTrigger("Idle");
        }
    }
}
