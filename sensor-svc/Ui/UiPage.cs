namespace SensorSvc.Ui;

public static class UiPage
{
	public const string Html = """
<!doctype html>
<html lang="en">
<head>
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Sensor Service Stats</title>
<style>
	body{font-family:system-ui,-apple-system,Segoe UI,Roboto,Ubuntu,Cantarell,Noto Sans,sans-serif;margin:20px;color:#222}
	table{border-collapse:collapse;min-width:420px}
	th,td{border:1px solid #ccc;padding:8px 10px;text-align:left}
	td.num{text-align:right}
	code{background:#f6f8fa;padding:2px 4px;border-radius:4px}
	.small{color:#666;font-size:12px}
</style>
</head>
<body>
	<h2>Sensor Service - Realtime Stats</h2>
	<div class="small">Auto refresh every 2s</div>
	<table>
		<thead>
			<tr><th>Metric</th><th>Value</th></tr>
		</thead>
		<tbody>
			<tr><td>Uptime (s)</td><td id="uptime" class="num">-</td></tr>
			<tr><td>RSS (MiB)</td><td id="rss" class="num">-</td></tr>
			<tr><td>CPU (%)</td><td id="cpu" class="num">-</td></tr>
			<tr><td>P50 (ms)</td><td id="p50" class="num">-</td></tr>
			<tr><td>P95 (ms)</td><td id="p95" class="num">-</td></tr>
			<tr><td>State</td><td id="state">-</td></tr>
		</tbody>
	</table>
	<p class="small">Endpoints: <code>/health</code>, <code>/stats</code>, <code>/ui</code></p>
<script>
async function loadStats(){
	try{
		const res = await fetch('/stats',{cache:'no-store'});
		const j = await res.json();
		document.getElementById('uptime').textContent = j.uptime_s?.toFixed?.(1) ?? j.uptime_s;
		document.getElementById('rss').textContent = j.rss_mib?.toFixed?.(1) ?? j.rss_mib;
		document.getElementById('cpu').textContent = j.cpu_pct?.toFixed?.(1) ?? j.cpu_pct;
		document.getElementById('p50').textContent = j.p50_ms?.toFixed?.(1) ?? j.p50_ms;
		document.getElementById('p95').textContent = j.p95_ms?.toFixed?.(1) ?? j.p95_ms;
		document.getElementById('state').textContent = j.state ?? '-';
	}catch(e){
		console.error(e);
	}
}
loadStats();
setInterval(loadStats, 2000);
</script>
</body>
</html>
""";
}


