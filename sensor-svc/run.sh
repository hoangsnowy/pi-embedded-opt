#!/bin/bash
cd /home/hoangsnowy/projects/pi-embedded-opt/sensor-svc
echo "Building..."
dotnet build
echo "Build exit code: $?"
echo "Running..."
dotnet run --urls="http://localhost:8080"
