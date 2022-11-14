// existing content ...

## IConfigurationValidator

The `IConfigurationValidator` interface provides a contract for validating system and channel configurations. It allows checking the validity of channel configurations, notification service configurations, and suggesting improvements to channel configurations.

Example usage:
```csharp
var validator = new ConfigurationValidator(ILogger<ConfigurationValidator>.CreateLogger<ConfigurationValidator>());
var config = new ChannelConfiguration
{
    DisplayName = "Example Channel",
    WebhookUrl = "https://example.com/webhook",
    TimeoutMs = 30000,
    MaxRetries = 5,
    AllowedEnvironments = new List<string> { "Production", "Staging" },
    IncludeCommitDetails = true,
    IncludeBuildUrl = true
};

var (isValid, warnings, errors) = validator.ValidateChannelConfiguration(config);
Console.WriteLine($"Is valid: {isValid}, Warnings: {string.Join(", ", warnings)}, Errors: {string.Join(", ", errors)}");

var suggestions = validator.SuggestImprovements(config);
Console.WriteLine($"Suggestions: {string.Join(", ", suggestions)}");
```

## IRequestLogger

The `IRequestLogger` interface provides a contract for logging HTTP requests and responses. It allows logging outgoing webhook requests, incoming webhook responses, and webhook errors. Implementations of this interface can store and retrieve request logs for analysis and debugging purposes.

Example usage:
```csharp
var logger = new RequestLogger(ILogger<RequestLogger>.CreateLogger<RequestLogger>());
var entry = new RequestLogEntry
{
    WebhookUrl = "https://example.com/webhook",
    Method = "POST",
    RequestHeaders = new Dictionary<string, string> { { "Content-Type", "application/json" } },
    RequestPayload = "{\"key\":\"value\"}",
    Timestamp = DateTime.UtcNow
};

logger.LogWebhookRequest(entry.WebhookUrl, entry.RequestPayload, entry.RequestHeaders);
logger.LogWebhookResponse(entry.WebhookUrl, 200, "{\"status\":\"ok\"}", 100);
logger.LogWebhookError(entry.WebhookUrl, "Error message");

var history = logger.GetRequestHistory();
foreach (var log in history)
{
    Console.WriteLine(log.GetSummary());
}
```

## CanaryDeploymentExtensions

The `CanaryDeploymentExtensions` class provides helper methods for analyzing and managing canary deployment status, health, and rollout progress. It includes methods to check deployment status, calculate health scores, determine promotion eligibility, and track traffic progression.

Example usage:
```csharp
var deployment = new CanaryDeployment
{
    Status = CanaryStatus.Active,
    CurrentSplit = new TrafficSplit { StablePercent = 80, CanaryPercent = 20 },
    CanaryMetrics = new CanaryMetrics { ErrorRatePercent = 0.5, P95LatencyMs = 150 },
    RolloutPlan = new List<RolloutStep>
    {
        new RolloutStep { CanaryPercent = 20, SoakDuration = TimeSpan.FromMinutes(10) },
        new RolloutStep { CanaryPercent = 50, SoakDuration = TimeSpan.FromMinutes(15) }
    },
    ActiveStep = new RolloutStep { CanaryPercent = 20, StartedAt = DateTime.UtcNow, Status = RolloutStepStatus.InProgress }
};

bool isActive = deployment.IsActive();
string statusSummary = deployment.GetStatusSummary();
bool canPromote = deployment.CanPromote();
double? nextPercentage = deployment.GetNextTrafficPercentage();
TimeSpan? soakRemaining = deployment.GetCurrentSoakRemaining();

Console.WriteLine(statusSummary);
Console.WriteLine($"Can promote: {canPromote}");
Console.WriteLine($"Next traffic percentage: {nextPercentage}%");
Console.WriteLine($"Soak remaining: {soakRemaining?.TotalMinutes:F1} minutes");
```

## NotificationProcessingWorkerExtensions

The `NotificationProcessingWorkerExtensions` class provides extension methods for configuring and monitoring `NotificationProcessingWorker` instances. It allows setting processing intervals, enabling detailed logging, creating health check tasks, and retrieving worker statistics.

Example usage:
```csharp
var worker = new NotificationProcessingWorker();
var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<NotificationProcessingWorker>();

// Configure worker with interval and logging
worker = worker.WithInterval(TimeSpan.FromMinutes(1))
               .WithDetailedLogging(logger);

// Create health check task
var healthCheckTask = worker.CreateHealthCheckTask(logger);

// Get and display statistics
var stats = worker.GetStatistics();
Console.WriteLine($"Processed: {stats.TotalProcessed}, Success Rate: {stats.SuccessRate:P}, Uptime: {stats.Uptime}");
```

## TestHttpClient

The `TestHttpClient` class is a mock implementation of `HttpClient` for testing purposes. It allows simulating different webhook response scenarios, such as success, failure, and network delays.

Example usage:
```csharp
var client = new TestHttpClient();
var response = await client.GetAsync("https://hooks.slack.com/services/test");
Console.WriteLine(response.StatusCode);
```