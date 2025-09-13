using System.Diagnostics;

namespace SensorSvc.Infrastructure;

public record HostSnapshot(double cpuProcPct, double rssMB, double gcMB, int threads, double rps);

public static class ProcSampler
{
	static readonly Process P = Process.GetCurrentProcess();
	static TimeSpan lastCpu = TimeSpan.Zero;
	static DateTime lastTs = DateTime.UtcNow;
	static long lastReq = 0;

	public static HostSnapshot Snapshot(long req)
	{
		try
		{
			P.Refresh();
			var now = DateTime.UtcNow;
			var dt = (now - lastTs).TotalSeconds;
			var cpu = P.TotalProcessorTime;
			var dCpu = (cpu - lastCpu).TotalSeconds;
			double cpuPct = dt > 0 ? (dCpu / dt) * 100.0 : 0;
			double rssMB = P.WorkingSet64 / (1024.0 * 1024.0);
			double gcMB = GC.GetTotalMemory(false) / (1024.0 * 1024.0);
			double rps = dt > 0 ? (req - lastReq) / dt : 0;
			lastCpu = cpu;
			lastTs = now;
			lastReq = req;
			return new HostSnapshot(Math.Round(cpuPct, 1), Math.Round(rssMB, 1), Math.Round(gcMB, 1), P.Threads.Count, Math.Round(rps, 1));
		}
		catch (Exception ex)
		{
			Console.WriteLine($"ProcSampler error: {ex.Message}");
			return new HostSnapshot(0, 0, 0, 0, 0);
		}
	}
}
