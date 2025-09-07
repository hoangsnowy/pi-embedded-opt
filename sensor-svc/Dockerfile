# syntax=docker/dockerfile:1.7-labs

FROM mcr.microsoft.com/dotnet/sdk:8.0.401-bookworm-slim AS build
WORKDIR /src
COPY sensor.csproj ./sensor.csproj
# Mitigate restore crashes under emulation
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    NUGET_XMLDOC_MODE=skip
RUN dotnet restore --disable-parallel ./sensor.csproj
COPY . .
RUN dotnet publish ./sensor.csproj -c Release -o /out -r linux-arm64 --self-contained true /p:PublishTrimmed=true /p:PublishSingleFile=true

FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
# Create non-root user (idempotent for Alpine)
RUN addgroup -S -g 10001 app 2>/dev/null || true && \
    adduser -S -D -H -u 10001 -G app app 2>/dev/null || true
USER app
COPY --from=build /out/ ./
EXPOSE 8080
ENTRYPOINT ["./sensor-svc"]



