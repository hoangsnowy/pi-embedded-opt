# Power FSM Documentation

*Tài liệu về máy trạng thái năng lượng*

## Objectives
- Understand the power state machine architecture
- Configure FSM behavior via environment variables
- Monitor state transitions and their impact
- Optimize power consumption patterns

## FSM States

### Active State
- **Trigger**: Recent request activity (within `ACTIVE_WINDOW_S`)
- **Sampling**: High frequency (`SAMPLE_ACTIVE_HZ`)
- **Behavior**: Full performance mode, immediate response

### Idle State  
- **Trigger**: No requests for `ACTIVE_WINDOW_S` seconds
- **Sampling**: Low frequency (`SAMPLE_IDLE_HZ`)
- **Behavior**: Reduced sampling, preparing for sleep

### Sleep State
- **Trigger**: Idle for `SLEEP_AFTER_S` additional seconds
- **Sampling**: Minimal (1Hz)
- **Behavior**: Deep power saving, minimal resource usage

## Configuration

| Variable | Default | Description |
|----------|---------|-------------|
| `POWER_FSM` | `0` | Enable/disable FSM (0=off, 1=on) |
| `SAMPLE_ACTIVE_HZ` | `100` | Active state sampling rate |
| `SAMPLE_IDLE_HZ` | `1` | Idle state sampling rate |
| `ACTIVE_WINDOW_S` | `10` | Seconds before transitioning to Idle |
| `SLEEP_AFTER_S` | `60` | Additional seconds before Sleep |

## State Transitions

```
Active ←→ Idle ←→ Sleep
  ↑         ↑       ↑
  |         |       |
  └─────────┴───────┘
   (request activity)
```

## Monitoring
- Current state available via `/stats` endpoint
- State changes logged in application output
- Impact visible in CPU and memory metrics

## Next Steps
- [UI Documentation](UI.md) - Monitor FSM states in dashboard
- [Experiments Guide](EXPERIMENTS.md) - Compare FSM vs baseline performance
- [Quick Start](QUICKSTART.md) - Get FSM running quickly
