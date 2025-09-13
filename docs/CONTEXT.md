# Repo Context (lite)
- Updated: 2025-09-13
- HEAD: Single .NET 8 minimal API + embedded HTML dashboard (energy + LED + FSM demo)

## Endpoints
- GET /health → ok
- GET /stats → {uptimeSeconds, rssMiB, cpuPercent, p50Ms, p95Ms, state}
- GET /energy?Pact&Pidle&Psleep → {sAct, sIdle, sSlp, mWh, ledActSeconds, ledIdleSeconds, ledSlpSeconds, ledMWh, totalMWh}
- POST /energy/reset → Reset time + LED energy counters
- GET /host → {cpuProcPct, rssMB, gcMB, threads, rps}
- GET /leds → {pins, states} (LED_COUNT states)
- POST /leds/pattern → Set LED pattern from binary string
- POST /leds/pattern/bar/{n} → Set first n LEDs on
- POST /button/press → Light CPU load (~2s)
- POST /load → Heavy CPU load (~5s)
- POST /gc → Force full GC (demo)
- GET /ui → Rich HTML dashboard with LED grid, energy panel, eco mode

## ENV (FSM + LED)
`POWER_FSM`, `SAMPLE_ACTIVE_HZ`, `SAMPLE_IDLE_HZ`, `ACTIVE_WINDOW_S`, `SLEEP_AFTER_S`, `LED_COUNT`, `LED_MW`, `LED_POOL` (1 enables 10MB LED buffer pooling), `GC_COMPACT` (0 disables compacting in /gc)

Notes:
- Each LED ON allocates (or reuses via pool) a 10MB byte[] (demo exaggeration). Pooling reduces LOH churn & fragmentation.
- `POWER_FSM` currently always behaves as enabled (toggle wiring planned: baseline path would treat all time as Active).

## UI quick notes
- Single HTML page: 10 LEDs (10MB each), energy panel (System / LED / Total), eco mode (dim + slower polling), visibility throttling
- Keyboard shortcuts: B→Light Load, L→Heavy Load, Space→toggle eco mode
- Auto-refresh stats/LED: 500ms; eco=1000ms; hidden tab=2000ms; energy refresh ~1.5s when Auto Energy ON
- CPU flash animation when triggering Light/Heavy load

## Architecture
- **Single App**: .NET 8 minimal API with embedded HTML dashboard
- **Port**: 8080 (backend + dashboard)
- **Compose files**: `docker-compose.yml` (baseline), `docker-compose.tuned.yml` (FSM enabled)

### Activity Gating (Tuned)
`lastRequestUnixMs` only refreshed on meaningful activity: Light/Heavy load endpoints, any LED still ON, or CPU% > threshold (2%). Passive polling (`/stats`, `/leds`, `/energy`) excluded so FSM can progress to Idle/Sleep while UI open. A background quiescence loop ages the timestamp (subtracts 5s) after ~2s with no LEDs + no work to accelerate Idle/Sleep transitions.

### GC & Memory Demo
`/gc` forces a full GC (optionally compact: `GC_COMPACT=0` to disable). `/diag/mem` surfaces heap size, fragmentation, pauses. LED_POOL=1 shows flatter RSS curve by reusing 10MB buffers instead of freeing (reduces LOH fragmentation).

### Energy Model
`/energy`: System mWh = (Pactive*tAct + Pidle*tIdle + Psleep*tSlp)/3600. LED mWh = (LED_MW * Σ(LED-on seconds scaled by on-count))/3600. Total = System + LED. LED time counters maintained per FSM state (Active / Idle / Sleep) to illustrate peripheral impact when base system is in low-power states.


