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
