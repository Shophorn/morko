using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSoundController : MonoBehaviour
{
    [Header("Audio Files")]
    public AudioClip[] stepsHardSurface;
    public AudioClip[] stepsGrass;
    public AudioClip[] stepsDirt;
    public AudioClip[] defaultFootsteps;

    [Header("Surface Tags")]
    public string tagHardSurface;
    public string tagGrass;
    public string tagDirt;

    [Space(10)]
    [Header("Adaptive System\n(default footsteps are played if disabled)")]
    
    public bool adaptiveFootsteps;
    public int terrainLayer = 14;

    [Header("Other")]
    public bool muteFootsteps;
    public AudioSource audioSrc;
    public enum SurfaceType { hard,grass,dirt,unknown};
    public SurfaceType currentSurface;
    public PlayerController pc;

    void FixedUpdate()
    {
        if(pc != null)
            adaptiveFootsteps = !pc.isMorko;

        if (adaptiveFootsteps)
            GetSurfaceType();
        else
            currentSurface = SurfaceType.unknown;
    }

    void PlayFootstep(float audioScale)
    {
        if(muteFootsteps == false)
        {
            switch (currentSurface)
            {
                case SurfaceType.hard:
                    audioSrc.PlayOneShot(stepsHardSurface[Random.Range(0, stepsHardSurface.Length)],audioScale);
                    break;
                case SurfaceType.grass:
                    audioSrc.PlayOneShot(stepsGrass[Random.Range(0, stepsGrass.Length)], audioScale);
                    break;
                case SurfaceType.dirt:
                    audioSrc.PlayOneShot(stepsDirt[Random.Range(0, stepsDirt.Length)], audioScale);
                    break;
                default:
                    audioSrc.PlayOneShot(defaultFootsteps[Random.Range(0, defaultFootsteps.Length)], audioScale);
                    break;
            }
        }  
    }

    void GetSurfaceType()
    {
        int layerMask = 1 << terrainLayer;

        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.position + Vector3.down * 5f, out hit, Mathf.Infinity, layerMask))
        {
            string surf = hit.collider.tag;

            if (surf == tagHardSurface)
                currentSurface = SurfaceType.hard;

            else if (surf == tagGrass)
                currentSurface = SurfaceType.grass;

            else if (surf == tagDirt)
                currentSurface = SurfaceType.dirt;

            else
                currentSurface = SurfaceType.unknown;

        }
        else
        {
            currentSurface = SurfaceType.unknown;
        }
    }
}
