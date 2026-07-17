# FakeWebhookTransport

`FakeWebhookTransport` is a test utility designed to simulate HTTP webhook interactions without making actual network requests. It captures outgoing request data into memory, allowing developers to verify the method, URL, headers, and body of notifications sent by the `dotnet-deploy-notify` system during unit testing. By implementing the transport interface with in-memory storage, it ensures tests remain fast, deterministic, and isolated from external dependencies.

## API

### `Requests`
```csharp
public List<CapturedRequest> Requests
```
A public list containing all requests captured by this transport instance. Each entry represents a simulated HTTP call made through this transport. This property is intended for assertion in test cases to verify that the correct number of requests were issued and that their contents match expectations. It does not throw exceptions under normal operation but may return an empty list if no requests have been made.

### `FakeWebhookTransport()` (Default Constructor)
```csharp
public FakeWebhookTransport()
```
Initializes a new instance of the `FakeWebhookTransport` class with default configuration. When used, the transport relies on internal defaults or subsequent property assignment to define the target URL, HTTP method, and headers for simulated requests. This constructor does not throw exceptions.

### `FakeWebhookTransport(string url, string method)`
```csharp
public FakeWebhookTransport(string url, string method)
```
Initializes a new instance of the `FakeWebhookTransport` class with a specific target URL and HTTP method.
*   **Parameters**:
    *   `url`: The destination URL for the simulated webhook.
    *   `method`: The HTTP method (e.g., "POST", "PUT") to be recorded for outgoing requests.
*   **Behavior**: Sets the `Url` and `Method` properties immediately upon instantiation.
*   **Exceptions**: May throw `ArgumentNullException` if either `url` or `method` is null, depending on internal validation logic.

### `Method`
```csharp
public string Method
```
Gets or sets the HTTP method used for simulated requests. This value is recorded in every `CapturedRequest` added to the `Requests` list when a send operation occurs. Changing this property affects only subsequent requests.

### `Url`
```csharp
public string Url
```
Gets or sets the target endpoint URL for simulated requests. This value is recorded in every `CapturedRequest` added to the `Requests` list when a send operation occurs. Modifying this property updates the destination for future simulated transmissions.

### `Body`
```csharp
public string Body
```
Gets or sets the default payload body to be included in simulated requests. When the transport is invoked to send a notification, this string is typically used as the request content unless overridden by the calling service. It is also recorded in the corresponding `CapturedRequest`.

### `Headers`
```csharp
public Dictionary<string, string> Headers
```
Gets or sets the collection of HTTP headers to be attached to simulated requests. This dictionary maps header names to their values. Any headers present in this collection at the time of transmission are recorded in the `CapturedRequest`. If the dictionary is null, no custom headers are applied.

## Usage

### Example 1: Basic Verification of Request Count and URL
This example demonstrates initializing the transport with specific parameters and verifying that a notification service correctly attempts to send a request to the expected endpoint.

```csharp
using System.Linq;
using Xunit;
// using YourNamespace.DeployNotify; 

public class NotificationTests
{
    [Fact]
    public void SendNotification_Should_Call_Webhook()
    {
        // Arrange
        var transport = new FakeWebhookTransport("https://api.example.com/hook", "POST");
        var service = new NotificationService(transport);

        // Act
        service.SendDeploymentStatus("success", "v1.0.0");

        // Assert
        Assert.Single(transport.Requests);
        var request = transport.Requests.First();
        Assert.Equal("https://api.example.com/hook", request.Url);
        Assert.Equal("POST", request.Method);
    }
}
```

### Example 2: Validating Headers and Payload Content
This example illustrates how to configure custom headers and inspect the captured body content to ensure the payload serialization matches the expected format.

```csharp
using System.Collections.Generic;
using Xunit;
// using YourNamespace.DeployNotify;

public class PayloadValidationTests
{
    [Fact]
    public void SendNotification_Should_Include_Auth_Header_And_Json_Body()
    {
        // Arrange
        var transport = new FakeWebhookTransport
        {
            Url = "https://secure.example.com/webhook",
            Method = "POST",
            Body = "{\"status\": \"deployed\"}",
            Headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer token123" },
                { "Content-Type", "application/json" }
            }
        };
        
        var service = new NotificationService(transport);

        // Act
        service.Trigger();

        // Assert
        var captured = transport.Requests[0];
        Assert.Equal("Bearer token123", captured.Headers["Authorization"]);
        Assert.Contains("deployed", captured.Body);
    }
}
```

## Notes

*   **Thread Safety**: The `Requests` list and `Headers` dictionary are not thread-safe by default. If `FakeWebhookTransport` is accessed concurrently from multiple threads (e.g., in async integration tests simulating parallel deployments), external synchronization or a thread-safe collection wrapper is required to prevent race conditions during read/write operations.
*   **Reference Semantics**: The `Headers` property exposes a mutable `Dictionary`. Modifying the contents of this dictionary after assigning it will affect all subsequent requests made by this instance. Similarly, the `Requests` list is mutable; clearing or modifying it externally will alter the history of captured calls.
*   **State Persistence**: Unlike a real network transport, this class retains state indefinitely until the instance is disposed or the `Requests` list is manually cleared. Ensure tests isolate instances properly to avoid leakage of request data between test cases.
*   **Null Handling**: While the constructors and properties allow setting values, passing `null` to the parameterized constructor or assigning `null` to `Headers` may result in `NullReferenceException` during the simulated send operation if the underlying implementation does not perform defensive checks.
