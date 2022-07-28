# INotificationProcessor

The `INotificationProcessor` interface defines the contract for handling the lifecycle of notification dispatches within the `dotnet-deploy-notify` system. It provides mechanisms for executing batch operations, retrying failed deliveries, and prioritizing messages based on urgency, while exposing comprehensive telemetry regarding processing outcomes, delivery attempts, and system performance metrics. Implementations of this interface are responsible for managing state transitions, tracking error conditions, and calculating aggregate statistics such as average delivery times and success rates.

## API

### Properties

#### `TotalProcessed`
*   **Type:** `int`
*   **Description:** Gets the total number of notifications that have completed processing, regardless of the outcome (success, failure, or skip).

#### `SuccessCount`
*   **Type:** `int`
*   **Description:** Gets the count of notifications that were successfully processed and delivered without errors.

#### `FailureCount`
*   **Type:** `int`
*   **Description:** Gets the count of notifications that encountered a critical error during processing and could not be delivered.

#### `SkippedCount`
*   **Type:** `int`
*   **Description:** Gets the number of notifications that were intentionally bypassed during the current processing cycle, typically due to filtering rules or duplicate detection.

#### `DurationMs`
*   **Type:** `long`
*   **Description:** Gets the total elapsed time in milliseconds for the most recent processing operation or the cumulative session duration.

#### `Errors`
*   **Type:** `List<string>`
*   **Description:** Gets a collection of error messages describing specific failures encountered during notification processing. This list is populated as failures occur.

#### `GetSummary`
*   **Type:** `string`
*   **Description:** Gets a formatted string representation summarizing the current processing statistics, including counts and duration.

#### `TotalNotifications`
*   **Type:** `int`
*   **Description:** Gets the total number of notifications currently queued or managed by the processor instance.

#### `PendingCount`
*   **Type:** `int`
*   **Description:** Gets the number of notifications that are queued but have not yet been attempted for processing.

#### `ProcessedCount`
*   **Type:** `int`
*   **Description:** Gets the number of notifications that have been handed off to the delivery mechanism, synonymous with `TotalProcessed` in many implementations but may represent a specific stage in the pipeline.

#### `TotalDeliveryAttempts`
*   **Type:** `int`
*   **Description:** Gets the aggregate number of delivery attempts made, including retries for failed messages.

#### `SuccessfulDeliveries`
*   **Type:** `int`
*   **Description:** Gets the count of individual delivery operations that resulted in a successful transmission.

#### `FailedDeliveries`
*   **Type:** `int`
*   **Description:** Gets the count of individual delivery operations that resulted in a transmission failure.

#### `AverageDeliveryTimeMs`
*   **Type:** `long`
*   **Description:** Gets the calculated average time in milliseconds taken to successfully deliver a notification.

#### `ActiveConfigurations`
*   **Type:** `int`
*   **Description:** Gets the number of active notification configurations or channels currently loaded and utilized by the processor.

#### `LastProcessedAt`
*   **Type:** `DateTime?`
*   **Description:** Gets the timestamp of the last successful processing event. Returns `null` if no processing has occurred yet.

#### `NotificationProcessor`
*   **Type:** `NotificationProcessor`
*   **Description:** Gets the concrete implementation instance of the notification processor associated with this interface context.

### Methods

#### `ProcessBatchAsync`
*   **Signature:** `public async Task<ProcessingResult> ProcessBatchAsync`
*   **Description:** Asynchronously processes a standard batch of pending notifications.
*   **Parameters:** None (operates on the internal queue).
*   **Return Value:** Returns a `ProcessingResult` object containing the outcome of the batch operation, including updated counts and status.
*   **Exceptions:** May throw exceptions if the underlying delivery infrastructure is unavailable or if the internal queue is in an invalid state.

#### `ProcessFailedAsync`
*   **Signature:** `public async Task<ProcessingResult> ProcessFailedAsync`
*   **Description:** Asynchronously attempts to re-process notifications that previously failed. This method typically implements retry logic with exponential backoff or specific failure handling strategies.
*   **Parameters:** None (operates on the internal failed queue).
*   **Return Value:** Returns a `ProcessingResult` indicating how many previously failed items were successfully retried versus those that failed again.
*   **Exceptions:** May throw if the retry limit for specific messages has been exceeded and the system is configured to halt, or if transient infrastructure errors persist.

#### `ProcessByPriorityAsync`
*   **Signature:** `public async Task<ProcessingResult> ProcessByPriorityAsync`
*   **Description:** Asynchronously processes notifications ordered by their assigned priority level, ensuring high-priority messages are dispatched before lower-priority ones.
*   **Parameters:** None (operates on the internal priority-sorted queue).
*   **Return Value:** Returns a `ProcessingResult` detailing the execution of the priority-based processing run.
*   **Exceptions:** May throw if priority metadata is missing or corrupted for queued items.

## Usage

### Example 1: Standard Batch Processing with Error Inspection
The following example demonstrates initializing the processor, executing a standard batch, and inspecting the resulting errors if the success count does not match the total processed count.

```csharp
public async Task ExecuteStandardBatch(INotificationProcessor processor)
{
    // Execute the batch processing
    var result = await processor.ProcessBatchAsync();

    // Output summary statistics
    Console.WriteLine(processor.GetSummary);

    // Inspect errors if failures occurred
    if (processor.FailureCount > 0)
    {
        Console.WriteLine($"Encountered {processor.FailureCount} failures.");
        foreach (var error in processor.Errors)
        {
            Console.WriteLine($"- {error}");
        }
    }

    // Check timing metrics
    Console.WriteLine($"Average delivery time: {processor.AverageDeliveryTimeMs}ms");
}
```

### Example 2: Priority Processing and Retry Logic
This example illustrates a workflow where high-priority items are processed first, followed by a targeted retry of any items that failed during the initial pass.

```csharp
public async Task ExecutePriorityAndRetry(INotificationProcessor processor)
{
    // Process high-priority notifications first
    var priorityResult = await processor.ProcessByPriorityAsync();
    
    Console.WriteLine($"Priority run complete. Success: {processor.SuccessCount}, Pending: {processor.PendingCount}");

    // If there are failures, attempt to process them specifically
    if (processor.FailureCount > 0)
    {
        Console.WriteLine("Initiating retry sequence for failed items...");
        var retryResult = await processor.ProcessFailedAsync();

        if (processor.LastProcessedAt.HasValue)
        {
            Console.WriteLine($"Retry sequence finished at {processor.LastProcessedAt.Value:HH:mm:ss}");
        }
        
        Console.WriteLine($"Final Failed Deliveries: {processor.FailedDeliveries}");
    }
}
```

## Notes

### Thread Safety
The presence of mutable counter properties (e.g., `SuccessCount`, `TotalDeliveryAttempts`) alongside asynchronous methods suggests that implementations of `INotificationProcessor` must handle concurrent access carefully. While the interface itself does not enforce synchronization, callers should assume that reading properties like `Errors` or `DurationMs` while `ProcessBatchAsync` is executing may yield intermediate or inconsistent states unless the specific implementation guarantees atomic snapshots. It is recommended to treat the property set as eventually consistent during active processing tasks.

### Edge Cases
*   **Empty Queues:** Invoking `ProcessBatchAsync`, `ProcessFailedAsync`, or `ProcessByPriorityAsync` when `PendingCount` is zero should return a valid `ProcessingResult` with zero counts rather than throwing an exception, though `DurationMs` may be negligible.
*   **Error List Growth:** The `Errors` property is a `List<string>`. In long-running processes with high failure rates, this list could grow indefinitely if not periodically cleared by the implementation or the consumer. Consumers should inspect `FailureCount` before iterating `Errors` to avoid performance degradation.
*   **Null Timestamps:** The `LastProcessedAt` property is nullable (`DateTime?`). Consumers must check for `HasValue` before accessing the timestamp, particularly immediately after instantiation or if no processing tasks have successfully completed.
*   **Delivery Attempts vs. Notifications:** `TotalDeliveryAttempts` may exceed `TotalNotifications` if the system implements automatic retries within a single batch operation. Logic relying on a 1:1 ratio between these two properties will be flawed.
