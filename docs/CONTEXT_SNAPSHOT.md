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
- `GET /stats` → JSON metrics (uptime, memory, CPU, latency, state)
- `GET /ui` → HTML dashboard (auto-refreshing)
- `GET /` → Redirect to `/ui`

### Environment Variables
- `POWER_FSM`: Enable/disable power state machine (0/1)
- `SAMPLE_ACTIVE_HZ`: Active state sampling rate (default: 100)
- `SAMPLE_IDLE_HZ`: Idle state sampling rate (default: 1)
- `ACTIVE_WINDOW_S`: Seconds before Idle transition (default: 10)
- `SLEEP_AFTER_S`: Additional seconds before Sleep (default: 60)

### Container Configurations
- **Baseline**: `docker-compose.yml` (FSM off, standard limits)
- **Tuned**: `docker-compose.tuned.yml` (FSM on, optimized constraints)

### Tooling
- **scripts/collect_stats.sh**: CSV data collection
- **scripts/plot_from_csv.py**: PNG chart generation
- **diagrams/**: PlantUML architecture and FSM diagrams

## Current State
- ✅ Core service implemented and functional
- ✅ Power FSM with configurable parameters
- ✅ Real-time metrics collection and display
- ✅ Docker containerization for ARM64
- ✅ Performance testing and visualization tools
- ✅ Comprehensive documentation structure

## Next Steps
- [Quick Start](QUICKSTART.md) - Get started with the service
- [FSM Documentation](FSM.md) - Understand power management
- [UI Documentation](UI.md) - Use the dashboard effectively
- [Experiments Guide](EXPERIMENTS.md) - Run performance tests
