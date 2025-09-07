using System.Diagnostics;

namespace SensorSvc.Infrastructure;

public sealed class ProcessCpuSampler
{
	private readonly int _intervalMs;
	private readonly CancellationTokenSource _cts = new();
	private readonly object _lock = new();
	private double _lastCpuPercent;
	private double _effectiveCpuCount;

	public ProcessCpuSampler(int sampleIntervalMs)
	{
		_intervalMs = Math.Max(250, sampleIntervalMs);
		_effectiveCpuCount = DetectEffectiveCpuCount();
	}

	public void Start()
	{
		_ = Task.Run(async () =>
		{
			var proc = Process.GetCurrentProcess();
			DateTime prevWall = DateTime.UtcNow;
			TimeSpan prevCpu = proc.TotalProcessorTime;
			int tick = 0;

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

					if ((++tick % 10) == 0)
					{
						_effectiveCpuCount = DetectEffectiveCpuCount();
					}

					double pctSingleCore = wallMs > 1 ? (cpuMs / wallMs) * 100.0 : 0.0;
					double cores = _effectiveCpuCount > 0.1 ? _effectiveCpuCount : Environment.ProcessorCount;
					double pctCgroup = wallMs > 1 ? (cpuMs / (wallMs * cores)) * 100.0 : 0.0;
					// Prefer cgroup-adjusted percentage but keep a tiny epsilon when nonzero CPU time observed
					double pct = Math.Max(0.0, Math.Min(100.0, pctCgroup));
					if (pct <= 0.0 && cpuMs > 0.0) pct = 0.1;
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

	private static double DetectEffectiveCpuCount()
	{
		try
		{
			var cpuMaxPath = "/sys/fs/cgroup/cpu.max";
			if (File.Exists(cpuMaxPath))
			{
				var parts = (File.ReadAllText(cpuMaxPath).Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
				if (parts.Length >= 2 && parts[0] != "max" && long.TryParse(parts[0], out var quota) && long.TryParse(parts[1], out var period) && period > 0)
				{
					return Math.Max(0.1, quota / (double)period);
				}
			}
			var quotaPath = "/sys/fs/cgroup/cpu/cpu.cfs_quota_us";
			var periodPath = "/sys/fs/cgroup/cpu/cpu.cfs_period_us";
			if (File.Exists(quotaPath) && File.Exists(periodPath))
			{
				if (long.TryParse(File.ReadAllText(quotaPath).Trim(), out var q) && long.TryParse(File.ReadAllText(periodPath).Trim(), out var p) && p > 0 && q > 0)
				{
					return Math.Max(0.1, q / (double)p);
				}
			}
		}
		catch { }
		return Environment.ProcessorCount;
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


