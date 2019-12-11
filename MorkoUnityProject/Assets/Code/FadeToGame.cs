using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeToGame : MonoBehaviour
{
    public GameObject loadingText;
    public CanvasGroup group;

    void Start()
    {
        
    }

    public void StartFade()
    {
        group.alpha = 1f;
        loadingText.SetActive(false);
        StartCoroutine(Fade());
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
