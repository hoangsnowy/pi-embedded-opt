using System.Diagnostics;

namespace SensorSvc.Infrastructure;

public sealed class ProcessCpuSampler
{
	private readonly int _intervalMs;
	private readonly CancellationTokenSource _cts = new();
	private readonly object _lock = new();
	private double _lastCpuPercent;

	public ProcessCpuSampler(int sampleIntervalMs)
	{
		_intervalMs = Math.Max(100, sampleIntervalMs);
	}

	public void Start()
	{
		_ = Task.Run(async () =>
		{
			var proc = Process.GetCurrentProcess();
			int cpuCount = Environment.ProcessorCount;

			DateTime prevWall = DateTime.UtcNow;
			TimeSpan prevCpu = proc.TotalProcessorTime;

			while (!_cts.IsCancellationRequested)
			{
				try
				{
					await Task.Delay(_intervalMs, _cts.Token);
					proc.Refresh();

					DateTime nowWall = DateTime.UtcNow;
					TimeSpan nowCpu = proc.TotalProcessorTime;

					double wallMs = (nowWall - prevWall).TotalMilliseconds;
					double cpuMs = (nowCpu - prevCpu).TotalMilliseconds;

					prevWall = nowWall;
					prevCpu = nowCpu;

					double pct = 0;
					if (wallMs > 1 && cpuCount > 0)
					{
						pct = Math.Min(100.0, Math.Max(0.0, (cpuMs / (wallMs * cpuCount)) * 100.0));
					}
					lock (_lock) _lastCpuPercent = pct;
				}
				catch (TaskCanceledException) { }
				catch { }
			}
		});
	}

	public void Stop() => _cts.Cancel();

	public double GetLastPercent()
	{
		lock (_lock) return _lastCpuPercent;
	}
}

public sealed class ProcessMetrics
{
	private readonly ProcessCpuSampler _cpu;

	public ProcessMetrics(ProcessCpuSampler cpu)
	{
		_cpu = cpu;
	}

	public double GetCpuPercent() => _cpu.GetLastPercent();

	public double GetRssMiB()
	{
		try
		{
			long bytes = Process.GetCurrentProcess().WorkingSet64;
			return bytes / 1024.0 / 1024.0;
		}
		catch
		{
			return 0;
		}
	}
}


