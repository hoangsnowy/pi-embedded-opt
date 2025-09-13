namespace SensorSvc.Ui;

public static class UiPage
{
	public const string Html = """
<!doctype html>
<html lang="en">
<head>
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<meta http-equiv="Cache-Control" content="no-cache, no-store, must-revalidate"/>
<meta http-equiv="Pragma" content="no-cache"/>
<meta http-equiv="Expires" content="0"/>
<title>Pi Embedded Dashboard</title>
<style>
	:root{--led-dim:1;--bg-primary:#0a0e14;--bg-secondary:#161a21;--bg-tertiary:#222832;--text-primary:#e8e8e8;--text-secondary:#9aa0a6;--accent:#3a4354;--success:#4caf50;--warning:#ff9800;--error:#f44336}
	*{box-sizing:border-box}
	body{margin:0;font-family:system-ui,-apple-system,sans-serif;background:var(--bg-primary);color:var(--text-primary);line-height:1.5}
	.page{padding:20px;max-width:1200px;margin:0 auto}
	.dashboard h1{margin:0 0 30px 0;color:var(--text-primary);font-size:2rem;font-weight:600}
	.panel{background:var(--bg-secondary);border:1px solid var(--accent);border-radius:12px;padding:20px;margin-bottom:20px}
	.panel h3{margin:0 0 15px 0;color:var(--text-primary);font-size:1.2rem}
	.led-grid{display:grid;grid-template-columns:repeat(10,1fr);gap:8px;margin:15px 0;padding:15px;background:var(--bg-tertiary);border-radius:8px}
	.led{aspect-ratio:1;border-radius:50%;border:2px solid var(--accent);background:#333;transition:all 0.2s ease}
	.led.on{background:var(--success);box-shadow:0 0 10px var(--success);filter:brightness(calc(0.85 * var(--led-dim)))}
	.led.off{background:#333}
	.controls{display:flex;gap:15px;align-items:center;flex-wrap:wrap;margin-top:15px}
	.btn{background:var(--accent);color:var(--text-primary);border:none;padding:8px 16px;border-radius:6px;cursor:pointer;font-size:14px;transition:background 0.2s ease}
	.btn:hover{background:#4a5568}
	.btn:active{transform:translateY(1px)}
	input[type="range"]{width:120px;margin:0 8px}
	.row{display:flex;gap:15px;align-items:center;flex-wrap:wrap;margin-bottom:15px}
	.chip{background:var(--accent);color:var(--text-primary);padding:4px 12px;border-radius:16px;font-size:12px;font-weight:500}
	.toggle{display:flex;align-items:center;gap:8px;cursor:pointer}
	.toggle input[type="checkbox"]{width:18px;height:18px;cursor:pointer}
	.badge{padding:2px 8px;border:1px solid var(--accent);border-radius:999px;font-size:12px;color:var(--text-secondary);background:var(--bg-tertiary)}
	.help-text{margin-top:8px;color:var(--text-secondary);font-size:12px;background:var(--bg-tertiary);padding:8px;border-radius:6px;border-left:3px solid var(--accent)}
	.demo-explanation{background:var(--bg-tertiary);border:2px solid var(--accent);border-radius:12px;padding:20px;margin-bottom:20px;text-align:center}
	.demo-explanation h3{color:var(--accent);margin-bottom:15px}
	.demo-explanation p{margin:8px 0;color:var(--text-secondary)}
	.notifications{position:fixed;top:20px;right:20px;z-index:1000}
	.notification{padding:12px 20px;margin:8px 0;border-radius:8px;color:white;font-weight:500;animation:slideIn 0.3s ease-out}
	.notification.success{background:#10b981}
	.notification.warning{background:#f59e0b}
	.notification.error{background:#ef4444}
	.cpu-flash{animation:cpuFlash 0.8s ease-out}
	@keyframes cpuFlash{0%{background:rgba(255,152,0,0.2)}50%{background:rgba(255,152,0,0.05)}100%{background:transparent}}
	@keyframes slideIn{from{transform:translateX(100%);opacity:0}to{transform:translateX(0);opacity:1}}
	.energy{background:var(--bg-tertiary);border:1px solid var(--accent);border-radius:12px;padding:15px;margin-top:10px}
	.energy-row{display:flex;gap:10px;flex-wrap:wrap;align-items:center;margin-bottom:10px}
	.energy-row input{width:90px;background:var(--bg-primary);border:1px solid var(--accent);color:var(--text-primary);border-radius:6px;padding:6px 8px;font-size:14px}
	.energy-row label{font-size:14px;color:var(--text-secondary)}
	.energy-stats{display:flex;gap:15px;align-items:baseline;flex-wrap:wrap}
	.energy-stats b{color:var(--success);font-size:1.1rem}
	.muted{color:var(--text-secondary);font-size:0.9rem}
	.stats-grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(150px,1fr));gap:15px}
	.stat{display:flex;flex-direction:column;gap:5px}
	.stat label{font-size:12px;color:var(--text-secondary);text-transform:uppercase;letter-spacing:0.5px}
	.stat span{font-size:1.2rem;font-weight:600;color:var(--text-primary)}
	.state-active{color:var(--success)}
	.state-idle{color:var(--warning)}
	.state-sleep{color:var(--text-secondary)}
	:root.eco{--led-dim:0.5}
	@media (max-width:768px){.page{padding:15px}.controls{flex-direction:column;align-items:stretch}.energy-row{flex-direction:column;align-items:stretch}.energy-row input{width:100%}}
</style>
</head>
<body>
	<div class="page">
		<div class="dashboard">
			<h1>🔋 Pi Energy Optimization Demo</h1>
			<div class="demo-explanation">
				<h3>🔋 Pi Energy Demo</h3>
				<p>LED = Memory | Button = CPU Load | Energy = Power Calculation</p>
			</div>
			
			<div id="notifications" class="notifications"></div>
			
			<div class="panel">
				<h3>LED Grid (10 LEDs)</h3>
				<div class="led-grid" id="ledGrid"></div>
				
				<div class="controls">
					<button onclick="lightLoad()" class="btn">🔥 Light Load (B) - CPU 2s</button>
					<button onclick="generateLoad()" class="btn">⚡ Generate Load (L) - Heavy CPU 5s</button>
					<button onclick="runGc()" class="btn">🧹 GC Collect</button>
					<label>
						💡 Active LEDs (bar): 
						<input type="range" min="0" max="10" value="0" id="ledSlider" onchange="updateBarPattern()" />
						<span id="ledCount">0</span>
						<small>(10MB per LED)</small>
					</label>
				</div>
			</div>

			<div class="panel">
				<div class="row">
					<label class="chip">ECO</label>
					<label class="toggle">
						<input type="checkbox" id="ecoMode" onchange="toggleEcoMode()" />
						Eco mode (Tiết kiệm năng lượng)
					</label>
					<label class="toggle">
						<input type="checkbox" id="autoEnergy" checked onchange="toggleAutoEnergy()" />
						Auto Energy
					</label>
					<span id="lowPowerBadge" class="badge" style="display:none">LOW POWER</span>
				</div>
				<div class="help-text">
					<small>Eco mode: Giảm tần suất cập nhật (1000ms thay vì 500ms) và làm mờ LED 50%</small>
				</div>

				<div class="energy">
					<div class="energy-row">
						<label>P_active (mW)</label>
						<input type="number" step="10" id="pact" value="1200" />
						<label>P_idle</label>
						<input type="number" step="10" id="pidle" value="400" />
						<label>P_sleep</label>
						<input type="number" step="10" id="psleep" value="80" />
						<button onclick="computeEnergy()" class="btn">📊 Compute mWh</button>
						<button onclick="resetEnergy()" class="btn">🔄 Reset Times</button>
					</div>
				<div class="energy-stats" style="flex-direction:column;align-items:flex-start;gap:6px">
					<div><span>System mWh:</span> <b id="mwh">—</b> <span class="muted" id="timeStats">Active: 0s • Idle: 0s • Sleep: 0s</span></div>
					<div id="ledStats">LED: 0sA / 0sI / 0sS • Led mWh: 0 • Total: 0</div>
				</div>
				<div class="help-text">
					<small>💡 Bấm "Light Load" để tạo Active (~2s) | Đợi không hoạt động 5s để chuyển Idle | "Heavy Load" tạo tải 5s</small>
				</div>
				</div>
			</div>

			<div class="panel">
				<h3>System Stats (Thống kê hệ thống)</h3>
				<div class="stats-grid">
					<div class="stat">
						<label>Uptime (Thời gian chạy)</label>
						<span id="uptime">—</span>
					</div>
					<div class="stat">
						<label>RSS (Bộ nhớ RAM)</label>
						<span id="rss">—</span>
					</div>
					<div class="stat">
						<label>CPU</label>
						<span id="cpu">—</span>
					</div>
					<div class="stat">
						<label>P50 (Độ trễ 50%)</label>
						<span id="p50">—</span>
					</div>
					<div class="stat">
						<label>P95 (Độ trễ 95%)</label>
						<span id="p95">—</span>
					</div>
					<div class="stat">
						<label>State (Trạng thái)</label>
						<span id="state" class="state-unknown">—</span>
					</div>
				</div>
				<div class="help-text">
					<small>P50/P95: Thời gian phản hồi API (ms). P50 = 50% requests nhanh hơn, P95 = 95% requests nhanh hơn</small>
				</div>
			</div>
		</div>
	</div>

<script>
let ledStates = new Array(10).fill(false);
let ecoMode = false;
let lowPower = false;
let pollMs = 500;
let autoEnergy = true;
let lastEnergyMs = 0;

function set(id, v) {
	const el = document.getElementById(id);
	if (el) el.textContent = v;
}

function setClass(id, className) {
	const el = document.getElementById(id);
	if (el) el.className = className;
}

function createLedGrid() {
	const grid = document.getElementById('ledGrid');
	grid.innerHTML = '';
	for (let i = 0; i < 10; i++) {
		const led = document.createElement('div');
		led.className = 'led off';
		led.dataset.index = i;
		grid.appendChild(led);
	}
}

function updateLedDisplay() {
	const leds = document.querySelectorAll('.led');
	leds.forEach((led, i) => {
		led.className = ledStates[i] ? 'led on' : 'led off';
	});
}

async function refreshStats() {
	try {
		const r = await fetch('/stats', {cache: 'no-store'});
		if (!r.ok) throw new Error('HTTP ' + r.status);
		const j = await r.json();
		set('uptime', (typeof j.uptime_s === 'number' ? j.uptime_s.toFixed(1) : j.uptime_s));
		set('rss', (typeof j.rss_mib === 'number' ? j.rss_mib.toFixed(1) : j.rss_mib) + ' MiB');
		set('cpu', (typeof j.cpu_pct === 'number' ? j.cpu_pct.toFixed(1) : j.cpu_pct) + '%');
		set('p50', (typeof j.p50_ms === 'number' ? j.p50_ms.toFixed(1) : j.p50_ms) + 'ms');
		set('p95', (typeof j.p95_ms === 'number' ? j.p95_ms.toFixed(1) : j.p95_ms) + 'ms');
		set('state', j.state || '—');
		setClass('state', 'state-' + (j.state || 'unknown').toLowerCase());
	} catch (err) {
		console.error('Failed to load stats:', err);
	}
}

async function refreshEnergy() {
	try {
		const pact = document.getElementById('pact').value;
		const pidle = document.getElementById('pidle').value;
		const psleep = document.getElementById('psleep').value;
		const r = await fetch(`/energy?Pact=${pact}&Pidle=${pidle}&Psleep=${psleep}`);
		if (!r.ok) throw new Error('HTTP ' + r.status);
		const j = await r.json();
		set('mwh', (typeof j.mWh === 'number') ? j.mWh.toFixed(3) : '—');
		set('timeStats', `Active: ${j.sAct ? j.sAct.toFixed(1) : '0'}s • Idle: ${j.sIdle ? j.sIdle.toFixed(1) : '0'}s • Sleep: ${j.sSlp ? j.sSlp.toFixed(1) : '0'}s`);
		if (typeof j.ledActSeconds === 'number') {
			const ledLine = `LED: ${(j.ledActSeconds||0).toFixed(1)}sA / ${(j.ledIdleSeconds||0).toFixed(1)}sI / ${(j.ledSlpSeconds||0).toFixed(1)}sS • Led mWh: ${(j.ledMWh||0).toFixed(3)} • Total: ${(j.totalMWh||0).toFixed(3)}`;
			const el = document.getElementById('ledStats'); if (el) el.textContent = ledLine;
		}
		if (j.sAct === 0 && j.sIdle === 0 && j.sSlp === 0) {
			showNotification('⚠️ Chưa có hoạt động! Bấm "Light Load" hoặc bật vài LED để tạo Active time', 'warning');
		}
	} catch (err) {
		console.error('Failed to load energy:', err);
	}
}

async function refreshLeds() {
	try {
		const r = await fetch('/leds');
		if (!r.ok) throw new Error('HTTP ' + r.status);
		const j = await r.json();
		if (j.states) {
			ledStates = [...j.states];
			updateLedDisplay();
		}
	} catch (err) {
		console.error('Failed to load LEDs:', err);
	}
}

async function lightLoad() {
	try {
		const response = await fetch('/button/press', {method: 'POST'});
		if (response.ok) {
			const result = await response.json();
			showNotification('🔥 Button pressed! CPU load for 2 seconds', 'success');
			flashCpu();
		} else {
			showNotification('❌ Button press failed', 'error');
		}
	} catch (err) {
		console.error('Failed to run light load:', err);
		showNotification('❌ Network error', 'error');
	}
}

async function generateLoad() {
	try {
		const response = await fetch('/load', {method: 'POST'});
		if (response.ok) {
			const result = await response.json();
			showNotification('⚡ Heavy CPU load for 5 seconds!', 'warning');
			flashCpu();
		} else {
			showNotification('❌ Load generation failed', 'error');
		}
	} catch (err) {
		console.error('Failed to generate load:', err);
		showNotification('❌ Network error', 'error');
	}
}

async function runGc() {
	try {
		const r = await fetch('/gc', {method: 'POST'});
		if (!r.ok) throw new Error('HTTP ' + r.status);
		const j = await r.json();
		showNotification(`🧹 GC: before ${j.beforeMB}MB → after ${j.afterMB}MB (Δ ${j.deltaMB}MB)`, 'info');
		await refreshStats();
	} catch (e) {
		showNotification('❌ GC failed', 'error');
	}
}

async function updateBarPattern() {
	const n = parseInt(document.getElementById('ledSlider').value);
	document.getElementById('ledCount').textContent = n;
	try {
		const response = await fetch(`/leds/pattern/bar/${n}`, {method: 'POST'});
		if (response.ok) {
			await refreshLeds();
			if (autoEnergy) await refreshEnergy();
			showNotification(`💡 ${n} LEDs ON (${n*10}MB memory allocated)`, 'info');
		} else {
			showNotification('❌ Failed to update LEDs', 'error');
		}
	} catch (err) {
		console.error('Failed to update LED pattern:', err);
		showNotification('❌ Network error', 'error');
	}
}

async function computeEnergy() {
	await refreshEnergy();
	showNotification('📊 Đã tính toán mWh dựa trên thời gian Active/Idle/Sleep', 'info');
}

async function resetEnergy() {
	try {
		await fetch('/energy/reset', {method: 'POST'});
		await refreshEnergy();
		showNotification('🔄 Đã reset tất cả thời gian về 0', 'info');
	} catch (err) {
		console.error('Failed to reset energy:', err);
		showNotification('❌ Reset failed', 'error');
	}
}

function toggleEcoMode() {
	ecoMode = document.getElementById('ecoMode').checked;
	pollMs = ecoMode ? 1000 : 500;
	if (lowPower) pollMs = Math.max(pollMs, 2000);
	document.documentElement.classList.toggle('eco', ecoMode);
	startPolling();
}

function toggleAutoEnergy() {
	autoEnergy = document.getElementById('autoEnergy').checked;
	if (autoEnergy) {
		lastEnergyMs = 0;
		refreshEnergy();
	}
}

function updateVisibility() {
	lowPower = document.hidden;
	document.getElementById('lowPowerBadge').style.display = lowPower ? 'inline' : 'none';
	pollMs = lowPower ? 2000 : (ecoMode ? 1000 : 500);
	startPolling();
}

let pollingInterval;
function startPolling() {
	if (pollingInterval) clearInterval(pollingInterval);
	pollingInterval = setInterval(async () => {
		await refreshStats();
		await refreshLeds();
		if (autoEnergy) {
			const now = Date.now();
			if (now - lastEnergyMs > 1500) {
				await refreshEnergy();
				lastEnergyMs = now;
			}
		}
	}, pollMs);
}

// Keyboard shortcuts
document.addEventListener('keydown', (e) => {
	switch (e.key.toUpperCase()) {
		case 'B': lightLoad(); break;
		case 'L': generateLoad(); break;
		case ' ': e.preventDefault(); document.getElementById('ecoMode').click(); break;
	}
});

// Initialize
console.log('Initializing dashboard...');
createLedGrid();
refreshStats();
refreshEnergy();
refreshLeds();
startPolling();

// Visibility change detection
document.addEventListener('visibilitychange', updateVisibility);
updateVisibility();

// Notification system
function showNotification(message, type = 'info') {
	const container = document.getElementById('notifications');
	const notification = document.createElement('div');
	notification.className = `notification ${type}`;
	notification.textContent = message;
	container.appendChild(notification);
	setTimeout(() => { if (notification.parentNode) notification.parentNode.removeChild(notification); }, 3000);
}

function flashCpu(){
	const el = document.getElementById('cpu');
	if(!el) return;
	el.classList.remove('cpu-flash');
	void el.offsetWidth; // force reflow to restart animation
	el.classList.add('cpu-flash');
}

console.log('Functions available:', { lightLoad: typeof lightLoad, generateLoad: typeof generateLoad, updateBarPattern: typeof updateBarPattern, computeEnergy: typeof computeEnergy, resetEnergy: typeof resetEnergy });
</script>
</body>
</html>
""";
}


