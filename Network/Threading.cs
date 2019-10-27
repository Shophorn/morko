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
			{
				throw new InvalidOperationException("'ThreadControl' is already running a thread");
			}

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

		~ThreadControl()
		{
			if (IsRunning)
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
				return;

			_thread.Abort();
			_thread = null;
			_runner = null;
		}	

		// Todo(Leo): We may need to use Finalizer to ensure thread is closed??
		~ThreadControl()
		{
			if (IsRunning)
			{
				Stop();
			}
		}
	}

	/* Atomic creates a wrapper around an object of type T,
	so that object can only be accessed from single place at a time.

	Note(Leo): This is not really an 'atomic' as in atomic operation, 
	but that's the best name I could come up with (Interlocked is already a
	class in System libraries).*/
	public class Atomic<T>
	{
		private T _value;
		private object threadLock = new object ();

		public Atomic() => _value = default(T);
		public Atomic(T value) 	=> _value = value;

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