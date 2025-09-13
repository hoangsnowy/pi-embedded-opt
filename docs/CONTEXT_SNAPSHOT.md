# Context Snapshot

*Ảnh chụp ngữ cảnh dự án*

## Objectives
- Capture current project state and architecture
- Document key components and their relationships
- Provide quick reference for new contributors
- Track evolution of system design

## Project Overview
Minimal .NET 8 service for ARM64 containers (Raspberry Pi class) with runtime power FSM, realtime stats, web UI, and tooling to benchmark baseline vs tuned modes.

## Architecture Components

### Core Service (`sensor-svc/`)
- **Program.cs**: Main application entry point with minimal API
- **Power/PowerFsm.cs**: State machine for power management
- **Infrastructure/**: Metrics collection and JSON serialization
- **Ui/UiPage.cs**: HTML dashboard generation
- **Util/AtomicLong.cs**: Thread-safe atomic operations

### Endpoints
- `GET /health` → `ok` (health check)
- `GET /stats` → Dynamic metrics: `uptime_s, rss_mib, cpu_pct, p50_ms, p95_ms, state` (latency buffer ≤512, CPU sampler ~800ms) – passive polling no longer marks activity
- `GET /energy?Pact&Pidle&Psleep` → Time-in-state counters (sAct/sIdle/sSlp) + computed `mWh` from power inputs
	- Extended fields: `ledAct_s, ledIdle_s, ledSlp_s, led_mWh, total_mWh` (per-LED power via LED_MW, default 50 mW)
- `POST /energy/reset` → Reset FSM time counters
- `GET /leds` / `POST /leds/pattern` / `POST /leds/pattern/bar/{n}` → LED demo state (1MB per ON LED allocated, freed when OFF)
- `POST /button/press` → Simulated 2s CPU burst (updates latency + CPU metrics)
- `POST /load` → Simulated heavier 5s CPU burst
- `GET /ui` → HTML dashboard (polls /stats, /leds, /energy)
- `GET /` → Redirect to `/ui`

### Environment Variables
- `POWER_FSM`: Enable/disable power state machine (0/1)
- `SAMPLE_ACTIVE_HZ`: Active state sampling rate (default: 100)
- `SAMPLE_IDLE_HZ`: Idle state sampling rate (default: 1)
- `ACTIVE_WINDOW_S`: Seconds before Idle transition (default: 10)
- `SLEEP_AFTER_S`: Additional seconds before Sleep (default: 60)
- `LED_COUNT`: Number of virtual LEDs (default 10)
 - `LED_MW`: Power (mW) per single LED ON (default 50) used in energy calculation

### Container Configurations
- **Baseline**: `docker-compose.yml` (FSM off, standard limits)
- **Tuned**: `docker-compose.tuned.yml` (FSM on, optimized constraints)

### Tooling
- **scripts/collect_stats.sh**: CSV data collection
- **scripts/plot_from_csv.py**: PNG chart generation
- **diagrams/**: PlantUML architecture and FSM diagrams

## Current State
- ✅ Core service running with dynamic CPU & latency metrics (sampler + middleware)
- ✅ Power FSM active: transitions Active→Idle→Sleep based on last request; time-in-state counters feed /energy
- ✅ Energy endpoint now computes mWh from real accumulated time * provided mW values
- ✅ Docker containerization for ARM64
- ✅ Scripts + plotting for experiments
- ✅ Documentation kept in sync (this snapshot)

## Next Steps
- [Quick Start](QUICKSTART.md) - Get started with the service
- [FSM Documentation](FSM.md) - Understand power management
- [UI Documentation](UI.md) - Use the dashboard effectively
- [Experiments Guide](EXPERIMENTS.md) - Run performance tests
