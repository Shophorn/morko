using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TextDotAnimation : MonoBehaviour
{
	public Text textObject;
	[Tooltip("Time in seconds to fully loop animation once")]
	public float timeForCycle = 1f;

	private string text;
	private static readonly string [] dotFrames = {"   ", ".  ", ".. ", "..."};

	private Coroutine animation;

	private void Awake()
	{
		text = textObject.text;
	}

	private void OnEnable()
	{
		IEnumerator Animation()
		{
			int frameCount = dotFrames.Length;
			int frameIndex = 0;
			var frameDelay = new WaitForSeconds(timeForCycle / frameCount);

			while(true)
			{
				textObject.text = $"{text}{dotFrames[frameIndex]}";
				frameIndex += 1;
				frameIndex %= frameCount;
				yield return frameDelay;
			}
		}
		animation = StartCoroutine(Animation());
	}

	private void OnDisable()
	{
		if (animation != null)
		{
			StopCoroutine(animation);
		}
	}

}