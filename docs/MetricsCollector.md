# MetricsCollector

`MetricsCollector` is a utility for gathering, storing, and analyzing numeric metric data over time. It maintains a named collection of raw values and provides both individual metric lookup and aggregate statistical summaries, making it suitable for performance monitoring, usage tracking, or any scenario where point-in-time measurements need to be recorded and later examined.

## API

### Constructors

**`public MetricsCollector(string name)`**

Creates a new collector with the given logical name. The name is immutable for the lifetime of the instance. Initializes an empty value list, sets `CreatedAt` to the current UTC time, and sets `LastUpdated` to the same timestamp.

### Methods

**`public void RecordMetric(double value)`**

Appends a numeric measurement to the collector’s value list. Updates `LastUpdated` to the current UTC time and increments `Count`. This is the primary ingestion point for raw data.

**`public void IncrementCounter()`**

A convenience method that records a value of `1.0`. Equivalent to calling `RecordMetric(1.0)`. Useful for simple occurrence counting without supplying an explicit magnitude.

**`public MetricValue? GetMetric(string name)`**

Retrieves a single `MetricValue` by its name. Returns `null` if no metric with that name exists in the underlying store. The exact storage and lookup mechanism is internal; the method signature implies a dictionary-like association between names and `MetricValue` instances.

**`public List<MetricValue> GetAllMetrics()`**

Returns a flat list of all `MetricValue` objects currently held by the collector. The order is unspecified. Returns an empty list if no metrics have been recorded.

**`public MetricStatistics? GetStatistics(string name)`**

Computes and returns aggregate statistics for the metric identified by `name`. Returns `null` if the named metric does not exist or has no recorded values. The returned `MetricStatistics` object contains `Count`, `Sum`, `Average`, `Min`, `Max`, and `Median` for the metric’s value series.

**`public void Clear()`**

Removes all recorded metrics and resets the collector to an empty state. `Count` returns to zero, the value list is emptied, and `LastUpdated` is set to the current UTC time. `CreatedAt` is preserved.

**`public void ResetMetric(string name)`**

Removes a single named metric and all its associated values from the collector. If the metric does not exist, the call has no effect. `LastUpdated` is updated only if a metric was actually removed.

### Properties

**`public string Name`** (MetricsCollector)

The logical name assigned at construction. Read-only.

**`public List<double> Values`**

Exposes the raw list of recorded numeric values. This is the same list populated by `RecordMetric` and `IncrementCounter`. External modifications to the returned list reference will affect the collector’s internal state.

**`public double Count`**

The total number of values recorded via `RecordMetric` and `IncrementCounter`. This is a running tally, not a computed property derived from `Values.Count`.

**`public DateTime CreatedAt`**

The UTC timestamp when the collector was instantiated. Never modified after construction.

**`public DateTime LastUpdated`**

The UTC timestamp of the most recent mutation — either a `RecordMetric`/`IncrementCounter` call, a `Clear`, or a successful `ResetMetric`.

### MetricValue Properties

**`public string Name`** (MetricValue)

The identifier for this individual metric.

**`public int Count`** (MetricValue)

The number of values recorded for this metric.

**`public double Sum`** (MetricValue)

The arithmetic sum of all values recorded for this metric.

**`public double Average`** (MetricValue)

The arithmetic mean. Behavior when `Count` is zero is implementation-defined; consumers should guard against division by zero.

**`public double Min`** (MetricValue)

The smallest value recorded. Behavior when no values exist is implementation-defined.

**`public double Max`** (MetricValue)

The largest value recorded. Behavior when no values exist is implementation-defined.

**`public double Median`** (MetricValue)

The median of the recorded values. For an even number of elements, the convention used (lower-middle, upper-middle, or interpolated) is implementation-specific.

## Usage

### Example 1: Recording and Retrieving Metrics

```csharp
var collector = new MetricsCollector("RequestLatency");

// Record several latency observations
collector.RecordMetric(12.5);
collector.RecordMetric(8.3);
collector.RecordMetric(15.1);
collector.IncrementCounter(); // records 1.0

// Retrieve raw values
List<double> raw = collector.Values; // [12.5, 8.3, 15.1, 1.0]
double totalCount = collector.Count; // 4

// Get statistics for a named metric
MetricStatistics? stats = collector.GetStatistics("RequestLatency");
if (stats != null)
{
    Console.WriteLine($"Avg: {stats.Average}, Median: {stats.Median}");
}
```

### Example 2: Managing Multiple Named Metrics

```csharp
var collector = new MetricsCollector("ServiceMetrics");

// Simulate recording under different metric names
collector.RecordMetric("cpu", 45.2);
collector.RecordMetric("cpu", 52.8);
collector.RecordMetric("memory", 1024.0);

MetricValue? cpuMetric = collector.GetMetric("cpu");
if (cpuMetric != null)
{
    Console.WriteLine($"CPU count: {cpuMetric.Count}, avg: {cpuMetric.Average}");
}

// Reset a single metric
collector.ResetMetric("cpu");

// Clear everything
collector.Clear();
```

## Notes

- **Null returns**: `GetMetric` and `GetStatistics` return `null` when the requested name is absent. Callers must null-check before accessing members of `MetricValue` or `MetricStatistics`.
- **Empty-state statistics**: When a metric exists but has zero recorded values, the behavior of `Average`, `Min`, `Max`, and `Median` is not defined by the signatures alone. Defensive code should verify `Count > 0` before consuming these properties.
- **Median convention**: The median calculation for even-length series is unspecified. Consumers needing a guaranteed interpolation method should compute it externally or consult the implementation source.
- **List exposure**: The `Values` property returns a direct reference to the internal list. External mutation of that list will alter the collector’s state and may cause `Count` to drift from `Values.Count`. Avoid modifying the returned list unless that effect is intentional.
- **Thread safety**: No synchronization primitives are evident from the public API. Concurrent calls to `RecordMetric`, `IncrementCounter`, `Clear`, or `ResetMetric` from multiple threads may cause race conditions, torn state, or corrupted aggregate data. External locking is required for multi-threaded use.
- **Timestamp granularity**: `CreatedAt` and `LastUpdated` use `DateTime` (likely `UtcNow`). Precision is limited to the system clock resolution; high-frequency recording may result in identical timestamps across successive calls.
