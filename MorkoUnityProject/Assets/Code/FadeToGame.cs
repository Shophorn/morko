using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeToGame : MonoBehaviour
{
    public GameObject loadingText;
    public CanvasGroup group;
    public AudioSource audioSrc;
    public AudioController ac;

    void Start()
    {
        
    }

    public void StartFade()
    {
        group.alpha = 1f;
        loadingText.SetActive(false);
        StartCoroutine(Fade());
        audioSrc.PlayOneShot(audioSrc.clip);
        ac = FindObjectOfType<AudioController>();
        if (ac != null)
            ac.OnGameStart();
    }

    IEnumerator Fade()
    {
        float time = 2f;
        while (time > 0f)
        {
            time -= 1 * Time.deltaTime;
            group.alpha = time / 2f;
            yield return null;
        }

        loadingText.SetActive(true);
    }
}
