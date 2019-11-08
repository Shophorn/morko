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

	// Todo(Leo): See if we should use background threads
	public class ThreadControl
	{
		private static readonly string logFilePath 
			= 	Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
				+ $"/morko_threadlog_{DateTime.Now.ToString("HH:mm:ss")}.log";

		private Thread _thread;

		public bool IsRunning => _thread != null;

		private ThreadControl () {}

		public static ThreadControl Start(IThreadRunner threadRunner)
		{
			var control = new ThreadControl();

			if (threadRunner == null)
				throw new ArgumentException("'threadRunner' argument cannot be null");

			control._thread = new Thread(() => 
			{
				try { threadRunner.Run(); }
				catch (ThreadAbortException) { /* This is okay, this is how we stop we threads */ }
				catch (Exception e)
				{
					throw;
					// System.IO.File.AppendAllText(logFilePath, $"{DateTime.Now}: {e}\n");
				}
				finally { threadRunner.CleanUp(); }
			});
			control._thread.Start();

			return control;
		}

		// public void Start(IThreadRunner threadRunner)
		// {
		// 	if (IsRunning)
		// 		throw new InvalidOperationException("'ThreadControl' is already running a thread");

		// 	if (threadRunner == null)
		// 		throw new ArgumentException("'threadRunner' argument cannot be null");

		// 	_thread = new Thread(() => 
		// 	{
		// 		try { threadRunner.Run(); }
		// 		catch (ThreadAbortException) { /* This is okay, this is how we stop we threads */ }
		// 		catch (Exception e)
		// 		{
		// 			System.IO.File.AppendAllText(logFilePath, $"{DateTime.Now}: {e}\n");
		// 		}
		// 		finally { threadRunner.CleanUp(); }
		// 	});
		// 	_thread.Start();
		// }

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