# Quick Start Guide

*Hướng dẫn khởi động nhanh*

## Objectives
- Get the service running in under 2 minutes
- Understand baseline vs tuned modes
- Run basic performance tests
- Collect and visualize metrics

## Prerequisites
- Docker and Docker Compose installed
- `hey` tool for load testing (optional)
- Python 3 for plotting (optional)

## Quick Start

### 1. Build and Run (Baseline)
```bash
cd sensor-svc
dotnet publish -c Release -r linux-arm64 --self-contained true \
  -p:PublishTrimmed=true -p:PublishSingleFile=true -o out-linux-arm64
cd ..
docker compose up -d
```

### 2. Test the Service
```bash
# Check health
curl http://localhost:8080/health

# View dashboard
xdg-open http://localhost:8080/ui

# Load test (optional)
hey -z 30s -c 20 http://localhost:8080/health
```

### 3. Run Tuned Mode
```bash
docker compose -f docker-compose.yml -f docker-compose.tuned.yml up -d --build
```

### 4. Collect Metrics
```bash
# Collect stats
bash scripts/collect_stats.sh http://localhost:8080/stats stats_baseline.csv

# Generate plots
python3 scripts/plot_from_csv.py stats_baseline.csv
```

## Next Steps
- [FSM Documentation](FSM.md) - Power state machine details
- [UI Documentation](UI.md) - Dashboard features and usage
- [Experiments Guide](EXPERIMENTS.md) - Running performance comparisons
- [Report Template](REPORT_TEMPLATE.md) - Documenting results
