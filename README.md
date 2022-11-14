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