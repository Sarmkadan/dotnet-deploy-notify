# NotificationException

`NotificationException` serves as the abstract base class for all exceptions within the `dotnet-deploy-notify` notification pipeline. It provides common context—such as the target channel type and configuration identifier—that downstream exception types refine for specific failure scenarios including configuration errors, validation failures, and delivery problems.

## API

### NotificationException

```
public abstract class NotificationException : Exception
```

Base exception type. Not intended for direct instantiation; derived types represent concrete failure modes.

#### Constructors

- **`NotificationException(string message)`**  
  Initializes a new instance with a descriptive error message.  
  *Parameters:* `message` — the human-readable description of the error condition.

#### Properties

- **`NotificationChannel? ChannelType`**  
  Gets the notification channel associated with the failure, or `null` if the channel could not be determined.  
  *Returns:* a `NotificationChannel` enum value or `null`.

- **`string? ConfigurationId`**  
  Gets the configuration identifier that was being processed when the exception occurred, or `null` if unavailable.  
  *Returns:* a string identifier or `null`.

---

### ChannelConfigurationException

```
public class ChannelConfigurationException : NotificationException
```

Thrown when a channel’s configuration is missing, incomplete, or structurally invalid.

#### Constructors

- **`ChannelConfigurationException(string message)`**  
  Initializes a new instance with a descriptive error message.  
  *Parameters:* `message` — the human-readable description of the configuration error.

#### Properties

- **`NotificationChannel Channel`**  
  Gets the specific channel whose configuration caused the failure.  
  *Returns:* a `NotificationChannel` value.

- **`int Attempts`**  
  Gets the number of configuration resolution attempts made before the exception was raised.  
  *Returns:* a non-negative integer.

- **`int? LastStatusCode`**  
  Gets the last HTTP status code received during configuration retrieval, if applicable.  
  *Returns:* an integer status code or `null`.

---

### WebhookDeliveryException

```
public class WebhookDeliveryException : NotificationException
```

Thrown when delivery to a webhook endpoint fails due to transport errors, non-success status codes, or response validation issues.

#### Properties

- **`List<string> ValidationErrors`**  
  Gets the collection of validation error messages accumulated during webhook response processing.  
  *Returns:* a `List<string>`, never `null` but possibly empty.

---

### NotificationValidationException

```
public class NotificationValidationException : NotificationException
```

Thrown when a notification payload or its associated metadata fails pre-delivery validation rules.

#### Constructors

- **`NotificationValidationException(string message)`**  
  Initializes a new instance with a descriptive error message.  
  *Parameters:* `message` — the human-readable description of the validation failure.

#### Properties

- **`NotificationChannel Channel`**  
  Gets the channel for which validation failed.  
  *Returns:* a `NotificationChannel` value.

- **`int? HttpStatusCode`**  
  Gets the HTTP status code associated with the validation failure, if the validation involved an HTTP round-trip.  
  *Returns:* an integer status code or `null`.

---

### NotificationDeliveryException

```
public class NotificationDeliveryException : NotificationException
```

Thrown when a notification cannot be delivered to its target channel after all configured retry or fallback policies have been exhausted.

#### Properties

- **`string? ConfigurationKey`**  
  Gets the configuration key that was active during the failed delivery attempt, or `null` if not applicable.  
  *Returns:* a string key or `null`.

---

### ConfigurationMissingException

```
public class ConfigurationMissingException : NotificationException
```

Thrown when a required configuration entry is entirely absent from the configuration store.

#### Constructors

- **`ConfigurationMissingException(string message)`**  
  Initializes a new instance with a descriptive error message.  
  *Parameters:* `message` — the human-readable description of the missing configuration.

## Usage

### Example 1: Catching and Inspecting a Channel Configuration Failure

```csharp
try
{
    await notificationService.SendAsync(new NotificationPayload
    {
        Channel = NotificationChannel.Slack,
        Content = "Deployment completed."
    });
}
catch (ChannelConfigurationException ex)
{
    Console.WriteLine($"Configuration error for channel {ex.Channel}: {ex.Message}");
    Console.WriteLine($"Attempts: {ex.Attempts}, Last status: {ex.LastStatusCode}");
    // Trigger configuration repair workflow
}
```

### Example 2: Handling a Webhook Delivery Failure with Validation Details

```csharp
try
{
    await webhookDispatcher.DispatchAsync(webhookPayload);
}
catch (WebhookDeliveryException ex) when (ex.ValidationErrors.Any())
{
    foreach (var error in ex.ValidationErrors)
    {
        logger.LogError("Webhook response validation error: {Error}", error);
    }
    // Escalate for manual review when response structure is unexpected
}
catch (NotificationDeliveryException ex)
{
    logger.LogWarning(
        "Delivery failed for configuration key {Key}: {Message}",
        ex.ConfigurationKey,
        ex.Message);
    // Queue for retry with exponential backoff
}
```

## Notes

- All exception types in this hierarchy are designed to be serializable for logging and diagnostic purposes; ensure that any custom data attached to derived types is likewise serializable if cross-process propagation is required.
- `ValidationErrors` on `WebhookDeliveryException` is guaranteed non-null; consumers can safely iterate without a null check, though the list may be empty when the failure is purely transport-related.
- `LastStatusCode` and `HttpStatusCode` are nullable integers because not all failure paths involve an HTTP response (e.g., timeout, DNS resolution failure, or validation against local schema).
- None of these exception types provide thread-safety guarantees for property mutation after construction. Properties are intended to be set at construction time and treated as read-only thereafter. If an exception instance is shared across threads for logging, ensure it is fully initialized before publication.
- `ConfigurationMissingException` is distinct from `ChannelConfigurationException`: the former indicates complete absence of a configuration entry, while the latter covers present-but-invalid configurations. Catch blocks should order these from most specific to least specific to avoid inadvertently swallowing the wrong type.
