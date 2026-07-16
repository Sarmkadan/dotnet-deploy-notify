# dotnet-deploy-notify

A .NET application for sending deployment notifications to various channels (Slack, Discord, Telegram, etc.).

## Features

- Send deployment notifications to multiple channels
- Support for Slack, Discord, and Telegram webhooks
- Batch notification processing
- Configurable channel strategies
- Integration with deployment pipelines

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


## Configuration

See `appsettings.example.json` for configuration examples.

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

## License

MIT License - see LICENSE file for details.
