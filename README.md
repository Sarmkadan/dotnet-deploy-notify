# dotnet-deploy-notify

A .NET application for sending deployment notifications to various channels (Slack, Discord, Telegram, etc.).

## Features

- Send deployment notifications to multiple channels
- Support for Slack, Discord, and Telegram webhooks
- Batch notification processing
- Configurable channel strategies
- Integration with deployment pipelines

## Architecture

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the module breakdown, composition roots, data flow, extension points, and known limitations. Short version: a console app whose active wiring is `AddNotificationServices` (validation → in-memory repositories → webhook dispatch via a typed `HttpClient`); the event bus, middleware pipeline, background workers, and canary engine are optional opt-in subsystems.

## BatchNotification

The `BatchNotification` class represents a collection of notifications to be sent together, allowing for batch processing and improved delivery efficiency. It provides properties and methods to manage the batch's status, notifications, channels, and delivery results.

Example usage:
```csharp
var batchNotification = new BatchNotification
{
  Name = "Deployment Alerts",
  Description = "Alerts for deployment notifications",
  Notifications = new List<DeploymentNotification>
  {
    new DeploymentNotification { /* initialize notification properties */ },
    new DeploymentNotification { /* initialize notification properties */ }
  },
  Channels = new List<NotificationChannel>
  {
    new NotificationChannel { /* initialize channel properties */ }
  }
};

if (batchNotification.IsValid())
{
  Console.WriteLine($"Batch {batchNotification.Name} is valid.");
  // Process the batch
  batchNotification.MarkAsSent();
  Console.WriteLine($"Batch {batchNotification.Name} sent successfully. Success rate: {batchNotification.GetSuccessRate():F1}%");
}
else
{
  Console.WriteLine("Invalid batch notification.");
}
```

## IChannelStrategy

The `IChannelStrategy` interface defines the contract for channel-specific notification strategies. It enables polymorphic handling of different notification channels (Slack, Discord, Telegram, etc.) through a unified interface, allowing the system to send notifications to various channels without tight coupling to specific implementations.

Each strategy implementation provides channel-specific logic for determining support (`CanHandle`) and sending notifications (`SendAsync`).

Example usage:
```csharp
// Register strategies
var resolver = new ChannelStrategyResolver(logger);
resolver.RegisterStrategy(new SlackChannelStrategy(webhookClient, logger));
resolver.RegisterStrategy(new DiscordChannelStrategy(webhookClient, logger));
resolver.RegisterStrategy(new TelegramChannelStrategy(webhookClient, logger));

// Get appropriate strategy for a channel
var channel = NotificationChannel.Slack;
if (resolver.IsSupported(channel))
{
    var strategy = resolver.GetStrategy(channel);
    var result = await strategy.SendAsync(
        new DeploymentNotification { /* notification data */ },
        new ChannelConfiguration { WebhookUrl = "https://hooks.slack.com/..." },
        "{ \"text\": \"Deployment completed!\" }"
    );
    
    if (result)
    {
        Console.WriteLine("Notification sent successfully!");
    }
}
```

## ChannelStrategyResolver

The `ChannelStrategyResolver` class manages the registration and retrieval of channel strategies. It provides methods to register strategies, check support for channels, and retrieve strategies.

Example usage:
```csharp
var resolver = new ChannelStrategyResolver(logger);

// Register strategies
resolver.RegisterStrategy(new SlackChannelStrategy(webhookClient, slackLogger));
resolver.RegisterStrategy(new DiscordChannelStrategy(webhookClient, discordLogger));

// Check if a channel is supported
bool isSlackSupported = resolver.IsSupported(NotificationChannel.Slack);

// Get a strategy
var strategy = resolver.GetStrategy(NotificationChannel.Discord);

// Get all registered strategies
var allStrategies = resolver.GetAllStrategies();
```

## ChannelAdapter

The `ChannelAdapter` class provides backward compatibility and simplifies sending notifications through the channel system. It automatically handles payload building and strategy resolution.

Example usage:
```csharp
var adapter = new ChannelAdapter(resolver, payloadBuilderFactory, logger);

var config = new ChannelConfiguration
{
    ChannelType = NotificationChannel.Telegram,
    WebhookUrl = "https://api.telegram.org/bot..."
};

bool success = await adapter.SendAsync(
    new DeploymentNotification
    {
        ProjectName = "MyApp",
        Version = "1.0.0",
        Status = DeploymentStatus.Success
    },
    config
);

if (success)
{
    Console.WriteLine("Notification sent!");
}
```

## Result

The `Result<T>` type provides a functional way to handle operations that might fail, avoiding exceptions for expected control flow. It encapsulates both successful values and error messages, allowing for chaining operations like `Map` and `Bind` to create clean, expressive pipelines.

Example usage:
```csharp
// Simple usage
public Result<int> Divide(int numerator, int denominator)
{
    if (denominator == 0)
        return Result<int>.Fail("Cannot divide by zero.");

    return Result<int>.Ok(numerator / denominator);
}

// Chaining operations
var result = Divide(10, 2)
    .OnSuccess(val => Console.WriteLine($"Result: {val}"))
    .OnFailure(err => Console.WriteLine($"Error: {err}"));

if (result.IsSuccess)
{
    var value = result.GetValueOrThrow();
}
```

## NotificationProcessingWorker

The `NotificationProcessingWorker` is a background worker that periodically processes pending notifications from the database, sending them through the configured channels. It runs on a configurable interval (default: 30 seconds) and provides statistics about processed notifications including success rates and uptime.

Example usage:
```csharp
// Create services
var notificationService = new NotificationService(
    new NotificationRepository(dbContext),
    new ChannelStrategyResolver(new WebhookClientFactory()),
    logger);

// Create worker with 1 minute interval
var worker = new NotificationProcessingWorker(
    notificationService,
    logger,
    TimeSpan.FromMinutes(1));

// Start the worker
await worker.StartAsync(cancellationToken);

// The worker will automatically process pending notifications every minute
// You can adjust the interval dynamically
worker.SetInterval(TimeSpan.FromSeconds(45));

// Stop the worker when needed
await worker.StopAsync(cancellationToken);

// Get statistics about processed notifications
var (totalProcessed, successRate, uptime) = worker.GetStatisticsCore();
Console.WriteLine($"Processed {totalProcessed} notifications with {successRate:P0} success rate over {uptime.TotalMinutes:F0} minutes");
```

## Supported Channels

The application currently supports the following notification channels:

- **Slack** - Team communication and alerts
- **Discord** - Community and team notifications  
- **Telegram** - Mobile and desktop notifications


## DotnetDeployNotifyOptions

The `DotnetDeployNotifyOptions` class provides centralized configuration for the deployment notification system. It controls notification behavior, retry logic, storage settings, webhook configurations, and channel-specific options, enabling consistent deployment notifications across different environments and deployment pipelines.

This configuration class is typically used in `appsettings.json` or loaded through dependency injection to customize notification delivery behavior.

Example usage:

```csharp
// Configure DotnetDeployNotifyOptions in appsettings.json
{
  "DotnetDeployNotifyOptions": {
    "Notification": {
      "DefaultPriority": "Normal",
      "EnableAuditLogging": true,
      "RetentionDays": 30,
      "IncludeCommitDetails": true,
      "IncludeBuildUrl": true
    },
    "Canary": {
      "Enabled": true,
      "Threshold": 0.1
    },
    "MaxRetries": 5,
    "WebhookTimeoutMs": 10000,
    "RetryDelayMs": 2000,
    "AutoProcessNotifications": true,
    "ProcessingIntervalSeconds": 30,
    "StorageType": "Database",
    "LogLevel": "Information",
    "StoragePath": "/var/data/notifications",
    "EnvironmentChannels": {
      "Production": {
        "ChannelType": "Slack",
        "WebhookUrl": "https://hooks.slack.com/services/...",
        "DisplayName": "Production Alerts"
      },
      "Development": {
        "ChannelType": "Discord",
        "WebhookUrl": "https://discord.com/api/webhooks/...",
        "DisplayName": "Dev Notifications"
      }
    },
    "DefaultPriority": "Normal",
    "TargetId": "production-channel-1"
  }
}

// Or configure programmatically in Program.cs
builder.Services.Configure<DotnetDeployNotifyOptions>(options =>
{
    options.MaxRetries = 3;
    options.WebhookTimeoutMs = 5000;
    options.RetryDelayMs = 1000;
    options.AutoProcessNotifications = true;
    options.ProcessingIntervalSeconds = 60;
    options.LogLevel = "Debug";
    options.StorageType = "FileSystem";
    options.RetentionDays = 90;
    options.IncludeCommitDetails = true;
    options.IncludeBuildUrl = true;
    options.DefaultPriority = "High";
    options.EnableAuditLogging = true;
    
    // Configure environment-specific channels
    options.EnvironmentChannels = new Dictionary<string, EnvironmentChannelConfig>
    {
        ["Production"] = new EnvironmentChannelConfig
        {
            ChannelType = "Slack",
            WebhookUrl = "https://hooks.slack.com/services/T123/B456/C789",
            DisplayName = "Production Alerts",
            TargetId = "C123456"
        },
        ["Staging"] = new EnvironmentChannelConfig
        {
            ChannelType = "Discord",
            WebhookUrl = "https://discord.com/api/webhooks/789/abc",
            DisplayName = "Staging Notifications",
            TargetId = "D789012"
        }
    };
});

## CanaryDeploymentEngineExtensionsJsonExtensions

The `CanaryDeploymentEngineExtensionsJsonExtensions` class provides System.Text.Json serialization helpers for the `CanaryDeploymentEngineExtensions` class metadata. It enables converting canary deployment engine extension type information to and from JSON format with configurable formatting options, supporting both compact and indented output formats.

This extension class is particularly useful for persisting canary deployment engine configuration metadata to configuration files, databases, or remote services, and for restoring it back into application memory. It provides methods for serialization (`ToJson()`), deserialization (`FromJson()`), and safe deserialization with error handling (`TryFromJson()`).

Example usage:

```csharp
// Serialize to JSON string (compact format)
string jsonCompact = CanaryDeploymentEngineExtensionsJsonExtensions.ToJson();
Console.WriteLine(jsonCompact);
// Output: {"type":"CanaryDeploymentEngineExtensions","namespace":"DotNetDeployNotify.Canary","assembly":"DotNetDeployNotify","methods":["TryAdvanceRolloutAsync","TryPromoteAsync","TryAbortAsync","GetCanaryPercentageNormalizedAsync"]}

// Serialize to JSON string (indented format)
string jsonIndented = CanaryDeploymentEngineExtensionsJsonExtensions.ToJson(indented: true);
Console.WriteLine(jsonIndented);
/* Output:
{
  "type": "CanaryDeploymentEngineExtensions",
  "namespace": "DotNetDeployNotify.Canary",
  "assembly": "DotNetDeployNotify",
  "methods": [
    "TryAdvanceRolloutAsync",
    "TryPromoteAsync",
    "TryAbortAsync",
    "GetCanaryPercentageNormalizedAsync"
  ]
}
*/

// Deserialize from JSON string
var deserializedMetadata = CanaryDeploymentEngineExtensionsJsonExtensions.FromJson(jsonCompact);
if (deserializedMetadata != null)
{
  Console.WriteLine($"Deserialized type: {deserializedMetadata.Type}");
  Console.WriteLine($"Namespace: {deserializedMetadata.Namespace}");
  Console.WriteLine($"Assembly: {deserializedMetadata.Assembly}");
  Console.WriteLine($"Methods count: {deserializedMetadata.Methods?.Length ?? 0}");
}

// Try deserialization with error handling
if (CanaryDeploymentEngineExtensionsJsonExtensions.TryFromJson(jsonCompact, out var result))
{
  Console.WriteLine("Successfully deserialized metadata");
}
else
{
  Console.WriteLine("Failed to deserialize metadata");
}
```

## CanaryDeploymentExtensionsJsonExtensions

The `CanaryDeploymentExtensionsJsonExtensions` class provides System.Text.Json serialization helpers for the `CanaryDeploymentExtensions` type metadata. It enables converting canary deployment extension type information to and from JSON format with configurable formatting options, supporting both compact and indented output formats.

This extension class is particularly useful for persisting canary deployment configuration metadata to configuration files, databases, or remote services, and for restoring it back into application memory. It provides three main methods: `ToJson()` for serialization, `FromJson()` for deserialization, and `TryFromJson()` for safe deserialization with error handling.

Example usage:

```csharp
// Serialize to JSON string (compact format)
string jsonCompact = CanaryDeploymentExtensionsJsonExtensions.ToJson();
Console.WriteLine(jsonCompact);
// Output: {"type":"CanaryDeploymentExtensions","namespace":"DotNetDeployNotify.Core.Models","assembly":"DotNetDeployNotify","methods":["IsActive","IsPromoted","IsFailedOrAborted","GetTrafficSplitDisplay","CalculateHealthScore","GetStatusSummary","CanPromote","GetNextTrafficPercentage","GetCurrentSoakRemaining","IsCurrentSoakComplete"]}

// Serialize to JSON string (indented format)
string jsonIndented = CanaryDeploymentExtensionsJsonExtensions.ToJson(indented: true);
Console.WriteLine(jsonIndented);
/* Output:
{
  "type": "CanaryDeploymentExtensions",
  "namespace": "DotNetDeployNotify.Core.Models",
  "assembly": "DotNetDeployNotify",
  "methods": [
    "IsActive",
    "IsPromoted",
    "IsFailedOrAborted",
    "GetTrafficSplitDisplay",
    "CalculateHealthScore",
    "GetStatusSummary",
    "CanPromote",
    "GetNextTrafficPercentage",
    "GetCurrentSoakRemaining",
    "IsCurrentSoakComplete"
  ]
}
*/

// Deserialize from JSON string
var deserializedMetadata = CanaryDeploymentExtensionsJsonExtensions.FromJson(jsonCompact);
if (deserializedMetadata != null)
{
  Console.WriteLine($"Deserialized type: {deserializedMetadata.Type}");
  Console.WriteLine($"Namespace: {deserializedMetadata.Namespace}");
  Console.WriteLine($"Assembly: {deserializedMetadata.Assembly}");
  Console.WriteLine($"Methods count: {deserializedMetadata.Methods?.Length ?? 0}");
}

// Try deserialization with error handling
if (CanaryDeploymentExtensionsJsonExtensions.TryFromJson(jsonCompact, out var result))
{
  Console.WriteLine("Successfully deserialized metadata");
}
else
{
  Console.WriteLine("Failed to deserialize metadata");
}
```

## CanaryServiceExtensionsJsonExtensions

The `CanaryServiceExtensionsJsonExtensions` class provides JSON serialization and deserialization utilities for `CanaryServiceExtensionsMetadata` objects. It enables converting canary service extension metadata to and from JSON format with configurable formatting options, supporting both strict and tolerant parsing scenarios.

This extension class is particularly useful for persisting canary deployment configuration metadata to configuration files, databases, or remote services, and for restoring it back into application memory.

Example usage:

```csharp
// Create canary service extensions metadata
var metadata = new CanaryServiceExtensionsJsonExtensions.CanaryServiceExtensionsMetadata
{
    Type = "CanaryServiceExtensions",
    Namespace = "DotNetDeployNotify.Infrastructure",
    Assembly = "DotNetDeployNotify.Infrastructure",
    Methods = new[] { "AddCanaryServices", "ConfigureCanaryOptions" }
};

// Serialize to JSON string (compact format)
string jsonCompact = CanaryServiceExtensionsJsonExtensions.ToJson(metadata);
Console.WriteLine(jsonCompact);
// Output: {"type":"CanaryServiceExtensions","namespace":"DotNetDeployNotify.Infrastructure","assembly":"DotNetDeployNotify.Infrastructure","methods":["AddCanaryServices","ConfigureCanaryOptions"]}

// Serialize to JSON string (indented format)
string jsonIndented = CanaryServiceExtensionsJsonExtensions.ToJson(metadata, indented: true);
Console.WriteLine(jsonIndented);
/* Output:
{
  "type": "CanaryServiceExtensions",
  "namespace": "DotNetDeployNotify.Infrastructure",
  "assembly": "DotNetDeployNotify.Infrastructure",
  "methods": [
    "AddCanaryServices",
    "ConfigureCanaryOptions"
  ]
}
*/

// Deserialize from JSON string
var deserializedMetadata = CanaryServiceExtensionsJsonExtensions.FromJson(jsonCompact);
if (deserializedMetadata != null)
{
    Console.WriteLine($"Deserialized type: {deserializedMetadata.Type}");
    Console.WriteLine($"Methods count: {deserializedMetadata.Methods?.Length ?? 0}");
}

// Try deserialization with error handling
if (CanaryServiceExtensionsJsonExtensions.TryFromJson(jsonCompact, out var result))
{
    Console.WriteLine("Successfully deserialized metadata");
}
else
{
    Console.WriteLine("Failed to deserialize metadata");
}
```

## TrafficSplitterExtensionsValidation

The `TrafficSplitterExtensionsValidation` class provides validation helpers for the `TrafficSplitterExtensions` extension methods used in canary deployments. It validates parameters passed to extension methods like `CreateLinearCanaryDeployment`, `CreateExponentialCanaryDeployment`, `CreateBlueGreenCanaryDeployment`, `ShouldProceedToNextStepAsync`, and `GetCanaryPercentageNormalized`, ensuring that canary deployment configurations are valid before they are used.

This validation class helps prevent runtime errors by validating project names, versions, traffic splits, and deployment configurations against business rules and constraints.

Example usage:

```csharp
// Validate linear canary deployment parameters before creating deployment
var validationProblems = TrafficSplitterExtensionsValidation.ValidateCreateLinearCanaryDeployment(
    projectName: "MyWebApp",
    canaryVersion: "2.1.0-preview",
    stableVersion: "2.0.0",
    stepCount: 5
);

if (validationProblems.Count > 0)
{
    Console.WriteLine("Validation failed:");
    foreach (var problem in validationProblems)
    {
        Console.WriteLine($"- {problem}");
    }
    return;
}

// If validation passes, proceed with deployment
var deployment = TrafficSplitterExtensions.CreateLinearCanaryDeployment(
    projectName: "MyWebApp",
    canaryVersion: "2.1.0-preview",
    stableVersion: "2.0.0",
    stepCount: 5
);

// Validate a traffic split percentage
var split = new TrafficSplit
{
    CanaryPercent = 10.5,
    StablePercent = 89.5
};

var splitProblems = TrafficSplitterExtensionsValidation.ValidateGetCanaryPercentageNormalized(split);
bool isSplitValid = TrafficSplitterExtensionsValidation.IsValidGetCanaryPercentageNormalized(split);

// Validate canary deployment state before proceeding to next step
var healthEvaluator = new CanaryHealthEvaluator();
var deploymentProblems = TrafficSplitterExtensionsValidation.ValidateShouldProceedToNextStepAsync(
    deployment,
    healthEvaluator
);

if (deploymentProblems.Count == 0)
{
    await TrafficSplitterExtensions.ShouldProceedToNextStepAsync(deployment, healthEvaluator);
}

// Use EnsureValid methods for immediate exception throwing on validation failure
TrafficSplitterExtensionsValidation.EnsureValidCreateExponentialCanaryDeployment(
    projectName: "PaymentService",
    canaryVersion: "3.2.0-beta",
    stableVersion: "3.1.0"
);
```

## DomainEventValidation

The `DomainEventValidation` class provides validation helpers for domain events to ensure data integrity throughout the deployment notification system. It offers extension methods that validate domain events (`DomainEvent`), notification created events (`NotificationCreatedEvent`), notification processed events (`NotificationProcessedEvent`), and channel delivery failed events (`ChannelDeliveryFailedEvent`).

The validation methods check for null values, empty strings, GUID format validity, date/time ranges, and collection constraints, returning detailed error messages for any validation failures. This ensures that domain events contain valid data before being processed or stored.

Example usage:

```csharp
// Create a valid domain event
var domainEvent = new DomainEvent
{
    EventId = Guid.NewGuid().ToString(),
    AggregateId = "deployment-123",
    OccurredAt = DateTime.UtcNow
};

// Validate and get list of problems
var problems = domainEvent.Validate();
if (problems.Count > 0)
{
    Console.WriteLine("Validation failed:");
    foreach (var problem in problems)
    {
        Console.WriteLine($"- {problem}");
    }
}

// Check if valid (returns true/false)
bool isValid = domainEvent.IsValid();
Console.WriteLine($"DomainEvent is valid: {isValid}");

// Ensure valid (throws ArgumentException if invalid)
DomainEventValidation.EnsureValid(domainEvent);

// Create a valid NotificationCreatedEvent
var createdEvent = new NotificationCreatedEvent
{
    EventId = Guid.NewGuid().ToString(),
    AggregateId = "notification-456",
    OccurredAt = DateTime.UtcNow,
    NotificationId = Guid.NewGuid().ToString(),
    ProjectName = "MyApplication",
    Version = "2.0.0",
    Channels = new List<string> { "Slack", "Discord", "Telegram" }
};

// Validate notification created event
var createdProblems = createdEvent.Validate();
if (createdProblems.Count == 0)
{
    Console.WriteLine("NotificationCreatedEvent is valid!");
}

// Validate NotificationProcessedEvent
var processedEvent = new NotificationProcessedEvent
{
    EventId = Guid.NewGuid().ToString(),
    AggregateId = "notification-789",
    OccurredAt = DateTime.UtcNow,
    NotificationId = Guid.NewGuid().ToString(),
    ProjectName = "MyApplication",
    Version = "2.0.0",
    Channels = new List<string> { "Slack", "Discord" },
    Success = true
};

var processedProblems = processedEvent.Validate();
bool processedIsValid = processedEvent.IsValid();
Console.WriteLine($"NotificationProcessedEvent valid: {processedIsValid}");

// Validate ChannelDeliveryFailedEvent
var failedEvent = new ChannelDeliveryFailedEvent
{
    EventId = Guid.NewGuid().ToString(),
    AggregateId = "delivery-101",
    OccurredAt = DateTime.UtcNow,
    NotificationId = Guid.NewGuid().ToString(),
    ProjectName = "MyApplication",
    Version = "2.0.0",
    ChannelName = "Slack",
    ErrorMessage = "Webhook connection timeout",
    AttemptNumber = 1
};

var failedProblems = failedEvent.Validate();
bool failedIsValid = failedEvent.IsValid();
Console.WriteLine($"ChannelDeliveryFailedEvent valid: {failedIsValid}");

// Use EnsureValid to throw exceptions on invalid events
try
{
    DomainEventValidation.EnsureValid(createdEvent);
    Console.WriteLine("Event validation passed!");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}
```

## NotificationExceptionExtensions

The `NotificationExceptionExtensions` class provides extension methods for notification-related exceptions that enhance error handling and diagnostics. It offers functionality to format error messages, categorize exceptions, determine error severity, and extract diagnostic information for logging and monitoring systems.

This extension class is particularly useful for implementing robust error handling in notification delivery systems, allowing applications to respond appropriately to different types of failures (configuration errors, delivery failures, validation errors) and provide detailed diagnostic information to monitoring tools.

Example usage:

```csharp
// Create and throw a notification exception
try
{
    var channelConfig = new ChannelConfiguration
    {
        ChannelType = NotificationChannel.Slack,
        WebhookUrl = "https://hooks.slack.com/services/T123/B456/C789",
        TargetId = "C123456"
    };
    
    if (string.IsNullOrEmpty(channelConfig.WebhookUrl))
    {
        throw new ChannelConfigurationException(
            "Webhook URL is required for Slack channel",
            NotificationChannel.Slack,
            "C123456"
        );
    }
}
catch (NotificationException ex)
{
    // Format the error message for user-friendly display
    string formattedMessage = ex.ToFormattedErrorMessage();
    Console.WriteLine($"Error: {formattedMessage}");
    
    // Check if it's a configuration error (can be fixed by updating config)
    bool isConfigError = ex.IsConfigurationError();
    if (isConfigError)
    {
        Console.WriteLine("Configuration error detected - check channel settings");
    }
    
    // Check if it's a delivery failure (might succeed on retry)
    bool isDeliveryFailure = ex.IsDeliveryFailure();
    if (isDeliveryFailure)
    {
        Console.WriteLine("Delivery failure - may succeed on retry");
    }
    
    // Check if it's a validation error (requires data fix)
    bool isValidationError = ex.IsValidationError();
    if (isValidationError)
    {
        Console.WriteLine("Validation error - check input data");
    }
    
    // Get error category for monitoring/alerting systems
    string errorCategory = ex.GetErrorCategory();
    Console.WriteLine($"Error category: {errorCategory}");
    
    // Get severity level (0-100) for prioritization
    int severityLevel = ex.GetSeverityLevel();
    Console.WriteLine($"Severity level: {severityLevel}");
    
    // Extract diagnostic information for logging
    var diagnostics = ex.GetDiagnosticInfo();
    foreach (var kvp in diagnostics)
    {
        Console.WriteLine($"{kvp.Key}: {kvp.Value}");
    }
}

// Example with webhook delivery failure
try
{
    throw new WebhookDeliveryException(
        "Failed to deliver notification to Discord",
        NotificationChannel.Discord,
        3,
        403
    );
}
catch (NotificationException ex)
{
    Console.WriteLine($"\nWebhook delivery error: {ex.ToFormattedErrorMessage()}");
    Console.WriteLine($"Is delivery failure: {ex.IsDeliveryFailure()}");
    Console.WriteLine($"Error category: {ex.GetErrorCategory()}");
    Console.WriteLine($"Severity level: {ex.GetSeverityLevel()}");
    
    var diagnostics = ex.GetDiagnosticInfo();
    Console.WriteLine($"\nDiagnostics:");
    Console.WriteLine($"  Channel: {diagnostics["Channel"]}");
    Console.WriteLine($"  Attempts: {diagnostics["Attempts"]}");
    Console.WriteLine($"  LastStatusCode: {diagnostics["LastStatusCode"]}");
}
```

## ServiceExtensionsMetadataJsonExtensions

The `ServiceExtensionsMetadataJsonExtensions` class provides JSON serialization and deserialization utilities for `ServiceExtensions` type information. It enables converting service extension metadata to and from JSON format with configurable formatting options, supporting both compact and indented output formats.

This extension class is particularly useful for persisting service extension configuration metadata to configuration files, databases, or remote services, and for restoring it back into application memory. It provides three main methods: `ToJson()` for serialization, `FromJson()` for deserialization, and `TryFromJson()` for safe deserialization with error handling.

## DeploymentHistoryEntryExtensions

The `DeploymentHistoryEntryExtensions` class provides extension methods for the `DeploymentHistoryEntry` type that enable common operations on deployment history records without modifying the original class. It includes methods for checking deployment status, working with tags, calculating durations, and filtering by time windows or environments.

These extension methods are particularly useful for deployment monitoring, rollback analysis, and historical reporting, providing a clean API for querying deployment history data.

Example usage:

```csharp
// Serialize ServiceExtensions metadata to JSON string (compact format)
string jsonCompact = ServiceExtensionsJsonExtensions.ToJson();
Console.WriteLine(jsonCompact);
// Output: {"type":"ServiceExtensions","namespace":"DotNetDeployNotify.Infrastructure","assembly":"DotNetDeployNotify","methods":["IsCritical","IsProduction","SupportsStatus","SupportsEnvironment","GetDescription","MergeMetadata","Clone","ToCompactString","GetSeverityLevel","ShouldRetry","GetRetryDelay"]}

// Serialize to JSON string (indented format)
string jsonIndented = ServiceExtensionsJsonExtensions.ToJson(indented: true);
Console.WriteLine(jsonIndented);
/* Output:
{
  "type": "ServiceExtensions",
  "namespace": "DotNetDeployNotify.Infrastructure",
  "assembly": "DotNetDeployNotify",
  "methods": [
    "IsCritical",
    "IsProduction",
    "SupportsStatus",
    "SupportsEnvironment",
    "GetDescription",
    "MergeMetadata",
    "Clone",
    "ToCompactString",
    "GetSeverityLevel",
    "ShouldRetry",
    "GetRetryDelay"
  ]
}
*/

// Deserialize from JSON string
var deserializedMetadata = ServiceExtensionsJsonExtensions.FromJson(jsonCompact);
if (deserializedMetadata != null)
{
Console.WriteLine($"Deserialized type: {deserializedMetadata.Type}");
Console.WriteLine($"Namespace: {deserializedMetadata.Namespace}");
Console.WriteLine($"Assembly: {deserializedMetadata.Assembly}");
Console.WriteLine($"Methods count: {deserializedMetadata.Methods?.Length ?? 0}");
}

// Try deserialization with error handling
if (ServiceExtensionsJsonExtensions.TryFromJson(jsonCompact, out var result))
{
Console.WriteLine("Successfully deserialized metadata");
}
else
{
Console.WriteLine("Failed to deserialize metadata");
}
```

## DeploymentHistoryEntryExtensions

The `DeploymentHistoryEntryExtensions` class provides extension methods for the `DeploymentHistoryEntry` type that enable common operations on deployment history records without modifying the original class. It includes methods for checking deployment status, working with tags, calculating durations, and filtering by time windows or environments.

These extension methods are particularly useful for deployment monitoring, rollback analysis, and historical reporting, providing a clean API for querying deployment history data.

Example usage:

```csharp
// Create a deployment history entry with tags and duration
var deploymentEntry = new DeploymentHistoryEntry
{
    ProjectName = "MyWebApp",
    Version = "2.0.0",
    TargetEnvironment = Environment.Production,
    DeployedAt = DateTime.UtcNow.AddMinutes(-30),
    FinalStatus = BuildStatus.Success,
    DurationSeconds = 125,
    Tags = new Dictionary<string, string>
    {
        {"build_number", "42"},
        {"triggered_by", "ci-pipeline"},
        {"canary", "true"}
    },
    IsRollback = false
};

// Check if deployment is within the last hour
bool isRecent = deploymentEntry.IsWithinTimeWindow(TimeSpan.FromHours(1));
Console.WriteLine($"Is within last hour: {isRecent}"); // true

// Check if deployment is within a specific time window relative to a reference time
var referenceTime = DateTime.UtcNow;
bool isInWindow = deploymentEntry.IsWithinTimeWindow(referenceTime, TimeSpan.FromHours(2));
Console.WriteLine($"Is within 2 hours of reference time: {isInWindow}");

// Check if deployment has a specific tag
bool hasCanaryTag = deploymentEntry.HasTag("canary");
Console.WriteLine($"Has canary tag: {hasCanaryTag}"); // true

// Get the value of a specific tag
string? buildNumber = deploymentEntry.GetTagValue("build_number");
Console.WriteLine($"Build number: {buildNumber}"); // "42"

// Get deployment duration as TimeSpan
TimeSpan? duration = deploymentEntry.GetDuration();
Console.WriteLine($"Duration: {duration?.TotalSeconds} seconds"); // 125

// Check if deployment was successful
bool isSuccessful = deploymentEntry.IsSuccessful();
Console.WriteLine($"Is successful: {isSuccessful}"); // true

// Check if deployment failed
bool isFailed = deploymentEntry.IsFailed();
Console.WriteLine($"Is failed: {isFailed}"); // false

// Get formatted duration string
string formattedDuration = deploymentEntry.GetFormattedDuration();
Console.WriteLine($"Formatted duration: {formattedDuration}"); // "2m 5s"

// Check if this is a rollback deployment
bool isRollback = deploymentEntry.IsRollback();
Console.WriteLine($"Is rollback: {isRollback}"); // false

// Get status summary
string statusSummary = deploymentEntry.GetStatusSummary();
Console.WriteLine($"Status summary: {statusSummary}"); // "SUCCESS"

// Check if deployment is in a specific environment
bool isInProduction = deploymentEntry.IsInEnvironment(Environment.Production);
Console.WriteLine($"Is in production: {isInProduction}"); // true

// Get all tags as a read-only dictionary
IReadOnlyDictionary<string, string> tags = deploymentEntry.GetTags();
foreach (var tag in tags)
{
    Console.WriteLine($"{tag.Key}: {tag.Value}");
}
```

## SearchCriteriaExtensionsJsonExtensions

The `SearchCriteriaExtensionsJsonExtensions` class provides JSON serialization and deserialization utilities for `SearchCriteriaExtensionsMetadata` objects. It enables converting search criteria extension metadata to and from JSON format with configurable formatting options, supporting both compact and indented output formats.

This extension class is particularly useful for persisting search criteria extension configuration metadata to configuration files, databases, or remote services, and for restoring it back into application memory. It provides three main methods: `ToJson()` for serialization, `FromJson()` for deserialization, and `TryFromJson()` for safe deserialization with error handling.

Example usage:

```csharp
// Serialize SearchCriteriaExtensions metadata to JSON string (compact format)
string jsonCompact = SearchCriteriaExtensionsJsonExtensions.ToJson();
Console.WriteLine(jsonCompact);
// Output: {"type":"SearchCriteriaExtensions","namespace":"DotNetDeployNotify.Search","assembly":"DotNetDeployNotify","methods":["Search","Find","Filter","GetResults"]}

// Serialize to JSON string (indented format)
string jsonIndented = SearchCriteriaExtensionsJsonExtensions.ToJson(indented: true);
Console.WriteLine(jsonIndented);
/* Output:
{
  "type": "SearchCriteriaExtensions",
  "namespace": "DotNetDeployNotify.Search",
  "assembly": "DotNetDeployNotify",
  "methods": [
    "Search",
    "Find",
    "Filter",
    "GetResults"
  ]
}
*/

// Deserialize from JSON string
var deserializedMetadata = SearchCriteriaExtensionsJsonExtensions.FromJson(jsonCompact);
if (deserializedMetadata != null)
{
  Console.WriteLine($"Deserialized type: {deserializedMetadata.Type}");
  Console.WriteLine($"Namespace: {deserializedMetadata.Namespace}");
  Console.WriteLine($"Assembly: {deserializedMetadata.Assembly}");
  Console.WriteLine($"Methods count: {deserializedMetadata.Methods?.Length ?? 0}");
}

// Try deserialization with error handling
if (SearchCriteriaExtensionsJsonExtensions.TryFromJson(jsonCompact, out var result))
{
  Console.WriteLine("Successfully deserialized metadata");
}
else
{
  Console.WriteLine("Failed to deserialize metadata");
}
```

## ChannelConfigurationExtensions

The `ChannelConfigurationExtensions` class provides extension methods for `ChannelConfiguration` instances that enhance their functionality. These methods enable operations like creating a deep copy of a channel configuration, checking if a configuration is valid for a specific environment or status, and getting human-readable representations of configuration properties.

Example usage:
```csharp
var config = new ChannelConfiguration
{
    ChannelType = NotificationChannel.Slack,
    WebhookUrl = "https://hooks.slack.com/services/T123/B456/C789",
    TargetId = "C123456"
};

// Create a deep copy of the configuration
var copy = config.DeepCopy();
Console.WriteLine($"Copy created: {copy.WebhookUrl}");

// Check if configuration is valid for production environment
bool isValid = config.IsEnvironmentAllowed(Environment.Production);
Console.WriteLine($"Is valid for production: {isValid}");

// Get human-readable channel type
string channelType = config.GetChannelTypeDisplay();
Console.WriteLine($"Channel type: {channelType}");
```

## CacheEntry

The `CacheEntry<T>` class represents a single cache entry in the in-memory cache system. It stores a cached value along with metadata such as expiration time and creation timestamp, enabling time-to-live (TTL) functionality for cache entries.

Each cache entry tracks when it was created, when it expires, and whether it has expired, providing the foundation for the cache's automatic expiration and cleanup system.

Example usage:

```csharp
// Create a cache entry with a string value
var cacheEntry = new CacheEntry<string>
{
    Value = "Hello, World!",
    ExpiresAt = DateTime.UtcNow.AddMinutes(10),
    CreatedAt = DateTime.UtcNow
};

Console.WriteLine($"Cache entry created at: {cacheEntry.CreatedAt:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine($"Expires at: {cacheEntry.ExpiresAt:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine($"Is expired: {cacheEntry.IsExpired}");

// Using with MemoryCacheService
var logger = new Logger<MemoryCacheService>(new LoggerFactory());
var cacheService = new MemoryCacheService(logger);

// Set a value in cache (automatically creates a CacheEntry internally)
cacheService.Set("greeting", "Hello, World!", TimeSpan.FromMinutes(10));

// Retrieve from cache
var cachedValue = cacheService.Get<string>("greeting");
if (cachedValue != null)
{
    Console.WriteLine($"Retrieved value: {cachedValue}");
}

// Get cache statistics
var stats = cacheService.GetStatistics();
Console.WriteLine($"Cache contains {stats.TotalItems} items");
Console.WriteLine($"Cache hit rate: {stats.HitRate:F1}%");
```

## RetryPolicy

The `RetryPolicy` class provides configuration for retrying failed operations with exponential backoff. It supports customizable retry parameters including maximum attempts, initial delay, backoff multiplier, maximum delay, and custom retry conditions. This policy is used by the `RetryHelper` class to implement resilient operation execution.

Example usage:

```csharp
// Create a retry policy with custom settings
var retryPolicy = new RetryPolicy
{
    MaxAttempts = 5,
    InitialDelay = TimeSpan.FromMilliseconds(200),
    BackoffMultiplier = 2.5,
    MaxDelay = TimeSpan.FromSeconds(60),
    ShouldRetry = ex => ex is not HttpRequestException httpEx || httpEx.StatusCode != System.Net.HttpStatusCode.TooManyRequests
};

// Use with RetryHelper
var retryHelper = new RetryHelper(logger);
var result = await retryHelper.ExecuteAsync(async () =>
{
    var response = await httpClient.GetAsync("https://api.example.com/data");
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsStringAsync();
}, retryPolicy);

Console.WriteLine($"Successfully retrieved data after retries: {result.Length} bytes");
```

## ChannelConfigurationBuilder

The `ChannelConfigurationBuilder` class provides a fluent interface for constructing `ChannelConfiguration` instances with a clean, readable API. It enables programmatic configuration of notification channels including Slack, Discord, and Telegram webhooks with support for filtering by environment, status, priority, and other channel-specific settings.

The builder supports method chaining to create complex channel configurations in a single expression, and provides factory methods for common channel types (Slack, Discord, Telegram).

Example usage:

```csharp
// Create a Slack channel configuration using the builder
var slackConfig = ChannelConfigurationBuilder.ForSlack()
    .WithName("Production Slack Alerts")
    .WithWebhook("https://hooks.slack.com/services/T123/B456/C789")
    .WithTargetId("C123456")
    .WithTimeout(5000)
    .WithRetries(3)
    .WithMinimumPriority(NotificationPriority.High)
    .IncludeCommitDetails()
    .IncludeBuildUrl()
    .OnlyProduction()
    .UseSlackBlockKit()
    .EnableEmojis()
    .Build();

Console.WriteLine($"Created Slack configuration: {slackConfig.DisplayName}");

// Create a Discord channel configuration with environment filtering
var discordConfig = ChannelConfigurationBuilder.ForDiscord()
    .WithName("Development Discord")
    .WithWebhook("https://discord.com/api/webhooks/789/abc")
    .WithTargetId("D789012")
    .WithTimeout(8000)
    .WithRetries(5)
    .AllowEnvironments(Environment.Development, Environment.Staging)
    .OnlyOnSuccess()
    .Build();

Console.WriteLine($"Created Discord configuration: {discordConfig.DisplayName}");

// Create a Telegram channel configuration with status filtering
var telegramConfig = ChannelConfigurationBuilder.ForTelegram()
    .WithName("Critical Alerts Telegram")
    .WithWebhook("https://api.telegram.org/bot12345:ABC-DEF/sendMessage")
    .WithTargetId("T987654")
    .WithTimeout(10000)
    .WithRetries(2)
    .WithMinimumPriority(NotificationPriority.Critical)
    .OnlyOnFailure()
    .Build();

Console.WriteLine($"Created Telegram configuration: {telegramConfig.DisplayName}");

// Use the configuration with a notification service
var channelConfigurations = new List<ChannelConfiguration> { slackConfig, discordConfig, telegramConfig };
```

## CanaryOptions

The `CanaryOptions` class configures canary deployment monitoring and rollback behavior. It defines thresholds for error rates, latency metrics, and deployment progression settings that determine when a canary deployment should automatically roll back or advance to the next stage.

Example usage:

```csharp
// Configure CanaryOptions in appsettings.json
{
  "CanaryOptions": {
    "Enabled": true,
    "AutoRollbackOnFailure": true,
    "AutoAdvanceOnSuccess": true,
    "LinearStepCount": 5,
    "StepSoakDuration": "00:05:00",
    "MaxDeploymentDuration": "02:00:00",
    "Thresholds": {
      "MaxErrorRatePercent": 1.5,
      "MaxP95LatencyMs": 500,
      "MaxP99LatencyMs": 1000
    },
    "AlertPriority": "High",
    "ErrorRateMultiplier": 2.0,
    "LatencyDegradationPercent": 30.0
  }
}

// Or configure programmatically in Program.cs
builder.Services.Configure<CanaryOptions>(options =>
{
  options.Enabled = true;
  options.AutoRollbackOnFailure = true;
  options.AutoAdvanceOnSuccess = true;
  options.LinearStepCount = 5;
  options.StepSoakDuration = TimeSpan.FromMinutes(5);
  options.MaxDeploymentDuration = TimeSpan.FromHours(2);
  
  options.Thresholds = new CanaryThresholds
  {
    MaxErrorRatePercent = 1.5,
    MaxP95LatencyMs = 500,
    MaxP99LatencyMs = 1000
  };
  
  options.AlertPriority = NotificationPriority.High;
  options.ErrorRateMultiplier = 2.0;
  options.LatencyDegradationPercent = 30.0;
});

// Usage in a deployment service
public class CanaryDeploymentService
{
  private readonly CanaryOptions _options;
  
  public CanaryDeploymentService(IOptions<CanaryOptions> options)
  {
    _options = options.Value;
  }
  
  public bool ShouldRollback(int errorCount, int totalRequests, double p95LatencyMs, double p99LatencyMs)
  {
    if (!_options.Enabled || !_options.AutoRollbackOnFailure)
      return false;
    
    var errorRate = (double)errorCount / totalRequests * 100;
    return errorRate > _options.Thresholds.MaxErrorRatePercent * _options.ErrorRateMultiplier ||
           p95LatencyMs > _options.Thresholds.MaxP95LatencyMs ||
           p99LatencyMs > _options.Thresholds.MaxP99LatencyMs;
  }
  
  public bool ShouldAdvance()
  {
    return _options.Enabled && _options.AutoAdvanceOnSuccess;
  }
}
```
```

## GuardExtensions

The `GuardExtensions` class provides a comprehensive set of guard clause extension methods for validating method parameters and preventing null reference exceptions. These methods enable defensive programming by throwing descriptive exceptions when validation rules are violated, ensuring consistent validation patterns throughout the application.

Example usage:

```csharp
// Validate method parameters with clear error messages
public void ProcessDeployment(string projectName, string version, int timeoutSeconds)
{
    projectName.ThrowIfNullOrEmpty("Project name is required");
    version.ThrowIfNullOrEmpty("Version is required");
    timeoutSeconds.ThrowIfLessThan(1000, "Timeout must be at least 1000 milliseconds");

    // Process deployment logic...
}

// Validate collections and strings
public void SendNotification(string webhookUrl, IEnumerable<string> channels)
{
    webhookUrl.ThrowIfInvalidUrl("Invalid webhook URL format");
    channels.ThrowIfNullOrEmpty("At least one notification channel is required");
    
    // Send notification logic...
}

// Validate business rules
public void DeployToEnvironment(string environment)
{
    environment.ThrowIfNullOrEmpty("Environment name is required");
    environment.ThrowIfLongerThan(20, "Environment name must be 20 characters or less");
    
    if (!environment.IsInRange("dev", "staging", "production"))
    {
        throw new ArgumentException("Environment must be one of: dev, staging, production");
    }

    // Deployment logic...
}

// Get values safely with fallback
public string GetConfigValue(string key)
{
    var value = _config[key].GetValueOrThrow($"Configuration key '{key}' not found");
    return value.MatchesPattern("^[a-zA-Z0-9_-]+$")
        ? value
        : throw new FormatException("Configuration value contains invalid characters");
}
```

## MathExtensions

The `MathExtensions` class provides a comprehensive set of extension methods for mathematical operations, unit conversions, and statistical calculations. It includes generic methods for value clamping and range checking, percentage calculations, rounding operations, and collection statistics like average and median calculations. The class also provides human-readable formatting for file sizes and time durations, plus financial calculations like compound interest.

Example usage:

```csharp
// Clamp a value between minimum and maximum
int clampedValue = 15.Clamp(10, 20); // Returns 15
int clampedLow = 5.Clamp(10, 20);   // Returns 10
int clampedHigh = 25.Clamp(10, 20);  // Returns 20

// Check if a value is between two bounds
bool isBetween = 15.IsBetween(10, 20); // Returns true
bool isOutside = 5.IsBetween(10, 20);   // Returns false

// Calculate percentages
int successCount = 85;
int totalCount = 100;
double successRate = successCount.ToPercentage(totalCount); // Returns 85.0

// Round to specific decimal places
decimal roundedMoney = 123.4567m.RoundTo(2); // Returns 123.46
double roundedValue = 3.14159.RoundTo(3);    // Returns 3.142

// Calculate statistics from collections
var numbers = new List<int> { 10, 20, 30, 40, 50 };
double average = numbers.Average(); // Returns 30.0
double median = numbers.Median();   // Returns 30.0
int sum = numbers.SafeSum();       // Returns 150

// Convert bytes to human-readable format
long fileSize = 1572864; // 1.5MB
string readableSize = fileSize.ToHumanReadableSize(); // Returns "1.5 MB"

// Convert milliseconds to human-readable duration
int durationMs = 125000;
string readableDuration = durationMs.ToHumanReadableDuration(); // Returns "2.08m"

// Calculate compound interest
decimal principal = 1000m;
decimal rate = 0.05m; // 5%
int periods = 12;
decimal futureValue = principal.CalculateCompoundInterest(rate, periods); // Returns ~1795.86

// Generate random numbers
var random = new Random();
int randomNumber = random.RandomBetween(1, 100); // Returns random integer between 1-100
```

## CollectionExtensions

The `CollectionExtensions` class provides a comprehensive set of extension methods for working with collections and enumerables in a functional and efficient way. These methods enable common collection operations like adding items conditionally, batch processing, partitioning, and statistical analysis without modifying the original collections.

The extension methods support various collection types including `ICollection<T>`, `IList<T>`, and `IEnumerable<T>`, providing safe operations that handle null values and edge cases appropriately.

Example usage:

```csharp
// Create a list of deployment notifications
var notifications = new List<DeploymentNotification>
{
    new DeploymentNotification { ProjectName = "App1", Status = DeploymentStatus.Success },
    new DeploymentNotification { ProjectName = "App2", Status = DeploymentStatus.Failed },
    new DeploymentNotification { ProjectName = "App3", Status = DeploymentStatus.Success }
};

// Add items conditionally
notifications.AddIfNotExists(new DeploymentNotification { ProjectName = "App1", Status = DeploymentStatus.Success });
notifications.AddRange(new[] { 
    new DeploymentNotification { ProjectName = "App4", Status = DeploymentStatus.Success },
    new DeploymentNotification { ProjectName = "App5", Status = DeploymentStatus.Success }
});

// Remove failed notifications
int removedCount = notifications.RemoveWhere(n => n.Status == DeploymentStatus.Failed);

// Split into batches for parallel processing
var batches = notifications.Chunk(2);
foreach (var batch in batches)
{
    Console.WriteLine($"Processing batch of {batch.Count} notifications");
}

// Partition into successful and failed notifications
var (successful, failed) = notifications.Partition(n => n.Status == DeploymentStatus.Success);
Console.WriteLine($"Successful: {successful.Count}, Failed: {failed.Count}");

// Get item at specific index safely
var firstNotification = notifications.GetAtIndexOrDefault(0);
var outOfRange = notifications.GetAtIndexOrDefault(100); // returns null

// Check if collection has items
bool hasItems = notifications.HasItems();
bool isEmpty = notifications.IsNullOrEmpty();

// Convert to comma-separated string
string projectNames = notifications.Select(n => n.ProjectName).ToCommaSeparatedString();

// Get random notification
var randomNotification = notifications.GetRandom();

// Shuffle notifications randomly
notifications.Shuffle();

// Count notifications by status
var statusCounts = notifications.CountBy(n => n.Status);
foreach (var kvp in statusCounts)
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
}

// Distinct notifications by project name
var distinctProjects = notifications.DistinctBy(n => n.ProjectName);
```

## EnumExtensions

The `EnumExtensions` class provides a comprehensive set of extension methods for working with enum types in .NET applications. These methods enable common enum operations like retrieving description attributes, checking flags, parsing strings to enums, generating human-readable names, and working with enum values in collections.

The extension methods support any enum type and provide safe operations that handle null values and edge cases appropriately.

Example usage:

```csharp
// Define an enum with Description attributes
public enum DeploymentStatus
{
    [System.ComponentModel.Description("Build started")]
    Started,
    
    [System.ComponentModel.Description("Build completed successfully")]
    Success,
    
    [System.ComponentModel.Description("Build failed")]
    Failed
}

// Get description from enum value
DeploymentStatus status = DeploymentStatus.Success;
string description = status.GetDescription(); // "Build completed successfully"

// Check if enum has a specific flag (for flag enums)
[System.Flags]
public enum NotificationChannels
{
    None = 0,
    Slack = 1,
    Discord = 2,
    Telegram = 4,
    All = Slack | Discord | Telegram
}

var channels = NotificationChannels.Slack | NotificationChannels.Discord;
bool hasSlack = channels.HasFlag(NotificationChannels.Slack); // true
bool hasTelegram = channels.HasFlag(NotificationChannels.Telegram); // false

// Get all values of an enum
List<DeploymentStatus> allStatuses = EnumExtensions.GetAllValues<DeploymentStatus>();
foreach (var status in allStatuses)
{
    Console.WriteLine($"Status: {status} - {status.GetDescription()}");
}

// Convert enum to human-readable string
DeploymentStatus status = DeploymentStatus.Success;
string humanReadable = status.ToHumanReadable(); // "Success"

// Safely parse string to enum
string statusText = "Success";
DeploymentStatus? parsedStatus = EnumExtensions.TryParse<DeploymentStatus>(statusText);
if (parsedStatus.HasValue)
{
    Console.WriteLine($"Parsed status: {parsedStatus.Value}");
}

// Get random enum value
DeploymentStatus randomStatus = EnumExtensions.GetRandomValue<DeploymentStatus>();
Console.WriteLine($"Random status: {randomStatus}");

// Check if enum value is in a list
var validStatuses = new[] { DeploymentStatus.Started, DeploymentStatus.Success };
bool isValid = status.IsIn(validStatuses); // true
```

## ResultTests

The `ResultTests` class provides comprehensive unit tests for the `Result<T>` functional error handling type. These tests verify the core functionality of creating successful and failed results, mapping operations, error handling, and exception safety through the `Try` pattern. The test suite ensures that the `Result<T>` type behaves correctly for all expected use cases in the deployment notification system.

Example usage:

```csharp
// Test successful result creation and value access
var successResult = Result<int>.Ok(42);
Assert.True(successResult.IsSuccess);
Assert.Equal(42, successResult.Value);
Assert.Null(successResult.Error);

// Test failure result creation and error handling
var failureResult = Result<string>.Fail("Webhook delivery failed");
Assert.False(failureResult.IsSuccess);
Assert.Equal("Webhook delivery failed", failureResult.Error);
Assert.Null(failureResult.Value);

// Test mapping operations
var mappedResult = Result<int>.Ok(5).Map(x => x * 10);
Assert.True(mappedResult.IsSuccess);
Assert.Equal(50, mappedResult.Value);

// Test error propagation (mapper should not be invoked)
var errorResult = Result<int>.Fail("Original error");
bool mapperInvoked = false;
var propagatedResult = errorResult.Map(x => { mapperInvoked = true; return x.ToString(); });
Assert.False(propagatedResult.IsSuccess);
Assert.Equal("Original error", propagatedResult.Error);
Assert.False(mapperInvoked);

// Test exception handling with Try
var exceptionResult = ResultExtensions.Try<int>(() => throw new InvalidOperationException("channel unavailable"));
Assert.False(exceptionResult.IsSuccess);
Assert.Equal("channel unavailable", exceptionResult.Error);

// Test successful Try operation
var successTryResult = ResultExtensions.Try(() => 99);
Assert.True(successTryResult.IsSuccess);
Assert.Equal(99, successTryResult.Value);
```

## StringExtensionsTests

The `StringExtensionsTests` class provides comprehensive unit tests for the `StringExtensions` utility methods. These tests verify that string manipulation functions like truncation, slug generation, boolean conversion, substring counting, and sensitive data masking work correctly for various input scenarios including edge cases like null values, empty strings, and whitespace.

Example usage:

```csharp
// Test truncation behavior with various inputs
var longMessage = "This is a very long deployment message that exceeds the limit";
var truncated = longMessage.Truncate(20);
Assert.Equal("This is a very lo...", truncated);
Assert.Equal(20, truncated.Length);

// Test null/empty string handling
string? nullMessage = null;
var emptyResult = nullMessage!.Truncate(10);
Assert.Empty(emptyResult);

// Test slug generation
var projectName = "My Deploy Project";
var slug = projectName.ToSlug();
Assert.Equal("my-deploy-project", slug);
Assert.DoesNotContain(" ", slug);

// Test boolean conversion with various string representations
Assert.True("1".ToBooleanSafe());
Assert.True("true".ToBooleanSafe());
Assert.True("yes".ToBooleanSafe());
Assert.False("0".ToBooleanSafe());
Assert.False("false".ToBooleanSafe());
Assert.False("no".ToBooleanSafe());

// Test substring counting
var message = "deploy success deploy failed deploy retry";
var count = message.CountOccurrences("deploy");
Assert.Equal(3, count);

// Test sensitive data masking
var shortToken = "abc";
var maskedShort = shortToken.MaskSensitive(visibleChars: 4);
Assert.Equal("****", maskedShort);

var longToken = "secret-api-token-12345";
var maskedLong = longToken.MaskSensitive(visibleChars: 4);
Assert.StartsWith("secr", maskedLong);
Assert.Contains("*", maskedLong);
Assert.Equal(longToken.Length, maskedLong.Length);
```

## NotificationTests

The `NotificationTests` class provides comprehensive unit tests for the notification builder and related notification functionality. These tests verify that notifications are correctly constructed with required fields, handle different statuses and priorities appropriately, validate channel configurations, and properly track delivery results. The test suite covers all public members of the notification system including `NotificationBuilder`, `DeploymentNotification`, `ChannelConfiguration`, `NotificationResult`, and `IValidationService`.

Example usage:

```csharp
// Create a notification using the builder pattern with all required fields
var notification = new NotificationBuilder()
    .WithProject("ApiGateway", "3.1.0")
    .WithStatus(BuildStatus.Success, "All checks passed")
    .WithBranch("main", "abc1234", "v.zaiets")
    .WithChannels(NotificationChannel.Slack, NotificationChannel.Telegram)
    .WithEnvironment(Environment.Production)
    .Build();

Console.WriteLine($"Created notification for {notification.ProjectName} v{notification.Version}");
Console.WriteLine($"Status: {notification.Status}, Priority: {notification.Priority}");
Console.WriteLine($"Channels: {string.Join(", ", notification.Channels)}");

// Create a failure notification that automatically sets critical priority
var failureNotification = new NotificationBuilder()
    .WithProject("PaymentService", "1.5.0")
    .WithBranch("hotfix/payment-crash")
    .WithChannels(NotificationChannel.Telegram)
    .AsFailure()
    .Build();

Console.WriteLine($"Failure notification - Status: {failureNotification.Status}, Priority: {failureNotification.Priority}");

// Create a deployment success notification with high priority
var successNotification = new NotificationBuilder()
    .WithProject("CatalogService", "2.0.0")
    .WithBranch("release/2.0")
    .WithChannels(NotificationChannel.Discord)
    .AsDeploymentSuccess()
    .Build();

Console.WriteLine($"Success notification - Status: {successNotification.Status}, Priority: {successNotification.Priority}");

// Test channel configuration filtering
var config = new ChannelConfiguration
{
    IsEnabled = true,
    DisplayName = "Production Slack",
    WebhookUrl = "https://hooks.slack.com/services/T123/B456/C789",
    MinimumPriority = NotificationPriority.High
};

var lowPriorityNotification = new DeploymentNotification
{
    Priority = NotificationPriority.Low,
    Status = BuildStatus.Success
};

bool shouldSendLow = config.ShouldSendNotification(lowPriorityNotification); // Returns false

var highPriorityNotification = new DeploymentNotification
{
    Priority = NotificationPriority.High,
    Status = BuildStatus.Success
};

bool shouldSendHigh = config.ShouldSendNotification(highPriorityNotification); // Returns true

// Test notification result tracking
var result = new NotificationResult
{
    NotificationId = Guid.NewGuid().ToString(),
    ConfigurationId = "cfg-slack-prod",
    Channel = NotificationChannel.Slack,
    DurationMs = 142
};

// Mark as successful
result.MarkAsSuccessful(200, "{\"ok\":true}");
Console.WriteLine($"Delivery status: {result.Status}, HTTP: {result.HttpStatusCode}");

// Mark as failed
result.MarkAsFailed("Connection refused", "HttpRequestException", 503);
Console.WriteLine($"Error: {result.ErrorMessage}, Exception: {result.ExceptionType}");

// Test metadata handling
var deploymentNotification = new DeploymentNotification();
deploymentNotification.SetMetadata("build_number", 42);
deploymentNotification.SetMetadata("triggered_by", "ci-pipeline");

int buildNumber = deploymentNotification.GetMetadata<int>("build_number");
string triggeredBy = deploymentNotification.GetMetadata<string>("triggered_by");

// Test delivery attempt tracking
deploymentNotification.IncrementDeliveryAttempt();
deploymentNotification.IncrementDeliveryAttempt();
Console.WriteLine($"Delivery attempts: {deploymentNotification.DeliveryAttempts}");

// Mark as processed
deploymentNotification.MarkAsProcessed();
Console.WriteLine($"Is processed: {deploymentNotification.IsProcessed}");

// Mock validation service for testing
var mockValidation = new Mock<IValidationService>();
mockValidation
    .Setup(s => s.ValidateNotification(It.IsAny<DeploymentNotification>()))
    .Returns(ValidationResult.Failure("Project name is required", "Version is required"));

var incompleteNotification = new DeploymentNotification();
var validationResult = mockValidation.Object.ValidateNotification(incompleteNotification);
Console.WriteLine($"Validation errors: {string.Join(", ", validationResult.Errors)}");
```

## PayloadBuilderTests

The `PayloadBuilderTests` class provides comprehensive unit tests for the `PayloadBuilder` class, which constructs notification payloads for different messaging channels (Slack, Discord, Telegram). These tests verify that payloads are correctly formatted for each channel type, include appropriate event types based on deployment status, and respect configuration options like emoji formatting, commit details inclusion, and build URL display.

The test suite covers all public methods of the `PayloadBuilder` class including channel-specific payload building methods, message formatting with various deployment statuses, and configuration-dependent formatting options.

Example usage:

```csharp
// Create payload builder with logger
var logger = new Logger<PayloadBuilder>(new LoggerFactory());
var payloadBuilder = new PayloadBuilder(logger);

// Create a deployment notification with all possible fields
var notification = new DeploymentNotification
{
    ProjectName = "MyApplication",
    Version = "2.0.0",
    Status = BuildStatus.Success,
    TargetEnvironment = Environment.Production,
    BranchName = "main",
    CommitHash = "abc123def456",
    CommitAuthor = "vlad",
    Message = "Version 2.0.0 deployed successfully",
    DurationSeconds = 180,
    BuildUrl = "https://ci.example.com/build/123",
    Priority = NotificationPriority.High,
    Channels = new List<NotificationChannel> { NotificationChannel.Slack, NotificationChannel.Discord }
};

// Create channel configurations
var slackConfig = new ChannelConfiguration
{
    ChannelType = NotificationChannel.Slack,
    DisplayName = "Production Slack",
    WebhookUrl = "https://hooks.slack.com/services/T123/B456/C789",
    TargetId = "C123456",
    EnableEmojis = true,
    UseSlackBlockKit = true,
    IncludeCommitDetails = true,
    IncludeBuildUrl = true
};

var telegramConfig = new ChannelConfiguration
{
    ChannelType = NotificationChannel.Telegram,
    DisplayName = "Production Telegram",
    WebhookUrl = "https://api.telegram.org/bot12345:ABC-DEF/sendMessage",
    TargetId = "-123456",
    EnableEmojis = true,
    IncludeCommitDetails = true,
    IncludeBuildUrl = true
};

// Build payloads for different channels
var webhookPayload = payloadBuilder.BuildPayload(notification, slackConfig);
var telegramMessage = payloadBuilder.BuildTelegramMessage(notification, telegramConfig);
var slackPayload = payloadBuilder.BuildSlackPayload(notification, slackConfig);
var discordPayload = payloadBuilder.BuildDiscordPayload(notification, slackConfig);

Console.WriteLine($"Telegram message length: {telegramMessage.Length} characters");
Console.WriteLine($"Slack payload type: {slackPayload.GetType().Name}");
Console.WriteLine($"Discord payload type: {discordPayload.GetType().Name}");
```

## ValidationServiceTests

The `ValidationServiceTests` class provides comprehensive unit tests for the `ValidationService` class, which validates deployment notifications and channel configurations before they are sent through various notification channels (Slack, Discord, Telegram, etc.). These tests verify that validation correctly identifies missing required fields, invalid URLs, negative values, and other validation rules, ensuring data integrity throughout the notification system.

The test suite covers all public validation methods including notification validation, channel configuration validation, URL validation, and email validation, with comprehensive test cases for both valid and invalid scenarios.

Example usage:

```csharp
// Create validation service
var validationService = new ValidationService();

// Validate a deployment notification with all required fields
var notification = new DeploymentNotification
{
    ProjectName = "MyApplication",
    Version = "2.0.0",
    BranchName = "main",
    Message = "Version 2.0.0 deployed successfully",
    Channels = new List<NotificationChannel> { NotificationChannel.Slack, NotificationChannel.Discord },
    Status = DeploymentStatus.Success,
    DurationSeconds = 180
};

var notificationResult = validationService.ValidateNotification(notification);
if (!notificationResult.IsValid)
{
    Console.WriteLine("Notification validation failed:");
    foreach (var error in notificationResult.Errors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Validate a channel configuration
var channelConfig = new ChannelConfiguration
{
    DisplayName = "Production Slack",
    WebhookUrl = "https://hooks.slack.com/services/T123/B456/C789",
    TargetId = "C123456",
    ChannelType = NotificationChannel.Slack,
    TimeoutMs = 5000,
    MaxRetries = 3,
    CustomHeaders = new Dictionary<string, string>()
};

var configResult = validationService.ValidateChannelConfiguration(channelConfig);
if (!configResult.IsValid)
{
    Console.WriteLine("Configuration validation failed:");
    foreach (var error in configResult.Errors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Validate individual fields
bool isValidUrl = validationService.IsValidUrl("https://example.com/webhook");
bool isValidEmail = validationService.IsValidEmail("admin@example.com");

Console.WriteLine($"URL validation: {(isValidUrl ? "Valid" : "Invalid")}");
Console.WriteLine($"Email validation: {(isValidEmail ? "Valid" : "Invalid")}");
```

## NotificationServiceTests

The `NotificationServiceTests` class provides comprehensive unit tests for the `NotificationService` class, which manages the core notification functionality including creating notifications, sending them through configured channels, and handling delivery results. These tests verify that notifications are correctly created, validated, and sent, with proper error handling for invalid inputs and edge cases. The test suite covers all public methods of the `NotificationService` class including notification creation, sending operations, retry logic, and result tracking.

Example usage:

```csharp
// Create required services for the notification service
var dbContext = new NotificationDbContext(/* connection string */);
var logger = new Logger<NotificationService>(new LoggerFactory());

var notificationRepository = new NotificationRepository(dbContext);
var configRepository = new ChannelConfigRepository(dbContext);
var resultRepository = new NotificationResultRepository(dbContext);

var webhookClientFactory = new WebhookClientFactory();
var dispatcher = new WebhookDispatcher(webhookClientFactory, logger);
var validationService = new ValidationService();

// Initialize the notification service
var notificationService = new NotificationService(
    notificationRepository,
    configRepository,
    resultRepository,
    dispatcher,
    validationService,
    logger
);

// Test CreateNotificationAsync with valid notification
var validNotification = new DeploymentNotification
{
    ProjectName = "MyApplication",
    Version = "2.0.0",
    Status = DeploymentStatus.Success,
    Priority = NotificationPriority.High,
    TargetEnvironment = "production",
    CommitAuthor = "vlad",
    BranchName = "main",
    Message = "Version 2.0.0 deployed successfully",
    Channels = new List<NotificationChannel> { NotificationChannel.Slack, NotificationChannel.Discord }
};

var notificationId = await notificationService.CreateNotificationAsync(validNotification);
Assert.NotNull(notificationId);
Console.WriteLine($"Created notification with ID: {notificationId}");

// Test SendNotificationAsync with valid notification ID
var sendResults = await notificationService.SendNotificationAsync(notificationId);
Assert.NotEmpty(sendResults);
Console.WriteLine($"Sent notification to {sendResults.Count} channels");

// Test SendNotificationAsync with invalid notification ID (should throw exception)
await Assert.ThrowsAsync<KeyNotFoundException>(
    async () => await notificationService.SendNotificationAsync("invalid-id")
);

// Test SendNotificationAsync with no channels specified (should return empty list)
var noChannelsNotification = new DeploymentNotification
{
    ProjectName = "TestApp",
    Version = "1.0.0",
    Status = DeploymentStatus.Success
};
var noChannelsId = await notificationService.CreateNotificationAsync(noChannelsNotification);
var noChannelsResults = await notificationService.SendNotificationAsync(noChannelsId);
Assert.Empty(noChannelsResults);
Console.WriteLine("SendNotificationAsync correctly returned empty list for notification with no channels");

// Test RetryFailedDeliveriesAsync with valid notification ID
var retryResults = await notificationService.RetryFailedDeliveriesAsync(notificationId);
Console.WriteLine($"Retried {retryResults.Count} failed deliveries");

// Test RetryFailedDeliveriesAsync with invalid notification ID (should throw exception)
await Assert.ThrowsAsync<KeyNotFoundException>(
    async () => await notificationService.RetryFailedDeliveriesAsync("invalid-id")
);
```

## NotificationProcessorTests

The `NotificationProcessorTests` class provides comprehensive unit tests for the `NotificationProcessor` class, which handles background processing of deployment notifications. These tests verify batch processing functionality, retry mechanisms for failed deliveries, priority-based processing, and statistics calculation. The test suite covers all public methods of the `NotificationProcessor` class including batch processing with various result scenarios, failed notification retry logic, priority-based processing order, and system statistics retrieval.

Example usage:

```csharp
// Create required services for the notification processor
var dbContext = new NotificationDbContext(/* connection string */);
var logger = new Logger<NotificationProcessor>(new LoggerFactory());

var notificationService = new NotificationService(
    new NotificationRepository(dbContext),
    new ChannelStrategyResolver(new WebhookClientFactory()),
    logger
);

var configRepository = new ChannelConfigRepository(logger);
var resultRepository = new NotificationResultRepository(logger);

// Create the notification processor
var processor = new NotificationProcessor(
    notificationService,
    new NotificationRepository(dbContext),
    configRepository,
    resultRepository,
    logger
);

// Process a batch of notifications (up to 100 at a time)
var batchResult = await processor.ProcessBatchAsync(100);
Console.WriteLine(batchResult.GetSummary());
// Output: "Processed 100 notifications: 95 successful, 5 failed, Success rate: 95.0%"

// Process failed notifications with retry logic (up to 3 attempts)
var retryResult = await processor.ProcessFailedAsync(3);
Console.WriteLine($"Retried {retryResult.TotalProcessed} notifications with {retryResult.SuccessCount} successful");

// Process notifications by priority (Critical > High > Normal > Low)
var priorityResult = await processor.ProcessByPriorityAsync();
Console.WriteLine($"Priority processing completed: {priorityResult.SuccessRate:P0} success rate");

// Get system-wide statistics
var stats = await processor.GetStatisticsAsync();
Console.WriteLine($"System statistics:");
Console.WriteLine($"  Total notifications: {stats.TotalNotifications}");
Console.WriteLine($"  Pending: {stats.PendingCount}");
Console.WriteLine($"  Success rate: {stats.SuccessRate:P0}");
Console.WriteLine($"  Health: {stats.HealthPercentage:F1}%");
Console.WriteLine($"  Average delivery time: {stats.AverageDeliveryTimeMs}ms");

// Process a specific batch with mixed results
var mixedBatch = new List<DeploymentNotification>
{
    new DeploymentNotification { ProjectName = "App1", Version = "1.0.0", Status = BuildStatus.Success },
    new DeploymentNotification { ProjectName = "App2", Version = "2.0.0", Status = BuildStatus.Failed },
    new DeploymentNotification { ProjectName = "App3", Version = "3.0.0", Status = BuildStatus.Success }
};

var mixedResult = await processor.ProcessBatchAsync(mixedBatch);
Console.WriteLine($"Mixed batch: {mixedResult.SuccessCount} successful, {mixedResult.FailureCount} failed");
```

## RollbackNotificationServiceTests

The `RollbackNotificationServiceTests` class provides comprehensive unit tests for the `RollbackNotificationService` class, which handles sending notifications related to deployment rollback operations. These tests verify message formatting for different channels (Slack, Discord, Telegram), notification dispatch functionality, and history tracking. The test suite covers all public methods including message formatting with various rollback statuses, notification sending methods, and history retrieval with filtering capabilities.

Example usage:

```csharp
// Create mock notification service and logger
var notificationService = Substitute.For<INotificationService>();
var logger = Substitute.For<ILogger<RollbackNotificationService>>();

// Initialize the rollback notification service
var rollbackNotificationService = new RollbackNotificationService(notificationService, logger);

// Create a rollback request
var rollbackRequest = new RollbackRequest
{
    ProjectName = "MyApplication",
    CurrentVersion = "2.1.0",
    TargetVersion = "2.0.5",
    TargetEnvironment = Environment.Production,
    RequestedBy = "vlad",
    Reason = "Critical regression in version 2.1.0",
    Channels = new List<NotificationChannel> { NotificationChannel.Slack, NotificationChannel.Discord },
    Priority = NotificationPriority.Critical
};

// Test message formatting for different channels
string slackMessage = rollbackNotificationService.FormatRollbackMessage(
    rollbackRequest, 
    RollbackStatus.InProgress, 
    NotificationChannel.Slack
);

string discordMessage = rollbackNotificationService.FormatRollbackMessage(
    rollbackRequest, 
    RollbackStatus.Completed, 
    NotificationChannel.Discord
);

string telegramMessage = rollbackNotificationService.FormatRollbackMessage(
    rollbackRequest, 
    RollbackStatus.Failed, 
    NotificationChannel.Telegram,
    "Database migration failed"
);

// Test notification sending methods
var initiatedResults = await rollbackNotificationService.NotifyRollbackInitiatedAsync(rollbackRequest);
var completedResults = await rollbackNotificationService.NotifyRollbackCompletedAsync(
    rollbackRequest,
    new RollbackResult { Status = RollbackStatus.Completed }
);
var failedResults = await rollbackNotificationService.NotifyRollbackFailedAsync(
    rollbackRequest, 
    "Deployment script timeout"
);

// Test history retrieval
var history = await rollbackNotificationService.GetRollbackNotificationHistoryAsync("MyApplication");
var filteredHistory = await rollbackNotificationService.GetRollbackNotificationHistoryAsync(
    "MyApplication", 
    limit: 10
);
```

## MetricsServiceTests

The `MetricsServiceTests` class provides comprehensive unit tests for the `MetricsService` class, which tracks and analyzes metrics related to notification delivery performance and system health. These tests verify that metrics are correctly recorded and retrieved for notifications created, delivery attempts (both successful and failed), validation failures, and configuration changes. The test suite covers all public methods including recording metrics, getting current snapshots, retrieving metrics for specific time periods, and getting channel-specific metrics.

Example usage:

```csharp
// Create the metrics service with required logger
var logger = new Logger<MetricsService>(new LoggerFactory());
var metricsService = new MetricsService(logger);

// Record metrics as operations occur
metricsService.RecordNotificationCreated();
metricsService.RecordDeliveryAttempt(NotificationChannel.Slack, success: true, durationMs: 150);
metricsService.RecordValidationFailure();
metricsService.RecordConfigurationChange();

// Get current metrics snapshot
var metrics = await metricsService.GetMetricsAsync();
Console.WriteLine($"Total notifications: {metrics.NotificationsCreated}");
Console.WriteLine($"Success rate: {metrics.GetSuccessRate():F1}%");
Console.WriteLine($"Average delivery time: {metrics.AverageDeliveryTimeMs}ms");
Console.WriteLine($"P95 delivery time: {metrics.P95DeliveryTimeMs}ms");

// Get channel-specific metrics
var slackMetrics = await metricsService.GetChannelMetricsAsync(NotificationChannel.Slack);
Console.WriteLine(slackMetrics.GetSummary());

// Get metrics for a specific time period
var yesterday = DateTime.UtcNow.AddDays(-1);
var todayMetrics = await metricsService.GetMetricsByPeriodAsync(yesterday, DateTime.UtcNow);
Console.WriteLine($"Yesterday's success rate: {todayMetrics.GetSuccessRate():F1}%");
```

## CustomTemplateEngineTests

The `CustomTemplateEngineTests` class provides comprehensive unit tests for the `CustomTemplateEngine` class, which manages custom template registration, retrieval, rendering, and deletion. These tests verify that templates are correctly stored, retrieved, updated, filtered, and rendered with various formatting options including case conversion filters.

Example usage:

```csharp
// Create template engine instance
var engine = new CustomTemplateEngine();

// Register a template with a name
engine.RegisterTemplate("deployment-message", "Project {{ProjectName}} version {{Version}} deployed to {{Environment}}");

// Retrieve a template by name
var template = engine.GetTemplate("deployment-message");
Assert.NotNull(template);

// Render template with variables
var variables = new Dictionary<string, string>
{
    {"ProjectName", "MyApplication"},
    {"Version", "2.0.0"},
    {"Environment", "Production"}
};

string rendered = engine.RenderInline("deployment-message", variables);
Console.WriteLine(rendered);
// Output: "Project MyApplication version 2.0.0 deployed to Production"

// Update an existing template
engine.RegisterTemplate("deployment-message", "Project {{ProjectName}} v{{Version}} deployed to {{Environment}}");

// Delete a template
bool deleted = engine.DeleteTemplate("deployment-message");
Assert.True(deleted);

// List all active templates
var templates = engine.ListTemplates();
foreach (var t in templates)
{
    Console.WriteLine($"Template: {t.Name}");
}

// Use case conversion filters
engine.RegisterTemplate("upper-template", "{{ProjectName | upper}}");
engine.RegisterTemplate("lower-template", "{{ProjectName | lower}}");
engine.RegisterTemplate("trim-template", "{{Message | trim}}");

string upperResult = engine.RenderInline("upper-template", new Dictionary<string, string> { {"ProjectName", "  MyApp  "} });
// Output: "MYAPP"

string lowerResult = engine.RenderInline("lower-template", new Dictionary<string, string> { {"ProjectName", "MyApp"} });
// Output: "myapp"
```

## StringExtensions

The `StringExtensions` class provides a comprehensive set of extension methods for string manipulation and formatting. These methods enable common string operations like truncation, case conversion, slug generation, sensitive data masking, and text normalization, providing utility for consistent string handling throughout the application.

Example usage:

```csharp
// Truncate a long message for display
string longMessage = "This is a very long deployment message that needs to be truncated for notification purposes";
string truncated = longMessage.Truncate(50); // "This is a very long deployment message that..."

// Convert text to URL-friendly slug format
string projectName = "My Awesome Project";
string slug = projectName.ToSlug(); // "my-awesome-project"

// Convert between different case formats
string camelCase = "helloWorld".ToPascalCase(); // "Helloworld"
string pascalCase = "HelloWorld".ToCamelCase(); // "helloWorld"

// Mask sensitive information like API keys or tokens
string apiKey = "sk_live_1234567890abcdef";
string maskedKey = apiKey.MaskSensitive(); // "sk_live_****"
string partiallyMasked = apiKey.MaskSensitive(8); // "sk_live_1234********"

// Check if string contains any of multiple substrings
bool containsError = "Deployment failed with error".ContainsAny("error", "failed", "exception"); // true
bool containsSuccess = "Deployment completed".ContainsAny("error", "failed", "exception"); // false

// Normalize line endings to Unix format
string windowsText = "Line 1\r\nLine 2\rLine 3";
string normalized = windowsText.NormalizeLineEndings(); // "Line 1\nLine 2\nLine 3"

// Count occurrences of a substring
int count = "deployment deployment deployment".CountOccurrences("deployment"); // 3

// Remove duplicate consecutive characters
string duplicateText = "Hellooooo Worlddddd";
string cleaned = duplicateText.RemoveDuplicateCharacters(); // "Helo Worldd"

// Extract specific number of words
string firstThreeWords = "This is a sample deployment message".TakeWords(3); // "This is a"

// Wrap text to specific line length
string longText = "This is a very long deployment notification message that needs to be wrapped to fit within standard line lengths for proper display in notifications";
string wrapped = longText.WrapText(40);
/*
This is a very long deployment
notification message that needs to be
wrapped to fit within standard line
lengths for proper display in
notifications
*/

// Safely convert string to boolean
bool flag1 = "true".ToBooleanSafe(); // true
bool flag2 = "yes".ToBooleanSafe(); // true
bool flag3 = "invalid".ToBooleanSafe(false); // false (default)
bool flag4 = "1".ToBooleanSafe(); // true
```

## ValidationRule

The `ValidationRule<T>` class is the base class for all validation rules in the system. It provides a generic interface for validating values of any type with customizable validation logic and error messages. Concrete validation rules inherit from this base class to implement specific validation behaviors.

The validation system supports:
- String validation (NotEmptyRule, LengthRule, EmailRule, UrlRule, PatternRule)
- Numeric validation (RangeRule)
- Composite validation with multiple rules
- Clear, descriptive error messages

Example usage:

```csharp
// Validate a project name is not empty
var projectNameValidator = new NotEmptyRule("Project name");
bool isValid = projectNameValidator.Validate("My Application");
string errorMessage = projectNameValidator.GetErrorMessage();
Console.WriteLine($"Validation result: {isValid}, Error: {errorMessage}");

// Validate string length (minimum 3 characters, maximum 50)
var nameValidator = new LengthRule("Name", minLength: 3, maxLength: 50);
bool nameIsValid = nameValidator.Validate("My Project");
Console.WriteLine($"Name validation: {nameIsValid}");

// Validate email format
var emailValidator = new EmailRule("Email address");
bool emailIsValid = emailValidator.Validate("user@example.com");
Console.WriteLine($"Email validation: {emailIsValid}");

// Validate URL format
var urlValidator = new UrlRule("Webhook URL");
bool urlIsValid = urlValidator.Validate("https://hooks.slack.com/services/T123/B456/C789");
Console.WriteLine($"URL validation: {urlIsValid}");

// Validate numeric range
var timeoutValidator = new RangeRule("Timeout", min: 1000, max: 30000);
bool timeoutIsValid = timeoutValidator.Validate(5000);
Console.WriteLine($"Timeout validation: {timeoutIsValid}");

// Validate with regex pattern
var slugValidator = new PatternRule("Slug", "^[a-z0-9-]+$");
bool slugIsValid = slugValidator.Validate("my-project-name");
Console.WriteLine($"Slug validation: {slugIsValid}");

// Use composite validator for multiple rules
var compositeValidator = new CompositeValidator<string>()
    .AddRule(new NotEmptyRule("Username"))
    .AddRule(new LengthRule("Username", minLength: 3, maxLength: 20))
    .AddRule(new PatternRule("Username", "^[a-zA-Z0-9_]+$"));

bool userNameValid = compositeValidator.Validate("john_doe");
if (!userNameValid)
{
    Console.WriteLine("Validation errors:");
    foreach (var error in compositeValidator.GetErrors())
    {
        Console.WriteLine($"- {error}");
    }
}
```

## TemplateServiceTests

The `TemplateServiceTests` class provides comprehensive unit tests for the `TemplateService` class, which handles template rendering and validation for deployment notifications. These tests verify that template variables are correctly replaced with notification values, edge cases are handled properly, and validation works as expected. The test suite covers all public methods of the `TemplateService` class including template rendering with various notification variables, HTML escaping, preset template access, and template validation.

Example usage:

```csharp
// Create the template service with a logger
var logger = new Logger<TemplateService>(new LoggerFactory());
var templateService = new TemplateService(logger);

// Create a deployment notification with all possible variables
var notification = new DeploymentNotification
{
    ProjectName = "MyApplication",
    Version = "2.0.0",
    Status = DeploymentStatus.Success,
    Message = "Version 2.0.0 deployed successfully",
    TargetEnvironment = Environment.Production,
    BranchName = "main",
    CommitHash = "abc123def456789",
    CommitAuthor = "vlad",
    RepositoryUrl = "https://github.com/org/repo",
    BuildUrl = "https://ci.example.com/build/123",
    DurationSeconds = 180,
    Priority = NotificationPriority.High,
    Channels = new List<NotificationChannel> { NotificationChannel.Slack }
};

// Render a template with variables
var template = "🚀 {{ProjectName}} v{{Version}} deployed to {{Environment}} by {{CommitAuthor}}";
var renderedMessage = templateService.RenderTemplate(template, notification);
Console.WriteLine(renderedMessage);
// Output: "🚀 MyApplication v2.0.0 deployed to Production by vlad"

// Get available template variables
var availableVariables = templateService.GetAvailableVariables();
Console.WriteLine($"Available variables: {string.Join(", ", availableVariables)}");

// Validate a template
var (isValid, errors) = templateService.ValidateTemplate("{{ProjectName}} v{{Version}}");
if (isValid)
{
    Console.WriteLine("Template is valid!");
}

// Render HTML-safe version for web notifications
var htmlSafeMessage = templateService.RenderHtmlSafe(template, notification);
```

## DeploymentHistoryServiceTests

The `DeploymentHistoryServiceTests` class provides comprehensive unit tests for the `DeploymentHistoryService` class, which manages deployment history tracking and statistics. These tests verify that deployment entries are correctly recorded, queried, and filtered, with proper validation for null and empty inputs. The test suite covers all public methods of the `DeploymentHistoryService` class including deployment recording, history retrieval, statistics calculation, and rollback tracking.

Example usage:

```csharp
// Initialize the deployment history service with a logger
var logger = Substitute.For<ILogger<DeploymentHistoryService>>();
var historyService = new DeploymentHistoryService(logger);

// Record a deployment event
var deploymentEntry = new DeploymentHistoryEntry
{
    ProjectName = "MyApplication",
    Version = "2.0.0",
    TargetEnvironment = Environment.Production,
    Status = DeploymentStatus.Success,
    DeployedAt = DateTime.UtcNow,
    DurationSeconds = 180,
    CommitSha = "abc123def",
    TriggeredBy = "vlad",
    Message = "Version 2.0.0 deployed successfully to production"
};

await historyService.RecordDeploymentAsync(deploymentEntry);

// Record from a notification
var notification = new DeploymentNotification
{
    ProjectName = "MyApplication",
    Version = "2.0.1",
    Status = DeploymentStatus.Success,
    TargetEnvironment = "production",
    CommitAuthor = "vlad",
    BranchName = "main",
    Message = "Hotfix deployed"
};

await historyService.RecordFromNotificationAsync(notification);

// Get project history (returns newest-first)
var projectHistory = await historyService.GetProjectHistoryAsync("MyApplication", limit: 10);
Console.WriteLine($"Found {projectHistory.Count} deployments for MyApplication");

// Get recent deployments across all projects
var recentDeployments = await historyService.GetRecentDeploymentsAsync(limit: 5);

// Get statistics for a project
var stats = await historyService.GetStatisticsAsync("MyApplication");
Console.WriteLine($"Success rate: {stats.SuccessRate:P0}");
Console.WriteLine($"Average duration: {stats.AverageDurationSeconds?.ToString("F0") ?? "N/A"} seconds");

// Get deployments by environment
var prodDeployments = await historyService.GetByEnvironmentAsync(Environment.Production, limit: 20);

// Get the last successful deployment
var lastSuccess = await historyService.GetLastSuccessfulDeploymentAsync("MyApplication", Environment.Production);

// Get rollback entries
var rollbacks = await historyService.GetRollbackEntriesAsync("MyApplication", limit: 10);

// Check if a deployment was successful
bool isSuccessful = deploymentEntry.IsSuccessful; // true for Success/DeploymentSuccess
```

## ServiceExtensionsValidation

The `ServiceExtensionsValidation` class provides extension methods for validating `DeploymentNotification` and `NotificationResult` objects. It includes methods to validate objects, check if they are valid, and ensure they are valid (throwing exceptions if not).

Example usage:
```csharp
// Create a valid deployment notification
var notification = new DeploymentNotification
{
    ProjectName = "MyApplication",
    Version = "2.0.0",
    Status = DeploymentStatus.Success,
    Priority = NotificationPriority.High,
    TargetEnvironment = "production",
    Channels = new List<NotificationChannel> { NotificationChannel.Slack, NotificationChannel.Discord },
    CommitAuthor = "vlad",
    BranchName = "main",
    CommitHash = "abc123def456",
    Message = "Version 2.0.0 deployed successfully",
    CreatedAt = DateTime.UtcNow,
    DurationSeconds = 180
};

// Validate and get list of problems
var problems = notification.Validate();
if (problems.Count > 0)
{
    Console.WriteLine("Validation failed:");
    foreach (var problem in problems)
    {
        Console.WriteLine($"- {problem}");
    }
}

// Check if valid (returns true/false)
bool isValid = notification.IsValid();
Console.WriteLine($"Notification is valid: {isValid}");

// Ensure valid (throws ArgumentException if invalid)
ServiceExtensionsValidation.EnsureValid(notification);

// Validate a NotificationResult
var result = new NotificationResult
{
    NotificationId = Guid.NewGuid().ToString(),
    ConfigurationId = "cfg-slack-prod",
    Channel = NotificationChannel.Slack,
    Status = DeliveryStatus.Success,
    DurationMs = 142,
    AttemptNumber = 1,
    AttemptedAt = DateTime.UtcNow
};

var resultProblems = result.Validate();
if (resultProblems.Count > 0)
{
    Console.WriteLine("Result validation failed:");
    foreach (var problem in resultProblems)
    {
        Console.WriteLine($"- {problem}");
    }
}

ServiceExtensionsValidation.EnsureValid(result);
```

## TypeHelper

The `TypeHelper` class provides a comprehensive set of utilities for working with .NET types and reflection. It includes methods for type checking, conversion, instantiation, and reflection operations, enabling type-safe operations and dynamic type handling throughout the application.

Example usage:

```csharp
// Check if a type is numeric
bool isNumeric = typeof(int).IsNumeric(); // true
bool isNumericGeneric = TypeHelper.IsNumeric<int>(); // true

// Check if a type is nullable
bool isNullable = typeof(int?).IsNullable(); // true
bool isNotNullable = typeof(int).IsNullable(); // false

// Get underlying type from nullable
Type underlyingType = typeof(int?).GetUnderlyingType(); // typeof(int)

// Check if a type implements an interface
bool implementsIDisposable = typeof(Stream).ImplementsInterface<IDisposable>(); // true

// Check if a type is an enum
bool isEnum = TypeHelper.IsEnum<DeploymentStatus>(); // true

// Check if a type is a collection
bool isCollection = typeof(List<int>).IsCollection(); // true
bool isNotCollection = typeof(string).IsCollection(); // false

// Get generic arguments from a type
Type[]? genericArgs = typeof(Dictionary<string, int>).GetGenericArguments(); // [typeof(string), typeof(int)]

// Check if a type is generic
bool isGeneric = typeof(List<int>).IsGeneric(); // true
bool isNotGeneric = typeof(string).IsGeneric(); // false

// Get a method by signature
var method = typeof(string).GetMethodBySignature("Substring", typeof(int), typeof(int));

// Get all properties, fields, or methods of a type
var properties = typeof(DeploymentNotification).GetAllProperties();
var fields = typeof(DeploymentNotification).GetAllFields();
var methods = typeof(DeploymentNotification).GetAllMethods();

// Check if a type has a parameterless constructor and create an instance
bool hasConstructor = typeof(DeploymentNotification).HasParameterlessConstructor(); // true
var instance = typeof(DeploymentNotification).CreateInstance();

// Convert values between types
object convertedInt = "42".ConvertTo(typeof(int)); // 42
int convertedString = "hello".ConvertTo<int>(); // 0 (default)
string convertedFromInt = 123.ConvertTo<string>(); // "123"

// Find types that inherit from a base type in an assembly
var assembly = Assembly.GetExecutingAssembly();
var types = assembly.FindTypesThatInherit(typeof(NotificationChannel));

// Get attributes from a type
var obsoleteAttr = typeof(ObsoleteAttribute).GetAttribute<AttributeUsageAttribute>();
var allAttributes = typeof(DeploymentNotification).GetAttributes<SerializableAttribute>();
```

## Configuration

See `appsettings.example.json` for configuration examples.

## INotificationFormatter

The `INotificationFormatter` interface defines the contract for formatting deployment notifications into various output formats (JSON, text, CSV, Markdown). It provides methods to convert `DeploymentNotification` objects into formatted strings and specify the appropriate content type for HTTP headers, enabling consistent formatting across different notification channels and systems.

Example usage:

```csharp
// Create a deployment notification
var notification = new DeploymentNotification
{
    ProjectName = "MyApplication",
    Version = "2.0.0",
    Status = DeploymentStatus.Success,
    TargetEnvironment = "production",
    BranchName = "main",
    CommitHash = "abc123def456",
    CommitAuthor = "vlad",
    Message = "Version 2.0.0 deployed successfully",
    Priority = NotificationPriority.High,
    CreatedAt = DateTime.UtcNow,
    Channels = new List<NotificationChannel> { NotificationChannel.Slack, NotificationChannel.Discord }
};

// Format as JSON
var jsonFormatter = NotificationFormatterFactory.CreateFormatter("json");
string jsonOutput = jsonFormatter.Format(notification);
Console.WriteLine($"JSON Content-Type: {jsonFormatter.GetContentType()}");
Console.WriteLine(jsonOutput);

// Format as human-readable text with emojis
var textFormatter = NotificationFormatterFactory.CreateFormatter("text");
if (textFormatter is TextNotificationFormatter textFormatterImpl)
{
    textFormatterImpl.EnableEmojis = true; // Enable status emojis
}
string textOutput = textFormatter.Format(notification);
Console.WriteLine($"\nText Content-Type: {textFormatter.GetContentType()}");
Console.WriteLine(textOutput);

// Format as Markdown with emojis
var markdownFormatter = NotificationFormatterFactory.CreateFormatter("markdown");
if (markdownFormatter is MarkdownNotificationFormatter mdFormatterImpl)
{
    mdFormatterImpl.EnableEmojis = true; // Enable status emojis
}
string markdownOutput = markdownFormatter.Format(notification);
Console.WriteLine($"\nMarkdown Content-Type: {markdownFormatter.GetContentType()}");
Console.WriteLine(markdownOutput);

// Format as CSV
var csvFormatter = NotificationFormatterFactory.CreateFormatter("csv");
string csvOutput = csvFormatter.Format(notification);
Console.WriteLine($"\nCSV Content-Type: {csvFormatter.GetContentType()}");
Console.WriteLine(csvOutput);
```

## IPayloadBuilder

The `IPayloadBuilder` interface defines the contract for building notification payloads for different messaging channels (Slack, Discord, Telegram, etc.). It provides methods to construct channel-specific message formats and webhook payloads from deployment notifications and channel configurations, enabling consistent formatting across multiple notification destinations.

Example usage:

```csharp
// Create payload builder with logger
var logger = new Logger<PayloadBuilder>(new LoggerFactory());
var payloadBuilder = new PayloadBuilder(logger);

// Create a deployment notification
var notification = new DeploymentNotification
{
    ProjectName = "MyApplication",
    Version = "2.0.0",
    Status = DeploymentStatus.Success,
    TargetEnvironment = "production",
    BranchName = "main",
    CommitHash = "abc123def456",
    CommitAuthor = "vlad",
    Message = "Version 2.0.0 deployed successfully",
    CreatedAt = DateTime.UtcNow,
    DurationSeconds = 180,
    BuildUrl = "https://ci.example.com/build/123"
};

// Create channel configuration
var slackConfig = new ChannelConfiguration
{
    ChannelType = NotificationChannel.Slack,
    WebhookUrl = "https://hooks.slack.com/services/...",
    UseSlackBlockKit = true,
    IncludeCommitDetails = true,
    IncludeBuildUrl = true
};

// Build payloads for different channels
var webhookPayload = payloadBuilder.BuildPayload(notification, slackConfig);
var telegramMessage = payloadBuilder.BuildTelegramMessage(notification, slackConfig);
var slackPayload = payloadBuilder.BuildSlackPayload(notification, slackConfig);
var discordPayload = payloadBuilder.BuildDiscordPayload(notification, slackConfig);

Console.WriteLine($"Telegram message length: {telegramMessage.Length} characters");
Console.WriteLine($"Slack payload type: {slackPayload.GetType().Name}");
Console.WriteLine($"Discord payload type: {discordPayload.GetType().Name}");
```

## IMetricsService

The `IMetricsService` interface provides methods for collecting and analyzing system metrics related to notification delivery performance and system health. It tracks notification creation, delivery attempts, success/failure rates, delivery times, validation failures, and configuration changes across all notification channels.

Example usage:

```csharp
// Create the metrics service with required logger
var logger = new Logger<MetricsService>(new LoggerFactory());
var metricsService = new MetricsService(logger);

// Record metrics as operations occur
metricsService.RecordNotificationCreated();
metricsService.RecordDeliveryAttempt(NotificationChannel.Slack, success: true, durationMs: 150);
metricsService.RecordValidationFailure();
metricsService.RecordConfigurationChange();

// Get current metrics snapshot
var metrics = await metricsService.GetMetricsAsync();
Console.WriteLine($"Total notifications: {metrics.NotificationsCreated}");
Console.WriteLine($"Success rate: {metrics.GetSuccessRate():F1}%");
Console.WriteLine($"Average delivery time: {metrics.AverageDeliveryTimeMs}ms");
Console.WriteLine($"P95 delivery time: {metrics.P95DeliveryTimeMs}ms");

// Get channel-specific metrics
var slackMetrics = await metricsService.GetChannelMetricsAsync(NotificationChannel.Slack);
Console.WriteLine(slackMetrics.GetSummary());

// Get metrics for a specific time period
var yesterday = DateTime.UtcNow.AddDays(-1);
var todayMetrics = await metricsService.GetMetricsByPeriodAsync(yesterday, DateTime.UtcNow);
Console.WriteLine($"Yesterday's success rate: {todayMetrics.GetSuccessRate():F1}%");
```

## MetricsCollector

The `MetricsCollector` class provides in-memory metric collection and analysis capabilities for tracking custom application metrics. It maintains a collection of metric values over time and provides statistical analysis including count, sum, average, min, max, and median calculations. The collector supports recording metrics, incrementing counters, retrieving individual metrics or all metrics, and clearing/resetting data.

Example usage:

```csharp
// Create a metrics collector for tracking API response times
var responseTimeCollector = new MetricsCollector("ApiResponseTimes");

// Record individual metric values
responseTimeCollector.RecordMetric(125.5);
responseTimeCollector.RecordMetric(89.2);
responseTimeCollector.RecordMetric(156.7);
responseTimeCollector.RecordMetric(95.1);

// Increment a counter metric
var errorCounter = new MetricsCollector("ApiErrors");
errorCounter.IncrementCounter();
errorCounter.IncrementCounter();
errorCounter.IncrementCounter();

// Get individual metric values
var metricValue = responseTimeCollector.GetMetric();
if (metricValue != null)
{
    Console.WriteLine($"Metric '{metricValue.Name}' has {metricValue.Count} values");
    Console.WriteLine($"Average: {metricValue.Average:F2}ms");
    Console.WriteLine($"Min: {metricValue.Min:F2}ms, Max: {metricValue.Max:F2}ms");
}

// Get all metrics
var allMetrics = responseTimeCollector.GetAllMetrics();
Console.WriteLine($"Collected {allMetrics.Count} metric values");

// Get statistical analysis
var statistics = responseTimeCollector.GetStatistics();
if (statistics != null)
{
    Console.WriteLine($"Statistics for '{statistics.Name}':");
    Console.WriteLine($"  Count: {statistics.Count}");
    Console.WriteLine($"  Sum: {statistics.Sum:F2}");
    Console.WriteLine($"  Average: {statistics.Average:F2}");
    Console.WriteLine($"  Min: {statistics.Min:F2}");
    Console.WriteLine($"  Max: {statistics.Max:F2}");
    Console.WriteLine($"  Median: {statistics.Median:F2}");
}

// Access metric properties directly
Console.WriteLine($"Metric created at: {responseTimeCollector.CreatedAt:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine($"Last updated: {responseTimeCollector.LastUpdated:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine($"Total values recorded: {responseTimeCollector.Count}");

// Clear all collected metrics
responseTimeCollector.Clear();
Console.WriteLine($"After clear - Count: {responseTimeCollector.Count}");

// Reset a specific metric
responseTimeCollector.ResetMetric();
Console.WriteLine($"After reset - Count: {responseTimeCollector.Count}, Name: {responseTimeCollector.Name}");
```

## IDeploymentHistoryService

The `IDeploymentHistoryService` interface provides methods for tracking and querying deployment history throughout the application. It records deployment events, stores historical data, and exposes aggregated statistics for monitoring deployment patterns, success rates, and rollback operations across projects and environments.

Example usage:

```csharp
// Create the service with required logger
var logger = new Logger<DeploymentHistoryService>(new LoggerFactory());
var historyService = new DeploymentHistoryService(logger);

// Record a deployment event
var deploymentEntry = new DeploymentHistoryEntry
{
    ProjectName = "MyApplication",
    Version = "2.0.0",
    TargetEnvironment = Environment.Production,
    Status = DeploymentStatus.Success,
    DeployedAt = DateTime.UtcNow,
    DurationSeconds = 180,
    CommitSha = "abc123def",
    TriggeredBy = "vlad",
    Message = "Version 2.0.0 deployed successfully to production"
};

await historyService.RecordDeploymentAsync(deploymentEntry);

// Record from a notification
var notification = new DeploymentNotification
{
    ProjectName = "MyApplication",
    Version = "2.0.1",
    Status = DeploymentStatus.Success,
    TargetEnvironment = "production",
    CommitAuthor = "vlad",
    BranchName = "main",
    Message = "Hotfix deployed"
};

await historyService.RecordFromNotificationAsync(notification);

// Get project history
var projectHistory = await historyService.GetProjectHistoryAsync("MyApplication", limit: 10);
Console.WriteLine($"Found {projectHistory.Count} deployments for MyApplication");

// Get recent deployments across all projects
var recentDeployments = await historyService.GetRecentDeploymentsAsync(limit: 5);

// Get statistics for a project
var stats = await historyService.GetStatisticsAsync("MyApplication");
Console.WriteLine($"Success rate: {stats.SuccessRate:P0}");
Console.WriteLine($"Average duration: {stats.AverageDurationSeconds?.ToString("F0") ?? "N/A"} seconds");

// Get deployments by environment
var prodDeployments = await historyService.GetByEnvironmentAsync(Environment.Production, limit: 20);

// Get the last successful deployment
var lastSuccess = await historyService.GetLastSuccessfulDeploymentAsync("MyApplication", Environment.Production);

// Get rollback entries
var rollbacks = await historyService.GetRollbackEntriesAsync("MyApplication", limit: 10);
```

## NotificationPipeline

The `NotificationPipeline` class provides a flexible middleware pipeline for processing deployment notifications through a series of processors. It enables validation, enrichment, filtering, and transformation of notifications before they are sent to channels, ensuring data integrity and compliance with channel-specific requirements.

The pipeline follows a fluent interface pattern, allowing processors to be chained together in a clean and readable way. Each processor validates or transforms the notification data, and the pipeline collects any errors that occur during processing.

Example usage:
```csharp
// Create pipeline with required logger
var logger = new Logger<NotificationPipeline>(new LoggerFactory());
var pipeline = new NotificationPipeline(logger);

// Add processors to the pipeline
pipeline.Use(new ValidationProcessor(logger))
     .Use(new EnrichmentProcessor())
     .Use(new FilterProcessor(configRepository, logger))
     .Use(new SanitizationProcessor());

// Execute the pipeline with a notification
var notification = new DeploymentNotification
{
    ProjectName = "MyApplication",
    Version = "2.1.0",
    Status = DeploymentStatus.Success,
    Priority = NotificationPriority.High,
    TargetEnvironment = "production",
    Channels = new List<NotificationChannel> { NotificationChannel.Slack, NotificationChannel.Discord },
    CommitAuthor = "vlad",
    BranchName = "main",
    Message = "Release 2.1.0 deployed successfully"
};

var result = await pipeline.ExecuteAsync(notification);

if (result.Success)
{
    Console.WriteLine("Pipeline processed notification successfully!");
    Console.WriteLine($"Processed notification ID: {result.ProcessedNotification?.Id}");
    Console.WriteLine($"Filtered channels: {string.Join(", ", result.ProcessedNotification?.Channels ?? new List<NotificationChannel>())}");
}
else
{
    Console.WriteLine("Pipeline processing failed:");
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"- {error}");
    }
}
```

## INotificationProcessor

The `INotificationProcessor` interface defines the contract for background notification processing in the system. It provides methods for processing notifications in batches, retrying failed deliveries, and prioritizing notifications by importance level. Implementations handle the core logic of sending notifications through configured channels and tracking delivery statistics.

Example usage:
```csharp
// Create required services
var notificationService = new NotificationService(
    new NotificationRepository(dbContext),
    new ChannelStrategyResolver(new WebhookClientFactory()),
    logger);

var configRepository = new ChannelConfigRepository(dbContext);
var resultRepository = new NotificationResultRepository(dbContext);

// Create processor
var processor = new NotificationProcessor(
    notificationService,
    new NotificationRepository(dbContext),
    configRepository,
    resultRepository,
    logger);

// Process batch of notifications
var batchResult = await processor.ProcessBatchAsync(100);
Console.WriteLine(batchResult.GetSummary());

// Process failed notifications (up to 3 retry attempts)
var retryResult = await processor.ProcessFailedAsync(3);
Console.WriteLine($"Retried {retryResult.TotalProcessed} notifications");

// Process by priority (Critical > High > Normal > Low)
var priorityResult = await processor.ProcessByPriorityAsync();
Console.WriteLine($"Priority processing: {priorityResult.SuccessRate:P0} success rate");

// Get system statistics
var stats = await processor.GetStatisticsAsync();
Console.WriteLine($"Total notifications: {stats.TotalNotifications}");
Console.WriteLine($"Pending: {stats.PendingCount}");
Console.WriteLine($"Health: {stats.HealthPercentage:F1}%");
```

## IBatchProcessor

The `IBatchProcessor<T>` interface and its implementations (`NotificationBatchProcessor` and `ResilientBatchProcessor<T>`) provide batch processing capabilities for efficient notification handling. These processors enable processing collections of items in configurable batch sizes with support for concurrent execution, retry logic, and detailed result tracking.

The `NotificationBatchProcessor` is designed for processing deployment notifications with built-in service integration, while the `ResilientBatchProcessor<T>` offers generic retry functionality for any async processing pipeline.

Example usage with `NotificationBatchProcessor`:

```csharp
// Create required services
var dbContext = new NotificationDbContext(/* connection string */);
var logger = new Logger<NotificationBatchProcessor>(new LoggerFactory());

var notificationService = new NotificationService(
    new NotificationRepository(dbContext),
    new ChannelStrategyResolver(new WebhookClientFactory()),
    logger
);

// Create batch processor with default batch size (10)
var batchProcessor = new NotificationBatchProcessor(notificationService, logger);

// Process a list of notifications in batches
var notifications = new List<DeploymentNotification>
{
    new DeploymentNotification { /* initialize */ },
    new DeploymentNotification { /* initialize */ },
    // ... more notifications
};

var processedNotifications = await batchProcessor.ProcessBatchAsync(notifications, batchSize: 20);

Console.WriteLine($"Processed {processedNotifications.Count} notifications in batches");
```

Example usage with `ResilientBatchProcessor<T>`:

```csharp
// Create required logger
var logger = new Logger<ResilientBatchProcessor<DeploymentNotification>>(new LoggerFactory());
var options = new BatchProcessingOptions
{
    DefaultBatchSize = 15,
    MaxConcurrentBatches = 3,
    DelayBetweenBatches = TimeSpan.FromMilliseconds(300),
    MaxRetries = 3
};

// Define the processing function
async Task ProcessNotificationAsync(DeploymentNotification notification)
{
    // Your notification processing logic here
    await Task.Delay(10); // Simulate work
}

// Create resilient batch processor
var processor = new ResilientBatchProcessor<DeploymentNotification>(
    ProcessNotificationAsync,
    logger,
    options
);

// Process items with automatic retry
var items = new List<DeploymentNotification> { /* your items */ };
var result = await processor.ProcessWithRetryAsync(items);

Console.WriteLine($"Processing completed: {result}");
Console.WriteLine($"Success rate: {result.SuccessRate:F1}%");
Console.WriteLine($"Total: {result.TotalProcessed}, Success: {result.SuccessCount}, Final failures: {result.FinalFailureCount}");
```

### Batch Processing Options

Both processors support configurable batch processing through `BatchProcessingOptions`:

- **DefaultBatchSize**: Number of items per batch (default: 10)
- **MaxConcurrentBatches**: Maximum concurrent batch operations (default: 5)
- **DelayBetweenBatches**: Time delay between batches to avoid overwhelming systems (default: 500ms)
- **MaxRetries**: Maximum retry attempts for failed items (default: 3)

### Processing Results

The `BatchProcessingResult` class provides detailed statistics:

- **SuccessCount**: Number of successfully processed items
- **FailureCount**: Number of items that failed during processing
- **FinalFailureCount**: Number of items that still failed after all retries
- **CompletedAt**: Timestamp when processing completed
- **SuccessRate**: Calculated success percentage
- **ToString()**: Human-readable summary of processing results

## INotificationService

The `INotificationService` interface defines the contract for managing deployment notifications throughout the application. It provides methods for creating notifications, sending them to configured channels, retrieving notification history, and managing delivery results. This service serves as the primary entry point for working with deployment notifications in the system.

Example usage:

```csharp
// Create required dependencies
var dbContext = new NotificationDbContext(/* connection string */);
var logger = new Logger<NotificationService>(new LoggerFactory());

// Create repositories
var notificationRepository = new NotificationRepository(dbContext);
var configRepository = new ChannelConfigRepository(dbContext);
var resultRepository = new NotificationResultRepository(dbContext);

// Create dispatcher and validation service
var webhookClientFactory = new WebhookClientFactory();
var dispatcher = new WebhookDispatcher(webhookClientFactory, logger);
var validationService = new ValidationService();

// Initialize the notification service
var notificationService = new NotificationService(
    notificationRepository,
    configRepository,
    resultRepository,
    dispatcher,
    validationService,
    logger
);

// Create a new deployment notification
var notification = new DeploymentNotification
{
    Id = Guid.NewGuid().ToString(),
    ProjectName = "MyApplication",
    Version = "2.0.0",
    Status = DeploymentStatus.Success,
    Priority = NotificationPriority.High,
    TargetEnvironment = "production",
    CommitAuthor = "vlad",
    BranchName = "main",
    Message = "Version 2.0.0 deployed to production",
    Channels = new List<NotificationChannel> { NotificationChannel.Slack, NotificationChannel.Discord }
};

// Create and queue the notification
var notificationId = await notificationService.CreateNotificationAsync(notification);
Console.WriteLine($"Created notification with ID: {notificationId}");

// Send pending notifications (processes all pending notifications)
var sendResults = await notificationService.SendPendingNotificationsAsync();
Console.WriteLine($"Sent {sendResults.Count} notifications");

// Get notification history for a project
var history = await notificationService.GetNotificationHistoryAsync("MyApplication", limit: 10);
Console.WriteLine($"Found {history.Count} notifications for MyApplication");

// Get delivery results for a specific notification
var deliveryResults = await notificationService.GetDeliveryResultsAsync(notificationId);
foreach (var result in deliveryResults)
{
    Console.WriteLine($"Channel {result.Channel}: {result.Status}");
}

// Retry failed deliveries if any
var failedResults = deliveryResults.Where(r => !r.IsSuccessful).ToList();
if (failedResults.Any())
{
    var retryResults = await notificationService.RetryFailedDeliveriesAsync(notificationId);
    Console.WriteLine($"Retried {retryResults.Count} failed deliveries");
}
```

## IValidationService

The `IValidationService` interface provides validation methods for deployment notifications, channel configurations, and webhook payloads. It ensures data integrity by validating required fields, URL formats, email addresses, and structural correctness before notifications are sent through various channels (Slack, Discord, Telegram, etc.).

Example usage:

```csharp
// Create validation service
var validationService = new ValidationService();

// Validate a deployment notification
var notification = new DeploymentNotification
{
    ProjectName = "MyApplication",
    Version = "2.0.0",
    BranchName = "main",
    Message = "Version 2.0.0 deployed successfully",
    Channels = new List<NotificationChannel> { NotificationChannel.Slack, NotificationChannel.Discord },
    Status = DeploymentStatus.Success
};

var notificationResult = validationService.ValidateNotification(notification);
if (!notificationResult.IsValid)
{
    Console.WriteLine("Notification validation failed:");
    foreach (var error in notificationResult.Errors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Validate a channel configuration
var channelConfig = new ChannelConfiguration
{
    DisplayName = "Production Slack",
    WebhookUrl = "https://hooks.slack.com/services/...",
    TargetId = "C123456",
    ChannelType = NotificationChannel.Slack,
    TimeoutMs = 5000,
    MaxRetries = 3
};

var configResult = validationService.ValidateChannelConfiguration(channelConfig);
if (!configResult.IsValid)
{
    Console.WriteLine("Configuration validation failed:");
    foreach (var error in configResult.Errors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Validate a webhook payload
var payload = new WebhookPayload
{
    EventId = Guid.NewGuid().ToString(),
    EventType = "deployment.success",
    Data = new WebhookData
    {
        ProjectName = "MyApplication",
        Version = "2.0.0",
        Status = "success"
    }
};

var payloadResult = validationService.ValidateWebhookPayload(payload);
if (!payloadResult.IsValid)
{
    Console.WriteLine("Webhook payload validation failed:");
    foreach (var error in payloadResult.Errors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Validate individual fields
bool isValidUrl = validationService.IsValidUrl("https://example.com/webhook");
bool isValidEmail = validationService.IsValidEmail("admin@example.com");

Console.WriteLine($"URL validation: {(isValidUrl ? "Valid" : "Invalid")}");
Console.WriteLine($"Email validation: {(isValidEmail ? "Valid" : "Invalid")}");
```

## IChannelConfigRepository

The `IChannelConfigRepository` interface provides data access methods for managing channel configurations in the system. It handles CRUD operations for notification channel configurations including Slack webhooks, Discord channels, Telegram bots, and other messaging platforms. The repository supports querying configurations by channel type, filtering enabled configurations, and pagination for large configuration sets.

Example usage:

```csharp
// Create repository with logger
var logger = new Logger<ChannelConfigRepository>(new LoggerFactory());
var configRepository = new ChannelConfigRepository(logger);

// Create a channel configuration for Slack
var slackConfig = new ChannelConfiguration
{
    Id = Guid.NewGuid().ToString(),
    DisplayName = "Production Slack",
    ChannelType = NotificationChannel.Slack,
    WebhookUrl = "https://hooks.slack.com/services/T123/B456/C789",
    TargetId = "C123456",
    IsEnabled = true,
    TimeoutMs = 5000,
    MaxRetries = 3,
    CreatedAt = DateTime.UtcNow
};

// Store the configuration
await configRepository.CreateAsync(slackConfig);
Console.WriteLine($"Created configuration: {slackConfig.DisplayName}");

// Retrieve by ID
var retrievedConfig = await configRepository.GetByIdAsync(slackConfig.Id);
if (retrievedConfig != null)
{
    Console.WriteLine($"Retrieved config for channel type: {retrievedConfig.ChannelType}");
}

// Get all configurations for a specific channel (e.g., Slack)
var slackConfigs = await configRepository.GetByChannelAsync(NotificationChannel.Slack);
Console.WriteLine($"Found {slackConfigs.Count} Slack configurations");

// Get all enabled configurations
var enabledConfigs = await configRepository.GetEnabledAsync();
Console.WriteLine($"Found {enabledConfigs.Count} enabled configurations");

// Update a configuration
slackConfig.IsEnabled = false;
slackConfig.MarkAsUpdated();
await configRepository.UpdateAsync(slackConfig);

// Delete a configuration
// await configRepository.DeleteAsync(slackConfig.Id);

// Get all configurations with pagination
var allConfigs = await configRepository.GetAllAsync(skip: 0, take: 100);
Console.WriteLine($"Total configurations in system: {allConfigs.Count}");
```

## INotificationRepository

The `INotificationRepository` interface provides data access methods for managing deployment notifications in the system. It serves as the primary contract for persisting, retrieving, updating, and deleting notification records, supporting operations such as fetching notifications by project, environment, status, or processing state. This repository is used throughout the application by services like `NotificationService`, `NotificationProcessingWorker`, and `RollbackService` to manage the lifecycle of deployment notifications.

Example usage:

```csharp
// Create repository with logger
var logger = new Logger<NotificationRepository>(new LoggerFactory());
var notificationRepository = new NotificationRepository(logger);

// Create a deployment notification
var notification = new DeploymentNotification
{
    Id = Guid.NewGuid().ToString(),
    ProjectName = "MyApplication",
    Version = "2.0.0",
    Status = BuildStatus.Success,
    TargetEnvironment = Environment.Production,
    Priority = NotificationPriority.High,
    Message = "Version 2.0.0 deployed successfully to production",
    Channels = new List<NotificationChannel> { NotificationChannel.Slack, NotificationChannel.Discord },
    CreatedAt = DateTime.UtcNow,
    CommitAuthor = "vlad",
    BranchName = "main",
    CommitHash = "abc123def456",
    DurationSeconds = 180,
    BuildUrl = "https://ci.example.com/build/123"
};

// Store the notification
await notificationRepository.CreateAsync(notification);
Console.WriteLine($"Created notification with ID: {notification.Id}");

// Retrieve by ID
var retrievedNotification = await notificationRepository.GetByIdAsync(notification.Id);
if (retrievedNotification != null)
{
    Console.WriteLine($"Retrieved notification: {retrievedNotification.ProjectName} v{retrievedNotification.Version}");
}

// Get all notifications for a project
var projectNotifications = await notificationRepository.GetByProjectAsync("MyApplication", limit: 50);
Console.WriteLine($"Found {projectNotifications.Count} notifications for MyApplication");

// Get pending (unprocessed) notifications
var pendingNotifications = await notificationRepository.GetPendingAsync();
Console.WriteLine($"Found {pendingNotifications.Count} pending notifications");

// Get notifications by environment
var productionNotifications = await notificationRepository.GetByEnvironmentAsync(Environment.Production);
Console.WriteLine($"Found {productionNotifications.Count} production notifications");

// Get notifications by status
var successNotifications = await notificationRepository.GetByStatusAsync(BuildStatus.Success, limit: 20);
Console.WriteLine($"Found {successNotifications.Count} successful notifications");

// Update a notification
notification.IsProcessed = true;
notification.ProcessedAt = DateTime.UtcNow;
await notificationRepository.UpdateAsync(notification);

// Delete a notification
// await notificationRepository.DeleteAsync(notification.Id);

// Get all notifications
var allNotifications = await notificationRepository.GetAllAsync();
Console.WriteLine($"Total notifications in system: {allNotifications.Count}");
```

## INotificationResultRepository

The `INotificationResultRepository` interface provides data access methods for managing notification delivery results in the system. It tracks the outcome of each delivery attempt to different channels, including success/failure status, timestamps, response data, and retry information. This repository enables comprehensive auditing of notification delivery attempts and supports operations like retrieving results by notification ID, channel, or filtering by date ranges.

Example usage:

```csharp
// Create repository with logger
var logger = new Logger<NotificationResultRepository>(new LoggerFactory());
var resultRepository = new NotificationResultRepository(logger);

// Create a notification result for a successful Slack delivery
var result = new NotificationResult
{
    Id = Guid.NewGuid().ToString(),
    NotificationId = "notification-123",
    Channel = NotificationChannel.Slack,
    Status = DeliveryStatus.Success,
    AttemptedAt = DateTime.UtcNow,
    ResponseCode = 200,
    ResponseBody = "ok",
    DurationMs = 150,
    RetryCount = 0,
    ErrorMessage = null
};

// Store the result
await resultRepository.CreateAsync(result);
Console.WriteLine($"Created result with ID: {result.Id} for notification {result.NotificationId}");

// Retrieve by ID
var retrievedResult = await resultRepository.GetByIdAsync(result.Id);
if (retrievedResult != null)
{
    Console.WriteLine($"Retrieved result: {retrievedResult.Status} for channel {retrievedResult.Channel}");
}

// Get all results for a notification
var notificationResults = await resultRepository.GetByNotificationIdAsync(result.NotificationId);
Console.WriteLine($"Found {notificationResults.Count} results for notification {result.NotificationId}");

// Get failed results for a notification
var failedResults = await resultRepository.GetFailedByNotificationIdAsync(result.NotificationId);
Console.WriteLine($"Found {failedResults.Count} failed results for notification {result.NotificationId}");

// Get results by channel
var slackResults = await resultRepository.GetByChannelAsync(NotificationChannel.Slack, limit: 100);
Console.WriteLine($"Found {slackResults.Count} Slack delivery results");

// Update a result
result.Status = DeliveryStatus.Success;
await resultRepository.UpdateAsync(result);

// Delete old results (older than 30 days)
// await resultRepository.DeleteOlderThanAsync(DateTime.UtcNow.AddDays(-30));

// Get all results with pagination
var allResults = await resultRepository.GetAllAsync(skip: 0, take: 100);
Console.WriteLine($"Total results in system: {allResults.Count}");
```

## IRollbackNotificationService

The `IRollbackNotificationService` interface provides methods for sending notifications related to deployment rollback operations. It handles dispatching notifications when rollbacks are initiated, completed, or failed, with support for multiple notification channels (Slack, Discord, Telegram, etc.). The service maintains a history of rollback notifications and provides formatted messages for different rollback statuses.

## IHealthCheckService

The `IHealthCheckService` interface provides methods for checking the health of the notification system and its configured channels. It performs comprehensive health checks to determine system status, channel availability, and delivery performance metrics. The service helps identify failing channels, calculate success rates, and provide actionable health reports for monitoring and alerting purposes.

Example usage:

```csharp
// Create required dependencies
var dbContext = new NotificationDbContext(/* connection string */);
var logger = new Logger<HealthCheckService>(new LoggerFactory());

// Create repositories
var configRepository = new ChannelConfigRepository(dbContext);
var resultRepository = new NotificationResultRepository(dbContext);

// Create dispatcher
var webhookClientFactory = new WebhookClientFactory();
var dispatcher = new WebhookDispatcher(webhookClientFactory, logger);

// Initialize the health check service
var healthCheckService = new HealthCheckService(
    configRepository,
    resultRepository,
    dispatcher,
    logger);

// Check overall system health
var systemHealth = await healthCheckService.CheckSystemHealthAsync();
Console.WriteLine($"System Health: {systemHealth.Status} ({systemHealth.HealthPercentage:F1}%)");
Console.WriteLine($"Failing channels: {systemHealth.FailingChannels}");
if (systemHealth.Errors.Any())
{
    Console.WriteLine("Errors:");
    foreach (var error in systemHealth.Errors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Check health of a specific channel
var slackConfig = await configRepository.GetByIdAsync("slack-config-id");
if (slackConfig != null)
{
    var channelHealth = await healthCheckService.CheckChannelHealthAsync(slackConfig.Id);
    Console.WriteLine($"\nChannel {channelHealth.ConfigName} ({channelHealth.Channel}): {channelHealth.Status}");
    Console.WriteLine($"Success rate: {channelHealth.SuccessRate:F1}%");
    Console.WriteLine($"Avg delivery time: {channelHealth.AvgDeliveryTimeMs}ms");
    Console.WriteLine($"Last success: {channelHealth.LastSuccessAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never"}");
    Console.WriteLine($"Last failure: {channelHealth.LastFailureAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never"}");
}

// Get complete health report
var healthReport = await healthCheckService.GetHealthReportAsync();
Console.WriteLine($"\nHealth Report:");
Console.WriteLine($"Total notifications: {healthReport.TotalNotifications}");
Console.WriteLine($"Total delivery attempts: {healthReport.TotalDeliveryAttempts}");
Console.WriteLine($"Successful deliveries: {healthReport.SuccessfulDeliveries}");
Console.WriteLine($"Failed deliveries: {healthReport.FailedDeliveries}");
Console.WriteLine($"Overall success rate: {healthReport.OverallSuccessRate:F1}%");

// Check all channels
var allChannelStatuses = await healthCheckService.CheckAllChannelsAsync();
Console.WriteLine($"\nChannel Statuses:");
foreach (var channelStatus in allChannelStatuses)
{
    Console.WriteLine($"- {channelStatus.ConfigName}: {channelStatus.Status} ({channelStatus.SuccessRate:F1}%)");
}
```

Example usage:

```csharp
// Create required dependencies
var dbContext = new NotificationDbContext(/* connection string */);
var logger = new Logger<NotificationService>(new LoggerFactory());

// Create repositories and services
var notificationRepository = new NotificationRepository(dbContext);
var configRepository = new ChannelConfigRepository(dbContext);
var resultRepository = new NotificationResultRepository(dbContext);
var webhookClientFactory = new WebhookClientFactory();
var dispatcher = new WebhookDispatcher(webhookClientFactory, logger);
var validationService = new ValidationService();

// Initialize services
var notificationService = new NotificationService(
    notificationRepository,
    configRepository,
    resultRepository,
    dispatcher,
    validationService,
    logger
);

var rollbackNotificationService = new RollbackNotificationService(
    notificationService,
    logger
);

// Create a rollback request
var rollbackRequest = new RollbackRequest
{
    ProjectName = "MyApplication",
    CurrentVersion = "2.1.0",
    TargetVersion = "2.0.5",
    TargetEnvironment = Environment.Production,
    RequestedBy = "vlad",
    Reason = "Critical regression in version 2.1.0",
    Channels = new List<NotificationChannel> { NotificationChannel.Slack, NotificationChannel.Discord },
    Priority = NotificationPriority.Critical
};

// Notify that a rollback has been initiated
var initiatedResults = await rollbackNotificationService.NotifyRollbackInitiatedAsync(rollbackRequest);
Console.WriteLine($"Initiated rollback notification sent to {initiatedResults.Count} channels");

// Simulate rollback completion
var rollbackResult = new RollbackResult
{
    RequestId = rollbackRequest.Id,
    ProjectName = rollbackRequest.ProjectName,
    RolledBackFromVersion = rollbackRequest.CurrentVersion,
    RolledBackToVersion = rollbackRequest.TargetVersion,
    Status = RollbackStatus.Completed
};

// Notify that the rollback completed successfully
var completedResults = await rollbackNotificationService.NotifyRollbackCompletedAsync(rollbackRequest, rollbackResult);
Console.WriteLine($"Completed rollback notification sent to {completedResults.Count} channels");

// Get rollback notification history for a project
var history = await rollbackNotificationService.GetRollbackNotificationHistoryAsync("MyApplication", limit: 10);
Console.WriteLine($"Found {history.Count} rollback notifications for MyApplication");

// Format a custom rollback message for a specific channel
var telegramMessage = rollbackNotificationService.FormatRollbackMessage(
    rollbackRequest,
    RollbackStatus.Completed,
    NotificationChannel.Telegram,
    "Rollback completed successfully in 2 minutes"
);
Console.WriteLine($"Telegram message: {telegramMessage}");

// Handle rollback failure
try
{
    // Some rollback operation that might fail...
}
catch (Exception ex)
{
    var failedResults = await rollbackNotificationService.NotifyRollbackFailedAsync(rollbackRequest, ex.Message);
    Console.WriteLine($"Failure notification sent to {failedResults.Count} channels");
}
```

## ITemplateService

The `ITemplateService` interface provides methods for rendering and validating notification message templates. It supports template rendering with deployment notification variables, template validation, and provides access to preset templates for common notification formats. The service enables consistent message formatting across different notification channels.

Example usage:

```csharp
// Create required logger
var logger = new Logger<TemplateService>(new LoggerFactory());

// Initialize the template service
var templateService = new TemplateService(logger);

// Get available template variables
var availableVariables = templateService.GetAvailableVariables();
Console.WriteLine($"Available variables: {string.Join(", ", availableVariables)}");

// Get preset templates
var presetTemplates = templateService.GetPresetTemplates();
Console.WriteLine($"Available templates: {string.Join(", ", presetTemplates.Keys)}");

// Create a deployment notification
var notification = new DeploymentNotification
{
    ProjectName = "MyApplication",
    Version = "2.0.0",
    Status = DeploymentStatus.Success,
    TargetEnvironment = "production",
    BranchName = "main",
    CommitHash = "abc123def456789",
    CommitAuthor = "vlad",
    Message = "Version 2.0.0 deployed successfully",
    DurationSeconds = 180,
    BuildUrl = "https://ci.example.com/build/123"
};

// Render a template
var template = presetTemplates["SuccessNotification"];
var renderedMessage = templateService.RenderTemplate(template, notification);
Console.WriteLine(renderedMessage);

// Validate a custom template
var customTemplate = "[{{Status}}] {{ProjectName}} v{{Version}} - {{Environment}}";
var (isValid, errors) = templateService.ValidateTemplate(customTemplate);
if (isValid)
{
    Console.WriteLine("Template is valid!");
}
else
{
    Console.WriteLine("Template validation errors:");
    foreach (var error in errors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Render HTML-safe version for web notifications
var htmlSafeMessage = templateService.RenderHtmlSafe(template, notification);
Console.WriteLine($"HTML-safe message: {htmlSafeMessage}");
```

## IRollbackService

The `IRollbackService` interface provides methods for initiating and managing deployment rollback operations. It enables one-click rollbacks to previous deployment versions with automatic notification dispatch to all configured channels (Slack, Discord, Telegram, etc.). The service tracks rollback status, maintains history, and provides cancellation capabilities for pending rollbacks.

Example usage:

```csharp
// Create required services
var dbContext = new NotificationDbContext(/* connection string */);
var logger = new Logger<RollbackService>(new LoggerFactory());

// Create repositories and services
var notificationRepository = new NotificationRepository(dbContext);
var configRepository = new ChannelConfigRepository(dbContext);
var resultRepository = new NotificationResultRepository(dbContext);
var webhookClientFactory = new WebhookClientFactory();
var dispatcher = new WebhookDispatcher(webhookClientFactory, logger);
var validationService = new ValidationService();

// Initialize the notification service
var notificationService = new NotificationService(
    notificationRepository,
    configRepository,
    resultRepository,
    dispatcher,
    validationService,
    logger
);

// Create the rollback service
var rollbackService = new RollbackService(notificationService, notificationRepository, logger);

// Create a rollback request
var rollbackRequest = new RollbackRequest
{
    ProjectName = "MyApplication",
    CurrentVersion = "2.1.0",
    TargetVersion = "2.0.5",
    TargetEnvironment = Environment.Production,
    RequestedBy = "vlad",
    Reason = "Critical regression in version 2.1.0 detected in production",
    Channels = new List<NotificationChannel> { NotificationChannel.Slack, NotificationChannel.Discord },
    Priority = NotificationPriority.Critical
};

// Initiate the rollback
var rollbackResult = await rollbackService.InitiateRollbackAsync(rollbackRequest);
Console.WriteLine($"Rollback initiated: {rollbackResult.Status}");
Console.WriteLine($"Rollback ID: {rollbackResult.Id}");
Console.WriteLine($"Rolled back from v{rollbackResult.RolledBackFromVersion} to v{rollbackResult.RolledBackToVersion}");

// Check rollback status
var status = await rollbackService.GetRollbackStatusAsync(rollbackResult.Id);
if (status != null)
{
    Console.WriteLine($"Rollback status: {status.Status}");
    Console.WriteLine($"Started at: {status.StartedAt}");
    Console.WriteLine($"Completed at: {status.CompletedAt}");
}

// Get rollback history for the project
var history = await rollbackService.GetRollbackHistoryAsync("MyApplication", limit: 10);
Console.WriteLine($"Found {history.Count} rollback operations for MyApplication");

// Cancel a pending rollback if needed
var wasCancelled = await rollbackService.CancelRollbackAsync(rollbackResult.Id);
if (wasCancelled)
{
    Console.WriteLine("Rollback was successfully cancelled");
}
```

## IntegrationTests

The `IntegrationTests` class contains comprehensive integration tests that verify the end-to-end functionality of the deployment notification system. These tests exercise the complete workflow from notification creation to delivery across various channels, ensuring all components work together correctly in real-world scenarios.

The integration tests cover:
- Complete end-to-end workflows from notification creation to successful delivery
- Multi-channel notification delivery to all configured channels
- Validation failure handling and exception throwing
- Retry mechanisms for failed deliveries with attempt tracking
- Concurrent processing of multiple notifications
- Channel filtering for only configured channels
- Main use case scenarios matching README documentation

Example usage:

```csharp
// Create required services and dependencies
var mockNotificationRepository = Substitute.For<INotificationRepository>();
var mockConfigRepository = Substitute.For<IChannelConfigRepository>();
var mockResultRepository = Substitute.For<INotificationResultRepository>();
var mockDispatcher = Substitute.For<IWebhookDispatcher>();
var mockValidationService = Substitute.For<IValidationService>();
var mockLogger = Substitute.For<ILogger<NotificationService>>();

// Initialize the notification service
var notificationService = new NotificationService(
    mockNotificationRepository,
    mockConfigRepository,
    mockResultRepository,
    mockDispatcher,
    mockValidationService,
    mockLogger);

// Create a deployment notification
var notification = new DeploymentNotification
{
    ProjectName = "MyApplication",
    Version = "2.0.0",
    Status = BuildStatus.Success,
    TargetEnvironment = Environment.Production,
    Message = "Version 2.0.0 deployed successfully",
    Channels = new List<NotificationChannel> { NotificationChannel.Slack, NotificationChannel.Discord }
};

// Validate and create the notification
var validationResult = validationService.ValidateNotification(notification);
if (validationResult.IsValid)
{
    var notificationId = await notificationService.CreateNotificationAsync(notification);
    
    // Send to all configured channels
    var deliveryResults = await notificationService.SendNotificationAsync(notificationId);
    
    // Check delivery status
    foreach (var result in deliveryResults)
    {
        if (result.IsSuccessful)
        {
            Console.WriteLine($"Successfully delivered to {result.Channel}");
        }
        else
        {
            Console.WriteLine($"Failed to deliver to {result.Channel}: {result.ErrorMessage}");
        }
    }
}
```

## IBatchNotificationService

The `IBatchNotificationService` interface provides methods for managing batch notifications, enabling efficient processing of multiple deployment notifications together. It supports creating batches, adding/removing notifications, sending batches to configured channels, and tracking batch statistics including delivery status and success rates.

Example usage:

```csharp
// Create required services
var dbContext = new NotificationDbContext(/* connection string */);
var logger = new Logger<NotificationService>(new LoggerFactory());

// Create repositories and services
var notificationRepository = new NotificationRepository(dbContext);
var configRepository = new ChannelConfigRepository(dbContext);
var resultRepository = new NotificationResultRepository(dbContext);
var webhookClientFactory = new WebhookClientFactory();
var dispatcher = new WebhookDispatcher(webhookClientFactory, logger);
var validationService = new ValidationService();

// Initialize the notification service
var notificationService = new NotificationService(
  notificationRepository,
  configRepository,
  resultRepository,
  dispatcher,
  validationService,
  logger
);

// Create the batch notification service
var batchNotificationService = new BatchNotificationService(notificationService, logger);

// Create a batch of notifications for a deployment
var batch = new BatchNotification
{
  Id = Guid.NewGuid().ToString(),
  Name = "Production Deployment Batch",
  Description = "Deployment notifications for version 2.0.0",
  Notifications = new List<DeploymentNotification>
  {
    new DeploymentNotification
    {
      Id = Guid.NewGuid().ToString(),
      ProjectName = "MyApplication",
      Version = "2.0.0",
      Status = DeploymentStatus.Success,
      TargetEnvironment = "production",
      Priority = NotificationPriority.High,
      Channels = new List<NotificationChannel> { NotificationChannel.Slack, NotificationChannel.Discord }
    },
    new DeploymentNotification
    {
      Id = Guid.NewGuid().ToString(),
      ProjectName = "MyApplication",
      Version = "2.0.0",
      Status = DeploymentStatus.Success,
      TargetEnvironment = "production",
      Priority = NotificationPriority.Normal,
      Channels = new List<NotificationChannel> { NotificationChannel.Telegram }
    }
  },
  Channels = new List<NotificationChannel> { NotificationChannel.Slack, NotificationChannel.Discord, NotificationChannel.Telegram },
  ScheduledAt = DateTime.UtcNow.AddMinutes(5)
};

// Create the batch
var batchId = await batchNotificationService.CreateBatchAsync(batch);
Console.WriteLine($"Created batch with ID: {batchId}");

// Add additional notifications to the batch
var additionalNotification = new DeploymentNotification
{
  Id = Guid.NewGuid().ToString(),
  ProjectName = "MyService",
  Version = "1.5.0",
  Status = DeploymentStatus.Success,
  TargetEnvironment = "production",
  Priority = NotificationPriority.Normal
};

await batchNotificationService.AddNotificationAsync(batchId, additionalNotification);

// Send the batch (will be sent at scheduled time)
var sendResults = await batchNotificationService.SendBatchAsync(batchId);
Console.WriteLine($"Sent batch with {sendResults.Count} delivery results");

// Get batch statistics
var stats = await batchNotificationService.GetBatchStatisticsAsync(batchId);
Console.WriteLine($"Batch Statistics:");
Console.WriteLine($"  Total notifications: {stats.NotificationCount}");
Console.WriteLine($"  Total delivery targets: {stats.TotalDeliveryTargets}");
Console.WriteLine($"  Successful deliveries: {stats.SuccessfulDeliveries}");
Console.WriteLine($"  Failed deliveries: {stats.FailedDeliveries}");
Console.WriteLine($"  Success rate: {stats.SuccessRate:P0}");
Console.WriteLine($"  Progress: {stats.ProgressPercentage:F1}%");

// Get all pending batches
var pendingBatches = await batchNotificationService.GetPendingBatchesAsync();
Console.WriteLine($"Found {pendingBatches.Count} pending batches");

// Cancel a batch if needed
await batchNotificationService.CancelBatchAsync(batchId);
```

## NotificationBuilder

The `NotificationBuilder` class provides a fluent interface for constructing `DeploymentNotification` instances with a clean, readable API. It supports setting all notification properties including project information, status, environment, build details, channels, priority, and metadata through method chaining.

Example usage:

```csharp
// Create a notification using the builder
var notification = new NotificationBuilder()
    .WithProject("MyApplication", "2.0.0")
    .WithStatus(BuildStatus.Success, "Deployment completed successfully")
    .WithEnvironment(Environment.Production)
    .WithBranch("main", "abc123def456", "vlad")
    .WithRepository("https://github.com/myorg/MyApplication")
    .WithBuildUrl("https://ci.example.com/build/123")
    .WithDuration(180)
    .WithChannels(NotificationChannel.Slack, NotificationChannel.Discord)
    .WithPriority(NotificationPriority.High)
    .WithMessage("✅ MyApplication v2.0.0 deployed successfully to production")
    .WithMetadata("buildNumber", "123")
    .WithMetadata("deployedBy", "vlad")
    .Build();

Console.WriteLine($"Created notification for {notification.ProjectName} v{notification.Version}");

// Use convenience methods for common scenarios
var successNotification = new NotificationBuilder()
    .WithProject("MyService", "1.5.2")
    .WithEnvironment(Environment.Staging)
    .AsSuccess()
    .WithMessage("✅ MyService v1.5.2 deployed to staging")
    .WithChannels(NotificationChannel.Slack)
    .Build();

var failureNotification = new NotificationBuilder()
    .WithProject("MyService", "1.5.2")
    .AsFailure()
    .WithMessage("❌ Build failed: Unit tests failed")
    .WithChannels(NotificationChannel.Slack, NotificationChannel.Discord)
    .Build();

var deploymentSuccess = new NotificationBuilder()
    .WithProject("MyApp", "3.0.0")
    .WithEnvironment(Environment.Production)
    .AsDeploymentSuccess()
    .WithMessage("🚀 MyApp v3.0.0 deployed to production")
    .WithBuildUrl("https://ci.example.com/build/456")
    .WithDuration(245)
    .WithChannels(NotificationChannel.Telegram)
    .Build();
```

## ServiceCollectionExtensions

The `ServiceCollectionExtensions` class provides extension methods for configuring application services in the dependency injection container. It offers a comprehensive set of methods to register all core services including CLI support, caching, formatting, serialization, event bus, middleware, integration, and background workers. The extension methods follow the standard Microsoft.Extensions.DependencyInjection pattern and return the `IServiceCollection` for method chaining.

For more granular control, the `ServiceConfigurationBuilder` fluent API allows you to selectively enable services using a builder pattern. This approach provides better readability and makes it easier to compose only the services your application needs.

Example usage with individual service registration:

```csharp
// Configure services in your Program.cs or Startup.cs
var services = new ServiceCollection();

// Register CLI services
services.AddCliServices();

// Register caching services
services.AddCachingServices();

// Register formatting services
services.AddFormattingServices();

// Register serialization services
services.AddSerializationServices();

// Register event bus services
services.AddEventBusServices();

// Register middleware services
services.AddMiddlewareServices();

// Register integration services
services.AddIntegrationServices();

// Register background workers
services.AddBackgroundWorkers();

// Build the service provider
var serviceProvider = services.BuildServiceProvider();

// Resolve and use services
var commandParser = serviceProvider.GetRequiredService<CommandParser>();
var cacheService = serviceProvider.GetRequiredService<ICacheService>();
var eventBus = serviceProvider.GetRequiredService<IEventBus>();
```

Example usage with the fluent ServiceConfigurationBuilder:

```csharp
// Configure services using the fluent builder pattern
var services = new ServiceCollection();

var configuredServices = new ServiceConfigurationBuilder(services)
    .WithCliSupport()
    .WithCaching()
    .WithFormatting()
    .WithSerialization()
    .WithEventBus()
    .WithMiddleware()
    .WithIntegration()
    .WithBackgroundWorkers()
    .Build();

// Or use the convenience method to register all services
var allServices = new ServiceCollection();
new ServiceConfigurationBuilder(allServices).WithAll().Build();

// Configure HTTP client with custom timeout
services.AddConfiguredHttpClient("webhookClient", timeoutSeconds: 60);

// Build the service provider
var serviceProvider = services.BuildServiceProvider();

// Configure the notification pipeline
var pipeline = serviceProvider.GetRequiredService<NotificationPipeline>();
pipeline.ConfigureNotificationPipeline(serviceProvider);

// Register event handlers
var eventBus = serviceProvider.GetRequiredService<IEventBus>();
services.RegisterEventHandlers(eventBus, serviceProvider);
```

## CommandParser

The `CommandParser` class provides command-line argument parsing with support for subcommands, options, and flags. It enables structured parsing of CLI arguments into typed command definitions with parameters and options, supporting both short (`-v`) and long (`--verbose`) option formats, required parameter validation, and automatic help text generation.

The parser supports built-in commands like `send`, `list`, `config`, `health`, and `history`, each with their own parameters and options. It returns a `ParsedCommand` object containing the extracted values, success status, and any error messages.

Example usage:

```csharp
// Create parser with logger
var logger = new Logger<CommandParser>(new LoggerFactory());
var commandParser = new CommandParser(logger);

// Parse command-line arguments
var args = new[] { "send", "my-app", "1.0.0", "--status", "success", "--environment", "production", "-c", "Slack,Discord" };
var parsedCommand = commandParser.Parse(args);

if (parsedCommand.Success)
{
 Console.WriteLine($"Command '{parsedCommand.CommandName}' parsed successfully!");
 Console.WriteLine($"Project: {parsedCommand.GetParameter("project")}");
 Console.WriteLine($"Version: {parsedCommand.GetParameter("version")}");
 Console.WriteLine($"Status: {parsedCommand.GetOption("status")}");
 Console.WriteLine($"Environment: {parsedCommand.GetOption("environment")}");
 Console.WriteLine($"Channels: {parsedCommand.GetOption("channels")}");
}
else
{
 Console.WriteLine($"Error parsing command: {parsedCommand.Error}");
}

// Get help text
var helpText = commandParser.GetHelpText();
Console.WriteLine(helpText);

// Get command-specific help
var sendHelp = commandParser.GetCommandHelpText("send");
Console.WriteLine(sendHelp);
```

## IAuditService

The `IAuditService` interface provides comprehensive auditing capabilities for tracking all notification-related operations within the system. It records detailed audit logs for notification creation, delivery attempts, configuration changes, validation failures, and system operations, enabling complete traceability and compliance monitoring across all notification channels.

Audit logs include timestamps, operation types, entity information, actors, statuses, and extensible metadata for detailed tracking of system activities.

Example usage:

```csharp
// Create required logger
var logger = new Logger<IAuditService>(new LoggerFactory());

// Create audit service
var auditService = new AuditService(logger);

// Log a notification creation event
var notificationId = Guid.NewGuid().ToString();
await auditService.LogNotificationCreatedAsync(
    notificationId,
    "DeploymentNotification",
    "vlad",
    new Dictionary<string, object>
    {
        { "projectName", "MyApplication" },
        { "version", "2.0.0" },
        { "targetEnvironment", "production" },
        { "priority", "High" }
    }
);

// Log a delivery attempt
var deliveryAttempt = await auditService.LogDeliveryAttemptAsync(
    notificationId,
    NotificationChannel.Slack,
    true,
    150,
    "Webhook delivered successfully",
    new Dictionary<string, object>
    {
        { "responseCode", 200 },
        { "retryCount", 0 }
    }
);

// Log a configuration change
await auditService.LogConfigurationChangeAsync(
    "ChannelStrategyResolver",
    "Configuration updated: Added new Slack webhook",
    new Dictionary<string, object>
    {
        { "oldWebhookUrl", "https://hooks.slack.com/old" },
        { "newWebhookUrl", "https://hooks.slack.com/new" },
        { "channelType", NotificationChannel.Slack }
    },
    "vlad"
);

// Log a validation failure
await auditService.LogValidationFailureAsync(
    notificationId,
    "DeploymentNotification",
    "Invalid project name format",
    new Dictionary<string, object>
    {
        { "projectName", "Invalid@Project!" },
        { "validationRule", "alphanumeric with hyphens" }
    }
);

// Retrieve audit logs for a specific notification
var notificationLogs = await auditService.GetAuditLogsAsync(notificationId);
Console.WriteLine($"Found {notificationLogs.Count} audit entries for notification {notificationId}");

// Retrieve notification-specific audit logs
var notificationAuditLogs = await auditService.GetNotificationAuditLogsAsync(notificationId);
foreach (var log in notificationAuditLogs)
{
    Console.WriteLine($"[{log.Timestamp:yyyy-MM-dd HH:mm:ss}] {log.Operation}: {log.Status}");
    Console.WriteLine($"  Entity: {log.EntityType}/{log.EntityId}, Actor: {log.Actor}");
}

// Clear old audit logs (older than 90 days)
await auditService.ClearOldLogsAsync(TimeSpan.FromDays(90));
Console.WriteLine("Old audit logs cleared successfully");
```

## BuildStatusConverter

The `BuildStatusConverter` class is a custom JSON converter that handles serialization and deserialization of the `BuildStatus` enum values. It provides proper conversion between enum values and their string representations in JSON format, enabling consistent JSON serialization for deployment status values throughout the application.

The converter supports case-insensitive parsing and defaults to `BuildStatus.Started` when encountering null or empty values during deserialization.

Example usage:

```csharp
// Create JSON serializer options with the BuildStatusConverter
var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    Converters = { new BuildStatusConverter() }
};

// Serialize a BuildStatus enum to JSON
var buildStatus = BuildStatus.Success;
string json = JsonSerializer.Serialize(buildStatus, options);
// Output: "Success"

// Deserialize JSON back to BuildStatus enum
string jsonInput = "\"Failed\"";
BuildStatus deserializedStatus = JsonSerializer.Deserialize<BuildStatus>(jsonInput, options);
// deserializedStatus = BuildStatus.Failed

// Using with JsonSerializationHelper
var helper = new JsonSerializationHelper();

// Serialize an object containing BuildStatus
var deployment = new { Status = BuildStatus.Started };
string serialized = helper.Serialize(deployment);

// Deserialize JSON containing BuildStatus
var result = helper.Deserialize<Dictionary<string, object>>(serialized);
// result["Status"] will be BuildStatus.Started

// Safe parsing with TryParse
var (success, parsedStatus) = SafeJsonParser.TryParse<BuildStatus>("\"Completed\"");
if (success) { /* use parsedStatus */ }

// Merge JSON objects containing BuildStatus
string merged = SafeJsonParser.MergeJsonObjects(
    "{\"status\":\"Started\"}",
    "{\"status\":\"Success\"}"
);
```

## DateTimeExtensions

The `DateTimeExtensions` class provides a comprehensive set of extension methods for working with `DateTime` values in .NET applications. These methods enable common date/time operations including relative time formatting, ISO string conversion, formatted string output, temporal comparisons, Unix timestamp conversion, and date/time rounding to nearest minute or hour boundaries. The extension methods support both UTC and local time zones and provide utility for consistent date/time handling throughout the application.

Example usage:

```csharp
// Create a deployment timestamp
var deploymentTime = DateTime.UtcNow.AddMinutes(-45);

// Convert to relative time string (e.g., "2 hours ago", "5 minutes ago")
string relativeTime = deploymentTime.ToRelativeTimeString();
Console.WriteLine($"Deployment happened {relativeTime}"); // "Deployment happened 45 minutes ago"

// Convert to ISO 8601 string format
string isoTime = deploymentTime.ToIsoString();
Console.WriteLine($"ISO timestamp: {isoTime}"); // "2024-07-16T14:30:00Z"

// Convert to formatted string with custom format
string formattedTime = deploymentTime.ToFormattedString("yyyy-MM-dd HH:mm:ss");
Console.WriteLine($"Formatted: {formattedTime}"); // "2024-07-16 14:30:00"

// Check if a datetime is in the past or future
bool isPast = deploymentTime.IsPast();
bool isFuture = deploymentTime.IsFuture();
Console.WriteLine($"Is past: {isPast}, Is future: {isFuture}");

// Calculate elapsed time in minutes and seconds
int minutesElapsed = deploymentTime.GetMinutesElapsed();
int secondsElapsed = deploymentTime.GetSecondsElapsed();
Console.WriteLine($"Elapsed: {minutesElapsed} minutes, {secondsElapsed} seconds");

// Round to nearest minute or hour
var roundedToMinute = deploymentTime.RoundToNearestMinute();
var roundedToHour = deploymentTime.RoundToNearestHour();
Console.WriteLine($"Rounded to minute: {roundedToMinute:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine($"Rounded to hour: {roundedToHour:yyyy-MM-dd HH:mm:ss}");

// Get start/end of day, week, or month
var startOfDay = deploymentTime.GetStartOfDay();
var endOfDay = deploymentTime.GetEndOfDay();
var startOfWeek = deploymentTime.GetStartOfWeek();
var startOfMonth = deploymentTime.GetStartOfMonth();
var endOfMonth = deploymentTime.GetEndOfMonth();

Console.WriteLine($"Start of day: {startOfDay:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine($"End of day: {endOfDay:yyyy-MM-dd HH:mm:ss}");

// Check if a date is today or yesterday
bool isToday = deploymentTime.IsToday();
bool isYesterday = deploymentTime.IsYesterday();
Console.WriteLine($"Is today: {isToday}, Is yesterday: {isYesterday}");

// Convert between DateTime and Unix timestamp
long unixTimestamp = deploymentTime.ToUnixTimestamp();
var fromUnix = DateTimeExtensions.FromUnixTimestamp(unixTimestamp);
Console.WriteLine($"Unix timestamp: {unixTimestamp}");
Console.WriteLine($"From Unix: {fromUnix:yyyy-MM-dd HH:mm:ss}");

// Calculate business days between two dates
var futureDate = DateTime.UtcNow.AddDays(7);
int businessDays = deploymentTime.GetBusinessDaysBetween(futureDate);
Console.WriteLine($"Business days between: {businessDays}");
```

## ObjectExtensions

The `ObjectExtensions` class provides a comprehensive set of extension methods for working with objects in a safe and functional way. These methods enable common object operations like null checking, type casting, property access, mapping, copying, and validation without modifying the original objects. The extension methods handle null values gracefully and provide utility for consistent object handling throughout the application.

Example usage:

```csharp
// Create a sample deployment configuration object
var deploymentConfig = new DeploymentNotification
{
    ProjectName = "MyApplication",
    Version = "2.0.0",
    Status = DeploymentStatus.Success,
    TargetEnvironment = "production",
    Priority = NotificationPriority.High,
    Message = "Version 2.0.0 deployed successfully"
};

// Null checking
if (deploymentConfig.IsNotNull())
{
    Console.WriteLine($"Processing deployment: {deploymentConfig.ProjectName}");
}

if (deploymentConfig.IsNull())
{
    Console.WriteLine("Configuration is null");
}

// Safe casting
var configObject = (object)deploymentConfig;
var castConfig = configObject.SafeCast<DeploymentNotification>();
Console.WriteLine($"Cast successful: {castConfig?.ProjectName}");

// Property access
var projectName = deploymentConfig.GetPropertyValue("ProjectName");
Console.WriteLine($"Project: {projectName}");

// Set property value
deploymentConfig.SetPropertyValue("Message", "Version 2.0.0 deployed successfully to production");

// Map to another type
var configDict = deploymentConfig.Map(config => config.ToDictionary());
Console.WriteLine($"Config has {configDict?.Count} properties");

// Shallow copy
var configCopy = deploymentConfig.ShallowCopy();
Console.WriteLine($"Copy created: {configCopy?.ProjectName}");

// Check if equals any value
deploymentConfig.Priority.EqualsAny(NotificationPriority.Low, NotificationPriority.Normal, NotificationPriority.High);

// Check if default
int defaultValue = 0;
bool isDefault = defaultValue.IsDefault(); // Returns true

// Get value or default
int? nullableValue = null;
int result = nullableValue.GetValueOrDefault(42); // Returns 42

// Chain operations
deploymentConfig
    .Chain(c => Console.WriteLine($"Chained: {c.ProjectName}"))
    .Chain(c => c.Validate(x => !string.IsNullOrEmpty(x.ProjectName)));

// Convert to string safely
string safeString = deploymentConfig.ToStringSafe();
string nullString = ((object?)null).ToStringSafe("default"); // Returns "default"

// Get type information
string typeName = deploymentConfig.GetTypeName();
string fullTypeName = deploymentConfig.GetFullTypeName();
```

## WebhookPayloadValidation

`WebhookPayloadValidation` provides extension methods for the `WebhookPayload` class to validate its contents against required fields, formats (like GUIDs, semantic versions, UTC timestamps), and structural rules. It allows developers to check for issues, determine validity, or enforce validity by throwing exceptions when payloads are malformed or invalid.

Example usage:

```csharp
using DotNetDeployNotify.Core.Models;

var payload = new WebhookPayload
{
    EventId = Guid.NewGuid().ToString(),
    EventType = "DeploymentStarted",
    Timestamp = DateTime.UtcNow,
    Source = "CI/CD Pipeline",
    SchemaVersion = "1.0.0",
    Data = new WebhookData
    {
        ProjectName = "MyProject",
        Version = "1.0.0",
        Status = "success"
    }
};

// Check if valid
if (payload.IsValid())
{
    // Process payload
}

// Ensure valid (throws ArgumentException if invalid)
payload.EnsureValid();

// Get validation errors
var errors = payload.Validate();
foreach (var error in errors)
{
    Console.WriteLine(error);
}
```

## BatchNotificationExtensionsJsonExtensions

The `BatchNotificationExtensionsJsonExtensions` class provides utility methods for serializing and deserializing metadata information about the `BatchNotificationExtensions` class. It simplifies exporting and importing type-related information, such as namespace, assembly, and available methods, as JSON.

Example usage:

```csharp
using DotNetDeployNotify.Core.Models;

// Serialize BatchNotificationExtensions metadata to JSON
string json = BatchNotificationExtensionsJsonExtensions.ToJson();
Console.WriteLine(json);

// Deserialize from JSON
if (BatchNotificationExtensionsJsonExtensions.TryFromJson(json, out var metadata))
{
    Console.WriteLine($"Type: {metadata.Type}");
    Console.WriteLine($"Assembly: {metadata.Assembly}");
    Console.WriteLine($"Methods: {string.Join(", ", metadata.Methods)}");
}
```

## License

MIT License - see LICENSE file for details.
