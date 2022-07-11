# TestHttpClient

A test utility class that provides a mockable HTTP client implementation for unit testing scenarios where HTTP calls need to be intercepted, verified, or simulated without making real network requests. It is designed to work with dependency injection and logging systems in .NET applications.

## API

### `TestHttpClient`

The primary class that implements `HttpClient` with additional testing capabilities. It allows interception of HTTP requests and responses for verification purposes.

### `SetupFakeRequest`

```csharp
public static void SetupFakeRequest(HttpRequestMessage request, HttpResponseMessage response)
```

Configures a fake response for a specific HTTP request. Subsequent calls matching the request will return the configured response instead of making real network calls.

**Parameters:**
- `request`: The `HttpRequestMessage` to match against incoming requests.
- `response`: The `HttpResponseMessage` to return when a matching request is made.

**Throws:**
- `ArgumentNullException`: If `request` or `response` is `null`.

### `AddProvider`

```csharp
public void AddProvider(ILoggerProvider provider)
```

Adds a logger provider to the internal logging system used by the test client. This enables capturing log output during test execution.

**Parameters:**
- `provider`: The `ILoggerProvider` to add to the logging system.

### `CreateLogger`

```csharp
public ILogger CreateLogger(string categoryName)
```

Creates a logger instance with the specified category name. Used for logging integration testing.

**Parameters:**
- `categoryName`: The category name for the logger.

**Returns:**
- An `ILogger` instance.

### `Dispose`

```csharp
public void Dispose()
```

Releases all resources used by the `TestHttpClient`. This includes cleaning up any registered fake responses and disposing of the internal logging system.

### `BeginScope<TState>`

```csharp
public IDisposable? BeginScope<TState>(TState state)
```

Begins a logical operation scope. Used for structured logging scenarios.

**Type Parameters:**
- `TState`: The type of the state object.

**Parameters:**
- `state`: The identifier for the scope.

**Returns:**
- An `IDisposable` that ends the logical operation scope when disposed.

### `IsEnabled`

```csharp
public bool IsEnabled(LogLevel logLevel)
```

Checks if the given log level is enabled. Used for conditional logging optimization.

**Parameters:**
- `logLevel`: The log level to check.

**Returns:**
- `true` if the log level is enabled; otherwise, `false`.

### `Log<TState>`

```csharp
public void Log<TState>(
    LogLevel logLevel,
    EventId eventId,
    TState state,
    Exception? exception,
    Func<TState, Exception?, string> formatter)
```

Writes a log entry. Used for logging integration testing.

**Type Parameters:**
- `TState`: The type of the object to be written.

**Parameters:**
- `logLevel`: Entry will be written on this level.
- `eventId`: Id of the event.
- `state`: The entry to be written. Can be also an object.
- `exception`: The exception related to this entry.
- `formatter`: Function to create a string message of the state and exception.

## Usage

### Basic HTTP request interception

```csharp
using var client = new TestHttpClient();
TestHttpClient.SetupFakeRequest(
    new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/data"),
    new HttpResponseMessage(HttpStatusCode.OK)
    {
        Content = new StringContent("{ \"value\": 42 }")
    });

var response = await client.GetAsync("https://api.example.com/data");
var content = await response.Content.ReadAsStringAsync();

Assert.Equal(HttpStatusCode.OK, response.StatusCode);
Assert.Equal("{ \"value\": 42 }", content);
```

### Logging integration testing

```csharp
using var client = new TestHttpClient();
var provider = new TestLoggerProvider();
client.AddProvider(provider);

var logger = client.CreateLogger("TestCategory");
logger.LogInformation("Test message {Value}", 123);

var logs = provider.GetLogs();
Assert.Single(logs);
Assert.Contains("Test message 123", logs[0].Message);
```

## Notes

- Fake responses are matched against incoming requests using the default `HttpRequestMessage` equality comparer, which compares method, request URI, and headers. The body content is not considered in matching.
- The logging system is thread-safe; multiple threads can safely call `CreateLogger`, `BeginScope`, `IsEnabled`, and `Log` concurrently.
- `Dispose` clears all registered fake responses and disposes of the internal logging system. Subsequent calls to `SetupFakeRequest` after disposal will throw `ObjectDisposedException`.
- The `TestHttpClient` does not validate the format of URIs or headers during request matching; invalid requests will not match any fake responses and will result in real network calls unless explicitly mocked.
- Log levels are determined by the internal logging system and are not influenced by the `TestHttpClient` itself. The `IsEnabled` method reflects the current logging configuration.
