using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waitress : MonoBehaviour
{

    public AudioSource audioSrc;
    public AudioClip clip;
    private AudioController ac;

    void Start()
    {
        StartCoroutine(Waiter());
    }

    IEnumerator Waiter()
    {
        yield return new WaitForSeconds(0.3f);
        audioSrc.PlayOneShot(clip);
        ac = FindObjectOfType<AudioController>();
        if (ac != null)
            ac.OnGameEnd();
    }
}
