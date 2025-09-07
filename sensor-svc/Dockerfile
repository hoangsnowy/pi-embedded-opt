# Runtime-only image. Build/publish on host first, then build this image.
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-bookworm-slim
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
# Non-root user (idempotent)
RUN groupadd -g 10001 app 2>/dev/null || true && \
    useradd -r -u 10001 -g app app 2>/dev/null || true
USER app
# Copy prebuilt artifacts (publish on host with -r linux-arm64)
COPY ./out-linux-arm64/ ./
EXPOSE 8080
ENTRYPOINT ["./sensor"]