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

[JsonSerializable(typeof(StatsDto))]
public partial class AppJsonContext : JsonSerializerContext
{
}


