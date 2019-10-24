/*
Leo Tamminen
shophorn@protonmail.com

Thanks to this guy for awesome naming
https://stackoverflow.com/questions/53916533/setactive-can-only-be-called-from-the-main-thread/56715254#56715254
*/

using System;
using System.Collections.Concurrent;
using UnityEngine;

public class MainThreadWorker : MonoBehaviour
{
	private readonly ConcurrentQueue<Action> jobs
		= new ConcurrentQueue<Action>();

	private static MainThreadWorker instance;

	private void Awake()
	{
		this.MakeMonoBehaviourSingleton();
	}

	private void Update()
	{
		while(jobs.TryDequeue(out Action action))
		{
			action.Invoke();
		}
	}

	public static void AddJob(Action action)
	{
		if (instance == null)
		{
			Debug.LogError("MainThreadWorker must be present and active in scene");
			return;
		}
		
		instance.jobs.Enqueue(action);
	}
}