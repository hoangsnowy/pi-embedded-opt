# Plan

1. Build Docker image for `linux/arm64` with trimmed, self-contained publish.
2. Run Baseline (`POWER_FSM=0`) via `docker compose` and verify `/health`, `/ui`.
3. Benchmark baseline with `hey`/`wrk` for at least 30–60s.
4. Collect stats from `/stats` every 1s → CSV.
5. Plot `p95.png` and `rss.png` from CSV; archive artifacts.
6. Run Tuned overlay compose with resource limits and security hardening.
7. Repeat benchmark, collect stats, and plot figures for tuned run.
8. Compare p95 latency and RSS over time; document findings in `docs/current.md`.
