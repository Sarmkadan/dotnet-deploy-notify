# IWebhookPayloadBuilder

`IWebhookPayloadBuilder` is an interface for constructing and sending webhook payloads in .NET applications. It provides methods to build payloads, configure webhook clients, and send notifications with detailed tracking of success, status codes, and errors.

## API

### `public string BuildPayload()`
Constructs the final webhook payload string from the configured builder state.
- **Returns**: A string representing the serialized payload.
- **Throws**: `InvalidOperationException` if required payload fields are missing or invalid.

### `public static IWebhookPayloadBuilder CreateBuilder()`
Creates a new instance of a webhook payload builder.
- **Returns**: A new `IWebhookPayloadBuilder` instance ready for configuration.
- **Throws**: No exceptions.

### `public WebhookClient`
Gets or sets the `WebhookClient` instance used to send the payload.
- **Type**: `WebhookClient`
- **Remarks**: Setting this to `null` may result in payloads not being sent. Ensure the client is properly initialized before use.

### `public async Task<WebhookResult> SendWebhookAsync()`
Sends the constructed payload to the configured webhook URL asynchronously.
- **Returns**: A `Task<WebhookResult>` containing the result of the webhook operation, including success status, status code, error message (if any), and duration.
- **Throws**: `InvalidOperationException` if the webhook URL is not set or the payload is invalid. May throw `HttpRequestException` or other network-related exceptions during transmission.

### `public string WebhookUrl`
Gets or sets the target URL for the webhook.
- **Type**: `string`
- **Remarks**: Must be a valid HTTPS URL. Empty or malformed URLs will cause `SendWebhookAsync` to fail.

### `public bool IsSuccessful`
Indicates whether the last webhook operation succeeded.
- **Type**: `bool`
- **Remarks**: `true` only if the HTTP status code is in the 2xx range and no exceptions occurred during transmission.

### `public int StatusCode`
Gets the HTTP status code returned by the webhook endpoint.
- **Type**: `int`
- **Remarks**: Will be `0` if the request failed to reach the server or an exception occurred.

### `public string? ErrorMessage`
Contains the error message if the webhook operation failed.
- **Type**: `string?`
- **Remarks**: `null` if the operation succeeded. Non-null values indicate failure, typically from exceptions or non-2xx responses.

### `public DateTime Duration`
Records the duration of the last webhook operation.
- **Type**: `DateTime`
- **Remarks**: Represents the start time of the operation. Use `DateTime.UtcNow - Duration` to compute elapsed time.

## Usage

### Example 1: Basic Webhook with JSON Payload
