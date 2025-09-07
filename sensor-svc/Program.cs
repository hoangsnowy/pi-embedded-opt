using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SensorSvc.Infrastructure;
using SensorSvc.Power;
using SensorSvc.Ui;
using SensorSvc.Util;

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
	var now = DateTimeOffset.UtcNow;
	var uptimeSeconds = (now - Process.GetCurrentProcess().StartTime.ToUniversalTime()).TotalSeconds;
	var (p50, p95, _) = latencyMetrics.GetPercentiles();
	var dto = new StatsDto(
		uptimeSeconds: Math.Round(uptimeSeconds, 1),
		rssMiB: Math.Round(processMetrics.GetRssMiB(), 1),
		cpuPercent: Math.Round(processMetrics.GetCpuPercent(), 1),
		p50Ms: Math.Round(p50, 1),
		p95Ms: Math.Round(p95, 1),
		state: fsm.CurrentState.ToString()
	);
	return Results.Json(dto, AppJsonContext.Default.StatsDto);
});

app.MapGet("/ui", async context =>
{
	context.Response.ContentType = "text/html; charset=utf-8";
	await context.Response.WriteAsync(UiPage.Html);
});

app.MapGet("/", () => Results.Redirect("/ui"));

app.Lifetime.ApplicationStopping.Register(() =>
{
	processCpuSampler.Stop();
	fsm.Stop();
});

app.Run();



