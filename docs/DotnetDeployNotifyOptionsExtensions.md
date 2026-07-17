# DotnetDeployNotifyOptionsExtensions

Provides static accessors and boolean checks for deployment notification configuration values. This class exposes a set of read-only properties and methods that retrieve settings such as webhook parameters, retry logic, canary deployment thresholds, and environment lists. All members are static and intended for use without instantiating the class.

## API

### `GetWebhookTimeoutMs`
- **Returns:** `int` – The webhook timeout in milliseconds.
- **Throws:** None.

### `GetMaxRetries`
- **Returns:** `int` – The maximum number of retry attempts for failed notifications.
- **Throws:** None.

### `IsAutoProcessingEnabled`
- **Returns:** `bool` – `true` if automatic processing of notifications is enabled; otherwise `false`.
- **Throws:** None.

### `GetPriority`
- **Returns:** `NotificationPriority` – The default priority level for notifications.
- **Throws:** None.

### `IsAuditLoggingEnabled`
- **Returns:** `bool` – `true` if audit logging is enabled; otherwise `false`.
- **Throws:** None.

### `GetDisplayName`
- **Returns:** `string` – The display name for the deployment or notification channel.
- **Throws:** None.

### `GetStoragePath`
- **Returns:** `string?` – The storage path, or `null` if not configured.
- **Throws:** None.

### `GetLogLevel`
- **Returns:** `string` – The log level (e.g., "Information", "Debug").
- **Throws:** None.

### `IsCanaryEnabled`
- **Returns:** `bool` – `true` if canary deployment is enabled; otherwise `false`.
- **Throws:** None.

### `IsCanaryAutoRollbackEnabled`
- **Returns:** `bool` – `true` if automatic rollback on canary failure is enabled; otherwise `false`.
- **Throws:** None.

### `IsCanaryAutoAdvanceEnabled`
- **Returns:** `bool` – `true` if automatic advancement between canary steps is enabled; otherwise `false`.
- **Throws:** None.

### `GetCanaryAlertPriority`
- **Returns:** `NotificationPriority` – The priority level for canary-related alerts.
- **Throws:** None.

### `GetCanaryMaxDeploymentDuration`
- **Returns:** `TimeSpan` – The maximum allowed duration for a canary deployment.
- **Throws:** None.

### `GetCanaryStepSoakDuration`
- **Returns:** `TimeSpan` – The duration each canary step must soak before advancing.
- **Throws:** None.

### `GetCanaryLinearStepCount`
- **Returns:** `int` – The number of linear steps in a canary deployment.
- **Throws:** None.

### `GetCanaryThresholds`
- **Returns:** `(double MaxErrorRatePercent, double MaxP95LatencyMs, double MaxP99LatencyMs)` – A tuple containing the maximum acceptable error rate (as a percentage), the 95th percentile latency (in milliseconds), and the 99th percentile latency (in milliseconds).
- **Throws:** None.

### `GetConfiguredEnvironments`
- **Returns:** `IEnumerable<string>` – A collection of environment names that are configured for notifications.
- **Throws:** None.

### `GetWebhookUrl`
- **Returns:** `string?` – The webhook URL, or `null` if not set.
- **Throws:** None.

### `GetChannelType`
- **Returns:** `string` – The notification channel type (e.g., "Slack", "Teams").
- **Throws:** None.

### `GetTargetId`
- **Returns:** `string?` – The target identifier for the notification channel, or `null` if not configured.
- **Throws:** None.

## Usage

```csharp
// Example 1: Checking auto-processing and webhook settings
if (DotnetDeployNotifyOptionsExtensions.IsAutoProcessingEnabled)
{
    int timeout = DotnetDeployNotifyOptionsExtensions.GetWebhookTimeoutMs;
    string? webhookUrl = DotnetDeployNotifyOptionsExtensions.GetWebhookUrl;
    Console.WriteLine($"Webhook URL: {webhookUrl ?? "not set"}, timeout: {timeout}ms");
}
```

```csharp
// Example 2: Retrieving canary thresholds and configuring a deployment
if (DotnetDeployNotifyOptionsExtensions.IsCanaryEnabled)
{
    var thresholds = DotnetDeployNotifyOptionsExtensions.GetCanaryThresholds;
    int steps = DotnetDeployNotifyOptionsExtensions.GetCanaryLinearStepCount;
    TimeSpan soak = DotnetDeployNotifyOptionsExtensions.GetCanaryStepSoakDuration;
    Console.WriteLine($"Canary: {steps} steps, soak {soak.TotalMinutes} min, max error {thresholds.MaxErrorRatePercent}%");
}
```

## Notes

- All members are static and read-only; they do not modify any state and are safe for concurrent access from multiple threads.
- Methods returning nullable types (`GetStoragePath`, `GetWebhookUrl`, `GetTargetId`) may return `null` when the corresponding configuration value is absent. Callers should check for `null` before using the result.
- The `GetConfiguredEnvironments` method returns a new collection each invocation; enumeration is safe but the collection should not be modified.
- No exceptions are thrown by any member under normal operation; values are assumed to be pre-validated or have sensible defaults.
