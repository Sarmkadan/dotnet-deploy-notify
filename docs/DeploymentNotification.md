# DeploymentNotification

Represents a deployment notification event containing metadata about a deployment and its delivery configuration. This type is used to capture deployment details (project, version, environment, commit information, etc.), track delivery state (processed, attempts), and configure how the notification is sent (channels, priority, custom metadata).

## API

### Properties

| Name | Type | Description |
|------|------|-------------|
| `Id` | `string` | Unique identifier for the notification. |
| `ProjectName` | `string` | Name of the project being deployed. |
| `Version` | `string` | Version identifier of the deployment (e.g., semantic version). |
| `Status` | `BuildStatus` | Current build status of the deployment (e.g., Success, Failure). |
| `Message` | `string` | Free‑text message associated with the deployment. |
| `TargetEnvironment` | `Environment` | Target environment for the deployment (e.g., Production, Staging). |
| `BranchName` | `string` | Source branch from which the deployment was triggered. |
| `CommitHash` | `string` | Full commit hash of the deployed revision. |
| `CommitAuthor` | `string` | Name or identifier of the commit author. |
| `RepositoryUrl` | `string` | URL of the source repository. |
| `BuildUrl` | `string` | URL pointing to the build or CI/CD pipeline run. |
| `DurationSeconds` | `int?` | Duration of the deployment in seconds, or `null` if unknown. |
| `CreatedAt` | `DateTime` | Timestamp when the notification was created. |
| `Channels` | `List<NotificationChannel>` | List of delivery channels (e.g., Slack, Email) to which the notification should be sent. |
| `Priority` | `NotificationPriority` | Priority level of the notification (e.g., Low, Normal, High). |
| `Metadata` | `Dictionary<string, object>` | Custom key‑value pairs for additional context. |
| `IsProcessed` | `bool` | Indicates whether the notification has been delivered. |
| `DeliveryAttempts` | `int` | Number of delivery attempts made so far. |
| `IsValid` | `bool` | Indicates whether the notification contains all required fields for delivery. |

### Methods

#### `GetSummary()`

Returns a human‑readable summary string of the notification.

- **Signature**: `public string GetSummary()`
- **Parameters**: None.
- **Return value**: A `string` containing a formatted summary (e.g., "Deployment of Project v1.2.3 to Production – Success").
- **Throws**: Does not throw.

## Usage

### Example 1: Creating and populating a notification

```csharp
var notification = new DeploymentNotification
{
    Id = Guid.NewGuid().ToString(),
    ProjectName = "MyApp",
    Version = "2.1.0",
    Status = BuildStatus.Success,
    Message = "Deployment completed successfully.",
    TargetEnvironment = Environment.Production,
    BranchName = "main",
    CommitHash = "a1b2c3d4e5f6...",
    CommitAuthor = "jane.doe",
    RepositoryUrl = "https://github.com/org/myapp",
    BuildUrl = "https://ci.example.com/builds/123",
    DurationSeconds = 45,
    CreatedAt = DateTime.UtcNow,
    Channels = new List<NotificationChannel> { NotificationChannel.Slack, NotificationChannel.Email },
    Priority = NotificationPriority.High,
    Metadata = new Dictionary<string, object>
    {
        ["deployer"] = "ci-bot",
        ["region"] = "us-east-1"
    },
    IsProcessed = false,
    DeliveryAttempts = 0
};

Console.WriteLine(notification.GetSummary());
// Output: "Deployment of MyApp v2.1.0 to Production – Success"
```

### Example 2: Validating and processing a notification

```csharp
var notification = LoadNotificationFromDatabase(id);

if (!notification.IsValid)
{
    Log.Warning("Notification {Id} is invalid; skipping delivery.", notification.Id);
    return;
}

if (notification.IsProcessed)
{
    Log.Info("Notification {Id} already processed.", notification.Id);
    return;
}

foreach (var channel in notification.Channels)
{
    bool delivered = await SendToChannelAsync(channel, notification);
    if (delivered)
    {
        notification.DeliveryAttempts++;
    }
}

notification.IsProcessed = true;
SaveNotification(notification);
```

## Notes

- **Nullability**: Properties of type `string` (`Id`, `ProjectName`, `Version`, `Message`, `BranchName`, `CommitHash`, `CommitAuthor`, `RepositoryUrl`, `BuildUrl`) may be `null` if not set. The `IsValid` property should be checked before delivery to ensure required fields are populated.
- **Empty collections**: `Channels` and `Metadata` can be empty lists/dictionaries. An empty `Channels` list means no delivery will be attempted.
- **Thread safety**: This type is not thread‑safe. Concurrent reads and writes to its mutable properties (including `Channels`, `Metadata`, `IsProcessed`, `DeliveryAttempts`) must be synchronized by the caller.
- **`DurationSeconds`**: When `null`, the deployment duration is unknown. Consumers should handle this case gracefully.
- **`GetSummary()`**: The exact format of the returned string is implementation‑defined and may change between versions. It should not be parsed programmatically.
