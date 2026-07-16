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

## License

MIT License - see LICENSE file for details.
