# DotNetDeployNotify

A .NET-based notification system for deployment events and operational alerts.
It formats deployment and rollback events for Slack, Telegram, Discord and generic
webhooks, applies per-channel filters, and can preview payloads without sending them.

## Quickstart

Requires the .NET 10 SDK.

```bash
# restore, build and run the test suite
dotnet build dotnet-deploy-notify.sln
dotnet test dotnet-deploy-notify.sln
```

Send a deployment notification from the CLI:

```bash
dotnet run --project dotnet-deploy-notify.csproj -- \
  send Checkout.Api 3.2.1 \
  --status DeploymentSuccess \
  --environment Production \
  --channels Slack,Telegram \
  --message "Deploy finished"
```

Preview exactly what would be sent, without dispatching anything, by adding
`--dry-run`. It renders the channel-specific payload (JSON for Slack/Discord/webhook,
HTML text for Telegram), masks any token embedded in the target URL, and shows which
channels a filter would suppress:

```bash
dotnet run --project dotnet-deploy-notify.csproj -- \
  send Checkout.Api 3.2.1 --status DeploymentSuccess --channels Slack --dry-run
```

Initiate a rollback (also supports `--dry-run`):

```bash
dotnet run --project dotnet-deploy-notify.csproj -- \
  rollback Checkout.Api 3.1.0 \
  --current-version 3.2.0 --environment Production \
  --channels Slack --reason "elevated 5xx" --dry-run
```

Wire the services into your own host through dependency injection:

```csharp
var services = new ServiceCollection();
services.AddLogging();
services.AddNotificationServices(configuration); // extension in DotNetDeployNotify.Infrastructure

var provider = services.BuildServiceProvider();
var notifications = provider.GetRequiredService<INotificationService>();
```

## Channel configuration reference

Channels are configured under the `DotnetDeployNotify:Notification:EnvironmentChannels`
section of `appsettings.json` (see `appsettings.example.json`). Each entry maps an
environment name to a channel:

```json
{
  "DotnetDeployNotify": {
    "Notification": {
      "MaxRetries": 3,
      "WebhookTimeoutMs": 10000,
      "IncludeCommitDetails": true,
      "IncludeBuildUrl": true,
      "DefaultPriority": "Normal",
      "EnvironmentChannels": {
        "Production": {
          "WebhookUrl": "https://hooks.slack.com/services/T000/B000/XXXX",
          "ChannelType": "Slack",
          "DisplayName": "Production Alerts",
          "TargetId": "prod-channel-id"
        },
        "Staging": {
          "WebhookUrl": "https://api.telegram.org/bot<token>/sendMessage",
          "ChannelType": "Telegram",
          "DisplayName": "Staging Bot",
          "TargetId": "-1001234567890"
        }
      }
    }
  }
}
```

A channel configuration (`ChannelConfiguration`) supports the following fields:

| Field | Type | Default | Purpose |
| --- | --- | --- | --- |
| `ChannelType` | `Telegram` \| `Slack` \| `Discord` \| `Webhook` \| `Email` | - | Transport / payload format |
| `WebhookUrl` | string | - | Endpoint the payload is POSTed to (required) |
| `DisplayName` | string | - | Human-readable name shown in logs and dry-run output (required) |
| `TargetId` | string | `""` | Chat / channel id specific to the platform |
| `IsEnabled` | bool | `true` | Disable a channel without deleting it |
| `IncludeCommitDetails` | bool | `true` | Add commit hash / author to the message |
| `IncludeBuildUrl` | bool | `true` | Add a link to the build |
| `MinimumPriority` | `Low` \| `Normal` \| `High` \| `Critical` | `Low` | Suppress notifications below this priority |
| `AllowedEnvironments` | list of environments | empty (all) | Only send for these environments |
| `AllowedStatuses` | list of build statuses | empty (all) | Only send for these statuses |
| `MaxRetries` | int | `3` | Delivery retry attempts |
| `TimeoutMs` | int | `10000` | Per-request timeout in milliseconds |
| `CustomHeaders` | map | empty | Extra HTTP headers forwarded on every request |
| `UseSlackBlockKit` | bool | `false` | Render Slack messages with Block Kit instead of legacy attachments |
| `EnableEmojis` | bool | `true` | Include emoji status indicators |

When a notification is dispatched, each configured channel evaluates
`MinimumPriority`, `AllowedEnvironments` and `AllowedStatuses` before sending;
`--dry-run` reports the reason whenever a channel would be skipped.

## WebhookPayload

The `WebhookPayload` class represents the structured data sent to webhooks, encapsulating event metadata and deployment details. It includes validation and serialization capabilities for webhook integration.

Example usage:
```csharp
var payload = new WebhookPayload
{
  EventType = "deployment",
  Data = new WebhookData
  {
    ProjectName = "MyApp",
    Version = "1.0.0",
    Status = "success",
    Message = "Deployment completed",
    Environment = "production",
    Branch = "main",
    CommitHash = "a1b2c3d",
    CommitAuthor = "john.doe",
    RepositoryUrl = "https://github.com/myorg/myapp",
    BuildUrl = "https://ci.myorg.com/build/123",
    DurationSeconds = 120
  }
};

if (payload.IsValid())
{
  string json = payload.ToJson();
  Console.WriteLine(json);
}
else
{
  Console.WriteLine("Invalid payload: " + string.Join(", ", payload.Errors));
}
```

## NotificationException

The `NotificationException` class serves as the base class for all notification-related errors in the system...

## RollbackRequest

The `RollbackRequest` type represents a request to roll back a deployment to a previous version. It contains metadata such as project, target version, environment, and notification channels. It can be validated and summarized.

Example usage:
```csharp
using DotNetDeployNotify.Core.Models;
using System;
using System.Collections.Generic;

var rollback = new RollbackRequest
{
  ProjectName = "MyApp",
  TargetVersion = "1.0.0",
  CurrentVersion = "1.0.1",
  TargetEnvironment = Environment.Production,
  RequestedBy = "alice",
  Reason = "Bug in new release",
  Channels = new List<NotificationChannel> { NotificationChannel.Email, NotificationChannel.Slack },
  Priority = NotificationPriority.High,
  Metadata = new Dictionary<string, object> { { "Ticket", "ABC-123" } }
};

if (rollback.IsValid())
{
  Console.WriteLine(rollback.GetSummary());
}
else
{
  Console.WriteLine("Invalid rollback request");
}
```

## NotificationResult

The `NotificationResult` class represents the outcome of a notification delivery attempt, tracking delivery status, response details, retry attempts, and timing information. It provides methods to mark deliveries as successful, failed, or scheduled for retry, and includes validation for result completeness.

Example usage:
```csharp
using DotNetDeployNotify.Core.Models;
using System;

// Create a notification result for a successful webhook delivery
var result = new NotificationResult
{
  NotificationId = "notif-12345",
  Channel = NotificationChannel.Webhook,
  ConfigurationId = "webhook-config-67890",
  Status = DeliveryStatus.Delivered,
  HttpStatusCode = 200,
  ResponseBody = "{\"status\": \"received\"}",
  DurationMs = 145,
  AttemptNumber = 1,
  AttemptedAt = DateTime.UtcNow
};

if (result.IsValid())
{
  Console.WriteLine(result.GetSummary());
  Console.WriteLine($"Status: {result.Status}");
  Console.WriteLine($"Duration: {result.DurationMs}ms");
}

// Mark a delivery as failed with error details
var failedResult = NotificationResult.CreateFailure(
  notificationId: "notif-54321",
  channel: NotificationChannel.Email,
  configId: "email-config-09876",
  errorMessage: "SMTP server unavailable",
  exceptionType: "SmtpException"
);

failedResult.MarkAsFailed("Connection timeout after 30 seconds", "TimeoutException", 504);

// Mark a delivery for retry
var retryResult = new NotificationResult
{
  NotificationId = "notif-abc123",
  Channel = NotificationChannel.Slack,
  ConfigurationId = "slack-config-xyz789",
  Status = DeliveryStatus.Retried,
  AttemptNumber = 2,
  DurationMs = 250,
  AttemptedAt = DateTime.UtcNow.AddMinutes(-5),
  LastRetryAt = DateTime.UtcNow.AddMinutes(-5),
  NextRetryAt = DateTime.UtcNow.AddMinutes(10)
};

Console.WriteLine(retryResult.GetSummary());
```

## DeploymentNotification

The `DeploymentNotification` class represents the core data for a deployment event, including project details, status, commit information, and notification channels. It provides functionality to validate the notification and generate a human-readable summary of the deployment event.

Example usage:
```csharp
using DotNetDeployNotify.Core.Models;
using System;
using System.Collections.Generic;

var notification = new DeploymentNotification
{
    Id = "notif-123",
    ProjectName = "MyApp",
    Version = "1.0.0",
    Status = BuildStatus.Success,
    Message = "Deployed successfully to production.",
    TargetEnvironment = Environment.Production,
    BranchName = "main",
    CommitHash = "a1b2c3d",
    CommitAuthor = "jane.doe",
    RepositoryUrl = "https://github.com/myorg/myapp",
    BuildUrl = "https://ci.myorg.com/build/123",
    DurationSeconds = 120,
    CreatedAt = DateTime.UtcNow,
    Channels = new List<NotificationChannel> { NotificationChannel.Slack },
    Priority = NotificationPriority.High,
    Metadata = new Dictionary<string, object> { { "User", "admin" } }
};

if (notification.IsValid)
{
    Console.WriteLine(notification.GetSummary);
}
```

## DeploymentHistoryEntry

The `DeploymentHistoryEntry` class represents a single recorded deployment event in the history log. It captures all essential deployment metadata including project details, version information, status, environment, commit data, timing information, and optional tags for categorization. This class is typically used to maintain an audit trail of all deployments for reporting, rollback tracking, and compliance purposes.

Example usage:
```csharp
using DotNetDeployNotify.Core.Models;
using System;
using System.Collections.Generic;

// Create a deployment history entry for a successful production deployment
var entry = new DeploymentHistoryEntry
{
    ProjectName = "MyWebApp",
    Version = "2.1.0",
    FinalStatus = BuildStatus.Success,
    TargetEnvironment = Environment.Production,
    BranchName = "main",
    CommitHash = "a1b2c3d4e5f67890",
    CommitAuthor = "john.doe@example.com",
    DeployedAt = DateTime.UtcNow,
    DurationSeconds = 185,
    Tags = new Dictionary<string, string>
    {
        { "Team", "Platform" },
        { "ReleaseType", "Scheduled" },
        { "CIBuild", "12345" }
    }
};

Console.WriteLine($"Deployment {entry.Id} recorded for {entry.ProjectName} v{entry.Version}");
Console.WriteLine($"Status: {entry.FinalStatus}, Duration: {entry.DurationSeconds} seconds");

// Create a history entry from a deployment notification
var notification = new DeploymentNotification
{
    ProjectName = "MyWebApp",
    Version = "2.1.0",
    Status = BuildStatus.Success,
    TargetEnvironment = Environment.Production,
    BranchName = "main",
    CommitHash = "a1b2c3d4e5f67890",
    CommitAuthor = "john.doe@example.com",
    CreatedAt = DateTime.UtcNow,
    DurationSeconds = 185,
    Metadata = new Dictionary<string, object>
    {
        { "User", "admin" },
        { "RollbackFromVersion", "2.0.5" }
    }
};

var historyEntry = DeploymentHistoryEntry.FromNotification(notification);
Console.WriteLine($"Created from notification: {historyEntry.ProjectName} v{historyEntry.Version}");

// Create a rollback entry
var rollbackEntry = new DeploymentHistoryEntry
{
    ProjectName = "MyWebApp",
    Version = "2.0.5",
    FinalStatus = BuildStatus.RollbackSuccess,
    TargetEnvironment = Environment.Production,
    BranchName = "hotfix/rollback-2.1.0",
    CommitHash = "f1e2d3c4b5a67890",
    CommitAuthor = "alice@example.com",
    DeployedAt = DateTime.UtcNow.AddMinutes(-10),
    DurationSeconds = 95,
    IsRollback = true,
    RolledBackFromVersion = "2.1.0",
    ErrorDetails = "Critical bug in 2.1.0: memory leak in API endpoint"
};

Console.WriteLine($"Rollback from {rollbackEntry.RolledBackFromVersion} to {rollbackEntry.Version}");
```
## CustomTemplate

The `CustomTemplate` class represents a user-defined named notification template stored in the engine registry. It enables customizable notification content with placeholders, conditional blocks, and metadata for organizing templates. Templates can be activated or deactivated, and the `Touch()` method updates the modification timestamp.

Example usage:
```csharp
using DotNetDeployNotify.Core.Models;
using System;

// Create a custom template for deployment notifications
var template = new CustomTemplate
{
    Name = "DeploymentSuccessTemplate",
    Description = "Template for successful deployment notifications",
    Content = @"Deployment successful!

Project: {ProjectName}
Version: {Version}
Environment: {Environment}
Branch: {Branch}
Commit: {CommitHash}
Duration: {DurationSeconds} seconds

Build URL: {BuildUrl}",
    Category = "Deployment",
    IsActive = true
};

// Register the template in the engine registry
EngineRegistry.RegisterTemplate(template);

// Retrieve the template by name
var retrievedTemplate = EngineRegistry.GetTemplate("DeploymentSuccessTemplate");

// Update template content and mark as modified
template.Content = @"🚀 Deployment Successful!

📦 Project: {ProjectName}
🏷️ Version: {Version}
🌍 Environment: {Environment}
🌿 Branch: {Branch}
✅ Commit: {CommitHash}
⏱️ Duration: {DurationSeconds} seconds

🔗 Build: {BuildUrl}";
    Category = "Deployment/Success";
    template.Touch();

// Deactivate a template (soft delete)
template.IsActive = false;
```