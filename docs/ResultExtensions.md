# ResultExtensions

The `ResultExtensions` class provides a set of static extension methods that enable functional composition, error handling, and transformation of `Result` and `Result<T>` instances. These methods allow you to chain operations, combine multiple results, apply predicates, map values, and handle fallbacks in a consistent, exception-safe manner. All methods are designed to work with the custom `Result` type used in the `dotnet-deploy-notify` project, which represents either a success (with an optional value) or a failure (with an error description).

## API

### `Try<T>`
```csharp
public static Result<T> Try<T>(Func<T> func)
```
Wraps the execution of a synchronous function that returns a value of type `T`. If the function completes successfully, a success `Result<T>` containing the value is returned. If the function throws an exception, a failure `Result<T>` is returned with the exception details.

- **Parameters**  
  `func` – A delegate that produces a value of type `T`.
- **Returns**  
  A `Result<T>` representing the outcome.
- **Throws**  
  Does not throw; all exceptions are captured.

### `TryAsync<T>`
```csharp
public static async Task<Result<T>> TryAsync<T>(Func<Task<T>> func)
```
Asynchronously wraps the execution of a function that returns a `Task<T>`. The result is a `Task<Result<T>>` that completes with a success or failure result.

- **Parameters**  
  `func` – A delegate that returns a `Task<T>`.
- **Returns**  
  A `Task<Result<T>>` representing the asynchronous outcome.
- **Throws**  
  Does not throw; all exceptions (including `OperationCanceledException`) are captured.

### `Try`
```csharp
public static Result Try(Action action)
```
Wraps the execution of a synchronous action that does not return a value. Returns a non-generic `Result` indicating success or failure.

- **Parameters**  
  `action` – A delegate to execute.
- **Returns**  
  A `Result` indicating success or failure.
- **Throws**  
  Does not throw; exceptions are captured.

### `Combine`
```csharp
public static Result Combine(params Result[] results)
```
Combines multiple non-generic `Result` instances into a single `Result`. If all results are successful, a success `Result` is returned. If any result is a failure, a failure `Result` is returned (typically containing the first error encountered).

- **Parameters**  
  `results` – An array of `Result` instances to combine.
- **Returns**  
  A `Result` representing the combined outcome.
- **Throws**  
  `ArgumentNullException` if `results` is `null`.

### `Combine<T>`
```csharp
public static Result<IReadOnlyList<T>> Combine<T>(params Result<T>[] results)
```
Combines multiple `Result<T>` instances into a single `Result<IReadOnlyList<T>>`. If all results are successful, a success result containing a read-only list of all values is returned. If any result is a failure, a failure result is returned.

- **Parameters**  
  `results` – An array of `Result<T>` instances to combine.
- **Returns**  
  A `Result<IReadOnlyList<T>>` containing the combined values or the first error.
- **Throws**  
  `ArgumentNullException` if `results` is `null`.

### `Where<T>`
```csharp
public static Result<T> Where<T>(this Result<T> result, Func<T, bool> predicate)
```
Filters a `Result<T>` based on a predicate. If the result is a success and the predicate returns `true`, the original result is returned. If the predicate returns `false`, a failure result is returned. If the result is already a failure, it is passed through unchanged.

- **Parameters**  
  `result` – The source result.  
  `predicate` – A function that tests the value.
- **Returns**  
  A `Result<T>` that is either the original success, a failure from the predicate, or the original failure.
- **Throws**  
  `ArgumentNullException` if `result` or `predicate` is `null`.  
  Exceptions thrown by `predicate` are not caught; they propagate.

### `Where<T, TNew>`
```csharp
public static Result<TNew> Where<T, TNew>(this Result<T> result, Func<T, bool> predicate, Func<T, TNew> selector)
```
Filters and transforms a `Result<T>` in one step. If the result is a success and the predicate returns `true`, the selector is applied to the value and a success `Result<TNew>` is returned. Otherwise, a failure result is returned (either from the original failure or from the predicate failing).

- **Parameters**  
  `result` – The source result.  
  `predicate` – A function that tests the value.  
  `selector` – A function that maps the value to a new type.
- **Returns**  
  A `Result<TNew>` representing the filtered and mapped outcome.
- **Throws**  
  `ArgumentNullException` if any argument is `null`.  
  Exceptions from `predicate` or `selector` propagate.

### `Select<T, TResult>`
```csharp
public static Result<TResult> Select<T, TResult>(this Result<T> result, Func<T, TResult> selector)
```
Maps the value of a successful `Result<T>` to a new type using the provided selector. If the result is a failure, it is propagated unchanged.

- **Parameters**  
  `result` – The source result.  
  `selector` – A function that transforms the value.
- **Returns**  
  A `Result<TResult>` containing the mapped value or the original error.
- **Throws**  
  `ArgumentNullException` if `result` or `selector` is `null`.  
  Exceptions from `selector` propagate.

### `SelectMany<T, TResult>`
```csharp
public static Result<TResult> SelectMany<T, TResult>(this Result<T> result, Func<T, Result<TResult>> binder)
```
Monadic bind operation. If the result is a success, applies the binder function to the value and returns the resulting `Result<TResult>`. If the result is a failure, it is propagated.

- **Parameters**  
  `result` – The source result.  
  `binder` – A function that takes a value of type `T` and returns a `Result<TResult>`.
- **Returns**  
  A `Result<TResult>` from the binder or the original failure.
- **Throws**  
  `ArgumentNullException` if `result` or `binder` is `null`.  
  Exceptions from `binder` propagate.

### `SelectMany<T, TIntermediate, TResult>`
```csharp
public static Result<TResult> SelectMany<T, TIntermediate, TResult>(this Result<T> result, Func<T, Result<TIntermediate>> binder, Func<T, TIntermediate, TResult> resultSelector)
```
Monadic bind with a result selector. First applies the binder to the value of a successful result, then uses the `resultSelector` to combine the original value and the intermediate value into a final result.

- **Parameters**  
  `result` – The source result.  
  `binder` – A function that returns a `Result<TIntermediate>`.  
  `resultSelector` – A function that combines the original value and the intermediate value into a `TResult`.
- **Returns**  
  A `Result<TResult>` containing the combined value or the first failure encountered.
- **Throws**  
  `ArgumentNullException` if any argument is `null`.  
  Exceptions from `binder` or `resultSelector` propagate.

### `Do<T>`
```csharp
public static Result<T> Do<T>(this Result<T> result, Action<T> action)
```
Performs a side effect on the value of a successful result without modifying it. The action is executed only if the result is a success. The original result is returned unchanged.

- **Parameters**  
  `result` – The source result.  
  `action` – A delegate that performs an operation on the value.
- **Returns**  
  The original `Result<T>`.
- **Throws**  
  `ArgumentNullException` if `result` or `action` is `null`.  
  Exceptions from `action` propagate.

### `DoAndReturn<T>`
```csharp
public static Result<T> DoAndReturn<T>(this Result<T> result, Func<T, T> func)
```
Performs a side effect that also returns a new value of the same type. The function is applied only if the result is a success. The result is replaced with the function’s return value.

- **Parameters**  
  `result` – The source result.  
  `func` – A function that takes the current value and returns a new value.
- **Returns**  
  A `Result<T>` containing the value returned by `func`, or the original failure.
- **Throws**  
  `ArgumentNullException` if `result` or `func` is `null`.  
  Exceptions from `func` propagate.

### `GetValueOrDefault<T>`
```csharp
public static T GetValueOrDefault<T>(this Result<T> result, T defaultValue = default)
```
Extracts the value from a successful result, or returns the specified default value if the result is a failure.

- **Parameters**  
  `result` – The source result.  
  `defaultValue` – The value to return if the result is a failure (optional; defaults to `default(T)`).
- **Returns**  
  The value of type `T` from the result, or `defaultValue`.
- **Throws**  
  `ArgumentNullException` if `result` is `null`.

### `OrElse<T>`
```csharp
public static Result<T> OrElse<T>(this Result<T> result, Result<T> fallback)
```
Provides a fallback result if the original result is a failure. If the original result is a success, it is returned unchanged. If it is a failure, the `fallback` result is returned.

- **Parameters**  
  `result` – The source result.  
  `fallback` – A `Result<T>` to use when the source is a failure.
- **Returns**  
  The original success result or the fallback result.
- **Throws**  
  `ArgumentNullException` if `result` or `fallback` is `null`.

```csharp
public static Result<T> OrElse<T>(this Result<T> result, Func<T> fallbackFactory)
```
Provides a fallback value computed by a factory function when the original result is a failure. If the original result is a success, it is returned unchanged. If it is a failure, a new success `Result<T>` is created from the value returned by `fallbackFactory`.

- **Parameters**  
  `result` – The source result.  
  `fallbackFactory` – A function that produces a fallback value of type `T`.
- **Returns**  
  The original success result or a new success result containing the fallback value.
- **Throws**  
  `ArgumentNullException` if `result` or `fallbackFactory` is `null`.  
  Exceptions from `fallbackFactory` propagate.

## Usage

### Example 1: Chaining operations with error handling

```csharp
using DeployNotify.Results;

public class DeploymentService
{
    public Result<DeploymentConfig> LoadConfig(string path)
    {
        return ResultExtensions.Try(() => File.ReadAllText(path))
            .Select(json => JsonSerializer.Deserialize<DeploymentConfig>(json))
            .Where(config => config.Environment != null, "Environment must be set")
            .Do(config => Logger.LogInfo($"Loaded config for {config.Environment}"));
    }

    public async Task<Result<DeploymentResult>> DeployAsync(DeploymentConfig config)
    {
        return await ResultExtensions.TryAsync(() => PerformDeploymentAsync(config))
            .Select(result => new DeploymentResult(result.Id, result.Status));
    }
}
```

### Example 2: Combining results and providing fallbacks

```csharp
using DeployNotify.Results;

public class HealthCheckOrchestrator
{
    public Result<IReadOnlyList<ServiceStatus>> CheckAllServices()
    {
        var checks = new[]
        {
            CheckService("api"),
            CheckService("web"),
            CheckService("db")
        };

        return ResultExtensions.Combine(checks)
            .OrElse(Result<IReadOnlyList<ServiceStatus>>.Success(new List<ServiceStatus>
            {
                new ServiceStatus("all", ServiceState.Unknown)
            }));
    }

    private Result<ServiceStatus> CheckService(string name)
    {
        return ResultExtensions.Try(() => PerformHealthCheck(name))
            .Select(status => new ServiceStatus(name, status));
    }
}
```

## Notes

- **Null arguments**: All methods throw `ArgumentNullException` when required arguments (results, delegates, predicates, etc.) are `null`. Always validate inputs before calling these methods.
- **Exception propagation**: Methods that accept user-provided delegates (`Where`, `Select`, `SelectMany`, `Do`, `DoAndReturn`, `OrElse` with factory) do **not** catch exceptions thrown by those delegates. Only the `Try` and `TryAsync` methods capture exceptions from the wrapped function.
- **Thread safety**: The `ResultExtensions` class itself is stateless and thread-safe. However, the delegates passed to these methods may introduce shared state; ensure that any mutable state accessed by those delegates is properly synchronized.
- **Async cancellation**: `TryAsync` captures `OperationCanceledException` and returns a failure result. If you need to propagate cancellation, inspect the returned `Result`’s error or use the `CancellationToken` within the delegate.
- **Combine order**: When combining results, the first failure encountered determines the error in the combined result. Subsequent failures are ignored.
- **Default value**: `GetValueOrDefault` uses `default(T)` if no explicit default is provided. For reference types this is `null`; for value types it is the zero-initialized value.
