# ChannelConfigurationExtensions

Extension methods for `ChannelConfiguration` that provide common operations, validations, and convenience accessors for notification channel configurations. These methods simplify working with channel settings, environment filtering, status filtering, and sensitive data handling.

## API

### DeepCopy

```csharp
public static ChannelConfiguration DeepCopy(this ChannelConfiguration configuration)
```

Creates a deep copy of a channel configuration instance.

- **configuration**: The channel configuration to copy
- **Returns**: A new `ChannelConfiguration` instance with all properties copied
- **Throws**: `ArgumentNullException` if `configuration` is null

The returned copy includes new collections for `AllowedEnvironments`, `AllowedStatuses`, `CustomHeaders`, and `Settings` to prevent shared mutable state.

---

### IsEnvironmentAllowed

```csharp
public static bool IsEnvironmentAllowed(this ChannelConfiguration configuration, Environment environment)
```

Determines whether the specified environment is allowed by this channel configuration.

- **configuration**: The channel configuration to check
- **environment**: The environment to validate
- **Returns**: `true` if the environment is allowed or if no environment restrictions exist; otherwise `false`
- **Throws**: `ArgumentNullException` if `configuration` or `environment` is null

Returns `true` when `AllowedEnvironments` is empty (no restrictions) or when the environment is present in the allowed list.

---

### IsStatusAllowed

```csharp
public static bool IsStatusAllowed(this ChannelConfiguration configuration, BuildStatus status)
```

Determines whether the specified build status is allowed by this channel configuration.

- **configuration**: The channel configuration to check
- **status**: The build status to validate
- **Returns**: `true` if the status is allowed or if no status restrictions exist; otherwise `false`
- **Throws**: `ArgumentNullException` if `configuration` or `status` is null

Returns `true` when `AllowedStatuses` is empty (no restrictions) or when the status is present in the allowed list.

---

### GetEffectiveTimeout

```csharp
public static int GetEffectiveTimeout(this ChannelConfiguration configuration, int defaultTimeoutMs = 10000)
```

Returns the effective timeout value for this channel, clamped to valid bounds.

- **configuration**: The channel configuration
- **defaultTimeoutMs**: The default timeout value to use if the configured value is invalid (default: 10000)
- **Returns**: The effective timeout in milliseconds, guaranteed to be between 1000 and 60000 inclusive
- **Throws**: `ArgumentNullException` if `configuration` is null

The value is constrained to the range `[1000, 60000]` to prevent invalid or extreme timeout configurations.

---

### GetEffectiveRetryCount

```csharp
public static int GetEffectiveRetryCount(this ChannelConfiguration configuration, int defaultMaxRetries = 3)
```

Returns the effective retry count for this channel, clamped to valid bounds.

- **configuration**: The channel configuration
- **defaultMaxRetries**: The default retry count to use if the configured value is invalid (default: 3)
- **Returns**: The effective retry count, guaranteed to be between 0 and 10 inclusive
- **Throws**: `ArgumentNullException` if `configuration` is null

The value is constrained to the range `[0, 10]` to prevent excessive or negative retry configurations.

---

### GetPriorityThresholdDisplay

```csharp
public static string GetPriorityThresholdDisplay(this ChannelConfiguration configuration)
```

Returns a human-readable string representation of the minimum priority threshold.

- **configuration**: The channel configuration
- **Returns**: A display string such as "Critical", "High", "Normal", or "Low"
- **Throws**: `ArgumentNullException` if `configuration` is null

Maps `NotificationPriority` values to their string equivalents; unknown values are converted using `ToString()`.

---

### GetChannelTypeDisplay

```csharp
public static string GetChannelTypeDisplay(this ChannelConfiguration configuration)
```

Returns a human-readable string representation of the channel type.

- **configuration**: The channel configuration
- **Returns**: A display string such as "Telegram", "Slack", "Discord", "Webhook", or "Email"
- **Throws**: `ArgumentNullException` if `configuration` is null

Maps `NotificationChannel` values to their display names; unknown values are converted using `ToString()`.

---

### ShouldIncludeCommitDetails

```csharp
public static bool ShouldIncludeCommitDetails(this ChannelConfiguration configuration)
```

Determines whether this channel should include commit details in notifications.

- **configuration**: The channel configuration
- **Returns**: `true` if commit details should be included; otherwise `false`
- **Throws**: `ArgumentNullException` if `configuration` is null

Returns `true` only when both `IncludeCommitDetails` is `true` and the channel `IsEnabled` is `true`.

---

### ShouldIncludeBuildUrl

```csharp
public static bool ShouldIncludeBuildUrl(this ChannelConfiguration configuration)
```

Determines whether this channel should include the build URL in notifications.

- **configuration**: The channel configuration
- **Returns**: `true` if the build URL should be included; otherwise `false`
- **Throws**: `ArgumentNullException` if `configuration` is null

Returns `true` only when both `IncludeBuildUrl` is `true` and the channel `IsEnabled` is `true`.

---

### GetCustomHeaders

```csharp
public static IReadOnlyDictionary<string, string> GetCustomHeaders(this ChannelConfiguration configuration)
```

Returns all custom headers as a read-only dictionary for safe access.

- **configuration**: The channel configuration
- **Returns**: An `IReadOnlyDictionary<string, string>` view of the custom headers
- **Throws**: `ArgumentNullException` if `configuration` is null

The returned dictionary is a read-only wrapper around the internal dictionary to prevent external modifications.

---

### GetSettings

```csharp
public static IReadOnlyDictionary<string, string> GetSettings(this ChannelConfiguration configuration)
```

Returns all settings as a read-only dictionary for safe access.

- **configuration**: The channel configuration
- **Returns**: An `IReadOnlyDictionary<string, string>` view of the settings
- **Throws**: `ArgumentNullException` if `configuration` is null

The returned dictionary is a read-only wrapper around the internal dictionary to prevent external modifications.

---

### GetSettingOrDefault

```csharp
public static string GetSettingOrDefault(
    this ChannelConfiguration configuration,
    string key,
    string defaultValue = ""
)
```

Gets a setting value with a fallback to a default value if the key is not found.

- **configuration**: The channel configuration
- **key**: The setting key to retrieve
- **defaultValue**: The value to return if the key is not present (default: empty string)
- **Returns**: The setting value if found; otherwise the default value
- **Throws**: `ArgumentNullException` if `configuration` is null; `ArgumentException` if `key` is null or empty

Returns the value associated with `key` in `Settings`, or `defaultValue` if the key does not exist.

---

### UsesSlackBlockKit

```csharp
public static bool UsesSlackBlockKit(this ChannelConfiguration configuration)
```

Determines whether this channel configuration uses Slack Block Kit format.

- **configuration**: The channel configuration
- **Returns**: `true` if using Slack Block Kit; otherwise `false`
- **Throws**: `ArgumentNullException` if `configuration` is null

Returns `true` only when both `UseSlackBlockKit` is `true` and the channel type is `NotificationChannel.Slack`.

---

### EmojisEnabled

```csharp
public static bool EmojisEnabled(this ChannelConfiguration configuration)
```

Determines whether emojis are enabled for this channel configuration.

- **configuration**: The channel configuration
- **Returns**: `true` if emojis are enabled; otherwise `false`
- **Throws**: `ArgumentNullException` if `configuration` is null

Returns `true` only when both `EnableEmojis` is `true` and the channel `IsEnabled` is `true`.

---

### GetMaskedForLogging

```csharp
public static ChannelConfiguration GetMaskedForLogging(this ChannelConfiguration configuration)
```

Returns a masked copy of the configuration suitable for logging, with sensitive data obscured.

- **configuration**: The channel configuration
- **Returns**: A new `ChannelConfiguration` instance with sensitive fields cleared and collections emptied
- **Throws**: `ArgumentNullException` if `configuration` is null

Sensitive fields such as `ApiToken`, `WebhookUrl`, and `TargetId` are masked, and both `CustomHeaders` and `Settings` dictionaries are replaced with empty collections. The method delegates to `GetMasked()` for masking logic and then clears the dictionaries.


## Usage

### Filtering notifications by environment

```csharp
var config = new ChannelConfiguration
{
    AllowedEnvironments = new List<Environment> { Environment.Production, Environment.Staging },
    IsEnabled = true
};

var env = Environment.Development;

if (config.IsEnvironmentAllowed(env))
{
    Console.WriteLine("Notifications will be sent for this environment.");
}
else
{
    Console.WriteLine("Notifications are filtered out for this environment.");
}
```

### Retrieving effective timeout and retry values

```csharp
var config = new ChannelConfiguration
{
    TimeoutMs = 5000,
    MaxRetries = 5,
    IsEnabled = true
};

int timeout = config.GetEffectiveTimeout(); // Returns 5000
int retries = config.GetEffectiveRetryCount(); // Returns 5

Console.WriteLine($"Timeout: {timeout}ms, Retries: {retries}");
```

## Notes

- All public methods validate their input parameters using `ArgumentNullException.ThrowIfNull` and throw on null arguments.
- Methods that return collections return read-only wrappers to prevent external mutation of internal state.
- `GetEffectiveTimeout` and `GetEffectiveRetryCount` clamp values to safe ranges to avoid invalid configurations.
- `ShouldIncludeCommitDetails`, `ShouldIncludeBuildUrl`, `UsesSlackBlockKit`, and `EmojisEnabled` combine multiple boolean flags, requiring both the feature flag and the channel to be enabled.
- `GetMaskedForLogging` returns a new instance with sensitive data masked and dictionaries cleared, making it safe for inclusion in logs or telemetry.
- The class is thread-safe for concurrent reads; concurrent modifications to the underlying `ChannelConfiguration` instance are not protected by these extension methods.