# RetryPolicy

`RetryPolicy` provides a configurable, composable mechanism for executing operations with automatic retry and optional circuit-breaker protection. It combines exponential backoff, jitter, and user-defined retry predicates to handle transient failures in distributed systems such as deployment notifications. The type also implements `ILogger` for diagnostic tracing of retry attempts and state transitions.

## API

### Public Members

#### `int MaxAttempts`
Gets or sets the maximum number of execution attempts, including the initial call. A value of 1 means no retries. Must be at least 1.

#### `TimeSpan InitialDelay`
Gets or sets the base delay applied before the first retry. Used as the starting point for exponential backoff calculations.

#### `double BackoffMultiplier`
Gets or sets the multiplier applied to the delay on each successive retry. For example, a value of 2.0 doubles the delay each attempt. Must be greater than or equal to 1.0.

#### `TimeSpan MaxDelay`
Gets or sets the upper bound on any computed retry delay. Delays calculated via exponential backoff are capped at this value.

#### `Func<Exception, bool>? ShouldRetry`
An optional predicate that determines whether a given exception is retryable. If `null`, all exceptions are considered retryable. Return `true` to allow a retry; `false` to rethrow immediately.

#### `RetryHelper`
Exposes the underlying retry helper instance, which encapsulates the core delay calculation and attempt-counting logic.

#### `async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)`
Executes an asynchronous operation with retry semantics.
- **Parameters:**
  - `action`: A delegate that accepts a `CancellationToken` and returns a `Task<T>`.
  - `cancellationToken`: A token that cancels both the operation and any pending retry delays.
- **Returns:** The result of the first successful execution.
- **Throws:** The last captured exception if all attempts are exhausted or `ShouldRetry` returns `false`. Throws `OperationCanceledException` if the token is cancelled.

#### `T Execute<T>(Func<T> action)`
Executes a synchronous operation with retry semantics.
- **Parameters:**
  - `action`: A delegate that returns a value of type `T`.
- **Returns:** The result of the first successful execution.
- **Throws:** The last captured exception if all attempts are exhausted or `ShouldRetry` returns `false`.

#### `ExponentialBackoff`
Exposes the exponential backoff strategy instance used for delay calculation. This object implements the core backoff algorithm.

#### `TimeSpan GetDelay(int attempt)`
Computes the delay for a given retry attempt number (0-based) using exponential backoff without jitter.
- **Parameters:**
  - `attempt`: The zero-based retry attempt index.
- **Returns:** The calculated delay, bounded by `MaxDelay`.

#### `TimeSpan GetDelayWithJitter(int attempt)`
Computes the delay for a given retry attempt number (0-based) using exponential backoff with randomised jitter to avoid thundering-herd effects.
- **Parameters:**
  - `attempt`: The zero-based retry attempt index.
- **Returns:** The calculated delay with jitter applied, bounded by `MaxDelay`.

#### `enum CircuitState`
Enumerates the possible states of the circuit breaker:
- `Closed` — Normal operation; requests flow through.
- `Open` — Requests fail fast without attempting execution.
- `HalfOpen` — A limited number of trial requests are permitted to test recovery.

#### `CircuitState State`
Gets the current state of the circuit breaker. This property reflects transitions caused by consecutive failures or successful recovery.

#### `CircuitBreakerWithBackoff`
Exposes the combined circuit-breaker and backoff strategy instance. This object manages state transitions and coordinates with the backoff policy.

#### `async Task<T> ExecuteAsync<T>(CircuitBreakerWithBackoff circuitBreaker, Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)`
Executes an asynchronous operation through a specific circuit-breaker instance with retry semantics.
- **Parameters:**
  - `circuitBreaker`: The circuit-breaker instance governing execution.
  - `action`: A delegate that accepts a `CancellationToken` and returns a `Task<T>`.
  - `cancellationToken`: A token that cancels both the operation and any pending retry delays.
- **Returns:** The result of the first successful execution.
- **Throws:** `CircuitOpenException` if the circuit is open; the last captured exception if retries are exhausted; `OperationCanceledException` if cancelled.

#### `IDisposable? BeginScope<TState>(TState state)`
Creates a logger scope for the given state. Implementation of `ILogger.BeginScope`. Returns a disposable that ends the scope, or `null` if scopes are not supported.

#### `bool IsEnabled(LogLevel logLevel)`
Checks whether logging is enabled for the specified log level. Implementation of `ILogger.IsEnabled`.

#### `void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)`
Writes a log entry. Implementation of `ILogger.Log`. Used internally to trace retry attempts, delay durations, and circuit state changes.

#### `void Dispose()`
Releases resources held by the retry policy, including any logger scopes and timer resources. Implementation of `IDisposable`.

## Usage

### Example 1: Basic Retry with Exponential Backoff

```csharp
var policy = new RetryPolicy
{
    MaxAttempts = 5,
    InitialDelay = TimeSpan.FromMilliseconds(200),
    BackoffMultiplier = 2.0,
    MaxDelay = TimeSpan.FromSeconds(10),
    ShouldRetry = ex => ex is HttpRequestException or TaskCanceledException
};

string result = policy.Execute(() =>
{
    // Simulate a flaky HTTP call
    using var client = new HttpClient();
    return client.GetStringAsync("https://api.example.com/status").Result;
});

Console.WriteLine($"Deployment status: {result}");
```

### Example 2: Async Execution with Circuit Breaker and Cancellation

```csharp
var policy = new RetryPolicy
{
    MaxAttempts = 3,
    InitialDelay = TimeSpan.FromSeconds(1),
    BackoffMultiplier = 1.5,
    MaxDelay = TimeSpan.FromSeconds(30),
    ShouldRetry = ex => ex is not InvalidOperationException
};

using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));

try
{
    await policy.ExecuteAsync(async token =>
    {
        await SendDeploymentNotificationAsync(token);
        return true;
    }, cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Deployment notification was cancelled.");
}
catch (Exception ex) when (policy.State == CircuitState.Open)
{
    Console.WriteLine($"Circuit open — fast-failing: {ex.Message}");
}
```

## Notes

- **Thread Safety:** Instance properties (`MaxAttempts`, `InitialDelay`, `BackoffMultiplier`, `MaxDelay`, `ShouldRetry`) are not synchronised. Configure the policy once before sharing it across threads. The `Execute` and `ExecuteAsync` methods are safe to call concurrently; delay timers and attempt counters are managed per invocation.
- **Jitter:** `GetDelayWithJitter` applies randomisation to the computed delay to reduce correlated retry spikes. The jitter algorithm uses a random factor within a bounded range; exact distribution is an implementation detail of `ExponentialBackoff`.
- **Circuit Breaker:** When `State` transitions to `Open`, subsequent calls through `ExecuteAsync` with the circuit-breaker overload fail immediately with a circuit-open exception. The breaker transitions to `HalfOpen` after a configured timeout, allowing a trial request. If that request succeeds, the circuit closes; otherwise it reopens.
- **Cancellation:** If a `CancellationToken` is cancelled during a retry delay, the delay is aborted and `OperationCanceledException` is thrown. If cancellation occurs during action execution, the exception propagates and is evaluated by `ShouldRetry`; `OperationCanceledException` typically should not be marked retryable.
- **Logging:** The type implements `ILogger` for internal diagnostics. External consumers can inspect retry behaviour by providing a logger implementation or by subscribing to log messages. `BeginScope` may return `null` if scoping is disabled.
- **Disposal:** Call `Dispose` when the policy is no longer needed, particularly if it holds timer resources or logger scopes. After disposal, further calls to `Execute` or `ExecuteAsync` may throw `ObjectDisposedException`.
