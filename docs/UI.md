cd # UI Dashboard Documentation

*Tài liệu về giao diện dashboard*

## Objectives
- Understand dashboard features and layout
- Monitor real-time system metrics
- Track power FSM state changes
- Interpret performance data

## Dashboard Features

### Real-time Metrics Table
- **Uptime**: Service running time in seconds
- **RSS Memory**: Resident Set Size in MiB
- **CPU %**: Process CPU utilization percentage
- **P50 Latency**: 50th percentile response time (ms)
- **P95 Latency**: 95th percentile response time (ms)
- **State**: Current FSM state (Active/Idle/Sleep)

### Auto-refresh
- **Interval**: ~500ms with exponential backoff
- **Visual**: Smooth updates without page reload
- **Performance**: Lightweight polling to minimize overhead

### Responsive Design
- **Mobile-friendly**: Works on small screens
- **Minimal dependencies**: Pure HTML/CSS/JS
- **Fast loading**: No external libraries

## Usage

### Accessing the Dashboard
```bash
# Direct access
curl http://localhost:8080/ui

# Browser access
xdg-open http://localhost:8080/ui
```

### Interpreting Data
- **High CPU %**: System under load or inefficient processing
- **Increasing P95**: Performance degradation, possible bottlenecks
- **State changes**: FSM responding to request patterns
- **Memory growth**: Potential memory leaks or high usage

### Troubleshooting
- **No updates**: Check if service is running
- **Stale data**: Verify `/stats` endpoint is responding
- **High latency**: Check system resources and FSM state

## Customization
- Modify `UiPage.cs` for layout changes
- Adjust refresh interval in client-side JavaScript
- Add new metrics by extending the stats endpoint

## Next Steps
- [FSM Documentation](FSM.md) - Understand state machine behavior
- [Experiments Guide](EXPERIMENTS.md) - Use UI for performance testing
- [Quick Start](QUICKSTART.md) - Get dashboard running quickly
