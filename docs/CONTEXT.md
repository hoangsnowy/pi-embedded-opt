# Repo Context (lite)
- Updated: 2025-01-27
- HEAD: Simplified to single app with rich HTML dashboard

## Endpoints
- GET /health → ok
- GET /stats → {uptime_s, rss_mib, cpu_pct, p50_ms, p95_ms, state}
- GET /energy?Pact&Pidle&Psleep → {sAct, sIdle, sSlp, mWh}
- POST /energy/reset → Reset time counters
- GET /host → {cpuProcPct, rssMB, gcMB, threads, rps}
- GET /leds → {pins, states} (10 LED states)
- POST /leds/pattern → Set LED pattern from binary string
- POST /leds/pattern/bar/{n} → Set first n LEDs on
- POST /button/press → Simulate button press
- POST /load → Generate CPU load
- GET /ui → Rich HTML dashboard with LED grid, energy panel, eco mode

## ENV (FSM + LED)
POWER_FSM, SAMPLE_ACTIVE_HZ, SAMPLE_IDLE_HZ, ACTIVE_WINDOW_S, SLEEP_AFTER_S, LED_COUNT

## UI quick notes
- Single HTML page: 10 LEDs, energy panel, eco mode, visibility throttling
- Keyboard shortcuts: B→button, L→load, Space→toggle eco mode
- Auto-refresh: 500ms normal, 1000ms eco, 2000ms hidden tab
- Served directly from backend at /ui endpoint

## Architecture
- **Single App**: .NET 8 minimal API with embedded HTML dashboard
- **Port**: 8080 (backend + dashboard)
- **Compose files**: `docker-compose.yml` (baseline), `docker-compose.tuned.yml` (FSM enabled)

## TODO next
- [x] Simplify to single application
- [x] Remove complex Blazor WASM setup
- [ ] Test all dashboard features
- [ ] Verify energy calculations and LED controls
