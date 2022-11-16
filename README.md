// existing content ...

## NotificationException

The `NotificationException` class serves as the base class for all notification-related errors in the system. It provides a common exception type for handling various notification processing issues. Derived exception types include `ChannelConfigurationException`, `WebhookDeliveryException`, `NotificationValidationException`, `NotificationDeliveryException`, `ConfigurationMissingException`, and `RepositoryException`.

Example usage:

```csharp
try
{
    // Simulate a notification processing failure
    throw new NotificationException("Failed to process notification");
}
catch (NotificationException ex)
{
    Console.WriteLine($"Notification error: {ex.Message}");
}

// Usage of derived exception types
try
{
    throw new ChannelConfigurationException("Invalid channel configuration", NotificationChannel.Email, "config-123");
}
catch (ChannelConfigurationException ex)
{
    Console.WriteLine($"Channel configuration error: {ex.Message} (Channel: {ex.ChannelType}, Config ID: {ex.ConfigurationId})");
}

try
{
    throw new WebhookDeliveryException("Webhook delivery failed", NotificationChannel.Teams, 3, 404);
}
catch (WebhookDeliveryException ex)
{
    Console.WriteLine($"Webhook delivery error: {ex.Message} (Attempts: {ex.Attempts}, Status Code: {ex.LastStatusCode})");
}

try
{
    throw new NotificationValidationException("Notification validation failed", new List<string> { "Invalid field" });
}
catch (NotificationValidationException ex)
{
    Console.WriteLine($"Validation error: {ex.Message}");
    foreach (var error in ex.ValidationErrors)
    {
        Console.WriteLine($"  - {error}");
    }
}

try
{
    throw new NotificationDeliveryException("Notification delivery failed", NotificationChannel.Slack, 403);
}
catch (NotificationDeliveryException ex)
{
    Console.WriteLine($"Delivery error: {ex.Message} (Channel: {ex.Channel}, HTTP Status Code: {ex.HttpStatusCode})");
}

try
{
    throw new ConfigurationMissingException("Missing configuration key", "my-key");
}
catch (ConfigurationMissingException ex)
{
    Console.WriteLine($"Configuration missing error: {ex.Message} (Key: {ex.ConfigurationKey})");
}
```

## TrafficSplitter
