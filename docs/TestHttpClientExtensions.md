# TestHttpClientExtensions

Provides test utilities for configuring `HttpClient` behavior and creating test-scoped logging infrastructure in the `dotnet-deploy-notify` test suite. The type combines static extension methods for `HttpClient`/`TestHttpClient` setup with a disposable scope mechanism for ambient test context.

## API

### `SetupSuccessResponse`
```csharp
public static void SetupSuccessResponse(this HttpClient client, HttpContent content = null)
```
Configures the given `HttpClient` (typically a `TestHttpClient` or delegating handler) to return a successful `200 OK` response with the supplied content. If `content` is `null`, an empty `StringContent` is used.

**Parameters**
- `client`: The `HttpClient` instance to configure.
- `content`: Optional response body. Defaults to empty content.

**Throws**
- `ArgumentNullException` if `client` is `null`.
- `InvalidOperationException` if the client's handler chain does not contain a configurable test handler.

---

### `SetupStatusCodeResponse`
```csharp
public static void SetupStatusCodeResponse(this HttpClient client, HttpStatusCode statusCode, HttpContent content = null)
```
Configures the given `HttpClient` to return a response with the specified `statusCode` and optional `content`.

**Parameters**
- `client`: The `HttpClient` instance to configure.
- `statusCode`: The HTTP status code to return.
- `content`: Optional response body. Defaults to empty content.

**Throws**
- `ArgumentNullException` if `client` is `null`.
- `InvalidOperationException` if the client's handler chain does not contain a configurable test handler.

---

### `CreateTestLogger`
```csharp
public static ILogger CreateTestLogger(string categoryName = "Test")
```
Creates an `ILogger` implementation that captures log entries in memory for assertion in tests. The logger writes to a test-accessible sink (e.g., `List<LogRecord>` or `ITestOutputHelper` adapter).

**Parameters**
- `categoryName`: Logger category name. Defaults to `"Test"`.

**Returns**
- An `ILogger` instance backed by a test sink.

**Throws**
- `ArgumentException` if `categoryName` is null or whitespace.

---

### `BeginTestScope<TState>`
```csharp
public static IDisposable BeginTestScope<TState>(TState state, Action<TState> onDispose = null)
```
Opens a disposable scope that captures ambient test context (e.g., correlation IDs, log scopes, or mock state). The generic `TState` allows callers to flow arbitrary state into the scope. When the returned `IDisposable` is disposed, `onDispose` is invoked with the captured state if provided.

**Type Parameters**
- `TState`: The type of state to associate with the scope.

**Parameters**
- `state`: The state object to capture for the scope duration.
- `onDispose`: Optional callback executed on disposal with the captured state.

**Returns**
- An `IDisposable` that ends the scope when disposed.

**Throws**
- `ArgumentNullException` if `state` is `null` and `TState` is a reference type.

---

### `Dispose`
```csharp
public void Dispose()
```
Releases resources held by the `TestHttpClientExtensions` instance, including any registered test handlers, log sinks, or ambient scope state. This method is idempotent; subsequent calls have no effect.

**Throws**
- Does not throw.

## Usage

### Configuring a test client for webhook delivery success
```csharp
using System.Net.Http;
using DotnetDeployNotify.Tests;

var handler = new TestHttpClientHandler();
var client = new HttpClient(handler);

client.SetupSuccessResponse(new StringContent("{\"status\":\"accepted\"}", Encoding.UTF8, "application/json"));

// Act: invoke webhook sender
await webhookSender.SendAsync(client, payload);

// Assert: verify request was made
Assert.Single(handler.Requests);
Assert.Equal(HttpMethod.Post, handler.Requests[0].Method);
```

### Using a test logger and scope for correlation ID propagation
```csharp
using Microsoft.Extensions.Logging;
using DotnetDeployNotify.Tests;

var logger = TestHttpClientExtensions.CreateTestLogger("Deployment");
using var scope = TestHttpClientExtensions.BeginTestScope("corr-1234", state => logger.LogInformation("Scope ended: {State}", state));

logger.LogInformation("Starting deployment");
await deploymentService.ExecuteAsync(logger);

scope.Dispose(); // logs "Scope ended: corr-1234"

// Verify logs
var records = ((TestLogger)logger).Records;
Assert.Contains(records, r => r.Message.Contains("Starting deployment"));
Assert.Contains(records, r => r.Message.Contains("Scope ended: corr-1234"));
```

## Notes

- **Thread safety**: The static configuration methods (`SetupSuccessResponse`, `SetupStatusCodeResponse`) mutate the handler chain of the provided `HttpClient`. They are not thread-safe for concurrent calls on the same `HttpClient` instance. Each test should use a dedicated `HttpClient`/`TestHttpClientHandler` instance.
- **Handler discovery**: Both setup methods search the `HttpClient`'s `HttpMessageHandler` chain for a `TestHttpClientHandler` (or compatible `ITestHttpHandler`). If the chain uses a custom delegating handler that does not expose the test surface, an `InvalidOperationException` is thrown.
- **Logger sink lifetime**: The `ILogger` returned by `CreateTestLogger` retains all log records in memory until the logger instance becomes eligible for garbage collection. In long-running test suites, dispose or clear the logger between tests to avoid memory growth.
- **Scope nesting**: `BeginTestScope` supports nested scopes; each call returns an independent `IDisposable`. Disposing an outer scope before an inner scope does not automatically dispose the inner scope—callers must manage nesting explicitly.
- **Dispose idempotency**: The instance `Dispose()` method can be called multiple times safely. It clears internal handler registrations and log sinks. After disposal, static methods remain functional but any instance-specific state (e.g., registered test handlers on a shared `HttpClient`) is reset.
- **Generic state capture**: `BeginTestScope<TState>` captures the `state` reference at call time. If `TState` is a mutable reference type, mutations after the call are visible to the `onDispose` callback. Prefer immutable value types or snapshots for reliable test assertions.
