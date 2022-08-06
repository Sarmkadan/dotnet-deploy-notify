# IValidationService

The `IValidationService` interface defines the contract for validating core components within the `dotnet-deploy-notify` system, including notification payloads, channel configurations, and webhook data. It provides a unified mechanism to assess validity through boolean flags and detailed error reporting via string lists, alongside specialized static factory methods for generating standard validation outcomes. Implementations of this service ensure that data integrity and configuration correctness are maintained before deployment notifications are processed or dispatched.

## API

### `IsValid`
```csharp
public bool IsValid { get; }
```
Indicates whether the most recent validation operation completed successfully without errors. This property returns `true` if the `Errors` collection is empty, and `false` otherwise. It does not throw exceptions.

### `Errors`
```csharp
public List<string> Errors { get; }
```
Provides a list of descriptive error messages accumulated during the last validation execution. If the validation was successful, this list is empty. The list is mutable by the implementation but exposed as a read-only property to the consumer. It does not throw exceptions.

### `Success`
```csharp
public static ValidationResult Success { get; }
```
A static read-only instance representing a successful validation result. This singleton object typically has `IsValid` set to `true` and an empty `Errors` collection. It is used to return immediate success states without instantiating new objects. It does not throw exceptions.

### `Failure`
```csharp
public static ValidationResult Failure { get; }
```
A static read-only instance representing a generic failed validation result. This singleton object typically has `IsValid` set to `false`. Depending on the implementation, it may contain a default error message or an empty error list intended to be populated by the caller. It does not throw exceptions.

### `ValidateNotification`
```csharp
public ValidationResult ValidateNotification(Notification notification);
```
Validates a specific notification object to ensure it contains all required fields and adheres to business logic constraints before processing.
*   **Parameters**: `notification` – The notification object to validate.
*   **Returns**: A `ValidationResult` indicating success or failure, populated with specific error messages if the notification is invalid.
*   **Throws**: May throw `ArgumentNullException` if the `notification` parameter is null.

### `ValidateChannelConfiguration`
```csharp
public ValidationResult ValidateChannelConfiguration(ChannelConfig config);
```
Validates the configuration settings for a notification channel (e.g., Slack, Email, Teams) to ensure connectivity parameters and credentials are correctly formatted.
*   **Parameters**: `config` – The channel configuration object to validate.
*   **Returns**: A `ValidationResult` indicating whether the configuration is viable.
*   **Throws**: May throw `ArgumentNullException` if the `config` parameter is null.

### `ValidateWebhookPayload`
```csharp
public ValidationResult ValidateWebhookPayload(string payload);
```
Validates the raw JSON or string payload received from an external webhook source to ensure it matches the expected schema for deployment events.
*   **Parameters**: `payload` – The raw string content of the webhook request.
*   **Returns**: A `ValidationResult` indicating if the payload is parseable and structurally correct.
*   **Throws**: May throw `ArgumentNullException` if the `payload` is null or empty, depending on implementation strictness.

### `IsValidUrl`
```csharp
public bool IsValidUrl(string url);
```
Determines if a provided string constitutes a well-formed absolute URI suitable for webhook endpoints or API callbacks.
*   **Parameters**: `url` – The string to evaluate as a URL.
*   **Returns**: `true` if the string is a valid absolute URI with http/https scheme; `false` otherwise.
*   **Throws**: Generally does not throw; returns `false` for malformed inputs.

### `IsValidEmail`
```csharp
public bool IsValidEmail(string email);
```
Determines if a provided string adheres to standard RFC 5322 email address formatting rules.
*   **Parameters**: `email` – The string to evaluate as an email address.
*   **Returns**: `true` if the format is valid; `false` otherwise.
*   **Throws**: Generally does not throw; returns `false` for malformed inputs.

## Usage

### Example 1: Validating a Channel Configuration
This example demonstrates how to use the service to verify a channel configuration before saving it. It utilizes the specific validation method and checks the resulting `IsValid` property and `Errors` collection.

```csharp
public void ConfigureChannel(IValidationService validator, ChannelConfig newConfig)
{
    var result = validator.ValidateChannelConfiguration(newConfig);

    if (!result.IsValid)
    {
        Console.WriteLine("Configuration failed validation:");
        foreach (var error in result.Errors)
        {
            Console.WriteLine($"- {error}");
        }
        return;
    }

    // Proceed to save configuration
    SaveConfiguration(newConfig);
}
```

### Example 2: Pre-flight Check for Webhook and Contact Data
This example shows the usage of primitive validation helpers (`IsValidUrl`, `IsValidEmail`) combined with payload validation to ensure all dependencies are ready before triggering a deployment notification.

```csharp
public bool PrepareDeployment(IValidationService validator, string webhookUrl, string adminEmail, string payload)
{
    if (!validator.IsValidUrl(webhookUrl))
    {
        Logger.Error("Invalid webhook URL format.");
        return false;
    }

    if (!validator.IsValidEmail(adminEmail))
    {
        Logger.Error("Invalid administrator email format.");
        return false;
    }

    var payloadResult = validator.ValidateWebhookPayload(payload);
    if (!payloadResult.IsValid)
    {
        Logger.Error("Webhook payload invalid: " + string.Join(", ", payloadResult.Errors));
        return false;
    }

    return true;
}
```

## Notes

*   **Thread Safety**: The instance members (`ValidateNotification`, `ValidateChannelConfiguration`, etc.) should be considered stateful regarding the `IsValid` and `Errors` properties if the implementation updates these properties directly on the service instance during execution. In such cases, the service instance is not thread-safe for concurrent calls on the same object. However, the static members (`Success`, `Failure`) are immutable and safe for concurrent access.
*   **Null Handling**: While primitive checkers (`IsValidUrl`, `IsValidEmail`) typically return `false` for null inputs to prevent flow interruption, complex object validators (`ValidateNotification`, etc.) generally enforce strict null checks and will throw `ArgumentNullException` to prevent ambiguous validation states. Callers should ensure objects are instantiated before passing them to these methods.
*   **Error Collection Mutability**: The `Errors` list returned by `ValidationResult` instances is often intended for read-only consumption by the caller. Modifying this list externally may not reflect changes in the internal state of the validator and could lead to inconsistent behavior if the list is reused internally by the service implementation.
*   **Static Singletons**: The `Success` and `Failure` static instances are shared across the application. Do not modify the `Errors` collection on these static instances directly, as this will affect all consumers relying on the default singleton state. Always create a new `ValidationResult` instance if custom error messages are required for a failure case.
