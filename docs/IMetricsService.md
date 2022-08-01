# IMetricsService

A service interface that provides metrics and statistics about notification delivery operations, including success rates, delivery times, and channel-specific performance data.

## API

### `Timestamp`
- **Type:** `DateTime`
- **Purpose:** The point in time when these metrics were recorded.
- **Remarks:** Read-only; represents the moment the metrics snapshot was taken.

### `NotificationsCreated`
- **Type:** `long`
- **Purpose:** Total number of notifications created during the reporting period.
- **Remarks:** Read-only; reflects cumulative count.

### `DeliveryAttempts`
- **Type:** `long`
- **Purpose:** Total number of delivery attempts made across all channels.
- **Remarks:** Read-only; reflects cumulative count.

### `SuccessfulDeliveries`
- **Type:** `long`
- **Purpose:** Total number of successful deliveries across all channels.
- **Remarks:** Read-only; reflects cumulative count.

### `FailedDeliveries`
- **Type:** `long`
- **Purpose:** Total number of failed deliveries across all channels.
- **Remarks:** Read-only; reflects cumulative count.

### `ValidationFailures`
- **Type:** `long`
- **Purpose:** Total number of validation failures encountered during notification processing.
- **Remarks:** Read-only; reflects cumulative count.

### `ConfigurationChanges`
- **Type:** `long`
- **Purpose:** Total number of configuration changes applied during the reporting period.
- **Remarks:** Read-only; reflects cumulative count.

### `AverageDeliveryTimeMs`
- **Type:** `long`
- **Purpose:** Average delivery time in milliseconds across all channels.
- **Remarks:** Read-only; calculated from successful deliveries only.

### `MinDeliveryTimeMs`
- **Type:** `long`
- **Purpose:** Minimum delivery time in milliseconds observed across all channels.
- **Remarks:** Read-only; `0` if no successful deliveries exist.

### `MaxDeliveryTimeMs`
- **Type:** `long`
- **Purpose:** Maximum delivery time in milliseconds observed across all channels.
- **Remarks:** Read-only; `0` if no successful deliveries exist.

### `P95DeliveryTimeMs`
- **Type:** `long`
- **Purpose:** 95th percentile delivery time in milliseconds across all channels.
- **Remarks:** Read-only; `0` if fewer than 20 successful deliveries exist.

### `P99DeliveryTimeMs`
- **Type:** `long`
- **Purpose:** 99th percentile delivery time in milliseconds across all channels.
- **Remarks:** Read-only; `0` if fewer than 100 successful deliveries exist.

### `ChannelMetrics`
- **Type:** `Dictionary<NotificationChannel, ChannelMetrics>`
- **Purpose:** Channel-specific metrics, keyed by notification channel.
- **Remarks:** Read-only; empty if no metrics are available for any channel.

### `GetSuccessRate()`
- **Returns:** `double` – Success rate as a value between `0.0` and `1.0`.
- **Purpose:** Calculates the ratio of successful deliveries to total delivery attempts.
- **Remarks:** Returns `0.0` if no delivery attempts have been made.

### `GetFailureRate()`
- **Returns:** `double` – Failure rate as a value between `0.0` and `1.0`.
- **Purpose:** Calculates the ratio of failed deliveries to total delivery attempts.
- **Remarks:** Returns `0.0` if no delivery attempts have been made.

### `Channel`
- **Type:** `NotificationChannel`
- **Purpose:** The notification channel these metrics pertain to.
- **Remarks:** Read-only; identifies the context of the metrics.

## Usage

### Example 1: Monitoring Notification Delivery Health
