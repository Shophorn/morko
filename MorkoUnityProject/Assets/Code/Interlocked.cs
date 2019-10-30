/* Interlocked creates a wrapper around an object of type T,
so that object can only be accessed from single thread at a time. */
public class Interlocked<T>
{
	private T _value;
	private object threadLock = new object ();

	public Interlocked() => _value = default(T);
	public Interlocked(T value) => _value = value;

	public T Value
	{
		get { lock (threadLock)	{ return _value; } }
		set { lock (threadLock) { _value = value; } }
	}
}