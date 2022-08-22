# MetricsServiceTests

`MetricsServiceTests` is the unit test suite for the `MetricsService` class in the `dotnet-deploy-notify` project. It validates the correctness of metrics recording, retrieval, and aggregation logic, covering notification counts, delivery attempts (success, failure, duration, per-channel tracking), validation failures, configuration changes, and snapshot-based reporting across different time periods and channels.

## API

### Constructors

#### `public MetricsServiceTests`
Default parameterless constructor. Initializes a new instance of the test class. The test framework creates an instance per test method unless configured otherwise.

---

### Test Methods

#### `public void RecordNotificationCreated_IncrementsNotificationCount`
Verifies that calling `RecordNotificationCreated` once increments the total notification count by exactly one. Asserts that the count moves from zero to one.

- **Parameters:** none (test method).
- **Returns:** void.
- **Throws:** assertion failures if the count is not incremented correctly.

---

#### `public void RecordNotificationCreated_WithMultipleCalls_CountsAccurately`
Verifies that multiple invocations of `RecordNotificationCreated` produce an accurate cumulative count equal to the number of calls.

- **Parameters:** none.
- **Returns:** void.
- **Throws:** assertion failures if the cumulative count does not match the expected total.

---

#### `public void RecordDeliveryAttempt_WithSuccessfulDelivery_IncrementsSuccessCount`
Confirms that recording a delivery attempt with a successful outcome increments the success counter by one and does not affect the failure counter.

- **Parameters:** none.
- **Returns:** void.
- **Throws:** assertion failures if the success count is not incremented or the failure count changes.

---

#### `public void RecordDeliveryAttempt_WithFailedDelivery_IncrementsFailureCount`
Confirms that recording a delivery attempt with a failed outcome increments the failure counter by one and does not affect the success counter.

- **Parameters:** none.
- **Returns:** void.
- **Throws:** assertion failures if the failure count is not incremented or the success count changes.

---

#### `public void RecordDeliveryAttempt_WithMixedResults_CountsBothSuccessAndFailure`
Records a sequence of both successful and failed delivery attempts and asserts that the success and failure counters reflect the exact number of each outcome independently.

- **Parameters:** none.
- **Returns:** void.
- **Throws:** assertion failures if either counter deviates from the expected value.

---

#### `public void RecordDeliveryAttempt_TracksDeliveryDuration`
Validates that the duration associated with a delivery attempt is captured and retrievable. Typically asserts that the recorded duration falls within an expected range or matches a supplied value.

- **Parameters:** none.
- **Returns:** void.
- **Throws:** assertion failures if the duration is missing, zero when it should not be, or outside acceptable bounds.

---

#### `public void RecordDeliveryAttempt_WithDifferentChannels_TracksPerChannel`
Records delivery attempts against distinct channels (e.g., Slack, email) and verifies that metrics are segregated per channel, with each channel’s counters independent of others.

- **Parameters:** none.
- **Returns:** void.
- **Throws:** assertion failures if channel-specific metrics are missing, aggregated incorrectly, or cross-contaminated.

---

#### `public void RecordValidationFailure_IncrementsValidationFailureCount`
Ensures that a single call to `RecordValidationFailure` increases the validation failure counter by one.

- **Parameters:** none.
- **Returns:** void.
- **Throws:** assertion failures if the counter does not increment as expected.

---

#### `public void RecordValidationFailure_WithMultipleCalls_CountsAccurately`
Ensures that multiple calls to `RecordValidationFailure` result in a cumulative count equal to the number of invocations.

- **Parameters:** none.
- **Returns:** void.
- **Throws:** assertion failures if the cumulative count is incorrect.

---

#### `public void RecordConfigurationChange_IncrementsConfigurationChangeCount`
Verifies that calling `RecordConfigurationChange` increments the configuration change counter by one.

- **Parameters:** none.
- **Returns:** void.
- **Throws:** assertion failures if the counter does not increment.

---

#### `public async Task GetMetricsAsync_WithNoActivity_ReturnsZeroMetrics`
Invokes `GetMetricsAsync` on a fresh `MetricsService` instance with no prior recorded activity and asserts that all metric values in the returned snapshot are zero.

- **Parameters:** none.
- **Returns:** `Task` (async test).
- **Throws:** assertion failures if any metric value is non-zero.

---

#### `public async Task GetMetricsAsync_ReturnsCurrentSnapshot`
Records a known set of activities, calls `GetMetricsAsync`, and asserts that the returned snapshot contains the exact counts and values matching the recorded activity at that moment.

- **Parameters:** none.
- **Returns:** `Task`.
- **Throws:** assertion failures if the snapshot does not reflect the current state.

---

#### `public async Task GetMetricsAsync_HasTimestamp`
Verifies that the snapshot returned by `GetMetricsAsync` includes a timestamp and that the timestamp is set to a recent time (typically close to `DateTime.UtcNow`).

- **Parameters:** none.
- **Returns:** `Task`.
- **Throws:** assertion failures if the timestamp is missing, default, or significantly divergent from the expected time.

---

#### `public async Task GetMetricsByPeriodAsync_WithPeriodIncludingActivity_ReturnsActivity`
Records activity within a defined time window, then queries `GetMetricsByPeriodAsync` with a period that fully contains that window, and asserts that the activity is present in the result.

- **Parameters:** none.
- **Returns:** `Task`.
- **Throws:** assertion failures if the activity is missing from the period result.

---

#### `public async Task GetMetricsByPeriodAsync_WithPeriodExcludingActivity_ReturnsZero`
Records activity, then queries `GetMetricsByPeriodAsync` with a period that starts after all activity occurred, and asserts that all returned metric values are zero.

- **Parameters:** none.
- **Returns:** `Task`.
- **Throws:** assertion failures if any metric value is non-zero for the excluded period.

---

#### `public async Task GetChannelMetricsAsync_WithSlackActivity_ReturnsSlackMetrics`
Records delivery activity specifically for the Slack channel, calls `GetChannelMetricsAsync` for Slack, and asserts that the returned metrics contain the expected Slack-specific counts.

- **Parameters:** none.
- **Returns:** `Task`.
- **Throws:** assertion failures if Slack metrics are missing or incorrect.

---

#### `public async Task GetChannelMetricsAsync_WithNoActivity_ReturnsEmptyMetrics`
Calls `GetChannelMetricsAsync` for a channel that has no recorded activity and asserts that the returned metrics object is empty or contains zero values.

- **Parameters:** none.
- **Returns:** `Task`.
- **Throws:** assertion failures if any metric value is non-zero.

---

#### `public async Task GetChannelMetricsAsync_WithMixedSuccessAndFailure_CalculatesSuccessRate`
Records a mix of successful and failed deliveries for a channel, retrieves channel metrics, and asserts that the success rate is calculated correctly as the ratio of successes to total attempts.

- **Parameters:** none.
- **Returns:** `Task`.
- **Throws:** assertion failures if the success rate is incorrect.

---

#### `public void MetricsSnapshot_GetSuccessRate_WithZeroAttempts_ReturnsZero`
Directly tests the `GetSuccessRate` method (or equivalent property) on a `MetricsSnapshot` object when the total number of delivery attempts is zero, asserting that the success rate is zero rather than throwing a divide-by-zero exception or returning `NaN`.

- **Parameters:** none.
- **Returns:** void.
- **Throws:** assertion failures if the result is not zero, or if an exception is thrown.

---

## Usage

### Example 1: Recording and Retrieving Basic Metrics

```csharp
[Test]
public async Task RecordAndRetrieve_BasicFlow()
{
    var service = new MetricsService();

    // Record some activity
    service.RecordNotificationCreated();
    service.RecordNotificationCreated();
    service.RecordDeliveryAttempt("slack", success: true, durationMs: 120);
    service.RecordDeliveryAttempt("slack", success: false, durationMs: 90);
    service.RecordValidationFailure();

    // Retrieve current snapshot
    var snapshot = await service.GetMetricsAsync();

    Assert.That(snapshot.TotalNotifications, Is.EqualTo(2));
    Assert.That(snapshot.TotalDeliveries, Is.EqualTo(2));
    Assert.That(snapshot.SuccessfulDeliveries, Is.EqualTo(1));
    Assert.That(snapshot.FailedDeliveries, Is.EqualTo(1));
    Assert.That(snapshot.ValidationFailures, Is.EqualTo(1));
    Assert.That(snapshot.Timestamp, Is.Not.EqualTo(default(DateTime)));
}
```

### Example 2: Per-Channel Metrics with Success Rate

```csharp
[Test]
public async Task ChannelMetrics_WithSuccessRate()
{
    var service = new MetricsService();

    // Mixed results for Slack
    service.RecordDeliveryAttempt("slack", success: true, durationMs: 200);
    service.RecordDeliveryAttempt("slack", success: true, durationMs: 150);
    service.RecordDeliveryAttempt("slack", success: false, durationMs: 300);

    // Activity for email
    service.RecordDeliveryAttempt("email", success: true, durationMs: 100);

    var slackMetrics = await service.GetChannelMetricsAsync("slack");

    Assert.That(slackMetrics.TotalAttempts, Is.EqualTo(3));
    Assert.That(slackMetrics.Successful, Is.EqualTo(2));
    Assert.That(slackMetrics.Failed, Is.EqualTo(1));
    Assert.That(slackMetrics.SuccessRate, Is.EqualTo(2.0 / 3.0).Within(0.001));

    var emailMetrics = await service.GetChannelMetricsAsync("email");
    Assert.That(emailMetrics.TotalAttempts, Is.EqualTo(1));
    Assert.That(emailMetrics.SuccessRate, Is.EqualTo(1.0));
}
```

## Notes

- **Zero-attempt success rate:** `MetricsSnapshot_GetSuccessRate_WithZeroAttempts_ReturnsZero` explicitly guards against division-by-zero scenarios. Consumers can safely call the success rate property on an empty snapshot without handling exceptions or `NaN`.
- **Timestamp precision:** `GetMetricsAsync_HasTimestamp` ensures the snapshot timestamp is set. Tests typically allow a small tolerance (e.g., a few seconds) between the recorded timestamp and `DateTime.UtcNow` to account for test execution overhead.
- **Period boundary semantics:** `GetMetricsByPeriodAsync` tests assume that the period’s start and end are inclusive/exclusive as defined by the implementation. Activity recorded exactly at a boundary may be included or excluded depending on the service contract; the tests validate the documented behavior.
- **Channel isolation:** `RecordDeliveryAttempt_WithDifferentChannels_TracksPerChannel` confirms that metrics are strictly partitioned by channel identifier. Channel names are treated as case-sensitive or case-insensitive according to the service’s design; the test suite enforces that contract.
- **Thread safety:** The test methods are synchronous or async tasks run sequentially by the test runner. The suite does not include explicit concurrency stress tests based on the listed members, so thread-safety guarantees of the underlying `MetricsService` are not directly validated here. If `MetricsService` is intended for concurrent use, additional tests would be required.
- **Async consistency:** All retrieval methods (`GetMetricsAsync`, `GetMetricsByPeriodAsync`, `GetChannelMetricsAsync`) are awaited in tests. The tests assume these methods complete synchronously in the test environment (no actual I/O or delays), but the async signatures are respected to match the real service contract.
