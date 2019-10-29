using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtoHeadSiphon : MonoBehaviour
{
    public ParticleSystem pSystem;
    public Transform gfxParent;
    public Transform target;
    public float scale = 1f;
    public bool siphon = false;
    public bool desiphon = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(siphon)
        {
            if(pSystem.isPlaying == false)
                pSystem.Play();
            gfxParent.localScale = new Vector3(scale, scale, scale);
            if(scale < 0.05f)
            {
                siphon = false;
                StartCoroutine(Teleport());
            }
            else
            {
                
                scale -= 0.5f * Time.deltaTime;
            }
        }
        if(desiphon)
        {
            transform.position = target.position;
            gfxParent.localScale = new Vector3(scale, scale, scale);
            
            if (scale > 1f)
            {
                pSystem.Stop();
            }
            else
            {
                scale += 0.5f * Time.deltaTime;
            }
        }
    }

    IEnumerator Teleport()
    {
        gfxParent.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        desiphon = true;
        gfxParent.gameObject.SetActive(true);
        
    }
}
