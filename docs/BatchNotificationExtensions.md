# BatchNotificationExtensions

Static helper class that provides read‑only access to aggregated information about a collection of `DeploymentNotification` objects. The members expose pre‑computed filters, counts, and summary strings that are useful for reporting, UI display, or decision‑making logic without requiring the caller to iterate over the raw collection themselves.

## API

### FilterByProject
- **Purpose**: Returns a list containing only those notifications that belong to the project currently tracked by the batch.
- **Return Value**: `List<DeploymentNotification>` – may be empty if no notifications match the project.
- **Exceptions**: 
  - `InvalidOperationException` if the internal notification source has not been set.

### FilterByEnvironment
- **Purpose**: Returns a list containing only those notifications that pertain to the environment currently tracked by the batch.
- **Return Value**: `List<DeploymentNotification>` – may be empty if no notifications match the environment.
- **Exceptions**: 
  - `InvalidOperationException` if the internal notification source has not been set.

### GetDeliveryStatistics
- **Purpose**: Provides a formatted string summarizing delivery outcomes (e.g., successes, failures, retries) for all notifications in the batch.
- **Return Value**: `string` – a multi‑line report suitable for logging or display.
- **Exceptions**: None.

### HasPendingNotifications
- **Purpose**: Indicates whether at least one notification in the batch is still pending processing.
- **Return Value**: `bool` – `true` if any pending notifications exist, otherwise `false`.
- **Exceptions**: None.

### GetPendingNotificationCount
- **Purpose**: Retrieves the total number of notifications that are pending.
- **Return Value**: `int` – zero or greater.
- **Exceptions**: None.

### GetProcessedNotificationCount
- **Purpose**: Retrieves the total number of notifications that have been successfully processed.
- **Return Value**: `int` – zero or greater.
- **Exceptions**: None.

### GetDetailedSummary
- **Purpose**: Returns a comprehensive string that includes counts, statuses, and channel information for the batch.
- **Return Value**: `string` – suitable for inclusion in a report or alert.
- **Exceptions**: None.

### IsTerminalState
- **Purpose**: Determines whether the batch has reached a terminal state (i.e., no further transitions are expected).
- **Return Value**: `bool` – `true` if the batch is terminal, otherwise `false`.
- **Exceptions**: None.

### GetUniqueChannelCount
- **Purpose**: Returns the number of distinct delivery channels (e.g., email, Slack, Teams) used by the notifications in the batch.
- **Return Value**: `int` – zero or greater.
- **Exceptions**: None.

### GetNotificationCountByProject
- **Purpose**: Returns the number of notifications associated with the project currently tracked by the batch.
- **Return Value**: `int` – zero or greater.
- **Exceptions**: None.

## Usage

```csharp
using DotNetDeployNotify;

// Assume `notifications` is an IEnumerable<DeploymentNotification> that has been
// supplied to the BatchNotificationExtensions context elsewhere in the application.

if (BatchNotificationExtensions.HasPendingNotifications)
{
    int pending = BatchNotificationExtensions.GetPendingNotificationCount;
    Console.WriteLine($"{pending} notifications are still pending.");
}

var projectSpecific = BatchNotificationExtensions.FilterByProject;
Console.WriteLine($"Project-specific notifications: {projectSpecific.Count}");

string stats = BatchNotificationExtensions.GetDeliveryStatistics;
File.AppendAllText("delivery.log", stats);
```

```csharp
using DotNetDeployNotify;

// After processing a batch, you may want to emit a final summary.

if (BatchNotificationExtensions.IsTerminalState)
{
    string summary = BatchNotificationExtensions.GetDetailedSummary;
    int uniqueChannels = BatchNotificationExtensions.GetUniqueChannelCount;
    Console.WriteLine($"Batch completed. {uniqueChannels} unique channels used.");
    Console.WriteLine(summary);
}
```

## Notes

- The properties rely on an internal notification source that must be configured before accessing any member; attempting to read a property before the source is set will result in an `InvalidOperationException`.
- All members are thread‑safe for concurrent read operations; however, mutating the underlying notification source while reading these properties may lead to inconsistent results.
- Returned lists are snapshots; subsequent changes to the source collection are not reflected in the lists returned by `FilterByProject` or `FilterByEnvironment`.
- The string‑returning members (`GetDeliveryStatistics`, `GetDetailedSummary`) are formatted for human readability and are not intended for programmatic parsing; use the count‑based members for programmatic logic.
