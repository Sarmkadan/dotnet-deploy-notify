# ChannelConfigurationBuilder

The `ChannelConfigurationBuilder` class provides a fluent API for constructing a `ChannelConfiguration` object that defines how notifications are sent to a specific channel (e.g., Slack, Discord, Telegram). It allows you to set the channel’s name, webhook URL, target identifier, timeout, retry policy, priority filters, and various feature toggles. The builder is mutable and not thread-safe; each method returns the same builder instance to enable method chaining.

## API

### `public ChannelConfigurationBuilder()`

Initializes a new instance of the builder with default values.  
No parameters.  
Does not throw.

### `public ChannelConfigurationBuilder WithName(string name)`

Sets the display name of the channel.  
**Parameters:**  
- `name` – A non-null, non-empty string representing the channel name.  
**Returns:** The same `ChannelConfigurationBuilder` instance.  
**Throws:** `ArgumentException` if `name` is `null` or empty.

### `public ChannelConfigurationBuilder WithWebhook(string webhookUrl)`

Sets the webhook URL used to send notifications.  
**Parameters:**  
- `webhookUrl` – A valid absolute URL string.  
**Returns:** The same `ChannelConfigurationBuilder` instance.  
**Throws:** `ArgumentException` if `webhookUrl` is `null`, empty, or not a valid absolute URI.

### `public ChannelConfigurationBuilder WithTargetId(string targetId)`

Sets an optional target identifier (e.g., a Slack channel ID or Discord channel ID).  
**Parameters:**  
- `targetId` – A non-null, non-empty string.  
**Returns:** The same `ChannelConfigurationBuilder` instance.  
**Throws:** `ArgumentException` if `targetId` is `null` or empty.

### `public ChannelConfigurationBuilder WithTimeout(TimeSpan timeout)`

Sets the maximum time to wait for a notification to be sent.  
**Parameters:**  
- `timeout` – A `TimeSpan` representing the timeout duration. Must be non-negative.  
**Returns:** The same `ChannelConfigurationBuilder` instance.  
**Throws:** `ArgumentOutOfRangeException` if `timeout` is negative.

### `public ChannelConfigurationBuilder WithRetries(int retries)`

Sets the number of retry attempts if sending fails.  
**Parameters:**  
- `retries` – A non-negative integer.  
**Returns:** The same `ChannelConfigurationBuilder` instance.  
**Throws:** `ArgumentOutOfRangeException` if `retries` is negative.

### `public ChannelConfigurationBuilder WithMinimumPriority(int priority)`

Sets the minimum priority level required for a notification to be sent through this channel.  
**Parameters:**  
- `priority` – An integer representing the priority threshold.  
**Returns:** The same `ChannelConfigurationBuilder` instance.  
**Throws:** None.

### `public ChannelConfigurationBuilder IncludeCommitDetails()`

Enables inclusion of commit details (e.g., author, message, hash) in the notification.  
**Parameters:** None.  
**Returns:** The same `ChannelConfigurationBuilder` instance.  
**Throws:** None.

### `public ChannelConfigurationBuilder IncludeBuildUrl()`

Enables inclusion of the build URL in the notification.  
**Parameters:** None.  
**Returns:** The same `ChannelConfigurationBuilder` instance.  
**Throws:** None.

### `public ChannelConfigurationBuilder AllowEnvironments(params string[] environments)`

Restricts notifications to only the specified deployment environments.  
**Parameters:**  
- `environments` – One or more environment names (e.g., "production", "staging"). Passing an empty array clears the filter.  
**Returns:** The same `ChannelConfigurationBuilder` instance.  
**Throws:** `ArgumentException` if any environment string is `null` or empty.

### `public ChannelConfigurationBuilder AllowStatuses(params string[] statuses)`

Restricts notifications to only the specified deployment statuses (e.g., "success", "failure").  
**Parameters:**  
- `statuses` – One or more status strings. Passing an empty array clears the filter.  
**Returns:** The same `ChannelConfigurationBuilder` instance.  
**Throws:** `ArgumentException` if any status string is `null` or empty.

### `public ChannelConfigurationBuilder OnlyProduction()`

Convenience method that restricts notifications to the "production" environment only. Equivalent to calling `AllowEnvironments("production")`.  
**Parameters:** None.  
**Returns:** The same `ChannelConfigurationBuilder` instance.  
**Throws:** None.

### `public ChannelConfigurationBuilder OnlyOnFailure()`

Configures the channel to send notifications only when a deployment fails.  
**Parameters:** None.  
**Returns:** The same `ChannelConfigurationBuilder` instance.  
**Throws:** None.

### `public ChannelConfigurationBuilder OnlyOnSuccess()`

Configures the channel to send notifications only when a deployment succeeds.  
**Parameters:** None.  
**Returns:** The same `ChannelConfigurationBuilder` instance.  
**Throws:** None.

### `public ChannelConfigurationBuilder UseSlackBlockKit()`

Enables Slack Block Kit formatting for messages (only applicable to Slack channels).  
**Parameters:** None.  
**Returns:** The same `ChannelConfigurationBuilder` instance.  
**Throws:** None.

### `public ChannelConfigurationBuilder EnableEmojis()`

Enables emoji decorations in notification messages.  
**Parameters:** None.  
**Returns:** The same `ChannelConfigurationBuilder` instance.  
**Throws:** None.

### `public ChannelConfiguration Build()`

Constructs and returns an immutable `ChannelConfiguration` instance based on the current builder state.  
**Parameters:** None.  
**Returns:** A `ChannelConfiguration` object.  
**Throws:** `InvalidOperationException` if required properties (e.g., webhook URL) have not been set.

### `public static ChannelConfigurationBuilder ForSlack()`

Creates a new `ChannelConfigurationBuilder` pre-configured with defaults suitable for Slack (e.g., default webhook format, Block Kit support).  
**Returns:** A new `ChannelConfigurationBuilder` instance.  
**Throws:** None.

### `public static ChannelConfigurationBuilder ForDiscord()`

Creates a new `ChannelConfigurationBuilder` pre-configured with defaults suitable for Discord.  
**Returns:** A new `ChannelConfigurationBuilder` instance.  
**Throws:** None.

### `public static ChannelConfigurationBuilder ForTelegram()`

Creates a new `ChannelConfigurationBuilder` pre-configured with defaults suitable for Telegram.  
**Returns:** A new `ChannelConfigurationBuilder` instance.  
**Throws:** None.

## Usage

### Example 1: Slack channel with commit details and failure-only notifications

```csharp
var slackConfig = ChannelConfigurationBuilder
    .ForSlack()
    .WithName("Deploy Alerts")
    .WithWebhook("https://hooks.slack.com/services/T00/B00/xxx")
    .WithTimeout(TimeSpan.FromSeconds(30))
    .WithRetries(3)
    .IncludeCommitDetails()
    .IncludeBuildUrl()
    .OnlyOnFailure()
    .UseSlackBlockKit()
    .EnableEmojis()
    .Build();
```

### Example 2: Discord channel with environment and status filters

```csharp
var discordConfig = ChannelConfigurationBuilder
    .ForDiscord()
    .WithName("Production Notifications")
    .WithWebhook("https://discord.com/api/webhooks/123/abc")
    .WithTargetId("123456789")
    .WithMinimumPriority(5)
    .AllowEnvironments("production", "staging")
    .AllowStatuses("success", "failure")
    .OnlyOnSuccess()
    .Build();
```

## Notes

- **Thread safety:** The builder is not thread-safe. Concurrent modifications from multiple threads may lead to inconsistent state. Each thread should use its own builder instance, or external synchronization must be applied.
- **Conflicting settings:** Calling both `OnlyOnFailure` and `OnlyOnSuccess` will result in the last call taking precedence. Similarly, `OnlyProduction` overrides any previous `AllowEnvironments` call. The `Build` method does not validate such conflicts; the resulting `ChannelConfiguration` will reflect the most recent setting.
- **Required properties:** At a minimum, a webhook URL must be set via `WithWebhook` before calling `Build`; otherwise, `Build` throws `InvalidOperationException`. The static factory methods (`ForSlack`, `ForDiscord`, `ForTelegram`) may pre-populate some defaults, but a webhook is still required.
- **Empty filters:** Passing an empty array to `AllowEnvironments` or `AllowStatuses` clears any previously set filters, effectively allowing all environments or statuses.
- **Timeout and retries:** A zero `TimeSpan` for timeout means no timeout (infinite wait). A zero retry count means no retries. Negative values are rejected.
- **Platform-specific features:** `UseSlackBlockKit` has no effect on non-Slack channels; it is ignored if the channel was not created via `ForSlack` or if the underlying provider does not support Block Kit.
