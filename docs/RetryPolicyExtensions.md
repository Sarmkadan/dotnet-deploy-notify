# RetryPolicyExtensions

Provides extension methods for creating and executing retry policies, along with basic logging capabilities. The type combines policy factories (e.g., `WithExponentialBackoff`) with helper methods for executing synchronous and asynchronous operations, and includes simple logger members (`BeginScope`, `IsEnabled`, `Log`) that can be used to trace retry activity.

## API

### BeginScope<TState>
**Purpose:** Begins a logical operation scope for logging.  
**Parameters:** `TState state` – The state to associate with the scope.  
**Return Value:** An `IDisposable` that ends the scope when disposed; returns `null` if scoping is not supported.  
**Exceptions:** None under normal conditions.

### IsEnabled
**Purpose:** Indicates whether the logger is currently enabled.  
**Parameters:** None.  
**Return Value:** `true` if logging is enabled; otherwise `false`.  
**Exceptions:** None.

### Log<TState>
**Purpose:** Writes a log entry using the supplied state.  
**Parameters:** `TState state` – The state to be logged.  
**Return Value:** `void`.  
**Exceptions:** May throw if the underlying logging sink fails (e.g., `ObjectDisposedException`).

### WithDefaults
**Purpose:** Creates a `RetryPolicy` with the library’s default retry settings.  
**Parameters:** None.  
**Return Value:** A new `RetryPolicy` instance configured with default retry count and backoff.  
**Exceptions:** None.

### WithImmediateRetries
**Purpose:** Creates a `RetryPolicy` that retries immediately a specified number of times.  
**Parameters:** `int retryCount` – Number of retry attempts (must be non‑negative).  
**Return Value:** A `RetryPolicy` that will retry immediately up to `retryCount` times.  
**Exceptions:** `ArgumentOutOfRangeException` if `retryCount` is negative.

### WithExponentialBackoff
**Purpose:** Creates a `RetryPolicy` that waits exponentially increasing delays between retries.  
**Parameters:**  
- `int retryCount` – Number of retry attempts (must be non‑negative).  
- `TimeSpan firstDelay` – Delay for the first retry (must be greater than zero).  
- `TimeSpan? maxDelay` (optional) – Upper bound for delays; `null` means no upper bound.  
- `double exponent` (optional) – Exponent used to calculate the delay; default is `2.0`.  
**Return Value:** A `RetryPolicy` configured with exponential backoff.  
**Exceptions:**  
- `ArgumentOutOfRangeException` if `retryCount` is negative.  
- `ArgumentOutOfRangeException` if `firstDelay` is less than or equal to `TimeSpan.Zero`.

### WithCustomRetryCondition
**Purpose:** Creates a `RetryPolicy` that uses a custom predicate to decide whether to retry based on the encountered exception.  
**Parameters:** `Func<Exception, bool> retryCondition` – Predicate returning `true` if the exception should trigger a retry.  
**Return Value:** A `RetryPolicy` that applies the supplied condition.  
**Exceptions:** `ArgumentNullException` if `retryCondition` is `null`.

### GetDelay
**Purpose:** Calculates the delay for a given retry attempt according to the policy’s backoff strategy.  
**Parameters:** `int retryAttempt` – Zero‑based index of the retry attempt (0 for the first retry).  
**Return Value:** `TimeSpan` representing the delay to wait before the next attempt.  
**Exceptions:** `ArgumentOutOfRangeException` if `retryAttempt` is negative or exceeds the policy’s configured retry count.

### GetDelayWithJitter
**Purpose:** Calculates the delay for a given retry attempt, adding random jitter to prevent thundering herd problems.  
**Parameters:** `int retryAttempt` – Zero‑based index of the retry attempt.  
**Return Value:** `TimeSpan` delay with jitter applied.  
**Exceptions:** Same as `GetDelay`.

### CreateHelper
**Purpose:** Creates a `RetryHelper` that encapsulates a policy for easier execution.  
**Parameters:** `RetryPolicy policy` – The policy to associate with the helper.  
**Return Value:** A `RetryHelper` instance ready to execute operations.  
**Exceptions:** `ArgumentNullException` if `policy` is `null`.

### ExecuteAsync<T>
**Purpose:** Asynchronously executes an operation, applying the supplied retry policy.  
**Parameters:**  
- `Func<Task<T>> operation` – The asynchronous operation to execute.  
- `RetryPolicy policy` – The policy governing retries.  
**Return Value:** `Task<T>` that completes with the operation’s result or propagates the final exception after all retries are exhausted.  
**Exceptions:**  
- `ArgumentNullException` if `operation` or `policy` is `null`.  
- Any exception thrown by `operation` after all retry attempts have been exhausted.

### Execute<T>
**Purpose:** Synchronously executes an operation, applying the supplied retry policy.  
**Parameters:**  
- `Func<T> operation` – The synchronous operation to execute.  
- `RetryPolicy policy` – The policy governing retries.  
**Return Value:** `T` result of the operation.  
**Exceptions:**  
- `ArgumentNullException` if `operation` or `policy` is `null`.  
- Any exception thrown by `operation` after all retry attempts have been exhausted.

### FormatConfiguration
**Purpose:** Produces a human‑readable string describing the policy’s configuration.  
**Parameters:** `RetryPolicy policy` – The policy to format.  
**Return Value:** `String` representation of the policy’s settings (e.g., retry count, delay type).  
**Exceptions:** `ArgumentNullException` if `policy` is `null`.

### ShouldRetryFor
**Purpose:** Determines whether the policy would retry for a given exception.  
**Parameters:**  
- `RetryPolicy policy` – The policy to evaluate.  
- `Exception exception` – The exception to test.  
**Return Value:** `true` if the policy would retry; otherwise `false`.  
**Exceptions:** `ArgumentNullException` if `policy` or `exception` is `null`.

### GetTotalAttempts
**Purpose:** Retrieves the total number of attempts (initial attempt plus retries) allowed by the policy.  
**Parameters:** `RetryPolicy policy` – The policy to query.  
**Return Value:** `Int32` total attempts.  
**Exceptions:** `ArgumentNullException` if `policy` is `null`.

### GetRetryDelays
**Purpose:** Enumerates the delays that the policy will apply between attempts.  
**Parameters:** `RetryPolicy policy` – The policy to query.  
**Return Value:** `IEnumerable<TimeSpan>` yielding each delay in order.  
**Exceptions:** `ArgumentNullException` if `policy` is `null`.

### GetRetryDelaysWithJitter
**Purpose:** Enumerates the delays with jitter applied.  
**Parameters:** `RetryPolicy policy` – The policy to query.  
**Return Value:** `IEnumerable<TimeSpan>` yielding jittered delays.  
**Exceptions:** `ArgumentNullException` if `policy` is `null`.

## Usage

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNetDeployNotify.Retry; // namespace containing RetryPolicyExtensions

public class HttpClientWrapper
{
    private static readonly HttpClient _client = new HttpClient();

    public async Task<string> GetJsonAsync(string url)
    {
        // Create a policy with exponential backoff: 3 retries, starting at 500ms, max 5 seconds.
        var policy = RetryPolicyExtensions.WithExponentialBackoff(
            retryCount: 3,
            firstDelay: TimeSpan.FromMilliseconds(500),
            maxDelay: TimeSpan.FromSeconds(5));

        // Execute the HTTP request asynchronously, applying the policy.
        return await RetryPolicyExtensions.ExecuteAsync(
            operation: () => _client.GetStringAsync(url),
            policy: policy);
    }
}
```

```csharp
using System;
using System.IO;
using DotNetDeployNotify.Retry;

public class FileReader
{
    public string ReadFile(string path)
    {
        // Create a policy that retries immediately up to 2 times.
        var policy = RetryPolicyExtensions.WithImmediateRetries(retryCount: 2);

        // Use the logger members to trace each attempt.
        if (RetryPolicyExtensions.IsEnabled)
        {
            using (RetryPolicyExtensions.BeginScope(scope: $"Reading file {path}"))
            {
                RetryPolicyExtensions.Log(state: $"Attempt 1 of {RetryPolicyExtensions.GetTotalAttempts(policy)}");
            }
        }

        // Execute the synchronous file read, applying the policy.
        return RetryPolicyExtensions.Execute(
            operation: () => File.ReadAllText(path),
            policy: policy);
    }
}
```

## Notes
- All static factory methods (`WithDefaults`, `WithImmediateRetries`, `WithExponentialBackoff`, `WithCustomRetryCondition`) are thread‑safe and return immutable `RetryPolicy` instances.  
- The logger members (`BeginScope`, `IsEnabled`, `Log`) delegate to an underlying logger; their thread‑safety depends on that logger’s implementation.  
- `ExecuteAsync` and `Execute` are safe to call concurrently from multiple threads, provided the supplied operation delegate is itself thread‑safe or does not rely on mutable shared state.  
- Policies that specify a `maxDelay` will clamp calculated delays to that bound; jitter is applied after clamping, so the final delay may still exceed `maxDelay` by the jitter amount if the jitter range is not similarly bounded.  
- `GetDelay` and `GetDelayWithJitter` throw when the retry attempt index is out of range; callers should ensure the index is less than the value returned by `GetTotalAttempts` minus one.  
- The `RetryHelper` returned by `CreateHelper` is a lightweight wrapper; it does not own the policy and therefore does not affect its lifetime.  
- Exception handling: If an operation throws an exception that the policy does not consider retryable (as determined by `ShouldRetryFor`), the exception is propagated immediately without further retries.  
- The `FormatConfiguration` method is intended for diagnostics and logging; its exact format may change between versions but will always include the policy’s key parameters.
