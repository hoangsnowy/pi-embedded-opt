#!/usr/bin/env bash
set -euo pipefail

URL="${1:-http://localhost:8080/stats}"
OUT="${2:-stats.csv}"

if ! command -v jq >/dev/null 2>&1; then
  echo "jq is required." >&2
  exit 1
fi

echo "ts,rss_mib,cpu_pct,p50_ms,p95_ms,state" > "$OUT"

while true; do
  TS=$(date +%s)
  RESP=$(curl -fsS "$URL" || true)
  if [[ -n "${RESP}" ]]; then
    RSS=$(echo "$RESP" | jq -r '.rss_mib // 0')
    CPU=$(echo "$RESP" | jq -r '.cpu_pct // 0')
    P50=$(echo "$RESP" | jq -r '.p50_ms // 0')
    P95=$(echo "$RESP" | jq -r '.p95_ms // 0')
    STATE=$(echo "$RESP" | jq -r '.state // "unknown"')
    echo "${TS},${RSS},${CPU},${P50},${P95},${STATE}" >> "$OUT"
  fi
  sleep 1
done



