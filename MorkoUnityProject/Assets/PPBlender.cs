using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PPBlender : MonoBehaviour
{
    public PostProcessVolume normal;
    public PostProcessVolume morko;
    public PostProcessVolume morkoGhost;
    [Range(0f,2f)]
    public float blendToMorko = 0;

    public FieldOfView fov;
    public float baseViewAngle;

    [Header("Debug")]
    public bool toMorko;
    public bool toGhost;
    public bool toHuman;

    private void Awake()
    {
        fov = GetComponent<FieldOfView>();
        baseViewAngle = fov.viewAngle;
        BlendEffects();
    }

    private IEnumerator Fade(float targetValue, float speed = 2f)
    {
        speed = Mathf.Clamp(speed, 0.5f, 10f);

        float blendTime = 1f;

        while (blendTime > 0f)
        {
            blendTime -= Time.deltaTime * 1f * speed;
            blendTime = Mathf.Clamp01(blendTime);
            blendToMorko = Mathf.Lerp(blendToMorko, targetValue, 1 - blendTime);
            BlendEffects();
            yield return null;
        }
    }

    public void FadeToMorko()
    {
        StartCoroutine(Fade(1));
    }
    public void FadeToGhost()
    {
        StartCoroutine(Fade(2));
    }
    public void FadeToHuman()
    {
        StartCoroutine(Fade(0));
    }

    public void BlendEffects()
    {
        blendToMorko = Mathf.Clamp(blendToMorko, 0, 2);
        fov.viewAngle = Mathf.Clamp(baseViewAngle + ((360 - baseViewAngle) * blendToMorko), 0, 360);

        if (blendToMorko < 0.0001)
        {
            fov.doRaycast = true;
            normal.weight = 1;
            morko.weight = 0;
            morkoGhost.weight = 0;
        }
        else if (blendToMorko < 1.01)
        {
            normal.weight = 1 - blendToMorko;
            morko.weight = blendToMorko;
            morkoGhost.weight = 0;
            fov.doRaycast = false;
        }
        else
        {
            normal.weight = 0;
            morko.weight = 2f - blendToMorko;
            morkoGhost.weight = blendToMorko - 1f;
            fov.doRaycast = false;

        }
    }

    //JUST FOR TESTING
    public void Update()
    {
        if(toGhost)
        {
            FadeToGhost();
            toGhost = false;
        }
        if(toMorko)
        {
            FadeToMorko();
            toMorko = false;

        }
        if(toHuman)
        {
            FadeToHuman();
            toHuman = false;
        }
    }
}
