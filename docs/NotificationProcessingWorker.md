# NotificationProcessingWorker

`NotificationProcessingWorker` is a long-running background service responsible for orchestrating the periodic execution of scheduled notification tasks. It extends `ScheduledTaskWorker` and manages the lifecycle of registered tasks—such as `CleanupExpiredNotificationsTask`—ensuring they execute on their defined intervals while respecting application startup and shutdown semantics.

## API

### NotificationProcessingWorker

```csharp
public NotificationProcessingWorker(ILogger<ScheduledTaskWorker> logger) : base(logger)
```

Constructs a new instance of the worker, forwarding the logger to the base `ScheduledTaskWorker`. The logger is used to record lifecycle events, task execution outcomes, and diagnostic information throughout the worker’s operation.

### StartAsync

```csharp
public override async Task StartAsync(CancellationToken cancellationToken)
```

Initiates the worker’s processing loop. This method is called by the host infrastructure when the application starts. It begins monitoring registered tasks and triggers execution when their intervals elapse. The returned `Task` completes when the worker has successfully started; it does not represent the entire lifetime of the background processing.

### StopAsync

```csharp
public override async Task StopAsync(CancellationToken cancellationToken)
```

Performs a graceful shutdown of the worker. When the host requests application termination, this method signals the processing loop to cease, allowing any in-flight task execution to complete within a reasonable timeframe. The returned `Task` completes once the worker has stopped.

### HealthCheckWorker

```csharp
public HealthCheckWorker HealthCheckWorker { get; }
```

Exposes a reference to a companion health-check worker. This property enables external monitoring components to query the health status of the notification processing pipeline.

### ScheduledTaskWorker

```csharp
public ScheduledTaskWorker(ILogger<ScheduledTaskWorker> logger) : base
```

The base class constructor invoked during initialization. It establishes the scheduling infrastructure that `NotificationProcessingWorker` relies on for interval tracking and task dispatch.

### RegisterTask

```csharp
public void RegisterTask(string name, TimeSpan interval, Func<Task> executeAsync)
```

Registers a named task to be executed on the specified interval. The `executeAsync` delegate is invoked each time the interval elapses. Tasks are identified by `name`; registering a duplicate name overwrites the previous registration. This method does not return a value and does not throw exceptions under normal circumstances.

### Name

```csharp
public string Name { get; }
```

Gets the unique name assigned to this scheduled task worker instance. The name is typically set during registration or configuration and is used for logging and diagnostic purposes.

### Interval

```csharp
public TimeSpan Interval { get; }
```

Gets the execution interval for the worker’s primary task. This value dictates how frequently the associated `ExecuteAsync` method is invoked. The interval is fixed after registration and does not change at runtime.

### LastRun

```csharp
public DateTime LastRun { get; }
```

Gets the timestamp of the most recent successful invocation of the worker’s task. Returns `DateTime.MinValue` if the task has never executed. This value is updated atomically after each completed execution.

### ShouldRun

```csharp
public bool ShouldRun { get; }
```

Indicates whether the worker’s task is due for execution based on the elapsed time since `LastRun` compared to `Interval`. Returns `true` when the interval has elapsed; otherwise `false`. This property is evaluated on each cycle of the scheduling loop.

### ExecuteAsync

```csharp
public abstract Task ExecuteAsync(CancellationToken cancellationToken)
```

Defines the abstract entry point for the worker’s scheduled logic. Derived classes must override this method to provide the specific work to be performed on each interval tick. The `cancellationToken` allows the implementation to cooperatively cancel long-running operations during shutdown.

### CleanupExpiredNotificationsTask

```csharp
public CleanupExpiredNotificationsTask CleanupExpiredNotificationsTask { get; }
```

Exposes the dedicated task responsible for purging expired notifications from the underlying store. This property provides direct access to the cleanup task instance, allowing external code to inspect its state or trigger manual cleanup if needed.

### ExecuteAsync (CleanupExpiredNotificationsTask)

```csharp
public override Task ExecuteAsync(CancellationToken cancellationToken)
```

The concrete implementation of the cleanup logic. When invoked, this method scans for and removes notification records that have exceeded their expiration threshold. The `cancellationToken` supports cooperative cancellation during lengthy cleanup operations. The returned `Task` completes when the cleanup pass finishes.

## Usage

### Example 1: Registering and Running the Worker in a Hosted Service

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<NotificationProcessingWorker>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<NotificationProcessingWorker>());

var host = builder.Build();

var worker = host.Services.GetRequiredService<NotificationProcessingWorker>();
worker.RegisterTask("CleanupExpiredNotifications", TimeSpan.FromMinutes(15), async () =>
{
    await worker.CleanupExpiredNotificationsTask.ExecuteAsync(CancellationToken.None);
});

await host.RunAsync();
```

### Example 2: Monitoring Task Health in a Controller

```csharp
[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly NotificationProcessingWorker _worker;

    public HealthController(NotificationProcessingWorker worker)
    {
        _worker = worker;
    }

    [HttpGet]
    public IActionResult GetHealth()
    {
        var lastRun = _worker.LastRun;
        var isDue = _worker.ShouldRun;
        var interval = _worker.Interval;

        return Ok(new
        {
            workerName = _worker.Name,
            lastRun = lastRun == DateTime.MinValue ? "Never" : lastRun.ToString("O"),
            nextRunDue = isDue,
            configuredInterval = interval.TotalMinutes + " minutes"
        });
    }
}
```

## Notes

- **Thread safety:** `RegisterTask`, `LastRun`, and `ShouldRun` are accessed from both the scheduling loop and external callers. The base implementation uses appropriate synchronization to prevent race conditions when updating execution timestamps and evaluating the run condition.
- **Graceful shutdown:** `StopAsync` signals cancellation to the inner loop but does not abort an in-progress `ExecuteAsync` invocation. Long-running task implementations should honor the `CancellationToken` passed to `ExecuteAsync` to avoid delaying application shutdown indefinitely.
- **Duplicate registration:** Calling `RegisterTask` with a name that already exists replaces the previous task and resets its interval tracking. This can cause the new task to execute immediately if the prior task’s interval had already elapsed.
- **Initial execution:** A newly registered task will execute on the first cycle where `ShouldRun` evaluates to `true`. If the task has never run (`LastRun` is `DateTime.MinValue`), `ShouldRun` returns `true` immediately, causing execution on the next loop iteration.
- **Health-check integration:** The `HealthCheckWorker` property is populated during construction and remains non-null for the lifetime of the instance. Consumers can safely access it without null checks after the worker is resolved from the dependency injection container.
- **Exception propagation:** Unhandled exceptions thrown from `ExecuteAsync` are caught by the scheduling infrastructure and logged, but they do not crash the worker or prevent subsequent executions. The `LastRun` timestamp is updated only on successful completion.
