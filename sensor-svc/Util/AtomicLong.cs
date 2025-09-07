namespace SensorSvc.Util;

public sealed class AtomicLong
{
	private long _value;
	public AtomicLong(long initial) { _value = initial; }
	public long Value
	{
		get => Interlocked.Read(ref _value);
		set => Interlocked.Exchange(ref _value, value);
	}
}


