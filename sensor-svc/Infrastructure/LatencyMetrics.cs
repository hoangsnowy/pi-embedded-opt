namespace SensorSvc.Infrastructure;

public sealed class LatencyMetrics
{
	private readonly object _lock = new();
	private readonly double[] _buffer;
	private int _count;
	private int _index;

	public LatencyMetrics(int maxSamples)
	{
		_buffer = new double[Math.Max(1, maxSamples)];
		_count = 0;
		_index = 0;
	}

	public void AddSample(double ms)
	{
		lock (_lock)
		{
			_buffer[_index] = ms;
			_index = (_index + 1) % _buffer.Length;
			if (_count < _buffer.Length) _count++;
		}
	}

	public (double p50, double p95, int count) GetPercentiles()
	{
		double[] snapshot;
		int count;
		lock (_lock)
		{
			count = _count;
			snapshot = new double[count];
			if (count == 0) return (0, 0, 0);

			int start = (_index - count + _buffer.Length) % _buffer.Length;
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = _buffer[(start + i) % _buffer.Length];
			}
		}
		Array.Sort(snapshot);
		double P(double p)
		{
			if (snapshot.Length == 0) return 0;
			if (snapshot.Length == 1) return snapshot[0];
			double rank = p * (snapshot.Length - 1);
			int low = (int)Math.Floor(rank);
			int high = (int)Math.Ceiling(rank);
			if (low == high) return snapshot[low];
			double frac = rank - low;
			return snapshot[low] + (snapshot[high] - snapshot[low]) * frac;
		}
		return (P(0.50), P(0.95), count);
	}
}


