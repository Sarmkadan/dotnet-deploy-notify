# IBatchNotificationService

Central service interface for managing and monitoring batches of notifications. It provides operations to create, track, and cancel batches, as well as retrieve statistics and results for ongoing notification deliveries.

## API

### `BatchId`
Gets the unique identifier for the current batch. This value is assigned when the batch is created and remains constant for its lifetime.

### `NotificationCount`
Gets the number of notifications currently in the batch.

### `TotalDeliveryTargets`
Gets the total number of delivery targets across all notifications in the batch.

### `CompletedDeliveries`
Gets the number of deliveries that have completed processing, regardless of success or failure.

### `PendingDeliveries`
Gets the number of deliveries that are currently in progress or queued for processing.

### `SuccessfulDeliveries`
Gets the number of deliveries that completed successfully.

### `FailedDeliveries`
Gets the number of deliveries that failed during processing.

### `AverageDeliveryTimeMs`
Gets the average time, in milliseconds, taken to deliver notifications in this batch.

### `SuccessRate`
Gets the ratio of successful deliveries to total completed deliveries in this batch, expressed as a value between 0.0 and 1.0.

### `BatchNotificationService`
Gets the service implementation type associated with this batch.

### `CreateBatchAsync()`
Creates a new notification batch and returns its unique identifier.

**Returns**
`Task<string>`: A task that resolves to the batch identifier.

**Throws**
`InvalidOperationException`: If a batch is already active or creation fails.

### `GetBatchAsync(string batchId)`
Retrieves the batch metadata for the specified batch identifier.

**Parameters**
- `batchId` (string): The unique identifier of the batch to retrieve.

**Returns**
`Task<BatchNotification?>`: A task that resolves to the batch metadata, or `null` if not found.

**Throws**
`ArgumentNullException`: If `batchId` is `null`.
`ArgumentException`: If `batchId` is empty or invalid.

### `AddNotificationAsync(string batchId, Notification notification)`
Adds a new notification to the specified batch.

**Parameters**
- `batchId` (string): The unique identifier of the batch.
- `notification` (Notification): The notification to add.

**Returns**
`Task`: A task that completes when the operation finishes.

**Throws**
`ArgumentNullException`: If `batchId` or `notification` is `null`.
`ArgumentException`: If `batchId` is empty or invalid.
`InvalidOperationException`: If the batch does not exist or is not accepting new notifications.

### `RemoveNotificationAsync(string batchId, string notificationId)`
Removes a notification from the specified batch.

**Parameters**
- `batchId` (string): The unique identifier of the batch.
- `notificationId` (string): The unique identifier of the notification to remove.

**Returns**
`Task`: A task that completes when the operation finishes.

**Throws**
`ArgumentNullException`: If `batchId` or `notificationId` is `null`.
`ArgumentException`: If `batchId` or `notificationId` is empty or invalid.
`InvalidOperationException`: If the batch does not exist.

### `SendBatchAsync(string batchId)`
Sends all notifications in the specified batch to their respective delivery targets.

**Parameters**
- `batchId` (string): The unique identifier of the batch.

**Returns**
`Task<List<NotificationResult>>`: A task that resolves to a list of results indicating the outcome of each delivery attempt.

**Throws**
`ArgumentNullException`: If `batchId` is `null`.
`ArgumentException`: If `batchId` is empty or invalid.
`InvalidOperationException`: If the batch does not exist or is already being processed.

### `GetPendingBatchesAsync()`
Retrieves all batches that have pending deliveries.

**Returns**
`Task<List<BatchNotification>>`: A task that resolves to a list of batches with pending deliveries.

### `CancelBatchAsync(string batchId)`
Cancels the specified batch, preventing further deliveries from being attempted.

**Parameters**
- `batchId` (string): The unique identifier of the batch to cancel.

**Returns**
`Task`: A task that completes when the operation finishes.

**Throws**
`ArgumentNullException`: If `batchId` is `null`.
`ArgumentException`: If `batchId` is empty or invalid.
`InvalidOperationException`: If the batch does not exist or has already completed.

### `GetBatchStatisticsAsync(string batchId)`
Retrieves aggregated statistics for the specified batch.

**Parameters**
- `batchId` (string): The unique identifier of the batch.

**Returns**
`Task<BatchStatistics>`: A task that resolves to the batch statistics.

**Throws**
`ArgumentNullException`: If `batchId` is `null`.
`ArgumentException`: If `batchId` is empty or invalid.
`InvalidOperationException`: If the batch does not exist.

## Usage

### Creating and sending a batch
