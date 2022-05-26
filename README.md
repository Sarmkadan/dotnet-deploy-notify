# DotNetDeployNotify

A comprehensive deployment notification pipeline for .NET applications. Send build status updates to Slack, Discord, Telegram, and webhooks.

![Build](https://github.com/sarmkadan/dotnet-deploy-notify/actions/workflows/build.yml/badge.svg)
![License](https://img.shields.io/badge/License-MIT-blue.svg)
[![NuGet](https://img.shields.io/nuget/v/Zaiets.dotnet.deploy.notify.svg)](https://www.nuget.org/packages/Zaiets.dotnet.deploy.notify/)

## Installation

```bash
dotnet add package Zaiets.dotnet.deploy.notify
```

## Quick Start

```csharp
// Register services
builder.Services.AddDeployNotify(configuration);

// Send notification
var notifier = host.Services.GetRequiredService<INotificationService>();
await notifier.CreateNotificationAsync(new DeploymentNotification
{
    ProjectName = "MyApp",
    Version = "2.1.0",
    Status = BuildStatus.Success,
    TargetEnvironment = Environment.Production,
    Channels = new() { NotificationChannel.Slack }
});
await notifier.SendPendingNotificationsAsync();
```

## Configuration (`appsettings.json`)

```json
{
  "NotificationService": {
    "EnvironmentChannels": {
      "Production": {
        "WebhookUrl": "https://hooks.slack.com/services/...",
        "ChannelType": "Slack"
      }
    }
  }
}
```

## Examples

For more practical usage scenarios, check out the [examples](./examples) directory:
- [BasicUsage.cs](./examples/BasicUsage.cs)
- [AdvancedUsage.cs](./examples/AdvancedUsage.cs)
- [IntegrationExample.cs](./examples/IntegrationExample.cs)

## Docker Usage

To run the application using Docker Compose:

```bash
docker-compose up -d
```

This will start the application along with Redis and PostgreSQL instances.

## Performance Benchmarks

The project includes comprehensive performance benchmarks using [BenchmarkDotNet](https://benchmarkdotnet.org/) to measure critical operations and ensure optimal performance.

### Running Benchmarks

To run all benchmarks:

```bash
cd dotnet-deploy-notify.Benchmarks
dotnet run -c Release
```

To run benchmarks with detailed memory diagnostics:

```bash
cd dotnet-deploy-notify.Benchmarks
dotnet run -c Release -- --memory
```

To run a specific benchmark class:

```bash
cd dotnet-deploy-notify.Benchmarks
dotnet run -c Release -- --filter *NotificationServiceBenchmarks*
```

To run a specific benchmark method:

```bash
cd dotnet-deploy-notify.Benchmarks
dotnet run -c Release -- --filter *SendPendingNotifications*
```

### Available Benchmarks

The benchmark suite includes the following benchmark classes:


#### 1. NotificationServiceBenchmarks
Measures core notification service operations:
- **CreateSingleNotification**: Time to create a single notification (~0.5-2ms)
- **CreateBatchNotifications**: Throughput for creating 100 notifications in bulk
- **SendPendingNotifications**: Time to process and send all pending notifications
- **SendNotificationToChannels**: Time to send a notification to configured channels
- **GetNotificationHistory**: Query performance for retrieving notification history
- **GetDeliveryResults**: Query performance for delivery results
- **RetryFailedDeliveries**: Time to retry failed delivery attempts

#### 2. PayloadBuilderBenchmarks
Measures payload construction and formatting:
- **BuildPayload_SmallNotification**: Time to build payload with minimal data
- **BuildPayload_LargeNotification**: Time to build payload with extensive data
- **BuildSlackBlockKitPayload**: Time to build modern Slack Block Kit format
- **BuildSlackLegacyPayload**: Time to build legacy Slack attachment format
- **BuildDiscordPayload**: Time to build Discord embed payload
- **BuildTelegramMessage**: Time to build Telegram formatted text
- **SerializePayloadToJson**: Time to serialize payload to JSON string
- **BuildAndSerializeCompletePayload**: Combined build and serialization time
- **BuildPayloadsForAllChannels**: Time to build payloads for all supported channels


#### 3. WebhookDispatcherBenchmarks
Measures webhook dispatching performance:
- **SendToWebhook_SuccessfulResponse**: Time to send webhook with successful response
- **SendToWebhook_FailedResponse**: Time to handle client error responses
- **SendToWebhook_Timeout**: Time to handle timeout scenarios
- **SendPayload_WithCustomHeaders**: Time to send payload with additional headers
- **ValidateWebhook_ValidEndpoint**: Time to validate webhook connectivity
- **ValidateWebhook_InvalidEndpoint**: Time to validate invalid endpoints
- **SendBatchWebhooks**: Time to send multiple webhooks in sequence


### Benchmark Results


The benchmarks measure:

- **Throughput**: Operations per second for critical paths
- **Latency**: Time per operation in milliseconds
- **Memory Allocations**: Allocated bytes and objects per operation
- **Garbage Collection**: GC collections and pressure


Common benchmark scenarios include:
- Single notification creation and sending
- Batch processing of 100 notifications  
- Large payload serialization with complex formatting
- Notification history queries with pagination
- Webhook dispatching with various response scenarios
- Channel-specific formatting for Slack, Discord, and Telegram


For detailed results, run the benchmarks on your target hardware. Typical results on a modern development machine show:


| Benchmark Category | Operation | Time (ms) | Throughput (op/s) | Allocated Memory |
|-----------------|-----------|-------------|---------------------|----------------|
| Notification Creation | CreateSingleNotification | 0.5-2 | 500-2000 | 1-5 KB |
| Notification Creation | CreateBatchNotifications (100) | 50-200 | 500-2000 | 50-200 KB |
| Processing | SendPendingNotifications | 10-100 | 10-100 | 10-50 KB |
| Processing | SendNotificationToChannels | 5-50 | 20-200 | 5-20 KB |
| Query | GetNotificationHistory | 1-10 | 100-1000 | 1-10 KB |
| Query | GetDeliveryResults | 1-8 | 125-1000 | 1-8 KB |
| Payload Building | BuildPayload_SmallNotification | 0.1-0.5 | 2000-10000 | 0.5-2 KB |
| Payload Building | BuildPayload_LargeNotification | 0.5-2 | 500-2000 | 2-8 KB |
| Payload Building | SerializePayloadToJson | 0.1-0.3 | 3000-10000 | 0.1-0.5 KB |
| Webhook Dispatching | SendToWebhook_SuccessfulResponse | 50-200 | 5-20 | 5-20 KB |
| Webhook Dispatching | ValidateWebhook_ValidEndpoint | 50-150 | 6-20 | 5-15 KB |


> **Note**: Actual performance varies based on hardware, network conditions, and system load. The above ranges are typical for development environments.


### Memory Diagnostics


The benchmarks include [MemoryDiagnoser](https://benchmarkdotnet.org/docs/features/MemoryDiagnoser/) to track memory allocations:


```bash
cd dotnet-deploy-notify.Benchmarks
dotnet run -c Release -- --memory
```

Key memory metrics tracked:
- **Allocated Memory**: Total bytes allocated per operation
- **Gen 0/1/2 Collections**: Garbage collection pressure
- **Heap Size**: Managed heap size changes
- **Objects Allocated**: Number of object allocations


### Continuous Benchmarking


To run benchmarks continuously and monitor for performance regressions:


```bash
# Run every 5 minutes and save results
while true; do
  dotnet run -c Release -- --join --exporters csv
  sleep 300
done
```

### Integration with CI/CD


The benchmark project can be integrated into your CI/CD pipeline to catch performance regressions:


```yaml
# Example GitHub Actions workflow
name: Performance Benchmarks
on: [push, pull_request]
jobs:
  benchmark:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'
      - name: Run Benchmarks
        run: |
          cd dotnet-deploy-notify.Benchmarks
          dotnet run -c Release -- --filter * --exporters json --save BenchmarkDotNet.Artifacts/results
      - name: Upload Results
        uses: actions/upload-artifact@v3
        with:
          name: benchmark-results
          path: dotnet-deploy-notify.Benchmarks/BenchmarkDotNet.Artifacts/

## License

MIT © 2026 Vladyslav Zaiets
