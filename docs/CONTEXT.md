# Repo Context (lite)
- Updated: 2025-01-27
- HEAD: Simplified to single app with rich HTML dashboard

## Endpoints
- GET /health → ok
- GET /stats → {uptimeSeconds, rssMiB, cpuPercent, p50Ms, p95Ms, state}
- GET /energy?Pact&Pidle&Psleep → {sAct, sIdle, sSlp, mWh, ledActSeconds, ledIdleSeconds, ledSlpSeconds, ledMWh, totalMWh}
- POST /energy/reset → Reset time + LED energy counters
- GET /host → {cpuProcPct, rssMB, gcMB, threads, rps}
- GET /leds → {pins, states} (LED_COUNT states)
- POST /leds/pattern → Set LED pattern from binary string
- POST /leds/pattern/bar/{n} → Set first n LEDs on
- POST /button/press → Simulate button press
- POST /load → Generate CPU load
- POST /gc → Force full GC (demo)
- GET /ui → Rich HTML dashboard with LED grid, energy panel, eco mode

## ENV (FSM + LED)
POWER_FSM, SAMPLE_ACTIVE_HZ, SAMPLE_IDLE_HZ, ACTIVE_WINDOW_S, SLEEP_AFTER_S, LED_COUNT, LED_MW

## UI quick notes
- Single HTML page: 10 LEDs, energy panel, eco mode, visibility throttling
- Keyboard shortcuts: B→button, L→load, Space→toggle eco mode
- Auto-refresh: 500ms normal, 1000ms eco, 2000ms hidden tab
- Served directly from backend at /ui endpoint

## Architecture
- **Single App**: .NET 8 minimal API with embedded HTML dashboard
- **Port**: 8080 (backend + dashboard)
- **Compose files**: `docker-compose.yml` (baseline), `docker-compose.tuned.yml` (FSM enabled)

### Activity Gating (Tuned)
Last activity only advances when: work endpoints (`/button/press`, `/load`) are hit, CPU% > 2%, or at least one LED is ON. Passive polling (`/stats`, `/leds`, `/energy`) is ignored so the FSM can drop to Idle/Sleep while dashboard remains open. A background loop ages the last-activity timestamp after ~2s of quiescence with no LEDs, accelerating Idle.

### GC Demo
`/gc` triggers a forced compacting collection; UI button shows before/after managed heap MB and helps illustrate memory from LED buffers being reclaimed after turning LEDs off.

## TODO next
- [x] Simplify to single application
- [x] Remove complex Blazor WASM setup
- [ ] Test all dashboard features
- [x] Verify energy calculations and LED controls (extended /energy fields documented)
