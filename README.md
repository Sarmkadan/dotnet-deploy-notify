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
