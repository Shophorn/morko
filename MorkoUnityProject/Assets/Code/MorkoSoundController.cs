using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MorkoSoundController : MonoBehaviour
{
    public AudioSource audioSrcActive;
    public AudioClip jumpRoar;
    public AudioClip attackRoar;
    public AudioClip attach;

    public AudioSource audioSrcPassive;
    public AudioClip madness;

    public float volume;
    public float currentVolume;
    public bool roarPlaying;

    public void Start()
    {
        if (audioSrcActive == null)
            audioSrcActive = GetComponent<AudioSource>();
        volume = audioSrcActive.volume;
    }

    public void PlayRoar()
    {
        //if (roarPlaying == false)
        //    StartCoroutine(Roar());
        audioSrcActive.PlayOneShot(jumpRoar);
    }

    public void PlayAttack()
    {
        Debug.Log("ATTACK PALYED");
        StopCoroutine(Roar());
        //if (roarPlaying == false)
        //{
        audioSrcActive.volume = volume;
        audioSrcActive.Stop();
        audioSrcActive.PlayOneShot(attach);
        //}


    }


    public void PlayAttach()
    {
        Debug.Log("ATTACH PALYED");
        StopCoroutine(Roar());
        //if (roarPlaying == false)
        //{
        audioSrcActive.volume = volume;
        audioSrcActive.Stop();
        audioSrcActive.PlayOneShot(attackRoar);
        //}
        
            
    }

    IEnumerator Roar()
    {
        roarPlaying = true;
        audioSrcActive.clip = jumpRoar;
        audioSrcActive.Play();

        float time = 2f;
        while (time > 0f)
        {
            time -= 1 * Time.deltaTime;
            currentVolume = volume * (time / 2f);
            audioSrcActive.volume = currentVolume;
            yield return null;
        }
        roarPlaying = false;
        audioSrcActive.Stop();
        audioSrcActive.volume = volume;
    }
}
