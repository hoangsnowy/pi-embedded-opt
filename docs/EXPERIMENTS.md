# Experiments Guide

*Hướng dẫn thực hiện thí nghiệm*

## Objectives
- Design and run performance experiments
- Compare baseline vs tuned configurations
- Collect meaningful metrics and data
- Analyze results and draw conclusions

## Experiment Types

### Baseline vs Tuned Comparison
- **Baseline**: FSM disabled, standard container limits
- **Tuned**: FSM enabled, optimized container constraints
- **Metrics**: CPU, memory, latency, power consumption

### Load Testing
- **Tools**: `hey`, `wrk`, or custom scripts
- **Patterns**: Constant load, burst traffic, gradual ramp-up
- **Duration**: 30s minimum, 5+ minutes for stable results

### FSM Behavior Analysis
- **State transitions**: Monitor Active → Idle → Sleep cycles
- **Response times**: Impact of state changes on latency
- **Resource usage**: CPU/memory patterns per state

## Running Experiments

### 1. Prepare Environment
```bash
# Build both configurations
docker compose build
docker compose -f docker-compose.tuned.yml build
```

### 2. Baseline Test
```bash
# Start baseline
docker compose up -d

# Collect data
bash scripts/collect_stats.sh http://localhost:8080/stats baseline.csv

# Load test
hey -z 60s -c 10 http://localhost:8080/health

# Generate plots
python3 scripts/plot_from_csv.py baseline.csv
```

### 3. Tuned Test
```bash
# Start tuned
docker compose -f docker-compose.yml -f docker-compose.tuned.yml up -d

# Repeat data collection
bash scripts/collect_stats.sh http://localhost:8080/stats tuned.csv
python3 scripts/plot_from_csv.py tuned.csv
```

### 4. Analysis
- Compare CSV files for trends
- Analyze PNG charts for visual patterns
- Document findings in report template

## Metrics to Track

### Performance
- **Latency**: P50, P95, P99 percentiles
- **Throughput**: Requests per second
- **CPU utilization**: Average and peak usage
- **Memory usage**: RSS, working set, peak consumption

### Power Efficiency
- **FSM state distribution**: Time spent in each state
- **State transition frequency**: How often FSM switches
- **Resource efficiency**: Performance per watt

## Best Practices
- Run multiple iterations for statistical significance
- Use consistent load patterns across tests
- Document all configuration changes
- Save raw data and analysis scripts
- Use version control for experiment code

## Next Steps
- [Report Template](REPORT_TEMPLATE.md) - Document your findings
- [UI Documentation](UI.md) - Monitor experiments in real-time
- [FSM Documentation](FSM.md) - Understand power management
