# IRollbackNotificationService

A service interface for sending notifications related to rollback operations in deployment workflows. It provides methods to notify about rollback initiation, completion, and failure, as well as retrieving historical notification records.

## API

### `Id`
A unique identifier for the rollback notification instance. Used to correlate notifications with specific rollback operations.

### `RollbackRequestId`
The unique identifier of the rollback request associated with this notification. Used to track notifications related to a specific rollback operation.

### `ProjectName`
The name of the project undergoing the rollback operation. Used to contextualize notifications for project-specific deployments.

### `RollbackStatus`
The current status of the rollback operation. Indicates whether the rollback is in progress, completed, or failed.

### `TriggerStatus`
The status of the trigger that initiated the rollback. Indicates whether the trigger was successful or not.

### `Channels`
A list of notification channels (e.g., email, Slack, webhook) through which notifications will be dispatched. Channels are configured per rollback operation.

### `DispatchedAt`
The timestamp when the notification was dispatched. Used for tracking and auditing purposes.

### `DeliveryResults`
A list of results from attempted notifications across all configured channels. Each result contains delivery status and metadata for a specific channel.

### `RollbackNotificationService`
The service implementation responsible for handling rollback notifications. This is the concrete type implementing `IRollbackNotificationService`.

### `NotifyRollbackInitiatedAsync()`
Notifies all configured channels that a rollback operation has been initiated.

- **Returns**: A `Task<List<NotificationResult>>` representing the asynchronous operation. The task resolves to a list of `NotificationResult` objects indicating the success or failure of notifications sent to each channel.
- **Exceptions**: May throw if the service is misconfigured or if channels fail to initialize.

### `NotifyRollbackCompletedAsync()`
Notifies all configured channels that a rollback operation has completed successfully.

- **Returns**: A `Task<List<NotificationResult>>` representing the asynchronous operation. The task resolves to a list of `NotificationResult` objects indicating the success or failure of notifications sent to each channel.
- **Exceptions**: May throw if the service is misconfigured or if channels fail to initialize.

### `NotifyRollbackFailedAsync()`
Notifies all configured channels that a rollback operation has failed.

- **Returns**: A `Task<List<NotificationResult>>` representing the asynchronous operation. The task resolves to a list of `NotificationResult` objects indicating the success or failure of notifications sent to each channel.
- **Exceptions**: May throw if the service is misconfigured or if channels fail to initialize.

### `FormatRollbackMessage()`
Formats a rollback notification message based on the current rollback context (e.g., project name, rollback request ID, status).

- **Returns**: A `string` containing the formatted message ready for dispatch.
- **Exceptions**: None.

### `GetRollbackNotificationHistoryAsync()`
Retrieves the historical records of notifications sent for a rollback operation.

- **Returns**: A `Task<List<RollbackNotificationRecord>>` representing the asynchronous operation. The task resolves to a list of historical `RollbackNotificationRecord` objects containing metadata about past notifications.
- **Exceptions**: May throw if the storage backend is unavailable or if the rollback request ID is invalid.

## Usage

### Example 1: Notifying Rollback Initiation
