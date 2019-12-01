using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class BackgroundVideo : MonoBehaviour
{
	[SerializeField] VideoPlayer videoPlayer;

    void Start()
    {
		videoPlayer.playbackSpeed = 1;
		videoPlayer.isLooping = true;
		videoPlayer.loopPointReached += EndReached;
    }

    void Update()
    {
		if (videoPlayer.frame == videoPlayer.frameCount / 2f)
			StartCoroutine(Pause());
    }

	IEnumerator Pause()
	{
		videoPlayer.Pause();
		yield return new WaitForSeconds(3);
		videoPlayer.Play();
	}

	private void EndReached(VideoPlayer vp)
	{
		StartCoroutine(Pause());
	}
}
