using System;
using System.Threading;

namespace Morko.Threading
{
	public interface IThreadRunner
	{
		void Run();
		void CleanUp();
	}

	/* Note(Leo): We start thread with an infinite loop and/or
	blocking io/network/other function calls. Calling Thread.Abort
	causes ThreadAbortException to be thrown from thread and by
	catching it we are able to exit gracefully. */
	
	// Todo(Leo): Maybe remove Exceptions???
	public class ThreadControl
	{
		private Thread _thread;

		public bool IsRunning => _thread != null;

		public void Start(IThreadRunner threadRunner)
		{
			if (IsRunning)
				throw new InvalidOperationException("'ThreadControl' is already running a thread");

			if (threadRunner == null)
				throw new ArgumentException("'threadRunner' argument cannot be null");

			_thread = new Thread(() => 
			{
				try { threadRunner.Run(); }
				catch (ThreadAbortException) { threadRunner.CleanUp(); }
				catch (Exception e)
				{
					System.IO.File.AppendAllText("w:/metropolia/morko/threadlog.log", $"{DateTime.Now}: {e}\n");
				}
			});
			_thread.Start();
		}

		public void Stop()
		{
			if (IsRunning == false)
				return;

			_thread.Abort();
			_thread = null;
		}

		public void StopAndWait()
		{
			if (IsRunning == false)
				return;

			_thread.Abort();
			_thread.Join();
			_thread = null;
		}

		~ThreadControl() => Stop();
	}
}