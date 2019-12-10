using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MorkoSoundController : MonoBehaviour
{
    public AudioSource audioSrc;
    public AudioClip roar;

    public float volume;
    public float currentVolume;
    public bool roarPlaying;

    public void Start()
    {
        volume = audioSrc.volume;
    }

    public void PlayRoar()
    {
        if (roarPlaying == false)
            StartCoroutine(Roar());
    }

    IEnumerator Roar()
    {
        roarPlaying = true;
        audioSrc.clip = roar;
        audioSrc.Play();

        float time = 2f;
        while (time > 0f)
        {
            time -= 1 * Time.deltaTime;
            currentVolume = volume * (time / 2f);
            audioSrc.volume = currentVolume;
            yield return null;
        }
        roarPlaying = false;
        audioSrc.Stop();
    }
}
