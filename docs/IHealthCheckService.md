# IHealthCheckService

`IHealthCheckService` represents the aggregated health snapshot of a notification delivery pipeline. It exposes both a top‑level system status and per‑channel metrics, allowing consumers to monitor delivery success rates, latency, pending workloads, and failure details without coupling to the underlying transport implementations.

## API

### SystemStatus
`HealthStatus SystemStatus`

Overall health classification of the notification system. The value is derived from the combined states of all configured channels and internal queue conditions. Possible values follow the `HealthStatus` enumeration (`Healthy`, `Degraded`, `Unhealthy`).

### ChannelStatuses
`List<ChannelHealthStatus> ChannelStatuses`

Per‑channel health breakdown. Each entry contains the channel‑specific metrics (`SuccessRate`, `AvgDeliveryTimeMs`, `LastSuccessAt`, `LastFailureAt`, `ConsecutiveFailures`, `ErrorMessage`) and its own `HealthStatus`. An empty list indicates no channels are currently registered.

### Status
`string Status`

Human‑readable label that mirrors `SystemStatus` (e.g. `"Healthy"`, `"Degraded"`, `"Unhealthy"`). This is a duplicate convenience property kept in sync with the enum; both return the same logical state.

### HealthPercentage
`double HealthPercentage`

A value between 0.0 and 100.0 representing the weighted health score across all channels. A score of 100.0 means every channel is operating without recent failures and with acceptable latency. The calculation weights channels equally unless a channel is explicitly disabled.

### PendingNotifications
`int PendingNotifications`

Number of notifications currently queued for delivery but not yet dispatched or acknowledged. This count spans all channels.

### FailingChannels
`int FailingChannels`

Count of channels whose `HealthStatus` is `Unhealthy` at the time of the snapshot. A channel is considered failing when its `ConsecutiveFailures` exceeds the configured threshold or its last delivery attempt resulted in a non‑transient error.

### Errors
`List<string> Errors`

Aggregated error messages collected from all unhealthy channels and system‑level faults (e.g. configuration load failures). Each string is a human‑readable summary of one distinct problem. The list may be empty when `SystemStatus` is `Healthy`.

### CheckedAt
`DateTime CheckedAt`

UTC timestamp indicating when this health snapshot was generated. Consumers should treat data as point‑in‑time; values do not update automatically.

### ConfigurationId
`string ConfigurationId`

Unique identifier of the notification configuration set that was evaluated. This corresponds to the deployment or environment configuration profile.

### Channel
`NotificationChannel Channel`

The specific channel type this snapshot primarily describes when the service is scoped to a single channel. In multi‑channel scenarios this property may reflect the first unhealthy channel or remain unset depending on the implementation context.

### ConfigName
`string ConfigName`

Human‑readable name of the configuration profile referenced by `ConfigurationId`. Intended for display in dashboards and logs.

### IsEnabled
`bool IsEnabled`

Indicates whether the notification pipeline is currently active. When `false`, delivery is suspended and health metrics may be stale or reset.

### SuccessRate
`double SuccessRate`

Aggregate delivery success rate (0.0–100.0) computed across all channels over the configured observation window. A value of 100.0 means every attempted notification in the window succeeded.

### AvgDeliveryTimeMs
`long AvgDeliveryTimeMs`

Average end‑to‑end delivery latency in milliseconds, measured from dispatch to acknowledgement, across all channels for the observation window.

### LastSuccessAt
`DateTime? LastSuccessAt`

UTC timestamp of the most recent successful delivery across any channel. `null` if no successful delivery has ever been recorded.

### LastFailureAt
`DateTime? LastFailureAt`

UTC timestamp of the most recent delivery failure across any channel. `null` if no failure has ever been recorded.

### ConsecutiveFailures
`int ConsecutiveFailures`

Number of consecutive failed delivery attempts since the last success, counted globally across all channels. Resets to zero on any successful delivery.

### ErrorMessage
`string? ErrorMessage`

The most recent error detail captured from any channel. `null` when no errors have occurred. This is a single message; for a complete list see `Errors`.

### TotalNotifications
`int TotalNotifications`

Cumulative count of all notifications processed (both successful and failed) since the pipeline was last started or reset.

## Usage

### Example 1: Monitoring overall system health in a background service

```csharp
public async Task MonitorHealthLoop(IHealthCheckService healthService, CancellationToken ct)
{
    while (!ct.IsCancellationRequested)
    {
        var snapshot = await healthService.GetSnapshotAsync(ct);

        if (snapshot.SystemStatus == HealthStatus.Unhealthy)
        {
            _logger.LogCritical(
                "System unhealthy ({Percent:F1}%). Failing channels: {Failing}. Errors: {Errors}",
                snapshot.HealthPercentage,
                snapshot.FailingChannels,
                string.Join("; ", snapshot.Errors));
        }
        else if (snapshot.PendingNotifications > _warningThreshold)
        {
            _logger.LogWarning(
                "Backlog building: {Pending} pending notifications across {Total} channels",
                snapshot.PendingNotifications,
                snapshot.ChannelStatuses.Count);
        }

        await Task.Delay(_pollInterval, ct);
    }
}
```

### Example 2: Inspecting per‑channel details for alert routing

```csharp
public IEnumerable<Alert> GenerateChannelAlerts(IHealthCheckService healthService)
{
    var snapshot = await healthService.GetSnapshotAsync();

    foreach (var channel in snapshot.ChannelStatuses)
    {
        if (channel.ConsecutiveFailures >= 3)
        {
            yield return new Alert
            {
                Severity = AlertSeverity.High,
                Channel = channel.Channel,
                Message = $"Channel has {channel.ConsecutiveFailures} consecutive failures. " +
                          $"Last error: {channel.ErrorMessage ?? "unknown"}. " +
                          $"Last failure at {channel.LastFailureAt:O}."
            };
        }
        else if (channel.AvgDeliveryTimeMs > 5000)
        {
            yield return new Alert
            {
                Severity = AlertSeverity.Medium,
                Channel = channel.Channel,
                Message = $"High latency: {channel.AvgDeliveryTimeMs} ms average."
            };
        }
    }
}
```

## Notes

- All properties reflect a point‑in‑time snapshot. Values do not stream or update automatically; consumers must request a fresh snapshot to observe changes.
- `Status` and `SystemStatus` are always consistent with each other. Prefer `SystemStatus` for programmatic comparisons and `Status` for display.
- `HealthPercentage` and `SuccessRate` are independent metrics. `HealthPercentage` incorporates latency and failure thresholds, while `SuccessRate` is strictly delivery outcome based.
- `Errors` may contain duplicate entries when multiple channels encounter the same underlying fault (e.g. a network partition affecting several transports). Deduplication is left to the consumer.
- `Channel` is meaningful only in single‑channel or “primary unhealthy channel” contexts. In multi‑channel deployments, rely on `ChannelStatuses` for accurate per‑channel data.
- Thread safety: The interface itself does not guarantee thread safety for its properties. Implementations are expected to return immutable snapshots or synchronized copies. Consumers may safely read properties from a single obtained snapshot concurrently, but obtaining a new snapshot while another thread is mutating internal state should be done through the implementation’s documented synchronization mechanism (typically `GetSnapshotAsync` handles this).
