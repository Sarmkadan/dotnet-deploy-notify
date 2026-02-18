# Migration Guide: v1.x to v2.0

## Overview

Version 2.0 introduces significant new features including **canary deployment with traffic splitting**, **automatic rollback mechanisms**, and **enhanced configuration options**. While the core library maintains backward compatibility, this guide covers all changes you need to know when migrating from v1.x to v2.0.


## Breaking Changes

### 1. Canary Deployment Configuration Structure

**v1.x:** Canary deployments were configured through simple boolean flags:

```json
{
  "Canary": {
    "Enabled": true,
    "Percentage": 10
  }
}
```

**v2.0:** Canary deployments now use a structured configuration with traffic splitting and rollback policies:

```json
{
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
  }
}
```

### 2. Notification Channel Configuration

**v1.x:** Channels were configured with simple webhook URLs:

```json
{
  "Channels": [
    {
      "Type": "Slack",
      "WebhookUrl": "https://hooks.slack.com/services/...",
      "Filter": {
        "Environments": ["production"],
        "Statuses": ["Success"]
      }
    }
  ]
}
```

**v2.0:** Channels now support additional metadata and priority-based routing:

```json
{
  "Channels": [
    {
      "Type": "Slack",
      "WebhookUrl": "https://hooks.slack.com/services/...",
      "Name": "Production Slack",
      "Priority": 1,
      "Filter": {
        "Environments": ["production"],
        "Statuses": ["Success", "Failed"],
        "MinimumPriority": "Medium"
      },
      "CustomHeaders": {
        "X-Custom-Header": "value"
      }
    }
  ]
}
```

### 3. DeploymentNotification Model Changes

**v1.x:** Basic notification structure:

```csharp
var notification = new DeploymentNotification
{
    ProjectName = "MyApp",
    Version = "1.0.0",
    Status = BuildStatus.Success,
    Environment = "production"
};
```

**v2.0:** Enhanced with canary deployment tracking:

```csharp
var notification = new DeploymentNotification
{
    ProjectName = "MyApp",
    Version = "2.0.0",
    Status = BuildStatus.Success,
    Environment = "production",
    CanaryDeployment = new CanaryDeployment
    {
        DeploymentId = "canary-20260518-001",
        TrafficPercentage = 10,
        IsCanary = false
    }
};
```

### 4. RollbackRequest Model

**New in v2.0:** Dedicated rollback request model:

```csharp
var rollbackRequest = new RollbackRequest
{
    DeploymentId = "canary-20260518-001",
    Environment = "production",
    RollbackReason = "High error rate detected in canary",
    RollbackToVersion = "1.9.2"
};

// Send rollback request
var result = await rollbackService.RequestRollbackAsync(rollbackRequest);
```

## New Features in v2.0


### 1. Canary Deployment with Traffic Splitting

Canary deployments allow gradual rollout of new versions to a subset of users:


```csharp
// Configure canary deployment
services.Configure<CanaryOptions>(configuration.GetSection("Canary"));

// Create canary deployment
var canaryDeployment = new CanaryDeployment
{
    DeploymentId = $"canary-{DateTime.UtcNow:yyyyMMdd-HHmm}",
    TrafficPercentage = 10, // Start with 10% traffic
    BaselineVersion = "1.9.2",
    CanaryVersion = "2.0.0"
};

// Register with deployment service
services.AddCanaryDeployment(canaryDeployment);
```

**Traffic Split Configuration:**
- `BaselinePercentage`: Percentage of traffic to baseline version (default: 90)
- `CanaryPercentage`: Percentage of traffic to canary version (default: 10)
- `AutoRollbackThreshold`: Error threshold (%) that triggers automatic rollback (default: 5)


### 2. Automatic Rollback Mechanism

Automatic rollback when error thresholds are exceeded:

```csharp
// Configure rollback policies
services.Configure<CanaryOptions>(options =>
{
    options.Rollback.Enabled = true;
    options.Rollback.FailureThreshold = 3; // Failures before rollback
    options.Rollback.DurationMinutes = 15; // Monitor duration
});

// Monitor canary deployment
var canaryEngine = host.Services.GetRequiredService<ICanaryDeploymentEngine>();
var status = await canaryEngine.MonitorDeploymentAsync("canary-20260518-001");

if (status.ShouldRollback)
{
    await canaryEngine.RollbackAsync("canary-20260518-001");
}
```

**Rollback Conditions:**
- Error rate exceeds configured threshold
- Deployment duration exceeds timeout
- Manual rollback request received

### 3. Traffic Splitter Service

Dynamically adjust traffic between baseline and canary versions:

```csharp
var trafficSplitter = host.Services.GetRequiredService<ITrafficSplitter>();

// Get current traffic split
var split = await trafficSplitter.GetTrafficSplitAsync("canary-20260518-001");

// Gradually increase canary traffic
await trafficSplitter.AdjustTrafficAsync(
    deploymentId: "canary-20260518-001",
    canaryPercentage: 25 // Increase from 10% to 25%
);
```

### 4. Enhanced Channel Configuration

**Priority-based Routing:**
```json
{
  "Channels": [
    {
      "Type": "Slack",
      "Priority": 1,
      "Filter": { "Environments": ["production"] }
    },
    {
      "Type": "Discord",
      "Priority": 2,
      "Filter": { "Environments": ["staging"] }
    },
    {
      "Type": "Telegram",
      "Priority": 3,
      "Filter": { "Environments": ["development"] }
    }
  ]
}
```

**Minimum Priority Filtering:**
```csharp
// Only send notifications with Medium or higher priority
var notification = new DeploymentNotification
{
    ProjectName = "MyApp",
    Version = "2.0.0",
    Status = BuildStatus.Success,
    Priority = NotificationPriority.High,
    Environment = "production"
};
```

### 5. Batch Notification Improvements

**Scheduled Batch Processing:**
```csharp
var batchService = host.Services.GetRequiredService<IBatchNotificationService>();

// Create batch with delay
var batch = await batchService.CreateBatchAsync(
    batchName: "release-2.0",
    delayMinutes: 5 // Wait 5 minutes before processing
);

// Add notifications to batch
await batchService.AddToBatchAsync(batch.Id, notification);
```

## Migration Steps

### Step 1: Update Configuration Files

1. **Backup your existing configuration:**
   ```bash
   cp appsettings.json appsettings.backup.json
   ```

2. **Review channel configurations:**
   - Add `Priority` field to all channels
   - Update filter syntax for `Environments` and `Statuses`
   - Add `CustomHeaders` if needed

3. **Add canary deployment configuration:**
   ```json
   {
     "Canary": {
       "Enabled": false,
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
     }
   }
   ```

### Step 2: Update Code for New Features


**For existing notification code (no changes required):**
```csharp
// Your existing code continues to work
var result = await notificationService.SendAsync(new DeploymentNotification
{
    ProjectName = "MyApp",
    Version = "2.0.0",
    Status = BuildStatus.Success,
    Environment = "production"
});
```

**To enable canary deployments:**
```csharp
// Add to your service configuration
services.AddCanaryDeployment();

// Create canary deployment when needed
var canaryDeployment = new CanaryDeployment
{
    DeploymentId = $"canary-{DateTime.UtcNow:yyyyMMdd-HHmm}",
    TrafficPercentage = 10
};
```

### Step 3: Test Configuration

1. **Validate new configuration:**
   ```bash
   dotnet run -- --validate-config
   ```

2. **Test canary deployment:**
   ```bash
   # Start with 5% traffic
   curl -X POST http://localhost:8080/api/canary/deploy \
     -H "Content-Type: application/json" \
     -d '{"deploymentId":"test-canary","trafficPercentage":5}'
   ```

3. **Verify rollback mechanism:**
   ```bash
   # Trigger rollback
   curl -X POST http://localhost:8080/api/canary/rollback \
     -H "Content-Type: application/json" \
     -d '{"deploymentId":"test-canary"}'
   ```

### Step 4: Gradual Rollout

1. Start with canary traffic at 5-10%
2. Monitor error rates and metrics
3. Gradually increase traffic to 25%, 50%, 75%
4. Promote to full deployment when stable

## Code Examples: Old vs New API


### Example 1: Simple Notification

**v1.x:**
```csharp
var notification = new DeploymentNotification
{
    ProjectName = "MyApi",
    Version = "1.0.0",
    Status = BuildStatus.Success,
    Environment = "production"
};

await notificationService.SendAsync(notification);
```

**v2.0:**
```csharp
var notification = new DeploymentNotification
{
    ProjectName = "MyApi",
    Version = "2.0.0",
    Status = BuildStatus.Success,
    Environment = "production",
    Priority = NotificationPriority.Medium
};

await notificationService.SendAsync(notification);
```

### Example 2: Batch Notification

**v1.x:**
```csharp
var batch = await batchService.CreateBatchAsync("release-1.0");

foreach (var service in services)
{
    await batchService.AddToBatchAsync(batch.Id, new DeploymentNotification
    {
        ProjectName = service.Name,
        Version = service.Version,
        Status = service.Status
    });
}

await batchService.ProcessBatchAsync(batch.Id);
```

**v2.0:**
```csharp
var batch = await batchService.CreateBatchAsync(
    batchName: "release-2.0",
    delayMinutes: 5 // Optional delay before processing
);

foreach (var service in services)
{
    await batchService.AddToBatchAsync(batch.Id, new DeploymentNotification
    {
        ProjectName = service.Name,
        Version = service.Version,
        Status = service.Status,
        Priority = service.Priority
    });
}

await batchService.ProcessBatchAsync(batch.Id);
```

### Example 3: Canary Deployment Setup

**v2.0 Only:**
```csharp
// Configure services
services.AddCanaryDeployment();
services.Configure<CanaryOptions>(configuration.GetSection("Canary"));

// Create canary deployment
var canary = new CanaryDeployment
{
    DeploymentId = $"canary-{DateTime.UtcNow:yyyyMMdd-HHmmss}",
    TrafficPercentage = 10,
    BaselineVersion = "1.9.2",
    CanaryVersion = "2.0.0",
    Environment = "production"
};

// Start canary deployment
var canaryEngine = host.Services.GetRequiredService<ICanaryDeploymentEngine>();
await canaryEngine.StartDeploymentAsync(canary);

// Monitor and adjust traffic
var trafficSplitter = host.Services.GetRequiredService<ITrafficSplitter>();

// After 30 minutes, increase traffic to 25%
await Task.Delay(TimeSpan.FromMinutes(30));
await trafficSplitter.AdjustTrafficAsync(canary.DeploymentId, 25);

// After successful monitoring, promote to full deployment
if (await canaryEngine.IsHealthyAsync(canary.DeploymentId))
{
    await canaryEngine.PromoteAsync(canary.DeploymentId);
}
```

### Example 4: Automatic Rollback

**v2.0 Only:**
```csharp
// Configure rollback
services.Configure<CanaryOptions>(options =>
{
    options.Rollback.Enabled = true;
    options.Rollback.FailureThreshold = 3;
    options.Rollback.DurationMinutes = 15;
});

// Monitor deployment
var canaryEngine = host.Services.GetRequiredService<ICanaryDeploymentEngine>();
var monitor = host.Services.GetRequiredService<IMetricsService>();

while (true)
{
    var status = await canaryEngine.GetStatusAsync(canary.DeploymentId);
    var metrics = await monitor.GetMetricsAsync(canary.DeploymentId);
    
    if (metrics.ErrorRate > 0.05) // 5% error rate
    {
        await canaryEngine.RollbackAsync(canary.DeploymentId);
        break;
    }
    
    await Task.Delay(TimeSpan.FromMinutes(5));
}
```

## Configuration Changes Reference

### CanaryOptions
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Enabled` | bool | false | Enable canary deployment |
| `TrafficSplit.BaselinePercentage` | int | 90 | Percentage for baseline version |
| `TrafficSplit.CanaryPercentage` | int | 10 | Percentage for canary version |
| `TrafficSplit.AutoRollbackThreshold` | int | 5 | Error threshold for auto-rollback (%) |
| `Rollback.Enabled` | bool | true | Enable automatic rollback |
| `Rollback.FailureThreshold` | int | 3 | Number of failures before rollback |
| `Rollback.DurationMinutes` | int | 15 | Monitoring duration before rollback |

### ChannelConfiguration (Enhanced)
| Property | Type | Description |
|----------|------|-------------|
| `Priority` | int | Channel priority (1-1000) |
| `Name` | string | Human-readable channel name |
| `CustomHeaders` | Dictionary<string, string> | Additional HTTP headers |
| `Filter.MinimumPriority` | NotificationPriority | Minimum notification priority to send |

## Rollback Procedures

### Manual Rollback
```bash
# Via API
curl -X POST http://localhost:8080/api/canary/rollback \
  -H "Content-Type: application/json" \
  -d '{"deploymentId":"canary-20260518-001","rollbackToVersion":"1.9.2"}'
```

```csharp
// Via service
var rollbackService = host.Services.GetRequiredService<IRollbackService>();
var result = await rollbackService.RequestRollbackAsync(new RollbackRequest
{
    DeploymentId = "canary-20260518-001",
    RollbackReason = "Manual rollback requested"
});
```

### Automatic Rollback
Configure in `appsettings.json`:
```json
{
  "Canary": {
    "Rollback": {
      "Enabled": true,
      "FailureThreshold": 3,
      "DurationMinutes": 15
    }
  }
}
```

## Testing Your Migration

### Validation Commands
```bash
# Validate configuration
./dotnet-deploy-notify validate-config

# Test canary deployment
./dotnet-deploy-notify canary test --deployment-id test-canary --traffic 10

# Test rollback
./dotnet-deploy-notify rollback test --deployment-id test-canary
```

### Health Checks
```bash
# Check application health
curl http://localhost:8080/health

# Check canary deployment status
curl http://localhost:8080/api/canary/status/canary-20260518-001
```

## Troubleshooting

### Configuration Errors
- **Error:** Missing `Canary` section in appsettings.json
  **Solution:** Add empty configuration or disable canary deployments
  ```json
  {
    "Canary": {
      "Enabled": false
    }
  }
  ```

- **Error:** Traffic split percentages don't sum to 100
  **Solution:** Ensure `BaselinePercentage + CanaryPercentage = 100`


### Deployment Errors
- **Error:** Canary deployment not starting
  **Solution:** Verify `Canary.Enabled = true` in configuration

- **Error:** Rollback not triggering
  **Solution:** Check `Rollback.FailureThreshold` and error metrics


### Performance Issues
- **Error:** High latency during canary monitoring
  **Solution:** Adjust monitoring interval or increase resources

## Support & Resources

- **Documentation:** https://github.com/sarmkadan/dotnet-deploy-notify
- **Issues:** GitHub issue tracker
- **Community:** Discussion forums

---

*Migration Date: 2026-05-18*
*Version: 2.0.0*