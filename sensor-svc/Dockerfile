############################################################
# Minimal Two-Stage NativeAOT Dockerfile (Option B Simplified)
# Build native ARM64 binary directly inside container
# Usage:
#   docker buildx build --platform linux/arm64 -t sensor-svc:aot .
# (For trimmed non-AOT single-file: set build arg AOT=0)
############################################################

ARG AOT=1
ARG RUNTIME=linux-arm64
ARG CONFIG=Release

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./sensor.csproj ./
RUN dotnet restore sensor.csproj
COPY . .

RUN if [ "$AOT" = "1" ]; then \
            echo "Publishing NativeAOT" && \
            dotnet publish sensor.csproj -c $CONFIG -r $RUNTIME -o /app/publish \
                -p:PublishAot=true -p:StripSymbols=true -p:InvariantGlobalization=true; \
        else \
            echo "Publishing trimmed single-file" && \
            dotnet publish sensor.csproj -c $CONFIG -r $RUNTIME -o /app/publish \
                -p:PublishTrimmed=true -p:PublishSingleFile=true -p:InvariantGlobalization=true; \
        fi

FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-bookworm-slim
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080 DOTNET_EnableDiagnostics=0 TZ=UTC
RUN groupadd -g 10001 app 2>/dev/null || true && \
        useradd -r -u 10001 -g app app 2>/dev/null || true
USER app
COPY --from=build /app/publish/ ./
RUN chmod +x ./sensor || true
EXPOSE 8080
ENTRYPOINT ["./sensor"]