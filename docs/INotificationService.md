# INotificationService

The `INotificationService` provides a comprehensive interface for managing the lifecycle of deployment notifications. It supports creating notification records, dispatching them to configured providers, managing pending queues, retrieving historical data, and handling retries for failed delivery attempts.

## API

### Constructor
- `NotificationService`: Initializes a new instance of the `NotificationService` class, setting up necessary dependencies for notification persistence and delivery dispatching.

### Methods

- `CreateNotificationAsync`: Persists a new notification request to the underlying data store and returns a unique identifier string for the created entry.
- `SendPendingNotificationsAsync`: Identifies all notifications in a "Pending" state and attempts to dispatch them. Returns a list of `NotificationResult` objects detailing the outcome of each attempt.
- `SendNotificationAsync`: Manually triggers the delivery process for a specific notification. Returns a list of `NotificationResult` objects representing the dispatch outcome.
- `GetNotificationHistoryAsync`: Retrieves a collection of all previously processed `DeploymentNotification` objects, providing an audit trail of deployment activities.
- `GetDeliveryResultsAsync`: Fetches the current delivery status for notifications, returning a list of `NotificationResult` objects summarizing success or failure states.
- `RetryFailedDeliveriesAsync`: Scans the system for notifications that failed during previous delivery attempts and attempts to resend them. Returns a list of `NotificationResult` objects for each retry attempt.

## Usage

```csharp
// Example 1: Creating and sending a notification
var service = new NotificationService(logger, repository);
var notificationId = await service.CreateNotificationAsync(deploymentInfo);

var results = await service.SendNotificationAsync(notificationId);
foreach (var result in results)
{
    Console.WriteLine($"Notification sent: {result.IsSuccess}");
}
```

```csharp
// Example 2: Retrying failed deployments
var service = new NotificationService(logger, repository);
var retryResults = await service.RetryFailedDeliveriesAsync();

if (retryResults.Any(r => !r.IsSuccess))
{
    logger.LogWarning("Some notifications failed to retry.");
}
```

## Notes

- **Thread Safety**: This service is designed to be used in asynchronous contexts. While implementations are typically thread-safe when dealing with external database or network resources, callers should ensure that the underlying data store access remains consistent.
- **Exceptions**: All methods are `async` and may throw exceptions related to connectivity issues (e.g., database timeouts, network failures when communicating with external notification providers) or data validation errors.
- **Performance**: Operations involving `SendPendingNotificationsAsync` or `RetryFailedDeliveriesAsync` can be I/O intensive depending on the number of pending or failed notifications. Consider wrapping these operations in appropriate timeout or retry policies.
