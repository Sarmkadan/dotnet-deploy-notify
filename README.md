# DotNetDeployNotify

A comprehensive deployment notification pipeline for .NET applications. Send build status updates to Telegram, Slack, Discord, and custom webhooks with full support for retries, validation, metrics, and batch processing.

## Features

**Multi-Channel Support**
- Telegram messaging
- Slack webhooks
- Discord webhooks
- Generic HTTP webhooks
- Email notifications (extensible)

**Robust Delivery**
- Automatic retry with exponential backoff
- Configurable timeouts and retry policies
- Dead-letter handling for failed deliveries
- Request/response logging for debugging

**Monitoring & Metrics**
- Real-time health checks
- Delivery metrics and analytics
- Channel-specific performance tracking
- Request/response history logging
- Audit logging of all operations

**Flexible Configuration**
- Channel filtering by environment and build status
- Priority-based routing
- Per-channel customization
- Template rendering for custom messages
- Settings validation with suggestions

**Batch Processing**
- Group multiple notifications
- Scheduled batch delivery
- Batch progress tracking
- Partial success handling

## Architecture

```
src/
├── Core/                 # Domain models and contracts
│   ├── Enums.cs         # BuildStatus, NotificationChannel, etc.
│   ├── Models/          # Data models
│   │   ├── DeploymentNotification.cs
│   │   ├── ChannelConfiguration.cs
│   │   ├── NotificationResult.cs
│   │   ├── WebhookPayload.cs
│   │   └── BatchNotification.cs
│   └── Exceptions/      # Custom exception types
│       └── NotificationException.cs
│
├── Services/            # Business logic layer
│   ├── NotificationService.cs       # Main orchestrator
│   ├── WebhookDispatcher.cs         # HTTP delivery
│   ├── PayloadBuilder.cs            # Format-specific payloads
│   ├── ValidationService.cs         # Data validation
│   ├── TemplateService.cs           # Message templating
│   ├── NotificationProcessor.cs     # Batch processing
│   ├── HealthCheckService.cs        # System health monitoring
│   ├── AuditService.cs              # Audit logging
│   ├── MetricsService.cs            # Analytics
│   └── BatchNotificationService.cs  # Batch management
│
├── Data/                # Data access layer
│   └── Repositories.cs  # In-memory repository implementations
│       ├── INotificationRepository
│       ├── IChannelConfigRepository
│       └── INotificationResultRepository
│
└── Infrastructure/      # Configuration and utilities
    ├── DependencyInjection.cs      # IoC setup
    ├── Constants.cs                # App constants
    ├── ServiceExtensions.cs        # Extension methods
    ├── RequestLogger.cs            # HTTP request logging
    └── ConfigurationValidator.cs   # Config validation
```

## Services

### Core Services

**NotificationService** (`INotificationService`)
- Create and manage deployment notifications
- Send notifications to configured channels
- Retrieve notification history
- Retry failed deliveries

**WebhookDispatcher** (`IWebhookDispatcher`)
- HTTP webhook delivery with timeout handling
- Webhook validation for connectivity checks
- Automatic retry logic with exponential backoff

**PayloadBuilder** (`IPayloadBuilder`)
- Platform-specific payload formatting
- Slack Block Kit formatting
- Discord Embed formatting
- Telegram HTML formatting

### Utility Services

**ValidationService** (`IValidationService`)
- Notification and configuration validation
- URL and email format checking
- Detailed error reporting

**NotificationProcessor** (`INotificationProcessor`)
- Batch processing of pending notifications
- Priority-based processing
- Failed notification retry handling

**HealthCheckService** (`IHealthCheckService`)
- System-wide health status
- Per-channel health metrics
- Connectivity validation

**AuditService** (`IAuditService`)
- Operation audit logging
- Event history tracking
- Configurable retention

**MetricsService** (`IMetricsService`)
- Delivery metrics collection
- Per-channel analytics
- Performance tracking (P95, P99 latency)

**TemplateService** (`ITemplateService`)
- Message template rendering
- Variable substitution
- Preset templates for common scenarios

**BatchNotificationService** (`IBatchNotificationService`)
- Batch creation and management
- Scheduled batch delivery
- Batch statistics tracking

## Key Models

### DeploymentNotification
- Core entity representing a deployment event
- Properties: ProjectName, Version, Status, Environment, BranchName, CommitHash, etc.
- Channels: List of NotificationChannel targets
- Priority levels for routing
- Metadata for custom data

### ChannelConfiguration
- Webhook URL and authentication
- Channel-specific filtering (environment, status, priority)
- Retry and timeout settings
- Custom headers for platform-specific requirements
- Message formatting options

### NotificationResult
- Delivery attempt record
- Status tracking (Delivered, Failed, Timeout, Skipped)
- HTTP status codes and response bodies
- Duration metrics
- Automatic retry scheduling

## Getting Started

### Prerequisites
- .NET 10 SDK
- HTTP access to webhook endpoints (Telegram, Slack, Discord, etc.)

### Building

```bash
cd /tmp/oss-projects/dotnet-deploy-notify
dotnet build
```

### Running

```bash
dotnet run
```

The application will:
1. Load configuration from `appsettings.json`
2. Setup sample Telegram, Slack, and Discord configurations
3. Create demo notifications
4. Process and deliver them to configured channels
5. Display delivery results

### Configuration

Edit `appsettings.json`:

```json
{
  "NotificationService": {
    "MaxRetries": 3,
    "WebhookTimeoutMs": 10000,
    "RetryDelayMs": 5000,
    "AutoProcessNotifications": true,
    "ProcessingIntervalSeconds": 30,
    "EnableAuditLogging": true,
    "RetentionDays": 30
  }
}
```

## Extending

### Adding a New Channel

1. Create a channel type in `Core/Enums.cs`
2. Implement formatting in `Services/PayloadBuilder.cs`
3. Add validation in `Services/ValidationService.cs`
4. Create channel configuration in your application

### Custom Validation Rules

Implement `IValidationService` or extend `ValidationService` to add domain-specific validation.

### Custom Message Templates

Use `TemplateService.RenderTemplate()` with available variables:
- `{{ProjectName}}`
- `{{Version}}`
- `{{Status}}`
- `{{Environment}}`
- `{{Branch}}`
- `{{CommitHashShort}}`
- `{{CommitAuthor}}`
- And more...

## Testing

The included demo in `Program.cs` creates sample notifications and sends them to configured channels. Monitor output to verify delivery.

## Project Statistics

- **Files**: 27
- **Lines of Code**: 5,325+
- **Service Classes**: 10
- **Repository Classes**: 3
- **Model Classes**: 8
- **Exception Types**: 5

## License

MIT © 2026 Vladyslav Zaiets

See LICENSE file for details.
