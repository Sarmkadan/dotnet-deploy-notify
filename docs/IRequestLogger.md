# IRequestLogger

`IRequestLogger` is an interface for logging and retrieving details of HTTP webhook requests and responses within the `dotnet-deploy-notify` system. It provides a structured way to capture request metadata, payloads, response data, and error information, enabling monitoring and debugging of webhook interactions.

## API

### Properties

#### `string Id`
A unique identifier for the request log entry. Used to correlate related request and response logs.

#### `string WebhookUrl`
The target URL of the webhook request.

#### `string Method`
The HTTP method used for the request (e.g., "GET", "POST").

#### `Dictionary<string, string> RequestHeaders`
A collection of HTTP headers included in the webhook request. Keys are header names, values are header values.

#### `string RequestPayload`
The body content of the webhook request, serialized as a string.

#### `int? ResponseStatusCode`
The HTTP status code returned in the response. Null if the request failed before receiving a response.

#### `string ResponseBody`
The body content of the webhook response, serialized as a string.

#### `long DurationMs`
The time taken for the webhook request to complete, in milliseconds. Calculated as the difference between response timestamp and request timestamp.

#### `DateTime Timestamp`
The UTC timestamp when the request was initiated.

#### `string? ErrorMessage`
An optional error message describing any failure during the request or response processing.

#### `string GetSummary`
A read-only property returning a human-readable summary of the request and response details.

### Methods

#### `RequestLogger`
A constructor for creating instances of `RequestLogger`. Initializes a new log entry with default values.

#### `void LogWebhookRequest(string method, string url, Dictionary<string, string> headers, string payload)`
Logs the initiation of a webhook request.  
- **Parameters**:  
  - `method`: The HTTP method (e.g., "POST").  
  - `url`: The target webhook URL.  
  - `headers`: HTTP headers to include in the request.  
  - `payload`: The request body content.  
- **Throws**:  
  - `ArgumentNullException` if `method`, `url`, or `headers` is null.  
  - `ArgumentException` if `method` is empty or whitespace.

#### `void LogWebhookResponse(int statusCode, string body)`
Logs the response received for a webhook request.  
- **Parameters**:  
  - `statusCode`: The HTTP status code.  
  - `body`: The response body content.  
- **Throws**:  
  - `ArgumentOutOfRangeException` if `statusCode` is outside the valid HTTP status code range (100–599).

#### `void LogWebhookError(string errorMessage)`
Logs an error encountered during the webhook request/response lifecycle.  
- **Parameters**:  
  - `errorMessage`: A description of the error.  
- **Throws**:  
  - `ArgumentNullException` if `errorMessage` is null.

#### `List<RequestLogEntry> GetRequestHistory()`
Retrieves a list of all logged request entries.  
- **Returns**: A list of `RequestLogEntry` objects representing historical webhook activity.  
- **Notes**: The returned list may be empty if no logs exist.

#### `void ClearOldLogs(int daysToKeep)`
Removes log entries older than the specified number of days.  
- **Parameters**:  
  - `daysToKeep`: The minimum age (in days) of logs to retain. Logs older than this value are deleted.  
- **Throws**:  
  - `ArgumentOutOfRangeException` if `daysToKeep` is negative.

## Usage

### Example 1: Logging a Webhook Request and Response
```csharp
var logger = new RequestLogger();
logger.LogWebhookRequest("POST", "https://example.com/webhook", new Dictionary<string, string> { { "Authorization", "Bearer token" } }, "{\"event\": \"deploy\"}");
logger.LogWebhookResponse(200, "{\"status\": \"success\"}");
Console.WriteLine(logger.GetSummary);
// Output: "POST https://example.com/webhook - 200 OK (Duration: 150ms)"
```

### Example 2: Handling Webhook Errors
```csharp
var logger = new RequestLogger();
try
{
    logger.LogWebhookRequest("POST", "https://invalid-url.com", new Dictionary<string, string>(), "{}");
    // Simulate failed request
    throw new HttpRequestException("Connection refused");
}
catch (Exception ex)
{
    logger.LogWebhookError(ex.Message);
}
var history = logger.GetRequestHistory();
Console.WriteLine(history[0].ErrorMessage); // Output: "Connection refused"
```

## Notes

- **Thread Safety**: Implementations of `IRequestLogger` must ensure thread-safe access to `RequestHeaders`, `GetRequestHistory`, and `ClearOldLogs` if used in concurrent environments. The `Dictionary<string, string>` and `List<RequestLogEntry>` types are not inherently thread-safe.
- **Duration Calculation**: `DurationMs` is calculated automatically when `LogWebhookResponse` is called. If `LogWebhookError` is used instead, `DurationMs` may remain zero or reflect partial timing.
- **Log Retention**: `ClearOldLogs` permanently deletes data. Use with caution in production environments to avoid unintended data loss.
- **Null Handling**: `ResponseStatusCode` and `ErrorMessage` are nullable/optional to accommodate failed requests that do not receive a valid HTTP response.
