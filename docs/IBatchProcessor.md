# IBatchProcessor

The `IBatchProcessor` type provides a configurable mechanism for processing batches of deployment notifications with support for concurrency control, retry logic, and progress tracking. It exposes properties to tune batch size, parallelism, inter-batch delays, and retry attempts, along with methods to execute batch processing asynchronously and to obtain a resilient retry wrapper. After processing, the instance reports success and failure counts, final failure count, and the completion timestamp.

## API

### `NotificationBatchProcessor` (property)

Gets the underlying `NotificationBatchProcessor` instance that handles the actual notification processing logic. This property is typically used to access processor-specific configuration or to invoke methods not exposed through the `IBatchProcessor` interface.

### `async Task<List<DeploymentNotification>> ProcessBatchAsync()`

Initiates asynchronous processing of the current batch of deployment notifications.  
- **Returns**: A `List<DeploymentNotification>` containing the notifications that were successfully processed.  
- **Throws**: `InvalidOperationException` if the processor is not properly initialized or if a batch is already in progress.  
- **Remarks**: The method respects the `DefaultBatchSize`, `MaxConcurrentBatches`, and `DelayBetweenBatches` settings. It does not perform automatic retries; use `ProcessWithRetryAsync` for resilient execution.

### `int DefaultBatchSize`

Gets or sets the default number of notifications to include in a single batch. Must be greater than zero.  
- **Default**: 10.

### `int MaxConcurrentBatches`

Gets or sets the maximum number of batches that can be processed concurrently. A value of 1 disables parallelism. Must be at least 1.  
- **Default**: 1.

### `TimeSpan DelayBetweenBatches`

Gets or sets the delay to wait between starting consecutive batches when `MaxConcurrentBatches` is greater than 1.  
- **Default**: `TimeSpan.Zero`.

### `int MaxRetries`

Gets or sets the maximum number of retry attempts for a failed batch when using `ProcessWithRetryAsync`. Must be non-negative.  
- **Default**: 3.

### `ResilientBatchProcessor` (property)

Gets a `ResilientBatchProcessor` instance that wraps the current `IBatchProcessor` with retry logic. The returned processor uses the `MaxRetries` setting and can be used to call `ProcessWithRetryAsync`.

### `async Task<BatchProcessingResult> ProcessWithRetryAsync()`

Executes batch processing with automatic retries on failure, up to `MaxRetries` attempts.  
- **Returns**: A `BatchProcessingResult` containing the overall outcome, including success/failure counts and any aggregated errors.  
- **Throws**: `InvalidOperationException` if the processor is not ready or if a retry cycle is already in progress.  
- **Remarks**: Each retry respects the `DelayBetweenBatches` and concurrency settings. After exhausting retries, the result includes the final failure count.

### `int SuccessCount`

Gets the total number of notifications successfully processed across all batches since the last reset or initialization.

### `int FailureCount`

Gets the total number of notifications that failed during processing (including transient failures that may later be retried).

### `int FinalFailureCount`

Gets the number of notifications that ultimately failed after all retry attempts were exhausted.

### `DateTime CompletedAt`

Gets the UTC timestamp when the last processing operation (batch or retry cycle) completed. `DateTime.MinValue` if no processing has finished.

### `override string ToString()`

Returns a human-readable summary of the processor state, including current counts and completion time.

## Usage

### Example 1: Basic batch processing with default settings

```csharp
using dotnet_deploy_notify;

var processor = new IBatchProcessor
{
    DefaultBatchSize = 20,
    MaxConcurrentBatches = 2,
    DelayBetweenBatches = TimeSpan.FromSeconds(1)
};

// Process a batch of notifications
List<DeploymentNotification> processed = await processor.ProcessBatchAsync();

Console.WriteLine($"Processed {processed.Count} notifications.");
Console.WriteLine($"Success count: {processor.SuccessCount}, Failure count: {processor.FailureCount}");
```

### Example 2: Resilient processing with retries and progress reporting

```csharp
using dotnet_deploy_notify;

var processor = new IBatchProcessor
{
    DefaultBatchSize = 50,
    MaxConcurrentBatches = 3,
    MaxRetries = 5,
    DelayBetweenBatches = TimeSpan.FromMilliseconds(500)
};

// Use the resilient wrapper
var resilient = processor.ResilientBatchProcessor;
BatchProcessingResult result = await resilient.ProcessWithRetryAsync();

Console.WriteLine($"Completed at: {processor.CompletedAt:O}");
Console.WriteLine($"Success: {result.SuccessCount}, Final failures: {result.FinalFailureCount}");

if (result.FinalFailureCount > 0)
{
    Console.WriteLine("Some notifications failed permanently.");
}
```

## Notes

- **Thread safety**: `IBatchProcessor` is not inherently thread-safe. Concurrent calls to `ProcessBatchAsync` or `ProcessWithRetryAsync` from multiple threads may lead to inconsistent state. Use external synchronization if parallel access is required.
- **Batch size validation**: Setting `DefaultBatchSize` to zero or a negative value will cause an `ArgumentOutOfRangeException` when processing begins. Similarly, `MaxConcurrentBatches` must be at least 1.
- **Retry exhaustion**: When `MaxRetries` is zero, `ProcessWithRetryAsync` behaves identically to `ProcessBatchAsync` (no retries). A negative value is not allowed.
- **Completion timestamp**: `CompletedAt` is updated only after a processing method completes (successfully or with failures). It is not reset between calls; the timestamp reflects the most recent completion.
- **Property changes after processing**: Modifying `DefaultBatchSize`, `MaxConcurrentBatches`, `DelayBetweenBatches`, or `MaxRetries` while a batch is in progress may have undefined behavior. It is recommended to configure these properties before starting any processing.
