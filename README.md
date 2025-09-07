# pi-embedded-opt

Minimal .NET 8 service for ARM64 containers (Raspberry Pi class) with runtime power FSM, realtime stats, web UI, and tooling to benchmark baseline vs tuned modes.

## Features
- Minimal API: `/health`, `/stats` (JSON), `/ui` (auto-refreshing table)
- Request latency middleware with sliding buffer (≤512) and p50/p95
- CPU% sampler via process `TotalProcessorTime` (~800ms window)
- Application-level Power FSM (Active → Idle → Sleep), env-driven
- Same image runs Baseline (FSM off) and Tuned (FSM on + cgroups + read-only + tmpfs)
- Scripts to collect stats to CSV and plot PNG charts
- PlantUML diagrams for FSM and runtime architecture

## Build
```bash
# Build multi-arch (ARM64) image
docker buildx build --platform linux/arm64 -t demo/sensor-svc:arm64 ./sensor-svc
```

## Run (Baseline)
```bash
# Runs with POWER_FSM=0
docker compose up -d
# Open UI and quick benchmark
xdg-open http://localhost:8080/ui || true
hey -z 30s -c 20 http://localhost:8080/health
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

## Notes
- Container runs as non-root user
- Runtime image: `mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine`
- Publish is trimmed, self-contained for `linux-arm64`

## Docs
- `docs/rules.md` – coding/deploy rules
- `docs/plan.md` – plan for build/benchmark/compare
- `docs/current.md` – current status and experiment notes
