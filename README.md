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

## DomainEvent

The `DomainEvent` class serves as the base class for all domain events in the system. It provides common event metadata including a unique identifier, timestamp, and aggregate identifier. Domain events enable loose coupling between components by allowing publishers to emit events without knowing their subscribers, and subscribers to react to specific event types without dependencies on the publishers.


Example usage:

```csharp
// Create a custom domain event
public class DeploymentCompletedEvent : DomainEvent
{
    public string ProjectName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
}

// Publish the event
var deploymentEvent = new DeploymentCompletedEvent
{
    AggregateId = "project-123",
    ProjectName = "MyWebApp",
    Version = "2.0.0",
    Environment = "Production",
    Duration = TimeSpan.FromMinutes(15)
};

// The event bus handles publishing
await eventBus.PublishAsync(deploymentEvent);

Console.WriteLine(deploymentEvent.ToString());
// Output: DeploymentCompletedEvent - project-123 @ 2024-07-15T10:30:45.123456Z
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

## SearchCriteria

The `SearchCriteria` class defines the filtering parameters available when searching deployment notifications. It supports filtering by project name, version, build status, environment, branch, author, date ranges, priority levels, notification channels, and message content. The criteria can be used with the `NotificationSearchEngine` to query and paginate through deployment notifications efficiently.

Example usage:
```csharp
var searchCriteria = new SearchCriteria
{
    ProjectName = "MyWebApp",
    Status = BuildStatus.DeploymentFailed,
    TargetEnvironment = Environment.Production,
    MinimumPriority = NotificationPriority.Medium,
    Channels = new List<NotificationChannel> { NotificationChannel.Email, NotificationChannel.Teams },
    CreatedAfter = DateTime.UtcNow.AddDays(-7),
    Limit = 50,
    Offset = 0
};

var searchEngine = new NotificationSearchEngine(logger);
var results = searchEngine.Search(allNotifications, searchCriteria);

Console.WriteLine($"Found {results.Total} notifications, showing {results.Returned}");
foreach (var notification in results.Items)
{
    Console.WriteLine($"[{notification.Status}] {notification.ProjectName}@{notification.Version} ({notification.TargetEnvironment})");
}
```

## RequestContext

The `RequestContext` class provides ambient request tracking functionality for tracking request execution across asynchronous boundaries. It captures correlation identifiers, timestamps, user information, and custom metadata, enabling comprehensive request tracing and debugging. The ambient context is stored using `AsyncLocal` for proper flow across async/await boundaries.



Example usage:

```csharp
// Set context for the current request
AmbientRequestContext.SetContext(new RequestContext
{
    UserId = "user-123",
    ClientId = "web-app",
    Metadata = new Dictionary<string, object>
    {
        ["requestSource"] = "web-portal",
        ["featureFlag"] = true,
        ["userAgent"] = "Mozilla/5.0"
    }
});

// Get the current context
var context = AmbientRequestContext.Current;
Console.WriteLine($"CorrelationId: {context.CorrelationId}");
Console.WriteLine($"RequestId: {context.RequestId}");
Console.WriteLine($"RequestTime: {context.RequestTime}");
Console.WriteLine($"UserId: {context.UserId}");
Console.WriteLine($"ClientId: {context.ClientId}");

// Use context scope for isolated operations
using (var scope = new RequestContextScope())
{
    scope.Context.SetMetadata("operation", "data-processing");
    scope.Context.ExecutionTimeMs = 150;
    
    // Execute work with the scoped context
    ProcessRequest(scope.Context);
}

// Execute work within a context using helper methods
RequestContextExtensions.ExecuteInContext(ctx =>
{
    ctx.SetMetadata("batchId", Guid.NewGuid().ToString());
    ctx.ExecutionTimeMs = 250;
    
    // Do work...
});

// Get or create context
var currentContext = RequestContextExtensions.GetOrCreateContext();
if (currentContext.HasMetadata("featureFlag"))
{
    var flagValue = currentContext.GetMetadata<bool>("featureFlag");
    Console.WriteLine($"Feature flag: {flagValue}");
}

// Clear context when done
AmbientRequestContext.ClearContext();
```
