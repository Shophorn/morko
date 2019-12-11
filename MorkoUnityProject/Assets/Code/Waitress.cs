using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waitress : MonoBehaviour
{


    public CanvasGroup group;

    void Start()
    {
        if (group == null)
            group = GetComponent<CanvasGroup>();
        group.alpha = 1f;
        StartCoroutine(Waiter());
    }

    IEnumerator Waiter()
    {
        yield return new WaitForSeconds(0.2f);
        float time = 1.5f;
        while (time > 0f)
        {
            time -= 1 * Time.deltaTime;
            group.alpha = time/1.5f;
            yield return null;
        }

    }
}
