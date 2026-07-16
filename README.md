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

## License

MIT License - see LICENSE file for details.
