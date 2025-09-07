#!/usr/bin/env python3
import sys
import csv
import matplotlib.pyplot as plt

if len(sys.argv) < 2:
    print("Usage: plot_from_csv.py <stats.csv>")
    sys.exit(1)

csv_path = sys.argv[1]

ts = []
p95 = []
rss = []

with open(csv_path, newline='') as f:
    reader = csv.DictReader(f)
    for row in reader:
        try:
            t = float(row['ts'])
            ts.append(t)
            p95.append(float(row['p95_ms']))
            rss.append(float(row['rss_mib']))
        except Exception:
            continue

if not ts:
    print("No data.")
    sys.exit(1)

# p95 over time
plt.figure()
plt.plot(ts, p95)
plt.xlabel('Time (s)')
plt.ylabel('P95 latency (ms)')
plt.title('P95 over time')
plt.grid(True, linestyle='--', alpha=0.4)
plt.tight_layout()
plt.savefig('p95.png')

# RSS over time
plt.figure()
plt.plot(ts, rss)
plt.xlabel('Time (s)')
plt.ylabel('RSS (MiB)')
plt.title('RSS over time')
plt.grid(True, linestyle='--', alpha=0.4)
plt.tight_layout()
plt.savefig('rss.png')


