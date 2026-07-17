# ChannelConfigurationBuilderExtensions

Provides extension methods for configuring notification channels in the deployment notification system. These extensions allow fine-grained control over channel behavior, including authentication, message formatting, timeouts, and environment/severity filtering.

## API

### `WithApiToken(ChannelConfigurationBuilder, string)`
Configures the API token used for authenticating with the notification channel. This is required for channels that use token-based authentication.

- **Parameters**
  - `builder`: The `ChannelConfigurationBuilder` instance to configure.
  - `token`: The API token string to use for authentication.
- **Return value**: The configured `ChannelConfigurationBuilder` for method chaining.
- **Throws**: `ArgumentNullException` if `builder` or `token` is `null`.

### `WithCustomHeader(ChannelConfigurationBuilder, string, string)`
Adds a custom HTTP header to requests sent to the notification channel.

- **Parameters**
  - `builder`: The `ChannelConfigurationBuilder` instance to configure.
  - `name`: The header name.
  - `value`: The header value.
- **Return value**: The configured `ChannelConfigurationBuilder` for method chaining.
- **Throws**: `ArgumentNullException` if `builder`, `name`, or `value` is `null`.

### `WithSetting(ChannelConfigurationBuilder, string, string)`
Sets a custom key-value pair in the channel configuration.

- **Parameters**
  - `builder`: The `ChannelConfigurationBuilder` instance to configure.
  - `key`: The setting key.
  - `value`: The setting value.
- **Return value**: The configured `ChannelConfigurationBuilder` for method chaining.
- **Throws**: `ArgumentNullException` if `builder`, `key`, or `value` is `null`.

### `WithIsEnabled(ChannelConfigurationBuilder, bool)`
Sets whether the channel is enabled for sending notifications.

- **Parameters**
  - `builder`: The `ChannelConfigurationBuilder` instance to configure.
  - `isEnabled`: `true` to enable the channel; otherwise, `false`.
- **Return value**: The configured `ChannelConfigurationBuilder` for method chaining.

### `WithMinimumPriority(ChannelConfigurationBuilder, NotificationPriority)`
Sets the minimum notification priority required for the channel to process messages.

- **Parameters**
  - `builder`: The `ChannelConfigurationBuilder` instance to configure.
  - `priority`: The minimum priority level.
- **Return value**: The configured `ChannelConfigurationBuilder` for method chaining.
- **Throws**: `ArgumentNullException` if `builder` is `null`.

### `AllowEnvironments(ChannelConfigurationBuilder, IEnumerable<string>)`
Restricts the channel to only process notifications for the specified environments.

- **Parameters**
  - `builder`: The `ChannelConfigurationBuilder` instance to configure.
  - `environments`: The allowed environment names.
- **Return value**: The configured `ChannelConfigurationBuilder` for method chaining.
- **Throws**: `ArgumentNullException` if `builder` or `environments` is `null`.

### `AllowStatuses(ChannelConfigurationBuilder, IEnumerable<DeploymentStatus>)`
Restricts the channel to only process notifications for the specified deployment statuses.

- **Parameters**
  - `builder`: The `ChannelConfigurationBuilder` instance to configure.
  - `statuses`: The allowed deployment statuses.
- **Return value**: The configured `ChannelConfigurationBuilder` for method chaining.
- **Throws**: `ArgumentNullException` if `builder` or `statuses` is `null`.

### `WithTimeoutSeconds(ChannelConfigurationBuilder, int)`
Sets the timeout for requests to the notification channel in seconds.

- **Parameters**
  - `builder`: The `ChannelConfigurationBuilder` instance to configure.
  - `seconds`: The timeout duration in seconds.
- **Return value**: The configured `ChannelConfigurationBuilder` for method chaining.
- **Throws**: `ArgumentOutOfRangeException` if `seconds` is less than `0`.

### `WithTimeoutMinutes(ChannelConfigurationBuilder, int)`
Sets the timeout for requests to the notification channel in minutes.

- **Parameters**
  - `builder`: The `ChannelConfigurationBuilder` instance to configure.
  - `minutes`: The timeout duration in minutes.
- **Return value**: The configured `ChannelConfigurationBuilder` for method chaining.
- **Throws**: `ArgumentOutOfRangeException` if `minutes` is less than `0`.

### `UseSlackBlockKit(ChannelConfigurationBuilder)`
Configures the channel to use Slack's Block Kit format for messages.

- **Parameters**
  - `builder`: The `ChannelConfigurationBuilder` instance to configure.
- **Return value**: The configured `ChannelConfigurationBuilder` for method chaining.

### `EnableEmojis(ChannelConfigurationBuilder)`
Enables the use of emojis in notification messages for this channel.

- **Parameters**
  - `builder`: The `ChannelConfigurationBuilder` instance to configure.
- **Return value**: The configured `ChannelConfigurationBuilder` for method chaining.

### `WithDisplayName(ChannelConfigurationBuilder, string)`
Sets a human-readable display name for the channel.

- **Parameters**
  - `builder`: The `ChannelConfigurationBuilder` instance to configure.
  - `displayName`: The display name to use.
- **Return value**: The configured `ChannelConfigurationBuilder` for method chaining.
- **Throws**: `ArgumentNullException` if `builder` or `displayName` is `null`.

### `ForWebhook(ChannelConfigurationBuilder)`
Configures the channel to use a webhook endpoint.

- **Parameters**
  - `builder`: The `ChannelConfigurationBuilder` instance to configure.
- **Return value**: The configured `ChannelConfigurationBuilder` for method chaining.

### `ForEmail(ChannelConfigurationBuilder)`
Configures the channel to use email as the delivery method.

- **Parameters**
  - `builder`: The `ChannelConfigurationBuilder` instance to configure.
- **Return value**: The configured `ChannelConfigurationBuilder` for method chaining.

### `IsValid(ChannelConfiguration)`
Determines whether the channel configuration is valid.

- **Parameters**
  - `configuration`: The `ChannelConfiguration` to validate.
- **Return value**: `true` if the configuration is valid; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `configuration` is `null`.

### `GetConfiguration(ChannelConfigurationBuilder)`
Builds and returns the `ChannelConfiguration` from the builder.

- **Parameters**
  - `builder`: The configured `ChannelConfigurationBuilder`.
- **Return value**: The built `ChannelConfiguration`.
- **Throws**: `InvalidOperationException` if the configuration is invalid.

### `GetMaskedConfiguration(ChannelConfigurationBuilder)`
Builds and returns the `ChannelConfiguration` with sensitive data (e.g., tokens) masked.

- **Parameters**
  - `builder`: The configured `ChannelConfigurationBuilder`.
- **Return value**: The built `ChannelConfiguration` with sensitive data masked.
- **Throws**: `InvalidOperationException` if the configuration is invalid.

## Usage

### Example 1: Configuring a Slack Webhook Channel
```csharp
var configuration = new ChannelConfigurationBuilder()
    .ForWebhook()
    .WithApiToken("xoxb-your-slack-token")
    .WithSetting("channel", "#deployments")
    .WithMinimumPriority(NotificationPriority.High)
    .AllowEnvironments(new[] { "production", "staging" })
    .UseSlackBlockKit()
    .EnableEmojis()
    .WithDisplayName("Slack Alerts")
    .WithTimeoutSeconds(30)
    .GetConfiguration();
```

### Example 2: Configuring an Email Channel with Custom Headers
```csharp
var configuration = new ChannelConfigurationBuilder()
    .ForEmail()
    .WithSetting("to", "team@example.com")
    .WithSetting("subject", "Deployment Notification")
    .WithCustomHeader("X-Custom-Header", "value")
    .WithIsEnabled(true)
    .AllowStatuses(new[] { DeploymentStatus.Success, DeploymentStatus.Failed })
    .WithTimeoutMinutes(2)
    .GetConfiguration();
```

## Notes

- All extension methods are thread-safe and may be called from any thread without additional synchronization.
- The `GetConfiguration` and `GetMaskedConfiguration` methods validate the configuration before returning it; invalid configurations throw `InvalidOperationException`.
- Timeout values are mutually exclusive; calling both `WithTimeoutSeconds` and `WithTimeoutMinutes` will result in the last one called taking precedence.
- The `IsValid` method does not modify the configuration and may be used to check validity before calling `GetConfiguration` or `GetMaskedConfiguration`.
- Sensitive data (e.g., API tokens) is masked only in the output of `GetMaskedConfiguration` and not in the internal representation of the configuration.
