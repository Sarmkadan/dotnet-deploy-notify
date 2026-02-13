# Docker Guide for dotnet-deploy-notify

This guide provides comprehensive instructions for running dotnet-deploy-notify using Docker and Docker Compose.

## Quick Start with Docker

### Prerequisites
- Docker Engine 20.10+
- Docker Compose v2+
- 64MB available RAM

### Run with Default Configuration
```bash
# Pull the latest image
docker pull sarmkadan/dotnet-deploy-notify:2.0

# Run with default settings
docker run -d -p 8080:8080 --name deploy-notify sarmkadan/dotnet-deploy-notify:2.0
```

### Run with Custom Configuration
```bash
# Create configuration directory
mkdir -p ./config

# Create appsettings.json (see Configuration section)
echo '{
  "NotificationService": {
    "MaxRetries": 3,
    "WebhookTimeoutMs": 10000
  },
  "Channels": [
    {
      "Type": "Slack",
      "WebhookUrl": "https://hooks.slack.com/services/YOUR/SLACK/WEBHOOK",
      "Name": "Production Slack"
    }
  ]
}' > ./config/appsettings.json

# Run with mounted configuration
docker run -d \
  -p 8080:8080 \
  -v $(pwd)/config:/app/config:ro \
  --name deploy-notify \
  sarmkadan/dotnet-deploy-notify:2.0
```

## Docker Compose Usage

### Basic docker-compose.yml
```yaml
version: '3.8'

services:
  deploy-notify:
    image: sarmkadan/dotnet-deploy-notify:2.0
    container_name: deploy-notify
    ports:
      - "8080:8080"
    volumes:
      - ./config/appsettings.json:/app/appsettings.json:ro
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 60s
```

### Multi-Instance Setup
```yaml
version: '3.8'

services:
  deploy-notify-primary:
    image: sarmkadan/dotnet-deploy-notify:2.0
    container_name: deploy-notify-primary
    ports:
      - "8080:8080"
    volumes:
      - ./config/primary.json:/app/appsettings.json:ro
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3

  deploy-notify-secondary:
    image: sarmkadan/dotnet-deploy-notify:2.0
    container_name: deploy-notify-secondary
    ports:
      - "8081:8080"
    volumes:
      - ./config/secondary.json:/app/appsettings.json:ro
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
```

### Production-Ready docker-compose.yml
```yaml
version: '3.8'

services:
  deploy-notify:
    image: sarmkadan/dotnet-deploy-notify:2.0
    container_name: deploy-notify
    ports:
      - "8080:8080"
    volumes:
      - ./config/appsettings.json:/app/appsettings.json:ro
      - ./logs:/app/logs
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080
    restart: unless-stopped
    deploy:
      resources:
        limits:
          memory: 512M
          cpus: '1.0'
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 60s
    logging:
      driver: "json-file"
      options:
        max-size: "10m"
        max-file: "3"
```

## Environment Variables Reference

### Core Environment Variables
| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | Production | Runtime environment |
| `ASPNETCORE_URLS` | http://+:8080 | Server binding URLs |
| `DOTNET_ENVIRONMENT` | *(inherits)* | .NET runtime environment |
| `DOTNET_RUNNING_IN_CONTAINER` | true | Container detection flag |

### Application-Specific Variables
| Variable | Default | Description |
|---------|---------|-------------|
| `NOTIFICATION_SERVICE_MAXRETRIES` | 3 | Maximum retry attempts |
| `NOTIFICATION_SERVICE_TIMEOUTMS` | 10000 | Webhook timeout (ms) |
| `NOTIFICATION_SERVICE_PROCESSINGINTERVAL` | 30 | Processing interval (seconds) |
| `HEALTHCHECK_ENDPOINT` | /health | Health check endpoint path |
| `LOGGING_LEVEL` | Information | Minimum log level |

### Example Environment Configuration
```bash
docker run -d \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Staging \
  -e NOTIFICATION_SERVICE_MAXRETRIES=5 \
  -e NOTIFICATION_SERVICE_TIMEOUTMS=15000 \
  --name deploy-notify \
  sarmkadan/dotnet-deploy-notify:2.0
```

## Configuration Files

### appsettings.json Structure
```json
{
  "NotificationService": {
    "MaxRetries": 3,
    "WebhookTimeoutMs": 10000,
    "RetryDelayMs": 5000,
    "AutoProcessNotifications": true,
    "ProcessingIntervalSeconds": 30,
    "EnableAuditLogging": true,
    "RetentionDays": 30
  },
  "Canary": {
    "Enabled": true,
    "TrafficSplit": {
      "BaselinePercentage": 90,
      "CanaryPercentage": 10,
      "AutoRollbackThreshold": 5
    },
    "Rollback": {
      "Enabled": true,
      "FailureThreshold": 3,
      "DurationMinutes": 15
    }
  },
  "Channels": [
    {
      "Type": "Slack",
      "WebhookUrl": "https://hooks.slack.com/services/YOUR/SLACK/WEBHOOK",
      "Name": "Production Slack",
      "Priority": 1,
      "Filter": {
        "Environments": ["production"],
        "Statuses": ["Success", "Failed"],
        "MinimumPriority": "Medium"
      }
    }
  ]
}
```

### Volume Mounting
```bash
# Mount configuration file
-v /host/path/appsettings.json:/app/appsettings.json:ro

# Mount logs directory
-v /host/path/logs:/app/logs

# Mount entire config directory
-v /host/path/config:/app/config:ro
```

## Health Check Endpoints

### Health Status
```bash
# Check if service is running
curl -f http://localhost:8080/health

# Check detailed health status
curl -f http://localhost:8080/health/detailed
```

### Health Response Format
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "Notification Service",
      "status": "Healthy",
      "responseTime": "5ms"
    },
    {
      "name": "Webhook Dispatcher",
      "status": "Healthy",
      "responseTime": "12ms"
    }
  ]
}
```

## Production Deployment Checklist

### ✅ Security
- [ ] Run as non-root user (UID 1000)
- [ ] Use read-only volume mounts where possible
- [ ] Configure network isolation
- [ ] Set resource limits (CPU, memory)
- [ ] Enable logging and monitoring

### ✅ Configuration
- [ ] Validate appsettings.json syntax
- [ ] Set appropriate retry policies
- [ ] Configure channel-specific settings
- [ ] Set up canary deployment policies
- [ ] Configure rollback thresholds

### ✅ Monitoring
- [ ] Enable health checks
- [ ] Configure log rotation
- [ ] Set up metrics collection
- [ ] Monitor resource usage
- [ ] Set up alerting for failures

### ✅ Networking
- [ ] Configure appropriate port mappings
- [ ] Set up reverse proxy (nginx, traefik)
- [ ] Configure SSL termination
- [ ] Set up firewall rules
- [ ] Plan for high availability

### ✅ Backup & Recovery
- [ ] Regular configuration backups
- [ ] Test rollback procedures
- [ ] Document recovery procedures
- [ ] Set up audit logging
- [ ] Configure retention policies

## Common Operations

### Starting the Service
```bash
# Start with docker-compose
docker-compose up -d

# Start with docker run
docker run -d -p 8080:8080 sarmkadan/dotnet-deploy-notify:2.0
```

### Stopping the Service
```bash
# Stop with docker-compose
docker-compose down

# Stop with docker
docker stop deploy-notify
```

### Viewing Logs
```bash
# View docker-compose logs
docker-compose logs -f

# View docker logs
docker logs -f deploy-notify
```

### Updating the Service
```bash
# Pull latest image
docker pull sarmkadan/dotnet-deploy-notify:2.0

# Restart service
docker-compose down
docker-compose up -d
```

### Executing Commands
```bash
# Enter container shell
docker exec -it deploy-notify /bin/sh

# Validate configuration
docker exec -it deploy-notify dotnet-deploy-notify validate-config

# Test notification
docker exec -it deploy-notify dotnet-deploy-notify test-notification
```

## Troubleshooting

### Common Issues

**Issue:** Container fails to start
**Solution:** Check logs with `docker logs deploy-notify`

**Issue:** Health check fails
**Solution:** Verify port mapping and network connectivity

**Issue:** Webhooks not sending
**Solution:** Check configuration file and webhook URLs

**Issue:** High memory usage
**Solution:** Set resource limits in docker-compose.yml

### Log Levels
```bash
# Enable debug logging
-e Logging__LogLevel__Default=Debug

# Enable detailed webhook logging
-e Logging__LogLevel__Webhook=Debug
```

### Performance Tuning
```yaml
deploy:
  resources:
    limits:
      memory: 512M
      cpus: '1.0'
    reservations:
      memory: 256M
      cpus: '0.5'
```

## Advanced Configuration

### Multi-Environment Setup
```yaml
version: '3.8'

services:
  deploy-notify:
    image: sarmkadan/dotnet-deploy-notify:2.0
    environment:
      - ASPNETCORE_ENVIRONMENT=${ENVIRONMENT:-Production}
    env_file:
      - .env.${ENVIRONMENT:-Production}
```

### Custom Network Configuration
```yaml
networks:
  deploy-notify-net:
    driver: bridge
    ipam:
      config:
        - subnet: 172.20.0.0/16

services:
  deploy-notify:
    networks:
      - deploy-notify-net
```

## Support

For Docker-specific issues:
- Docker documentation: https://docs.docker.com/
- Docker Compose reference: https://docs.docker.com/compose/compose-file/
- .NET containerization guide: https://learn.microsoft.com/en-us/dotnet/core/docker/

For application issues:
- GitHub repository: https://github.com/sarmkadan/dotnet-deploy-notify
- Issue tracker: https://github.com/sarmkadan/dotnet-deploy-notify/issues