# DotnetDeployNotifyOptions

`DotnetDeployNotifyOptions` is the central configuration class for the `dotnet-deploy-notify` library. It aggregates all settings required to control notification delivery, canary deployment behavior, retry policies, storage preferences, and environment-specific channel routing. An instance of this class is typically constructed and passed to the notification engine to define how deployment events are processed and dispatched.

## API

### NotificationConfig Notification
Gets or sets the notification configuration object that defines templates, formatting rules, and delivery constraints for standard deployment notifications. This property must be set before processing begins; a null value will cause an `ArgumentNullException` during engine initialization.

### CanaryOptions Canary
Gets or sets the canary deployment options, including rollout percentages, evaluation intervals, and health check criteria. When null, canary behavior is disabled. The engine validates this object at startup and throws an `ArgumentException` if required sub-properties are missing while canary mode is implicitly active.

### int MaxRetries
Gets or sets the maximum number of retry attempts for failed notification deliveries. Must be a non-negative integer. Values less than zero cause an `ArgumentOutOfRangeException` during validation. A value of zero disables retries entirely.

### int WebhookTimeoutMs
Gets or sets the timeout in milliseconds for individual webhook HTTP requests. Must be greater than zero. The engine throws an `ArgumentOutOfRangeException` if a non-positive value is supplied. Defaults to 5000 if not explicitly set.

### int RetryDelayMs
Gets or sets the base delay in milliseconds between retry attempts. The actual delay may incorporate exponential backoff depending on the retry strategy. Must be non-negative; negative values throw an `ArgumentOutOfRangeException`.

### bool AutoProcessNotifications
Gets or sets whether the engine automatically processes queued notifications on a timer. When `true`, the internal processing loop starts immediately upon engine activation. When `false`, notifications must be processed manually via explicit API calls.

### int ProcessingIntervalSeconds
Gets or sets the interval in seconds between automatic processing cycles when `AutoProcessNotifications` is `true`. Must be at least 1. Values below 1 throw an `ArgumentOutOfRangeException`.

### string StorageType
Gets or sets the storage backend identifier. Supported values include `"InMemory"`, `"FileSystem"`, and `"Sqlite"`. An unrecognized value causes an `ArgumentException` during storage provider resolution. This property is case-insensitive.

### string LogLevel
Gets or sets the minimum log level for internal logging output. Accepted values follow standard severity names: `"Trace"`, `"Debug"`, `"Information"`, `"Warning"`, `"Error"`, `"Critical"`. Invalid values throw an `ArgumentException`.

### string? StoragePath
Gets or sets the optional file system path used when `StorageType` is `"FileSystem"` or `"Sqlite"`. If null and the storage type requires a path, the engine throws an `InvalidOperationException` at initialization. For in-memory storage, this property is ignored.

### bool IncludeCommitDetails
Gets or sets whether deployment notifications include source control commit information (hash, message, author). When `true`, the engine attempts to resolve commit metadata from the deployment context; failures are logged but do not throw.

### bool IncludeBuildUrl
Gets or sets whether deployment notifications include a link to the CI/CD build that produced the deployment artifact. When `true`, the build URL is appended to the notification payload if available.

### string DefaultPriority
Gets or sets the default priority label assigned to notifications that do not have an explicit priority. Typical values are `"Low"`, `"Normal"`, `"High"`, `"Critical"`. An empty or null string defaults to `"Normal"`. Unrecognized values are preserved but may be ignored by downstream consumers.

### bool EnableAuditLogging
Gets or sets whether detailed audit records are written for every notification lifecycle event (queued, sent, failed, retried). Audit logs are stored using the configured `StorageType` and are subject to `RetentionDays`.

### int RetentionDays
Gets or sets the number of days audit logs and notification history records are retained before automatic cleanup. Must be non-negative. A value of zero disables retention cleanup entirely. Negative values throw an `ArgumentOutOfRangeException`.

### Dictionary<string, EnvironmentChannelConfig> EnvironmentChannels
Gets or sets a dictionary mapping environment names (e.g., `"staging"`, `"production"`) to their respective channel configurations. Each `EnvironmentChannelConfig` specifies overrides for webhook URL, channel type, and display name. A null dictionary is treated as empty. Duplicate keys are not permitted; the last write wins during deserialization.

### string WebhookUrl
Gets or sets the default webhook URL used when no environment-specific override exists. Must be a valid absolute URI; invalid formats throw a `UriFormatException` during validation.

### string ChannelType
Gets or sets the default channel type identifier (e.g., `"Slack"`, `"Teams"`, `"Discord"`, `"GenericWebhook"`). Unrecognized values cause an `ArgumentException` when the engine attempts to resolve the corresponding channel adapter.

### string DisplayName
Gets or sets the human-readable display name for the notification sender. This name appears in notification headers and audit logs. An empty string is permitted and results in the default sender name being used.

### string TargetId
Gets or sets the target identifier for the notification destination (e.g., channel ID, room ID, conversation ID). This value is passed to the channel adapter and its format depends on the `ChannelType`. An empty string is permitted for channel types that do not require a target ID.

## Usage

### Example 1: Basic Configuration with Automatic Processing
```csharp
var options = new DotnetDeployNotifyOptions
{
    WebhookUrl = "https://hooks.slack.com/services/T000/B000/XXXX",
    ChannelType = "Slack",
    DisplayName = "Deploy Bot",
    TargetId = "C0123456",
    DefaultPriority = "Normal",
    AutoProcessNotifications = true,
    ProcessingIntervalSeconds = 30,
    MaxRetries = 3,
    RetryDelayMs = 2000,
    WebhookTimeoutMs = 10000,
    IncludeCommitDetails = true,
    IncludeBuildUrl = true,
    LogLevel = "Information",
    EnableAuditLogging = true,
    RetentionDays = 90,
    StorageType = "Sqlite",
    StoragePath = "/var/lib/deploy-notify/state.db"
};

var engine = new NotificationEngine(options);
await engine.StartAsync();
```

### Example 2: Multi-Environment Setup with Canary Deployments
```csharp
var options = new DotnetDeployNotifyOptions
{
    DefaultPriority = "High",
    MaxRetries = 5,
    RetryDelayMs = 5000,
    WebhookTimeoutMs = 15000,
    AutoProcessNotifications = false,
    StorageType = "InMemory",
    LogLevel = "Debug",
    EnableAuditLogging = false,
    RetentionDays = 0,
    Canary = new CanaryOptions
    {
        InitialPercentage = 10,
        IncrementPercentage = 20,
        EvaluationIntervalMinutes = 5,
        HealthCheckEndpoint = "/health"
    },
    EnvironmentChannels = new Dictionary<string, EnvironmentChannelConfig>
    {
        ["staging"] = new EnvironmentChannelConfig
        {
            WebhookUrl = "https://hooks.slack.com/services/T000/B111/YYYY",
            ChannelType = "Slack",
            DisplayName = "Staging Deploy Bot",
            TargetId = "C0234567"
        },
        ["production"] = new EnvironmentChannelConfig
        {
            WebhookUrl = "https://teams.example.com/webhook/abc123",
            ChannelType = "Teams",
            DisplayName = "Production Deploy Bot",
            TargetId = "general"
        }
    }
};

var engine = new NotificationEngine(options);
await engine.InitializeAsync();

// Manually process notifications for a specific environment
await engine.ProcessNotificationsAsync("production");
```

## Notes

- **Validation timing**: Most property validation occurs during engine initialization (`StartAsync` or `InitializeAsync`), not at property assignment. Setting invalid values on the options object itself does not throw immediately.
- **Thread safety**: `DotnetDeployNotifyOptions` is not thread-safe for concurrent writes. Properties should be fully configured on a single thread before passing the instance to the engine. Once the engine is started, modifying properties on the shared options instance produces undefined behavior.
- **Storage path requirements**: When `StorageType` is `"FileSystem"` or `"Sqlite"`, the directory specified in `StoragePath` must exist and be writable by the process. The engine does not create parent directories automatically and will throw an `IOException` if the path is inaccessible.
- **Environment channel fallback**: If an environment name is not found in `EnvironmentChannels`, the engine falls back to the top-level `WebhookUrl`, `ChannelType`, `DisplayName`, and `TargetId` properties. An entirely empty configuration (no defaults and no matching environment entry) causes an `InvalidOperationException` when attempting to deliver a notification.
- **Retry behavior**: The `RetryDelayMs` serves as the base delay. The actual delay between retries may increase exponentially depending on internal retry policy implementation. Setting `MaxRetries` to a high value combined with a short `RetryDelayMs` may cause rapid retry storms against failing endpoints.
- **Audit log retention**: Cleanup of expired audit records runs on the same processing interval when `AutoProcessNotifications` is enabled. If automatic processing is disabled, retention cleanup must be triggered manually or records will accumulate indefinitely regardless of the `RetentionDays` value.
- **Canary interaction**: When `Canary` is configured, the engine may generate additional notification events for canary stage transitions. These notifications respect the same channel routing and priority rules as standard deployment notifications.
