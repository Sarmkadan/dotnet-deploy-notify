// existing content ...

## ServiceExtensions

The `ServiceExtensions` class provides extension methods for analyzing and manipulating deployment notifications and channel configurations. It includes methods to determine notification severity, check environment/status compatibility, format statuses for display, clone notifications, and calculate retry delays. These extensions simplify common operations in notification processing pipelines.

Example usage:
```csharp
var notification = new DeploymentNotification
{
    Status = BuildStatus.DeploymentFailed,
    TargetEnvironment = Environment.Production,
    Priority = NotificationPriority.High
};

var channelConfig = new ChannelConfiguration
{
    AllowedStatuses = new List<BuildStatus> { BuildStatus.DeploymentFailed, BuildStatus.Success },
    AllowedEnvironments = new List<Environment> { Environment.Production }
};

bool isCritical = notification.IsCritical(); // true (status is DeploymentFailed)
bool isProd = notification.IsProduction(); // true (environment is Production)
bool supportsStatus = channelConfig.SupportsStatus(notification.Status); // true
bool supportsEnv = channelConfig.SupportsEnvironment(notification.TargetEnvironment); // true
string statusDesc = notification.Status.GetDescription(); // "Deployment failed"
string envDesc = notification.TargetEnvironment.GetDescription(); // "Production"

if (isCritical && supportsStatus && supportsEnv)
{
    var cloned = notification.Clone();
    cloned.MergeMetadata(new DeploymentNotification { Metadata = { { "alert", "urgent" } } });
    string logEntry = cloned.ToCompactString(); // "[DeploymentFailed] MyProject@1.0.0 (Production/main)"
}
```

## TrafficSplitter

The `TrafficSplitter` class is responsible for producing per-request routing decisions and generating strategy-specific rollout plans based on the current `CanaryOptions` configuration. It computes traffic splits for canary deployments, determines whether requests should be routed to the canary version, and evaluates canary health by comparing metrics against configured thresholds.



Example usage:
```csharp
// Configure services
services.Configure<CanaryOptions>(options =>
{
    options.Strategy = CanaryStrategy.Linear;
    options.LinearStepCount = 5;
    options.StepSoakDuration = TimeSpan.FromMinutes(15);
    options.Thresholds = new CanaryThresholds
    {
        MaxErrorRatePercent = 1.0,
        MaxP95LatencyMs = 200,
        MaxP99LatencyMs = 500,
        ErrorRateMultiplier = 2.0,
        LatencyDegradationPercent = 50.0
    };
});

services.AddSingleton<ITrafficSplitter, TrafficSplitter>();
services.AddSingleton<ICanaryHealthEvaluator, CanaryHealthEvaluator>();

// In your deployment service
var splitter = serviceProvider.GetRequiredService<ITrafficSplitter>();
var evaluator = serviceProvider.GetRequiredService<ICanaryHealthEvaluator>();

// Generate rollout plan
var rolloutPlan = splitter.GenerateRolloutPlan(CanaryStrategy.Linear);

// Create deployment with current and canary versions
var deployment = new CanaryDeployment
{
    ProjectName = "MyWebApp",
    StableVersion = "v1.2.3",
    CanaryVersion = "v1.2.4-rc1",
    TargetEnvironment = Environment.Staging,
    RolloutPlan = rolloutPlan,
    CurrentSplit = new TrafficSplit { CanaryPercent = 0 }
};

// Compute next traffic split
deployment.CurrentSplit = splitter.ComputeNextSplit(deployment);

// Determine if request should route to canary
bool routeToCanary = splitter.ShouldRouteToCanary(deployment.CurrentSplit);

// Evaluate canary health
var evaluationResult = await evaluator.EvaluateAsync(deployment);

if (evaluationResult.IsHealthy)
{
    Console.WriteLine("Canary is healthy, proceeding with rollout");
}
else
{
    Console.WriteLine($"Canary unhealthy: {evaluationResult.Reason}");
}
```
```

## CanaryDeploymentEngine

The `CanaryDeploymentEngine` orchestrates canary deployments end-to-end, managing traffic splitting, step-by-step rollout advancement, health evaluation, automatic rollback on failure, and lifecycle notifications dispatched via the notification pipeline. It coordinates between the traffic splitter, health evaluator, rollback service, and notification service to provide a complete canary deployment workflow.





Example usage:

```csharp
// Configure required services
services.AddSingleton<ITrafficSplitter, TrafficSplitter>();
services.AddSingleton<ICanaryHealthEvaluator, CanaryHealthEvaluator>();
services.AddSingleton<IRollbackService, RollbackService>();
services.AddSingleton<INotificationService, NotificationService>();

// Register the engine
services.AddSingleton<ICanaryDeploymentService, CanaryDeploymentEngine>();

// Configure canary options
services.Configure<CanaryOptions>(options =>
{
    options.Strategy = CanaryStrategy.Linear;
    options.LinearStepCount = 5;
    options.StepSoakDuration = TimeSpan.FromMinutes(15);
    options.AutoAdvanceOnSuccess = true;
    options.AutoRollbackOnFailure = true;
    options.Thresholds = new CanaryThresholds
    {
        MaxErrorRatePercent = 1.0,
        MaxP95LatencyMs = 200,
        MaxP99LatencyMs = 500,
        ErrorRateMultiplier = 2.0,
        LatencyDegradationPercent = 50.0
    };
});

// In your deployment controller/service
var engine = serviceProvider.GetRequiredService<ICanaryDeploymentService>();

// Start a new canary deployment
var deployment = await engine.StartCanaryAsync(new CanaryDeploymentRequest
{
    ProjectName = "MyWebApp",
    StableVersion = "v1.2.3",
    CanaryVersion = "v1.2.4-rc1",
    TargetEnvironment = Environment.Staging,
    Strategy = CanaryStrategy.Linear,
    NotificationChannels = new List<string> { "teams", "email" },
    Priority = NotificationPriority.High,
    InitiatedBy = "ci-pipeline",
    BranchName = "main",
    CommitHash = "abc123",
    BuildUrl = "https://ci.example.com/build/123",
    Metadata = new Dictionary<string, object>
    {
        ["featureFlag"] = "new-auth-logic"
    }
});

Console.WriteLine($"Canary started: {deployment.Id}");

// Advance to next rollout step (e.g., after soak period)
deployment = await engine.AdvanceRolloutAsync(deployment.Id);

// Evaluate canary health after each advancement
var healthResult = await engine.EvaluateHealthAsync(deployment.Id);
if (healthResult.IsHealthy)
{
    Console.WriteLine("Canary is healthy, continuing rollout");
}
else
{
    Console.WriteLine($"Canary unhealthy: {healthResult.Reason}");
}

// Promote to full traffic once canary is validated
deployment = await engine.PromoteAsync(deployment.Id);

// Or abort if issues are detected
deployment = await engine.AbortAsync(deployment.Id, "High error rate detected");

// Query active deployments
var activeDeployments = await engine.GetActiveDeploymentsAsync();

// Get deployment history for a project
var history = await engine.GetDeploymentHistoryAsync("MyWebApp", limit: 20);
```
