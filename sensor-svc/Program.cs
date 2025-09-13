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

// --- Runtime state / metrics wiring ---
var startTime = DateTimeOffset.UtcNow;
long requestCount = 0;
var lastRequestUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
var latency = new SensorSvc.Infrastructure.LatencyMetrics(512);
var cpuSampler = new SensorSvc.Infrastructure.ProcessCpuSampler(800);
cpuSampler.Start();
var processMetrics = new SensorSvc.Infrastructure.ProcessMetrics(cpuSampler);
var fsm = new SensorSvc.Power.PowerFsm();
fsm.Start(() => Volatile.Read(ref lastRequestUnixMs));

// --- LED simulation & power accumulation state ---
// Allow overriding LED count via environment variable (default 10)
var ledCount = int.TryParse(Environment.GetEnvironmentVariable("LED_COUNT"), out var lc) && lc > 0 ? lc : 10;
// Memory buffers to simulate 10MB per LED when ON (demo purpose) - nullable elements (off LEDs are null)
var ledBuffers = new byte[]?[ledCount];
var ledStates = Enumerable.Repeat(false, ledCount).ToArray();
const int LedBytesPer = 10_000_000; // 10MB
var ledPooling = (Environment.GetEnvironmentVariable("LED_POOL") ?? "0") == "1";
var ledPool = new Stack<byte[]>();
object ledPoolLock = new();

// LED power accumulation (per FSM state). Simple periodic sampler piggybacks on request middleware timing; we also sample when /energy called.
double ledActS = 0, ledIdleS = 0, ledSlpS = 0; // seconds accumulated with >=1 LED on in each FSM state
long lastLedSampleMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
double PerLedMilliW = double.TryParse(Environment.GetEnvironmentVariable("LED_MW"), out var lm) && lm >= 0 ? lm : 50.0; // default 50 mW per LED

void SampleLedPower()
{
    var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    var dt = (nowMs - Interlocked.Read(ref lastLedSampleMs)) / 1000.0;
    if (dt <= 0) return;
    Interlocked.Exchange(ref lastLedSampleMs, nowMs);
    int onCount = ledStates.Count(b => b);
    if (onCount == 0) return; // no LEDs on -> no accumulation
    var state = fsm.CurrentState;
    var scaledDt = dt * onCount; // accumulate LED-on seconds scaled by number of LEDs
    switch (state)
    {
        case SensorSvc.Power.FsmState.Active: ledActS += scaledDt; break;
        case SensorSvc.Power.FsmState.Idle: ledIdleS += scaledDt; break;
        case SensorSvc.Power.FsmState.Sleep: ledSlpS += scaledDt; break;
    }
}


app.MapGet("/health", () => Results.Text("ok", "text/plain", Encoding.UTF8));

app.MapGet("/stats", () =>
{
    var uptime = (DateTimeOffset.UtcNow - startTime).TotalSeconds;
    var rss = processMetrics.GetRssMiB();
    var cpu = processMetrics.GetCpuPercent();
    var (p50, p95, _) = latency.GetPercentiles();
    var state = fsm.CurrentState.ToString();
    var stats = new StatsDto
    {
        UptimeSeconds = Math.Round(uptime, 1),
        RssMiB = Math.Round(rss, 1),
        CpuPercent = Math.Round(cpu, 1),
        P50Ms = Math.Round(p50, 2),
        P95Ms = Math.Round(p95, 2),
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
// NOTE: /energy now exposes extended LED-related fields:
// { sAct, sIdle, sSlp, mWh, ledActSeconds, ledIdleSeconds, ledSlpSeconds, ledMWh, totalMWh }
// Base mWh = (Pact*sAct + Pidle*sIdle + Psleep*sSlp)/3600. LED mWh = LED_MW * (sum(LED-on seconds scaled by count))/3600.
// Passive polling (/stats,/leds,/energy) is intentionally excluded from "activity" to let FSM progress to Idle/Sleep while UI is open.
app.MapGet("/energy", (double Pact, double Pidle, double Psleep) =>
{
    SampleLedPower();
    var (sAct, sIdle, sSlp) = fsm.GetTimeInState();
    Pact = Math.Max(0, Pact); Pidle = Math.Max(0, Pidle); Psleep = Math.Max(0, Psleep);
    // mWh = (mW * seconds)/3600_000 -> (mW * s)/3600 converts to mWh
    var baseMWh = (Pact * sAct + Pidle * sIdle + Psleep * sSlp) / 3600.0;
    // LED energy: per LED mW * (sum of led-on seconds) / 3600
    var ledSecondsTotal = ledActS + ledIdleS + ledSlpS; // already scaled by number of LEDs on
    var ledMWh = (PerLedMilliW * ledSecondsTotal) / 3600.0;
    var total = baseMWh + ledMWh;
    var energy = new EnergyDto
    {
        SAct = Math.Round(sAct, 2),
        SIdle = Math.Round(sIdle, 2),
        SSlp = Math.Round(sSlp, 2),
        MWh = Math.Round(baseMWh, 4),
        LedActSeconds = Math.Round(ledActS, 2),
        LedIdleSeconds = Math.Round(ledIdleS, 2),
        LedSlpSeconds = Math.Round(ledSlpS, 2),
        LedMWh = Math.Round(ledMWh, 4),
        TotalMWh = Math.Round(total, 4)
    };
    return Results.Json(energy, AppJsonContext.Default.EnergyDto);
});

app.MapPost("/energy/reset", () => { fsm.ResetTimeCounters(); ledActS = ledIdleS = ledSlpS = 0; lastLedSampleMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); return Results.Ok(); });

// Manual GC endpoint for demo: forces full blocking collection and returns before/after managed heap sizes.
app.MapPost("/gc", () =>
{
    try
    {
        var allowCompact = (Environment.GetEnvironmentVariable("GC_COMPACT") ?? "1") != "0";
        long before = GC.GetTotalMemory(false);
        bool compactAttempted = true;
        bool compactSucceeded = true;
        try
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: allowCompact);
        }
        catch (PlatformNotSupportedException)
        {
            compactSucceeded = false; // platform/runtime refused compacting
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: false);
        }
        catch (Exception)
        {
            // Some other unexpected exception during compacting attempt; fall back to non-compacting
            compactSucceeded = false;
            try { GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: false); }
            catch { /* ignore secondary failure */ }
        }
        GC.WaitForPendingFinalizers();
        long after = GC.GetTotalMemory(true);
        var deltaMB = (after - before) / (1024.0 * 1024.0);
        Console.WriteLine($"[GC] before={before/1024/1024:F2}MB after={after/1024/1024:F2}MB delta={deltaMB:F2}MB compactAttempted={compactAttempted} compactSucceeded={compactSucceeded} allowCompact={allowCompact}");
        var dto = new GcResultDto(
            Math.Round(before / (1024.0 * 1024.0), 3),
            Math.Round(after / (1024.0 * 1024.0), 3),
            Math.Round(deltaMB, 3),
            compactAttempted,
            compactSucceeded,
            allowCompact,
            GC.CollectionCount(0),
            GC.CollectionCount(1),
            GC.CollectionCount(2)
        );
        return Results.Json(dto, AppJsonContext.Default.GcResultDto);
    }
    catch (Exception ex)
    {
        // Return structured error so UI/user can see cause instead of opaque 500
        var err = new { error = ex.GetType().Name, message = ex.Message };
        return Results.Json(err, statusCode: 500);
    }
});

// Memory diagnostics endpoint
app.MapGet("/diag/mem", () =>
{
    var info = GC.GetGCMemoryInfo();
    var proc = Process.GetCurrentProcess();
    proc.Refresh();
    double ToMB(long bytes) => bytes / 1024.0 / 1024.0;
    double? lastPause = null; double? avgPause = null;
    if (info.PauseDurations.Length > 0)
    {
        lastPause = info.PauseDurations[^1].TotalMilliseconds;
        double sum = 0;
        for (int i = 0; i < info.PauseDurations.Length; i++) sum += info.PauseDurations[i].TotalMilliseconds;
        avgPause = sum / info.PauseDurations.Length;
    }
    var dto = new MemDiagDto
    {
        HeapSizeMB = Math.Round(ToMB(info.HeapSizeBytes), 2),
        FragmentedMB = Math.Round(ToMB(info.FragmentedBytes), 2),
        CommittedMB = Math.Round(ToMB(info.TotalCommittedBytes), 2),
        Gen0 = GC.CollectionCount(0),
        Gen1 = GC.CollectionCount(1),
        Gen2 = GC.CollectionCount(2),
        WorkingSetMB = Math.Round(ToMB(proc.WorkingSet64), 2),
        PrivateMB = Math.Round(ToMB(proc.PrivateMemorySize64), 2),
        LohCompactionMode = System.Runtime.GCSettings.LargeObjectHeapCompactionMode.ToString(),
        PauseLastMs = lastPause,
        PauseAvgMs = avgPause,
        Time = DateTimeOffset.UtcNow
    };
    return Results.Json(dto, AppJsonContext.Default.MemDiagDto);
});

// Raw text variant for quick curl debugging if JSON serialization is suspected
app.MapPost("/gc/raw", () =>
{
    try
    {
        var allowCompact = (Environment.GetEnvironmentVariable("GC_COMPACT") ?? "1") != "0";
        long before = GC.GetTotalMemory(false);
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: allowCompact);
        GC.WaitForPendingFinalizers();
        long after = GC.GetTotalMemory(true);
        var deltaMB = (after - before) / (1024.0 * 1024.0);
        return Results.Text($"GC OK before={before/1024/1024:F2}MB after={after/1024/1024:F2}MB delta={deltaMB:F2}MB compact={allowCompact}");
    }
    catch (Exception ex)
    {
        return Results.Text($"GC ERROR {ex.GetType().Name}: {ex.Message}");
    }
});

app.MapGet("/host", () =>
{
    var process = Process.GetCurrentProcess();
    process.Refresh();
    
    var host = new HostDto
    {
        CpuProcPct = 0.0,
        RssMB = Math.Round(process.WorkingSet64 / (1024.0 * 1024.0), 1),
        GcMB = Math.Round(GC.GetTotalMemory(false) / (1024.0 * 1024.0), 1),
        Threads = process.Threads.Count,
        Rps = 0.0
    };
    return Results.Json(host, AppJsonContext.Default.HostDto);
});

// LED endpoints

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
    // Capture elapsed LED-on interval prior to state change
    SampleLedPower();
    var m = Convert.ToUInt64(bits, 2);
    for (int i = 0; i < ledCount; i++)
    {
        bool on = ((m >> i) & 1UL) == 1;
        ledStates[i] = on;
        if (on && ledBuffers[i] == null)
        {
            if (ledPooling)
            {
                lock (ledPoolLock)
                    ledBuffers[i] = ledPool.Count > 0 ? ledPool.Pop() : new byte[LedBytesPer];
            }
            else
            {
                ledBuffers[i] = new byte[LedBytesPer];
            }
        }
        if (!on && ledBuffers[i] != null)
        {
            if (ledPooling)
            {
                var buf = ledBuffers[i]!;
                ledBuffers[i] = null; // logically released
                lock (ledPoolLock) ledPool.Push(buf); // retained for reuse
            }
            else
            {
                ledBuffers[i] = null; // eligible for GC
            }
        }
    }
    return Results.Ok(new { bits });
});

app.MapPost("/leds/pattern/bar/{n:int}", (int n) =>
{
    // Capture elapsed LED-on interval prior to bar update
    SampleLedPower();
    n = Math.Clamp(n, 0, ledCount);
    for (int i = 0; i < ledCount; i++)
    {
        bool on = i < n;
        ledStates[i] = on;
        if (on && ledBuffers[i] == null)
        {
            if (ledPooling)
            {
                lock (ledPoolLock)
                    ledBuffers[i] = ledPool.Count > 0 ? ledPool.Pop() : new byte[LedBytesPer];
            }
            else
            {
                ledBuffers[i] = new byte[LedBytesPer];
            }
        }
        if (!on && ledBuffers[i] != null)
        {
            if (ledPooling)
            {
                var buf = ledBuffers[i]!;
                ledBuffers[i] = null;
                lock (ledPoolLock) ledPool.Push(buf);
            }
            else
            {
                ledBuffers[i] = null;
            }
        }
    }
    var result = new LedPatternDto(n);
    return Results.Json(result, AppJsonContext.Default.LedPatternDto);
});

// Button and load simulation
app.MapPost("/button/press", async () =>
{
    // Simulate short CPU load (~2s) using tight loop Task
    var sw = Stopwatch.StartNew();
    await Task.Run(() =>
    {
        while (sw.ElapsedMilliseconds < 2000)
        {
            // Busy work
            _ = Math.Sqrt(sw.ElapsedMilliseconds + 123.456);
        }
    });
    var result = new ButtonPressDto(true, "Button pressed - CPU load for 2 seconds");
    return Results.Json(result, AppJsonContext.Default.ButtonPressDto);
});

app.MapPost("/load", async () =>
{
    var sw = Stopwatch.StartNew();
    await Task.Run(() =>
    {
        while (sw.ElapsedMilliseconds < 5000)
        {
            // Heavier busy work
            double v = 0;
            for (int i = 0; i < 10_000; i++) v += Math.Sin(i * 0.001);
            if (v < 0) Console.Write("");
        }
    });
    var result = new LoadDto("generated", "Heavy CPU load for 5 seconds");
    return Results.Json(result, AppJsonContext.Default.LoadDto);
});

// --- Middleware for request metrics & activity tracking ---
bool AnyLedOn() => ledStates.Any(s => s);
bool IsWorkPath(string p) => p.StartsWith("/button") || p.StartsWith("/load");
const double CpuActivityThreshold = 2.0; // % CPU to consider as real activity

app.Use(async (ctx, next) =>
{
    var sw = Stopwatch.StartNew();
    await next();
    sw.Stop();
    latency.AddSample(sw.Elapsed.TotalMilliseconds);
    Interlocked.Increment(ref requestCount);
    var path = ctx.Request.Path.Value ?? string.Empty;

    // Passive polling still ignored: /stats, /leds, /energy
    if (!(path.StartsWith("/stats") || path.StartsWith("/leds") || path.StartsWith("/energy")))
    {
        // Activity gating (Option A): only update if meaningful
        var cpuNow = processMetrics.GetCpuPercent();
        if (IsWorkPath(path) || AnyLedOn() || cpuNow > CpuActivityThreshold)
        {
            Volatile.Write(ref lastRequestUnixMs, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }
    }
    SampleLedPower();
});

// Forced idle accelerator (Option E): shorten effective active window when system quiescent
_ = Task.Run(async () =>
{
    const int checkMs = 1000;
    const int quiescentMs = 2000; // after 2s of no gated activity + no LEDs, allow quick Idle
    while (true)
    {
        try
        {
            await Task.Delay(checkMs);
            if (!AnyLedOn())
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var last = Volatile.Read(ref lastRequestUnixMs);
                if (now - last > quiescentMs)
                {
                    // "Age" last activity so FSM sees longer inactivity (push back further)
                    // Here we subtract extra ACTIVE_WINDOW_S equivalent (simplistic approach)
                    Volatile.Write(ref lastRequestUnixMs, last - 5000); // pretend 5s older
                }
            }
        }
        catch { /* swallow */ }
    }
});

// Do not hardcode localhost here; rely on ASPNETCORE_URLS (set in Dockerfile) so it binds to 0.0.0.0 inside container.
app.Run();
