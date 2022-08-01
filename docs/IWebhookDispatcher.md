# IWebhookDispatcher
The `IWebhookDispatcher` type is designed to handle the dispatching of notifications to webhooks, providing a standardized interface for sending and validating webhook notifications. This interface is crucial in scenarios where applications need to notify external services or systems of specific events or updates, ensuring that these notifications are delivered reliably and securely.

## API
The `IWebhookDispatcher` interface includes the following public members:
- `WebhookDispatcher`: The constructor for creating instances of `WebhookDispatcher`.
- `SendToWebhookAsync`: An asynchronous method that sends a notification to a webhook. It returns a `NotificationResult` object, indicating the outcome of the operation. This method is used to dispatch notifications to webhooks, allowing for the handling of asynchronous operations and potential exceptions that may occur during the sending process.
- `SendPayloadAsync`: Another asynchronous method that sends a payload to a webhook. Similar to `SendToWebhookAsync`, it returns a `NotificationResult` object. The primary difference lies in the specifics of what is being sent (e.g., a custom payload versus a predefined notification).
- `ValidateWebhookAsync`: An asynchronous method used to validate a webhook. It returns a boolean value indicating whether the webhook is valid. This method is essential for ensuring that webhooks are properly configured and can receive notifications before attempting to send them.

## Usage
Here are two examples of using the `IWebhookDispatcher` interface in C#:
```csharp
// Example 1: Sending a notification to a webhook
var dispatcher = new WebhookDispatcher();
var result = await dispatcher.SendToWebhookAsync("https://example.com/webhook", "Notification content");
if (result.Success)
{
    Console.WriteLine("Notification sent successfully.");
}
else
{
    Console.WriteLine("Failed to send notification: " + result.ErrorMessage);
}

// Example 2: Validating a webhook
var dispatcher = new WebhookDispatcher();
var isValid = await dispatcher.ValidateWebhookAsync("https://example.com/webhook");
if (isValid)
{
    Console.WriteLine("Webhook is valid.");
}
else
{
    Console.WriteLine("Webhook is not valid.");
}
```

## Notes
When using the `IWebhookDispatcher` interface, consider the following:
- **Thread Safety**: Since the methods are asynchronous, they are designed to be thread-safe. However, the implementation details of any class implementing this interface may affect thread safety. Always review the documentation of the specific implementation being used.
- **Error Handling**: Both `SendToWebhookAsync` and `SendPayloadAsync` may throw exceptions if there are issues with the webhook URL, network connectivity, or the payload being sent. `ValidateWebhookAsync` may also throw exceptions if there are issues validating the webhook. Proper error handling should be implemented when using these methods.
- **Webhook Security**: When sending notifications to webhooks, ensure that the webhook URLs are secure (HTTPS) to prevent interception of sensitive data. Also, be cautious with the data being sent to avoid exposing sensitive information.
