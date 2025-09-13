# pi-embedded-opt

Minimal .NET 8 service for ARM64 containers (Raspberry Pi class) with runtime power FSM, realtime stats, web UI, and tooling to benchmark baseline vs tuned modes.

## Features
- Minimal API: `/health`, `/stats`, `/energy`, `/host`, `/leds`, `/button/press`, `/load`
- Request latency middleware with sliding buffer (≤512) and p50/p95
- CPU% sampler via process `TotalProcessorTime` (~800ms window)
- Application-level Power FSM (Active → Idle → Sleep), env-driven
- Energy panel: enter P_active/P_idle/P_sleep (mW) → shows mWh; Reset clears counters
- LED grid: 10 virtual LEDs with pattern controls and bar slider
- Rich HTML dashboard with eco mode and visibility throttling
- Same image runs Baseline (FSM off) and Tuned (FSM on + cgroups + read-only + tmpfs)
- Scripts to collect stats to CSV and plot PNG charts
- PlantUML diagrams for FSM and runtime architecture

## Build
Publish on host first (Dockerfile copies from `sensor-svc/out-linux-arm64`):
```bash
cd sensor-svc
dotnet restore
# Debian/glibc runtime
dotnet publish -c Release -r linux-arm64 --self-contained true \
  -p:PublishTrimmed=true -p:PublishSingleFile=true -o out-linux-arm64
cd ..
```
Build image (runtime-only):
```bash
docker buildx build --platform linux/arm64 -t demo/sensor-svc:arm64 ./sensor-svc
```

## Run (Baseline)
```bash
# Runs with POWER_FSM=0
docker compose up -d
# Open dashboard and quick benchmark
xdg-open http://localhost:8080/ui || true
hey -z 30s -c 20 http://localhost:8080/health
```

## Run (Tuned)
```bash
# Overlay tuned limits and FSM=1
docker compose -f docker-compose.yml -f docker-compose.tuned.yml up -d --build
# Repeat collection/plots with a new file, e.g. stats_tuned.csv
bash scripts/collect_stats.sh http://localhost:8080/stats stats_tuned.csv
python3 scripts/plot_from_csv.py stats_tuned.csv
```

## Development Mode
```bash
# Run backend directly
cd sensor-svc && dotnet run
# Dashboard: http://localhost:8080/ui
```

## Collect Stats & Plot
```bash
# Collect 1s samples from /stats
bash scripts/collect_stats.sh http://localhost:8080/stats stats_baseline.csv
# Plot p95.png and rss.png
python3 scripts/plot_from_csv.py stats_baseline.csv
```

## Run (Tuned)
```bash
# Overlay tuned limits and FSM=1
docker compose -f docker-compose.yml -f docker-compose.tuned.yml up -d --build
# Repeat collection/plots with a new file, e.g. stats_tuned.csv
bash scripts/collect_stats.sh http://localhost:8080/stats stats_tuned.csv
python3 scripts/plot_from_csv.py stats_tuned.csv
```

## Endpoints
- `GET /health` → `ok`
- `GET /stats` → `{uptime_s, rss_mib, cpu_pct, p50_ms, p95_ms, state}`
- `GET /energy?Pact&Pidle&Psleep` → `{sAct, sIdle, sSlp, mWh}`
- `POST /energy/reset` → Reset time counters
- `GET /host` → `{cpuProcPct, rssMB, gcMB, threads, rps}`
- `GET /leds` → `{pins, states}` (10 LED states)
- `POST /leds/pattern` → Set LED pattern from binary string
- `POST /leds/pattern/bar/{n}` → Set first n LEDs on
- `POST /button/press` → Simulate button press
- `POST /load` → Generate CPU load
- `GET /ui` → HTML dashboard (refresh ~2s)

## Environment
- `POWER_FSM` (0/1)
- `SAMPLE_ACTIVE_HZ` (default 100)
- `SAMPLE_IDLE_HZ` (1)
- `ACTIVE_WINDOW_S` (10)
- `SLEEP_AFTER_S` (60)

## Compose Profiles
- `docker-compose.yml`: Baseline (FSM off)
- `docker-compose.tuned.yml`: Adds mem/cpu/pids limits, read_only, tmpfs, no-new-privileges, FSM on

## Diagrams
- FSM: `diagrams/fsm.puml`
- Architecture: `diagrams/architecture.puml`

## Quick Reference

| Command | Description |
|---------|-------------|
| `docker compose up -d` | Backend with dashboard (port 8080) |
| `docker compose -f docker-compose.tuned.yml up -d` | Backend with FSM enabled |
| `cd sensor-svc && dotnet run` | Development mode |

## Notes
- Container runs as non-root user
- Runtime image: `mcr.microsoft.com/dotnet/runtime-deps:8.0-bookworm-slim`
- Dashboard is served directly from the backend at `/ui` endpoint
- Publish is trimmed, self-contained for `linux-arm64`

## Docs
- [Quick Start](docs/QUICKSTART.md) – Get running in 2 minutes
- [FSM Documentation](docs/FSM.md) – Power state machine details
- [UI Documentation](docs/UI.md) – Dashboard features and usage
- [Experiments Guide](docs/EXPERIMENTS.md) – Performance testing
- [Report Template](docs/REPORT_TEMPLATE.md) – Document results
- [Context Snapshot](docs/CONTEXT_SNAPSHOT.md) – Project overview

## Kick-Off Prompt 
Do the lite Repo Sync now:
- Read the paths listed in .cursorrules.
- Print a 10–12 line repo snapshot (endpoints, ENV, UI features, polling).
- Then propose a 3–5 step surgical plan for my next change (list files to edit).
- Keep endpoints/env names stable; minimal diffs; update docs/CONTEXT.md if behavior changes.
