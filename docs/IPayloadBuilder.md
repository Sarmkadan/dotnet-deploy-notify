# IPayloadBuilder
The `IPayloadBuilder` type is designed to construct and format payload data for various notification platforms, including webhooks, Telegram, Slack, and Discord. It provides a standardized interface for building payloads, allowing developers to easily integrate notification functionality into their applications.

## API
* `PayloadBuilder`: The constructor for the `IPayloadBuilder` type, responsible for initializing a new instance.
* `WebhookPayload BuildPayload`: Builds a payload for a webhook notification. Returns a `WebhookPayload` object representing the constructed payload.
* `string BuildTelegramMessage`: Builds a message for a Telegram notification. Returns a `string` containing the formatted message.
* `object BuildSlackPayload`: Builds a payload for a Slack notification. Returns an `object` representing the constructed payload.
* `object BuildDiscordPayload`: Builds a payload for a Discord notification. Returns an `object` representing the constructed payload.

## Usage
The following examples demonstrate how to use the `IPayloadBuilder` type to construct payloads for different notification platforms:
```csharp
// Example 1: Building a webhook payload
var payloadBuilder = new PayloadBuilder();
var webhookPayload = payloadBuilder.BuildPayload();
// Use the webhookPayload object to send a notification

// Example 2: Building a Telegram message and a Slack payload
var payloadBuilder2 = new PayloadBuilder();
var telegramMessage = payloadBuilder2.BuildTelegramMessage();
var slackPayload = payloadBuilder2.BuildSlackPayload();
// Use the telegramMessage string and slackPayload object to send notifications
```

## Notes
When using the `IPayloadBuilder` type, consider the following edge cases and thread-safety remarks:
* The `BuildPayload`, `BuildTelegramMessage`, `BuildSlackPayload`, and `BuildDiscordPayload` methods may throw exceptions if the underlying data is invalid or incomplete.
* The `IPayloadBuilder` type is not inherently thread-safe. If multiple threads access the same instance concurrently, it may lead to inconsistent or unexpected behavior. To ensure thread safety, consider creating a new instance for each thread or using synchronization mechanisms to protect access to the instance.
* The `object` return type of the `BuildSlackPayload` and `BuildDiscordPayload` methods indicates that the constructed payload may be a complex object with various properties. Be sure to handle these objects accordingly to avoid errors or data loss.
