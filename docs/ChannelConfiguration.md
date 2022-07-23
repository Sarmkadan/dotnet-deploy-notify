# ChannelConfiguration

Represents the configuration for a notification channel used in deployment notifications, including channel type, target endpoints, filtering rules, and presentation options.

## API

### `Id`
Unique identifier for the channel configuration. Must be non-empty and unique within the system.

### `ChannelType`
Type of notification channel (e.g., Webhook, Slack). Determines how notifications are delivered. Must be a valid `NotificationChannel` enum value.

### `WebhookUrl`
Target URL for webhook-based channels. Required when `ChannelType` is Webhook. Must be a valid HTTPS URL.

### `ApiToken`
Authentication token for API-based channels. Optional; used for securing API calls to the notification endpoint.

### `TargetId`
Identifier for the specific target (e.g., Slack workspace ID, Teams group ID). Optional; used to route notifications to the correct destination.

### `DisplayName`
Human-readable name for the channel, shown in UI and logs. Optional; defaults to a generated name if omitted.

### `IsEnabled`
Indicates whether the channel is active and will process notifications. Defaults to `true`.

### `IncludeCommitDetails`
Determines whether commit messages and authors are included in notifications. Defaults to `false`.

### `IncludeBuildUrl`
Determines whether a direct link to the build or deployment is included in notifications. Defaults to `false`.

### `MinimumPriority`
Minimum notification priority level required for this channel to process a notification. Uses the `NotificationPriority` enum. Defaults to `NotificationPriority.Normal`.

### `AllowedEnvironments`
List of environments (e.g., "Production", "Staging") for which this channel will send notifications. Empty list means all environments are allowed.

### `AllowedStatuses`
List of build or deployment statuses (e.g., "Succeeded", "Failed") that this channel will process. Empty list means all statuses are allowed.

### `MaxRetries`
Maximum number of retry attempts for failed notification deliveries. Must be a non-negative integer. Defaults to `3`.

### `TimeoutMs`
Timeout in milliseconds for outgoing HTTP requests to notification endpoints. Must be a positive integer. Defaults to `5000`.

### `CustomHeaders`
Additional HTTP headers to include in requests to the notification endpoint. Optional; defaults to an empty dictionary.

### `Settings`
Channel-specific settings as key-value pairs. Optional; structure and values depend on the `ChannelType`.

### `UseSlackBlockKit`
Indicates whether Slack notifications should use Block Kit formatting. Only applicable when `ChannelType` is Slack. Defaults to `false`.

### `EnableEmojis`
Indicates whether emojis should be included in notifications. Optional; defaults to `false`.

### `CreatedAt`
Timestamp indicating when the channel configuration was created. Set automatically on creation; immutable.

### `UpdatedAt`
Timestamp indicating when the channel configuration was last updated. Updated automatically on modification; `null` if never updated.

## Usage
