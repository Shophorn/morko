using System;
using System.Threading;

namespace Morko.Threading
{
	public interface IThreadRunner
	{
		void Run();
		void CleanUp();
	}

	// 	/* Note(Leo): We start thread with an infinite loop and/or
	// 	blocking io/network/other function calls. Calling Thread.Abort
	// 	causes ThreadAbortException to be thrown from thread and by
	// 	catching it we are able to exit gracefully. */
	public class ThreadControl
	{
		private Thread _thread;

		public bool IsRunning => _thread != null;

		public void Start(IThreadRunner threadRunner)
		{
			if (_thread != null)
			{
				throw new InvalidOperationException("'ThreadControl' is already running a thread");
			}

			_thread = new Thread(() => 
			{
				try { threadRunner.Run(); }
				catch (ThreadAbortException) { threadRunner.CleanUp(); }
			});
			_thread.Start();
		}

		public void Stop()
		{
			if (_thread == null)
			{
				throw new InvalidOperationException("'ThreadControl' is not running a thread");
			}

			_thread.Abort();
			_thread = null;
		}

		~ThreadControl()
		{
			if (_thread != null)
			{
				Stop();
			}
		}

	}

	public class ThreadControl<TRunner> where TRunner : class, IThreadRunner
	{
		private Thread _thread;
		private TRunner _runner;

		public TRunner Runner => _runner;
		public bool IsRunning => _thread != null;

		public void Start(TRunner threadRunner)
		{
			if (IsRunning)
				throw new InvalidOperationException("'ThreadControl' is already running a thread");

			if (threadRunner == null)
				throw new ArgumentException("'threadRunner' argument cannot be null");

			_runner = threadRunner;
			_thread = new Thread(() => 
			{
				try { threadRunner.Run(); }
				catch (ThreadAbortException) { threadRunner.CleanUp(); }
			});
			_thread.Start();
		}

		public void Stop()
		{
			if (IsRunning == false)
				throw new InvalidOperationException("'ThreadControl' is not running a thread");

			_thread.Abort();
			_thread = null;
			_runner = null;
		}	

		// Todo(Leo): We may need to use Finalizer to ensure thread is closed??
		~ThreadControl()
		{
			if (_thread != null)
			{
				Stop();
			}
		}
	}

	public class Synchronized<T>
	{
		private T _value;
		private object threadLock = new object ();

		public T Read()
		{
			lock (threadLock)
				return _value;
		}

		public void Write(T value)
		{
			lock (threadLock)
				_value = value;
		}
	}
}