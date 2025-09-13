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

	public EnergyDto() { }
	public EnergyDto(double sAct, double sIdle, double sSlp, double mWh)
	{
		SAct = sAct;
		SIdle = sIdle;
		SSlp = sSlp;
		MWh = mWh;
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

[JsonSerializable(typeof(StatsDto))]
[JsonSerializable(typeof(LedsDto))]
[JsonSerializable(typeof(EnergyDto))]
[JsonSerializable(typeof(HostDto))]
[JsonSerializable(typeof(ButtonPressDto))]
[JsonSerializable(typeof(LoadDto))]
[JsonSerializable(typeof(LedPatternDto))]
public partial class AppJsonContext : JsonSerializerContext
{
}


