# NotificationProcessingWorkerExtensions

Extension methods for configuring and interacting with `NotificationProcessingWorker` instances, including health monitoring and scheduled task creation.

## API

### `WithInterval`

Configures the worker to process notifications at a specified time interval.

- **Parameters**
  - `worker`: The `NotificationProcessingWorker` instance to configure.
  - `interval`: The time span between processing cycles.
- **Return Value**
  Returns the configured `worker` for method chaining.
- **Exceptions**
  Throws `ArgumentNullException` if `worker` or `interval` is `null`.
  Throws `ArgumentOutOfRangeException` if `interval` is not positive.

### `WithDetailedLogging`

Enables detailed logging for the worker, including processing metrics and errors.

- **Parameters**
  - `worker`: The `NotificationProcessingWorker` instance to configure.
  - `logger`: The `ILogger` instance to use for logging.
- **Return Value**
  Returns the configured `worker` for method chaining.
- **Exceptions**
  Throws `ArgumentNullException` if `worker` or `logger` is `null`.

### `CreateHealthCheckTask`

Creates a scheduled task that performs health checks on the worker.

- **Parameters**
  - `worker`: The `NotificationProcessingWorker` instance to monitor.
  - `interval`: The time span between health checks.
- **Return Value**
  Returns a `ScheduledTask` configured to run health checks at the specified interval.
- **Exceptions**
  Throws `ArgumentNullException` if `worker` is `null`.
  Throws `ArgumentOutOfRangeException` if `interval` is not positive.

### `GetStatistics`

Retrieves runtime statistics for the worker, including processed notifications and success rate.

- **Return Value**
  Returns a tuple containing:
  - `TotalProcessed`: The total number of notifications processed.
  - `SuccessRate`: The success rate of processed notifications (0.0 to 1.0).
  - `Uptime`: The duration since the worker started.
- **Exceptions**
  Throws `InvalidOperationException` if the worker has not been started.

### `NotificationProcessingHealthCheckTask`

A health check task that verifies the worker is operational.

- **Properties**
  - `Worker`: The `NotificationProcessingWorker` instance being monitored.

### `ExecuteAsync`

Executes the health check task, performing the health verification.

- **Parameters**
  - `stoppingToken`: A cancellation token to observe while executing.
- **Return Value**
  Returns a `Task` representing the asynchronous operation.
- **Exceptions**
  Throws `OperationCanceledException` if the operation is canceled via `stoppingToken`.

## Usage

### Example 1: Configuring a Worker with Interval and Logging
