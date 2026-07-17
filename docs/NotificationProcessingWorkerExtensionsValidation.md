# NotificationProcessingWorkerExtensionsValidation

Provides validation logic for notification processing worker extensions and implements a lightweight logger‑like interface (`BeginScope`, `IsEnabled`, `Log`) that can be used within those workers to emit diagnostic information.

## API

### ValidateWorkerExtensions
- **Purpose**: Performs validation of the configured worker extensions and collects any error messages.
- **Parameters**: None.
- **Return Value**: An `IReadOnlyList<string>` containing validation error messages. The list is empty when the configuration is valid.
- **Exceptions**: This method does not throw exceptions under normal operation. If internal state is corrupted (e.g., the object has been disposed), an `ObjectDisposedException` may be thrown.

### IsWorkerExtensionsValid
- **Purpose**: Determines whether the worker extensions configuration passes validation.
- **Parameters**: None.
- **Return Value**: `true` if `ValidateWorkerExtensions` returns an empty list; otherwise `false`.
- **Exceptions**: None.

### EnsureWorkerExtensionsValid
- **Purpose**: Validates the worker extensions configuration and throws if any issues are found.
- **Parameters**: None.
- **Return Value**: `void`.
- **Exceptions**: Throws an `InvalidOperationException` whose message contains the concatenated validation errors returned by `ValidateWorkerExtensions`. If validation succeeds, no exception is thrown.

### BeginScope\<TState\>
- **Purpose**: Begins a logical operation scope for logging, allowing contextual information to be attached to subsequent log entries.
- **Parameters**: 
  - `state`: The state object to associate with the scope.
- **Return Value**: An `IDisposable?` that, when disposed, ends the scope. Returns `null` if the logger does not support scoping.
- **Exceptions**: Throws `ArgumentNullException` if `state` is `null`. May throw `ObjectDisposedException` if the logger has been disposed.

### IsEnabled
- **Purpose**: Checks whether logging is enabled for the specified log level.
- **Parameters**: 
  - `logLevel`: The `LogLevel` to test.
- **Return Value**: `true` if writes with the given `logLevel` will be processed; otherwise `false`.
- **Exceptions**: None.

### Log\<TState\>
- **Purpose**: Writes a log entry with the specified level, event identifier, state, exception, and formatter.
- **Parameters**: 
  - `logLevel`: The severity level of the log entry.
  - `eventId`: An identifier for the logging event.
  - `state`: The state object to be formatted.
  - `exception`: An optional exception associated with the log entry.
  - `formatter`: A function that formats the `state` and optional `exception` into a string message.
- **Return Value**: `void`.
- **Exceptions**: Throws `ArgumentNullException` if `formatter` is `null`. May throw `ObjectDisposedException` if the logger has been disposed. Any exception thrown by the `formatter` delegate is propagated to the caller.

## Usage

### Validating worker extensions before startup
```csharp
using DotNetDeployNotify.Extensions; // namespace containing NotificationProcessingWorkerExtensionsValidation

public class WorkerHost
{
    public void Start()
    {
        // Validate configuration; fail fast if invalid.
        NotificationProcessingWorkerExtensionsValidation.EnsureWorkerExtensionsValid();

        // Proceed with worker initialization...
        var worker = new NotificationProcessingWorker();
        worker.Run();
    }
}
```

### Using the logger‑like members inside a worker
```csharp
using Microsoft.Extensions.Logging;
using DotNetDeployNotify.Extensions;

public class NotificationProcessingWorker : INotificationProcessingWorker
{
    private readonly NotificationProcessingWorkerExtensionsValidation _logger;

    public NotificationProcessingWorker()
    {
        _logger = new NotificationProcessingWorkerExtensionsValidation();
    }

    public void Process(Notification notification)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            using (_logger.BeginScope(new { NotificationId = notification.Id }))
            {
                _logger.Log(LogLevel.Information,
                            new EventId(100, "ProcessStart"),
                            notification,
                            null,
                            (state, ex) => $"Processing notification {state.Id}");

                try
                {
                    // ... processing logic ...
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error,
                                new EventId(101, "ProcessFailed"),
                                notification,
                                ex,
                                (state, exc) => $"Failed to process notification {state.Id}: {exc.Message}");
                }
            }
        }
    }
}
```

## Notes
- The static validation methods (`ValidateWorkerExtensions`, `IsWorkerExtensionsValid`, `EnsureWorkerExtensionsValid`) operate on immutable configuration data and are safe to call concurrently from multiple threads.
- Instance members (`BeginScope`, `IsEnabled`, `Log`) mirror the `ILogger` interface; thread‑safety depends on the underlying logger implementation. The provided type does not add additional synchronization, so callers should assume the same guarantees as a standard `ILogger`.
- `BeginScope` may return `null` if scoping is not supported; callers must check for `null` before attempting to dispose.
- `EnsureWorkerExtensionsValid` throws an `InvalidOperationException` that includes all validation messages; if you need to inspect individual messages, call `ValidateWorkerExtensions` first.
- The `Log` method will invoke the supplied `formatter` delegate; any exception thrown by the delegate is not caught and will propagate to the caller.
- Returned read‑only lists from `ValidateWorkerExtensions` are truly read‑only; attempting to modify them will result in a `NotSupportedException`.
