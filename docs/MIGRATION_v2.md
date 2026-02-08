# Migration Guide: v1.0 to v2.0

## Overview

Version 2.0 introduces Docker containerization support and improved deployment options. The core library API remains backward compatible, but the application now runs as a containerized service with enhanced health checking and monitoring capabilities.

## Key Changes in v2.0

### Docker Support
- Added multi-stage `Dockerfile` with optimized build and runtime stages
- Build stage uses .NET 10 SDK for compilation
- Runtime stage uses lightweight .NET 10 runtime base image
- Non-root user execution (UID 1000) for security

### Docker Compose
- Added `docker-compose.yml` for simplified local and production deployments
- Configurable port binding (default: 8080)
- Environment variable support for ASP.NET Core configuration
- Built-in restart policy (`unless-stopped`)
- Health check configuration with automatic container restart on failure

### Health Check Endpoint
- Container health check via `/health` endpoint
- Configurable check intervals (30s), timeout (5s), and retry count (3)
- Graceful startup period (10s) to allow application initialization

### Environment Variables
- `ASPNETCORE_URLS`: Configure listening port (default: http://+:8080)
- `ASPNETCORE_ENVIRONMENT`: Set runtime environment (default: Production)
- Support for custom appsettings.json via volume mounting

## Migration Steps

### From Library to Docker

If you were using the NuGet package in another project:

1. No changes required to existing code - the library is fully backward compatible
2. To run as a standalone service, use the Docker image:

```bash
docker-compose up -d
```

### Configuration Migration

1. Review your existing `appsettings.json` for channel configurations
2. Mount your configuration into the container:

```yaml
volumes:
  - ./appsettings.json:/app/appsettings.json:ro
```

3. Set environment variables as needed:

```bash
docker-compose -e ASPNETCORE_ENVIRONMENT=Staging up -d
```

### Port Configuration

- v1.0: Application could run on any configured port
- v2.0: Containerized service listens on 8080 by default
- To change the exposed port, modify `docker-compose.yml`:

```yaml
ports:
  - "9000:8080"  # External:Internal mapping
```

And update `ASPNETCORE_URLS`:

```yaml
environment:
  - ASPNETCORE_URLS=http://+:8080
```

## Breaking Changes

None. The library maintains full backward compatibility.

## New Features

### Container Networking
- Services can communicate via service name (dotnet-deploy-notify)
- Network isolation with dedicated bridge network
- Multi-container orchestration support

### Health Monitoring
- Automated health checks with configurable intervals
- Container auto-restart on health check failure
- Integration with orchestration platforms (Docker Swarm, Kubernetes)

### Volume Support
- Read-only configuration volume for runtime settings
- Support for custom appsettings.json per environment
- Persistent logging if configured

## Deployment Recommendations

### Local Development

```bash
docker-compose -f docker-compose.yml up
```

### Production Deployment

1. Review security configurations
2. Use environment-specific appsettings files:

```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

3. Configure logging and monitoring
4. Set resource limits:

```yaml
services:
  dotnet-deploy-notify:
    deploy:
      resources:
        limits:
          cpus: '1.0'
          memory: 512M
```

### Kubernetes Integration

Use the Docker image with your Kubernetes manifests:

```yaml
image: dotnet-deploy-notify:2.0.0
ports:
  - containerPort: 8080
livenessProbe:
  httpGet:
    path: /health
    port: 8080
  initialDelaySeconds: 10
  periodSeconds: 30
```

## Rollback to v1.0

To revert to v1.0:

```bash
git checkout v1.0.0
```

The v1.0 NuGet package continues to be available on NuGet.org.

## Support

For issues or questions related to Docker deployment, please refer to:
- Docker documentation: https://docs.docker.com/
- Docker Compose reference: https://docs.docker.com/compose/compose-file/
- .NET containerization guide: https://learn.microsoft.com/en-us/dotnet/
