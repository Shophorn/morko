using System;
using System.Threading;
using System.Collections.Generic;

namespace Morko.Threading
{
	public class LockedList<T>
	{
		private List<T> list;
		private object threadLock = new object();

		public LockedList()
		{
			list = new List<T>();
		}

		public LockedList(Func<List<T>> createFunction)
		{
			this.list = createFunction();
		}

		public void Lock()
		{
			Monitor.Enter(threadLock);
		}

		public void Unlock()
		{
			Monitor.Exit(threadLock);
		}

		private void Try(Action operation)
		{
			if (Monitor.IsEntered(threadLock))
			{
				operation();
			}
			else
			{
				throw new SynchronizationLockException($"{typeof(LockedList<T>)} is locked");
			}
		}

		private U Try<U>(Func<U> operation)
		{
			if (Monitor.IsEntered(threadLock))
			{
				return operation();
			}
			else
			{
				throw new SynchronizationLockException($"{typeof(LockedList<T>)} is locked");
			}	
		}

		public int Count 			=> Try(() => list.Count);
		public void Add(T item) 	=> Try(() => list.Add(item));
		public T this [int index] 	=> Try(() => list[index]);
		
	}

}