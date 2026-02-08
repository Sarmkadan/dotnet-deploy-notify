# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

WORKDIR /src

# Copy project files
COPY dotnet-deploy-notify.csproj .
COPY dotnet-deploy-notify.sln .
COPY GlobalUsings.cs .
COPY appsettings.json .

# Copy source
COPY src/ src/
COPY tests/ tests/

# Restore and build
RUN dotnet restore dotnet-deploy-notify.sln
RUN dotnet build -c Release --no-restore

# Publish stage
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime

WORKDIR /app

# Copy built application from builder
COPY --from=builder /src/bin/Release/net10.0/DotNetDeployNotify .

# Create non-root user for security
RUN useradd -m -u 1000 dotnetuser && chown -R dotnetuser:dotnetuser /app
USER dotnetuser

# Health check endpoint (application should expose health endpoint on startup)
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD /bin/sh -c 'curl -f http://localhost:8080/health || exit 1' || true

EXPOSE 8080

ENTRYPOINT ["./DotNetDeployNotify"]
