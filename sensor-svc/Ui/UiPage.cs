namespace SensorSvc.Ui;

public static class UiPage
{
	public const string Html = """
<!doctype html>
<html lang=\"en\">
<head>
<meta charset=\"utf-8\"/>
<meta name=\"viewport\" content=\"width=device-width,initial-scale=1\"/>
<title>Sensor Service UI</title>
<style>
	body{font-family:system-ui,Segoe UI,Roboto,Ubuntu,sans-serif;margin:16px;color:#222}
	table{border-collapse:collapse;min-width:420px}
	th,td{border:1px solid #ccc;padding:8px 10px;text-align:left}
	td.num{text-align:right}
	.small{color:#666;font-size:12px;margin:6px 0}
	#err{color:#b00020}
</style>
</head>
<body>
	<h2>Sensor Service - Stats</h2>
	<div class=\"small\">Auto refresh every 2s • Last updated: <span id=\"ts\">-</span></div>
	<table>
		<thead>
			<tr><th>Metric</th><th>Value</th></tr>
		</thead>
		<tbody>
			<tr><td>Uptime (s)</td><td id=\"uptime\" class=\"num\">-</td></tr>
			<tr><td>RSS (MiB)</td><td id=\"rss\" class=\"num\">-</td></tr>
			<tr><td>CPU (%)</td><td id=\"cpu\" class=\"num\">-</td></tr>
			<tr><td>P50 (ms)</td><td id=\"p50\" class=\"num\">-</td></tr>
			<tr><td>P95 (ms)</td><td id=\"p95\" class=\"num\">-</td></tr>
			<tr><td>State</td><td id=\"state\">-</td></tr>
		</tbody>
	</table>
	<div id=\"err\" class=\"small\"></div>
<script>
function set(id,v){const el=document.getElementById(id); if(el) el.textContent=v;}
async function refresh(){
	const e=document.getElementById('err'); e.textContent='';
	try{
		const r=await fetch('/stats',{cache:'no-store'});
		if(!r.ok) throw new Error('HTTP '+r.status);
		const j=await r.json();
		set('uptime', (typeof j.uptime_s==='number'? j.uptime_s.toFixed(1): j.uptime_s));
		set('rss', (typeof j.rss_mib==='number'? j.rss_mib.toFixed(1): j.rss_mib));
		set('cpu', (typeof j.cpu_pct==='number'? j.cpu_pct.toFixed(1): j.cpu_pct));
		set('p50', (typeof j.p50_ms==='number'? j.p50_ms.toFixed(1): j.p50_ms));
		set('p95', (typeof j.p95_ms==='number'? j.p95_ms.toFixed(1): j.p95_ms));
		set('state', j.state||'-');
		set('ts', new Date().toLocaleTimeString());
	}catch(err){ e.textContent='Failed to load /stats: '+err.message; }
}
refresh();
setInterval(refresh,2000);
</script>
</body>
</html>
""";
}


