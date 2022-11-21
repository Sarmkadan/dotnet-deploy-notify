// existing content ...

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
