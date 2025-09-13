using System.Text.Json.Serialization;

namespace SensorSvc.Infrastructure;

public sealed class StatsDto
{
	[JsonPropertyName("uptime_s")] public double UptimeSeconds { get; init; }
	[JsonPropertyName("rss_mib")] public double RssMiB { get; init; }
	[JsonPropertyName("cpu_pct")] public double CpuPercent { get; init; }
	[JsonPropertyName("p50_ms")] public double P50Ms { get; init; }
	[JsonPropertyName("p95_ms")] public double P95Ms { get; init; }
	[JsonPropertyName("state")] public string State { get; init; } = "Unknown";

	public StatsDto() { }
	public StatsDto(double uptimeSeconds, double rssMiB, double cpuPercent, double p50Ms, double p95Ms, string state)
	{
		UptimeSeconds = uptimeSeconds;
		RssMiB = rssMiB;
		CpuPercent = cpuPercent;
		P50Ms = p50Ms;
		P95Ms = p95Ms;
		State = state;
	}
}

public sealed class LedsDto
{
	[JsonPropertyName("pins")] public int[] Pins { get; init; } = Array.Empty<int>();
	[JsonPropertyName("states")] public bool[] States { get; init; } = Array.Empty<bool>();

	public LedsDto() { }
	public LedsDto(int[] pins, bool[] states)
	{
		Pins = pins;
		States = states;
	}
}

public sealed class EnergyDto
{
	[JsonPropertyName("sAct")] public double SAct { get; init; }
	[JsonPropertyName("sIdle")] public double SIdle { get; init; }
	[JsonPropertyName("sSlp")] public double SSlp { get; init; }
	[JsonPropertyName("mWh")] public double MWh { get; init; }
	// LED-specific accumulated time per FSM state (seconds)
	[JsonPropertyName("ledActSeconds")] public double LedActSeconds { get; init; }
	[JsonPropertyName("ledIdleSeconds")] public double LedIdleSeconds { get; init; }
	[JsonPropertyName("ledSlpSeconds")] public double LedSlpSeconds { get; init; }
	// Additional energy from LEDs only (mWh) and total including them
	[JsonPropertyName("ledMWh")] public double LedMWh { get; init; }
	[JsonPropertyName("totalMWh")] public double TotalMWh { get; init; }

	public EnergyDto() { }
	public EnergyDto(double sAct, double sIdle, double sSlp, double mWh,
		double ledAct, double ledIdle, double ledSlp, double ledMWh, double totalMWh)
	{
		SAct = sAct; SIdle = sIdle; SSlp = sSlp; MWh = mWh;
		LedActSeconds = ledAct; LedIdleSeconds = ledIdle; LedSlpSeconds = ledSlp;
		LedMWh = ledMWh; TotalMWh = totalMWh;
	}
}

public sealed class HostDto
{
	[JsonPropertyName("cpuProcPct")] public double CpuProcPct { get; init; }
	[JsonPropertyName("rssMB")] public double RssMB { get; init; }
	[JsonPropertyName("gcMB")] public double GcMB { get; init; }
	[JsonPropertyName("threads")] public int Threads { get; init; }
	[JsonPropertyName("rps")] public double Rps { get; init; }

	public HostDto() { }
	public HostDto(double cpuProcPct, double rssMB, double gcMB, int threads, double rps)
	{
		CpuProcPct = cpuProcPct;
		RssMB = rssMB;
		GcMB = gcMB;
		Threads = threads;
		Rps = rps;
	}
}

public record ButtonPressDto(bool Pressed, string Message);
public record LoadDto(string Load, string Message);
public record LedPatternDto(int N);

public sealed class GcResultDto
{
	[JsonPropertyName("beforeMB")] public double BeforeMB { get; init; }
	[JsonPropertyName("afterMB")] public double AfterMB { get; init; }
	[JsonPropertyName("deltaMB")] public double DeltaMB { get; init; }
	[JsonPropertyName("compactAttempted")] public bool CompactAttempted { get; init; }
	[JsonPropertyName("compactSucceeded")] public bool CompactSucceeded { get; init; }
	[JsonPropertyName("allowCompact")] public bool AllowCompact { get; init; }
	[JsonPropertyName("gen0")] public int Gen0 { get; init; }
	[JsonPropertyName("gen1")] public int Gen1 { get; init; }
	[JsonPropertyName("gen2")] public int Gen2 { get; init; }

	public GcResultDto() { }
	public GcResultDto(double beforeMB, double afterMB, double deltaMB, bool compactAttempted, bool compactSucceeded, bool allowCompact, int gen0, int gen1, int gen2)
	{
		BeforeMB = beforeMB; AfterMB = afterMB; DeltaMB = deltaMB; CompactAttempted = compactAttempted; CompactSucceeded = compactSucceeded; AllowCompact = allowCompact; Gen0 = gen0; Gen1 = gen1; Gen2 = gen2;
	}
}

[JsonSerializable(typeof(StatsDto))]
[JsonSerializable(typeof(LedsDto))]
[JsonSerializable(typeof(EnergyDto))]
[JsonSerializable(typeof(HostDto))]
[JsonSerializable(typeof(ButtonPressDto))]
[JsonSerializable(typeof(LoadDto))]
[JsonSerializable(typeof(LedPatternDto))]
[JsonSerializable(typeof(GcResultDto))]
[JsonSerializable(typeof(MemDiagDto))]
public partial class AppJsonContext : JsonSerializerContext
{
}

public sealed class MemDiagDto
{
	[JsonPropertyName("heapSizeMB")] public double HeapSizeMB { get; init; }
	[JsonPropertyName("fragmentedMB")] public double FragmentedMB { get; init; }
	[JsonPropertyName("committedMB")] public double CommittedMB { get; init; }
	[JsonPropertyName("gen0")] public int Gen0 { get; init; }
	[JsonPropertyName("gen1")] public int Gen1 { get; init; }
	[JsonPropertyName("gen2")] public int Gen2 { get; init; }
	[JsonPropertyName("workingSetMB")] public double WorkingSetMB { get; init; }
	[JsonPropertyName("privateMB")] public double PrivateMB { get; init; }
	[JsonPropertyName("lohCompactionMode")] public string LohCompactionMode { get; init; } = string.Empty;
	[JsonPropertyName("pauseLastMs")] public double? PauseLastMs { get; init; }
	[JsonPropertyName("pauseAvgMs")] public double? PauseAvgMs { get; init; }
	[JsonPropertyName("time")] public DateTimeOffset Time { get; init; } = DateTimeOffset.UtcNow;
}


