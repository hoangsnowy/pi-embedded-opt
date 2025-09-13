# Experiment Report Template

*Mẫu báo cáo thí nghiệm*

## Objectives
- Standardize experiment documentation
- Ensure comprehensive result reporting
- Facilitate comparison between experiments
- Maintain experiment history and learnings

## Report Structure

### 1. Executive Summary
- **Objective**: What was being tested
- **Key Findings**: Main results and insights
- **Recommendations**: Next steps or conclusions

### 2. Experiment Setup
- **Date/Time**: When experiment was conducted
- **Environment**: Hardware, OS, Docker version
- **Configuration**: Baseline vs Tuned settings
- **Load Pattern**: Type and intensity of testing

### 3. Methodology
- **Tools Used**: hey, scripts, monitoring tools
- **Duration**: Test run time and sampling intervals
- **Data Collection**: Metrics captured and frequency
- **Success Criteria**: What constitutes a successful test

### 4. Results
- **Performance Metrics**: Latency, throughput, CPU, memory
- **Power Efficiency**: FSM behavior, resource utilization
- **Charts/Graphs**: Visual representation of data
- **Statistical Analysis**: Trends, patterns, anomalies

### 5. Analysis
- **Comparison**: Baseline vs Tuned performance
- **Trade-offs**: Benefits and costs of optimizations
- **Bottlenecks**: Identified limitations or issues
- **Unexpected Results**: Surprising findings or behaviors

### 6. Conclusions
- **Key Learnings**: What was discovered
- **Recommendations**: Suggested improvements
- **Future Work**: Additional experiments needed
- **Production Readiness**: Deployment considerations

## Data Attachments
- [ ] Raw CSV data files
- [ ] Generated PNG charts
- [ ] Configuration files used
- [ ] Log files and error reports
- [ ] Analysis scripts and code

## Example Metrics Table

| Metric | Baseline | Tuned | Improvement |
|--------|----------|-------|-------------|
| P50 Latency (ms) | 2.1 | 1.8 | 14% |
| P95 Latency (ms) | 8.5 | 7.2 | 15% |
| CPU % (avg) | 45.2 | 38.7 | 14% |
| Memory (MiB) | 128.5 | 95.3 | 26% |
| FSM State Changes | N/A | 12 | N/A |

## Next Steps
- [Experiments Guide](EXPERIMENTS.md) - Plan your next experiment
- [UI Documentation](UI.md) - Use dashboard for monitoring
- [FSM Documentation](FSM.md) - Understand power management features
