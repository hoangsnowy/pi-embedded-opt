using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SensorSvc.Infrastructure;
using SensorSvc.Ui;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(opt =>
{
    opt.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    opt.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    opt.SerializerOptions.WriteIndented = false;
    opt.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default);
});

var app = builder.Build();

var latencyMetrics = new LatencyMetrics(maxSamples: 512);
var processCpuSampler = new ProcessCpuSampler(sampleIntervalMs: 800);
var processMetrics = new ProcessMetrics(processCpuSampler);
var fsm = new PowerFsm();
var lastRequestTicks = new AtomicLong(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
var startTime = DateTimeOffset.UtcNow;

processCpuSampler.Start();
fsm.Start(() => lastRequestTicks.Value);

app.Use(async (context, next) =>
{
    var sw = Stopwatch.StartNew();
    try
    {
        await next();
    }
    finally
    {
        sw.Stop();
        latencyMetrics.AddSample(sw.Elapsed.TotalMilliseconds);
        lastRequestTicks.Value = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
});

app.MapGet("/health", () => Results.Text("ok", "text/plain", Encoding.UTF8));

app.MapGet("/stats", () =>
{
    var uptime = (DateTimeOffset.UtcNow - startTime).TotalSeconds;
    var processSnapshot = processMetrics.GetSnapshot();
    var p50 = latencyMetrics.GetPercentile(0.5);
    var p95 = latencyMetrics.GetPercentile(0.95);
    var state = fsm.CurrentState.ToString();
    
    var stats = new StatsDto
    {
        UptimeSeconds = Math.Round(uptime, 1),
        RssMiB = Math.Round(processSnapshot.RssMB, 1),
        CpuPercent = Math.Round(processSnapshot.CpuPercent, 1),
        P50Ms = Math.Round(p50, 1),
        P95Ms = Math.Round(p95, 1),
        State = state
    };
    return Results.Json(stats, AppJsonContext.Default.StatsDto);
});

app.MapGet("/ui", async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.WriteAsync(UiPage.Html);
});

app.MapGet("/", () => Results.Redirect("/ui"));

// Energy endpoints
app.MapGet("/energy", (double Pact, double Pidle, double Psleep) =>
{
    var timeCounters = fsm.GetTimeCounters();
    var mWh = (Pact * timeCounters.ActiveSeconds + Pidle * timeCounters.IdleSeconds + Psleep * timeCounters.SleepSeconds) / 3600.0;
    
    var energy = new EnergyDto
    {
        SAct = Math.Round(timeCounters.ActiveSeconds, 1),
        SIdle = Math.Round(timeCounters.IdleSeconds, 1),
        SSlp = Math.Round(timeCounters.SleepSeconds, 1),
        MWh = Math.Round(mWh, 3)
    };
    return Results.Json(energy, AppJsonContext.Default.EnergyDto);
});

app.MapPost("/energy/reset", () =>
{
	fsm.ResetTimeCounters();
	return Results.Ok();
});

app.MapGet("/host", () =>
{
    var hostSnapshot = ProcSampler.Snapshot(Interlocked.Read(ref lastRequestTicks.Value));
    var host = new HostDto
    {
        CpuProcPct = hostSnapshot.CpuProcPct,
        RssMB = hostSnapshot.RssMB,
        GcMB = hostSnapshot.GcMB,
        Threads = hostSnapshot.Threads,
        Rps = hostSnapshot.Rps
    };
    return Results.Json(host, AppJsonContext.Default.HostDto);
});

// LED endpoints
var ledCount = 10; // Fixed to 10 for now
var ledStates = Enumerable.Repeat(false, ledCount).ToArray();

// Simulate memory usage based on LED count
var ledMemoryData = new List<byte[]>();

void ApplyMask(ulong m)
{
	for (int i = 0; i < ledCount; i++)
		ledStates[i] = ((m >> i) & 1UL) == 1;
	
	// Simulate memory usage - each LED uses 1MB when ON
	ledMemoryData.Clear();
	for (int i = 0; i < ledCount; i++)
	{
		if (ledStates[i])
		{
			// Allocate 1MB per LED when ON
			ledMemoryData.Add(new byte[1024 * 1024]);
		}
	}
}

app.MapGet("/leds", () =>
{
    var leds = new LedsDto
    {
        Pins = Array.Empty<int>(),
        States = ledStates
    };
    return Results.Json(leds, AppJsonContext.Default.LedsDto);
});

app.MapPost("/leds/pattern", (string bits) =>
{
	var m = Convert.ToUInt64(bits, 2);
	ApplyMask(m);
	return Results.Ok(new { bits });
});

app.MapPost("/leds/pattern/bar/{n:int}", (int n) =>
{
	ulong m = 0;
	for (int i = 0; i < Math.Clamp(n, 0, ledCount); i++)
		m |= (1UL << i);
	ApplyMask(m);
	var result = new LedPatternDto(n);
	return Results.Json(result, AppJsonContext.Default.LedPatternDto);
});

// Button and load simulation
app.MapPost("/button/press", () =>
{
	lastRequestTicks.Value = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
	_ = Task.Run(async () =>
	{
		var endTime = DateTime.UtcNow.AddSeconds(2);
		while (DateTime.UtcNow < endTime)
		{
			_ = Math.Sqrt(DateTime.UtcNow.Ticks);
			await Task.Delay(1);
		}
	});
	
	var result = new ButtonPressDto(true, "Button pressed - CPU load for 2 seconds");
	return Results.Json(result, AppJsonContext.Default.ButtonPressDto);
});

app.MapPost("/load", () =>
{
	lastRequestTicks.Value = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
	_ = Task.Run(async () =>
	{
		var endTime = DateTime.UtcNow.AddSeconds(5);
		while (DateTime.UtcNow < endTime)
		{
			for (int i = 0; i < 1000; i++)
			{
				_ = Math.Sqrt(DateTime.UtcNow.Ticks);
				_ = Math.Sin(DateTime.UtcNow.Ticks);
				_ = Math.Cos(DateTime.UtcNow.Ticks);
			}
			await Task.Delay(1);
		}
	});
	
	var result = new LoadDto("generated", "Heavy CPU load for 5 seconds");
	return Results.Json(result, AppJsonContext.Default.LoadDto);
});

// Temporarily disabled to fix crash
// app.Lifetime.ApplicationStopping.Register(() =>
// {
//     processCpuSampler.Stop();
//     fsm.Stop();
// });

app.Run();



