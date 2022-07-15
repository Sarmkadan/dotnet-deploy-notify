# RequestContext

`RequestContext` provides an ambient execution scope that carries correlation identifiers, caller identity, arbitrary metadata, and timing information across synchronous and asynchronous boundaries. It is designed for notification processing pipelines where tracing, auditing, and per-request state must flow through deeply nested service calls without explicit parameter threading.

## API

### Static Members

#### `RequestContext.Context`
Gets the `RequestContext` instance associated with the current execution scope. Returns `null` when no context has been established.

#### `static void SetContext(RequestContext context)`
Replaces the ambient `RequestContext` with the given instance. The previous context, if any, is detached. Passing `null` clears the ambient context.

#### `static void ClearContext()`
Removes the ambient `RequestContext` from the current scope, leaving `Context` as `null`.

#### `static void Reset()`
Resets all state on the currently ambient `RequestContext` to defaults. If no context is ambient, this is a no-op. Does not remove the context from scope.

#### `static RequestContext GetOrCreateContext()`
Returns the ambient `RequestContext` if one exists; otherwise creates a new default instance, sets it as the ambient context, and returns it. Never returns `null`.

#### `static void ExecuteInContext(RequestContext context, Action action)`
Executes the provided `action` synchronously within the scope of the given `context`. The ambient context is restored to its prior value after execution completes or throws.

- **Parameters:**
  - `context` — the `RequestContext` to install as ambient during execution. Must not be `null`.
  - `action` — the delegate to invoke.
- **Throws:** `ArgumentNullException` if either argument is `null`. Any exception thrown by `action` propagates after context restoration.

#### `static async Task ExecuteInContextAsync(RequestContext context, Func<Task> asyncAction)`
Executes the provided asynchronous delegate within the scope of the given `context`. The ambient context is restored after the task completes, faults, or is cancelled.

- **Parameters:**
  - `context` — the `RequestContext` to install as ambient. Must not be `null`.
  - `asyncAction` — the asynchronous delegate to invoke.
- **Returns:** A `Task` representing the asynchronous operation.
- **Throws:** `ArgumentNullException` if either argument is `null`. Exceptions from `asyncAction` propagate normally.

### Instance Members

#### `string CorrelationId`
A unique identifier that correlates all operations belonging to the same logical request or message. Intended to survive service boundaries.

#### `string RequestId`
A unique identifier for the current processing request. Typically more granular than `CorrelationId`.

#### `DateTime RequestTime`
The UTC timestamp at which the request context was initialized or the request was received.

#### `string? UserId`
An optional identifier for the authenticated user initiating the request. `null` when not applicable.

#### `string? ClientId`
An optional identifier for the client application or system originating the request. `null` when not applicable.

#### `Dictionary<string, object> Metadata`
A mutable dictionary carrying arbitrary key-value pairs scoped to the request. Callers may read and write directly. Thread-safe only when used with the synchronization guarantees described in Notes.

#### `int ExecutionTimeMs`
The elapsed wall-clock time in milliseconds between context creation and the moment the property is read. Computed from `RequestTime`.

#### `void SetMetadata(string key, object value)`
Stores a value in the `Metadata` dictionary under the given key. Overwrites any existing entry with the same key.

- **Parameters:**
  - `key` — a non-null string key.
  - `value` — the object to store; may be `null`.
- **Throws:** `ArgumentNullException` when `key` is `null`.

#### `T? GetMetadata<T>(string key)`
Retrieves a value from `Metadata` and attempts to cast it to type `T`. Returns `default(T)` when the key is not present or the value is not assignable to `T`.

- **Parameters:**
  - `key` — the string key to look up.
- **Returns:** The stored value cast to `T`, or `default(T)`.
- **Throws:** `ArgumentNullException` when `key` is `null`.

#### `bool HasMetadata(string key)`
Returns `true` if the `Metadata` dictionary contains the specified key, regardless of its value.

- **Parameters:**
  - `key` — the string key to check.
- **Throws:** `ArgumentNullException` when `key` is `null`.

### `RequestContextScope` (Nested Type)

Implements `IDisposable`. Installs a given `RequestContext` as the ambient context upon construction and restores the previous ambient context upon disposal. Designed for use in `using` blocks.

#### `void Dispose()`
Restores the ambient context that was active before the scope was created. Safe to call multiple times; subsequent calls are no-ops.

## Usage

### Example 1: Basic request pipeline with metadata

```csharp
public async Task ProcessNotificationAsync(NotificationMessage message)
{
    var context = new RequestContext
    {
        CorrelationId = message.CorrelationId,
        RequestId = Guid.NewGuid().ToString("N"),
        RequestTime = DateTime.UtcNow,
        UserId = message.UserId,
        ClientId = message.ClientId
    };

    context.SetMetadata("Priority", message.Priority);
    context.SetMetadata("RetryAttempt", message.RetryCount);

    await RequestContext.ExecuteInContextAsync(context, async () =>
    {
        await ValidateMessageAsync();
        await EnrichAndRouteAsync();
        await PersistOutcomeAsync();
    });
}

private async Task PersistOutcomeAsync()
{
    var ctx = RequestContext.Context;
    var priority = ctx?.GetMetadata<int>("Priority") ?? 0;
    var elapsed = ctx?.ExecutionTimeMs ?? 0;

    logger.LogInformation(
        "Request {RequestId} completed in {Elapsed}ms with priority {Priority}",
        ctx?.RequestId, elapsed, priority);
}
```

### Example 2: Scoped override with `RequestContextScope`

```csharp
public void FanOutToHandlers(NotificationMessage message)
{
    var parentContext = RequestContext.GetOrCreateContext();

    foreach (var handler in _handlers)
    {
        var childContext = new RequestContext
        {
            CorrelationId = parentContext.CorrelationId,
            RequestId = $"{parentContext.RequestId}-{handler.Name}",
            RequestTime = DateTime.UtcNow,
            UserId = parentContext.UserId,
            ClientId = parentContext.ClientId
        };

        using (new RequestContextScope(childContext))
        {
            handler.Process(message);
        }
        // Ambient context is restored to parentContext here
    }
}
```

## Notes

- **Thread safety:** `RequestContext` instances are not inherently thread-safe. The ambient context is typically stored in an execution-context-aware slot (e.g., `AsyncLocal<T>`), which flows with `async/await` continuations but does not synchronize parallel access from multiple threads. Concurrent modifications to `Metadata` or properties from different threads sharing the same context instance must be externally synchronized.
- **`AsyncLocal` flow:** `SetContext`, `ExecuteInContext`, and `RequestContextScope` rely on a storage mechanism that flows across logical call contexts in asynchronous code. Code that uses `ConfigureAwait(false)` may still observe the ambient context if the underlying storage uses `AsyncLocal<T>`; verify the implementation if you depend on context flowing after `ConfigureAwait(false)`.
- **`ExecutionTimeMs` is computed on read:** The property calculates the difference between `DateTime.UtcNow` and `RequestTime` each time it is accessed. Repeated reads during long-running operations will yield increasing values. The value is not cached or frozen.
- **`Reset()` vs `ClearContext()`:** `Reset()` zeroes out the fields of the currently ambient instance but leaves it in scope. `ClearContext()` removes the instance from scope entirely, so `Context` becomes `null`. Choose based on whether downstream code should see a fresh empty context or no context at all.
- **`GetOrCreateContext()` creates a default instance:** The created instance has `RequestTime` set to `DateTime.UtcNow`, empty `Metadata`, and null identity fields. `CorrelationId` and `RequestId` are not automatically populated; callers should set them after creation if needed.
- **`RequestContextScope` disposal:** Always dispose the scope, ideally via a `using` statement. Failure to dispose leaves the overridden context installed indefinitely, which can cause tracing and identity data to leak across unrelated operations.
