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

// Simple variables for testing
var startTime = DateTimeOffset.UtcNow;
var requestCount = 0;

app.MapGet("/health", () => Results.Text("ok", "text/plain", Encoding.UTF8));

app.MapGet("/stats", () =>
{
    var uptime = (DateTimeOffset.UtcNow - startTime).TotalSeconds;
    var process = Process.GetCurrentProcess();
    process.Refresh();
    
    var stats = new StatsDto
    {
        UptimeSeconds = Math.Round(uptime, 1),
        RssMiB = Math.Round(process.WorkingSet64 / (1024.0 * 1024.0), 1),
        CpuPercent = 0.0, // Simplified for now
        P50Ms = 1.0, // Simplified for now
        P95Ms = 2.0, // Simplified for now
        State = "Active"
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
    var energy = new EnergyDto
    {
        SAct = 10.0, // Simulated values
        SIdle = 5.0,
        SSlp = 2.0,
        MWh = Math.Round((Pact * 10.0 + Pidle * 5.0 + Psleep * 2.0) / 3600.0, 3)
    };
    return Results.Json(energy, AppJsonContext.Default.EnergyDto);
});

app.MapPost("/energy/reset", () => Results.Ok());

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
var ledCount = 10;
var ledStates = Enumerable.Repeat(false, ledCount).ToArray();

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
    for (int i = 0; i < ledCount; i++)
        ledStates[i] = ((m >> i) & 1UL) == 1;
    return Results.Ok(new { bits });
});

app.MapPost("/leds/pattern/bar/{n:int}", (int n) =>
{
    for (int i = 0; i < Math.Clamp(n, 0, ledCount); i++)
        ledStates[i] = true;
    for (int i = n; i < ledCount; i++)
        ledStates[i] = false;
    
    var result = new LedPatternDto(n);
    return Results.Json(result, AppJsonContext.Default.LedPatternDto);
});

// Button and load simulation
app.MapPost("/button/press", () =>
{
    var result = new ButtonPressDto(true, "Button pressed - CPU load for 2 seconds");
    return Results.Json(result, AppJsonContext.Default.ButtonPressDto);
});

app.MapPost("/load", () =>
{
    var result = new LoadDto("generated", "Heavy CPU load for 5 seconds");
    return Results.Json(result, AppJsonContext.Default.LoadDto);
});

app.Run("http://localhost:8080");
