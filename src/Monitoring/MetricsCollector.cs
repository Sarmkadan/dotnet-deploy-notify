#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Monitoring;

/// <summary>
/// Collects and aggregates application metrics
/// </summary>
public class MetricsCollector
{
    private readonly object _lock = new();
    private readonly Dictionary<string, MetricValue> _metrics = new();
    private readonly ILogger<MetricsCollector> _logger;

    public MetricsCollector(ILogger<MetricsCollector> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Records a numeric metric
    /// </summary>
    public void RecordMetric(string name, double value)
    {
        lock (_lock)
        {
            if (_metrics.TryGetValue(name, out var existing))
            {
                existing.Values.Add(value);
                existing.LastUpdated = DateTime.UtcNow;
            }
            else
            {
                _metrics[name] = new MetricValue
                {
                    Name = name,
                    Values = new List<double> { value },
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };
            }

            _logger.LogDebug("Recorded metric: {Name} = {Value}", name, value);
        }
    }

    /// <summary>
    /// Increments a counter metric
    /// </summary>
    public void IncrementCounter(string name, double amount = 1)
    {
        lock (_lock)
        {
            if (_metrics.TryGetValue(name, out var existing))
            {
                existing.Count += amount;
                existing.LastUpdated = DateTime.UtcNow;
            }
            else
            {
                _metrics[name] = new MetricValue
                {
                    Name = name,
                    Count = amount,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };
            }
        }
    }

    /// <summary>
    /// Gets a specific metric by name
    /// </summary>
    public MetricValue? GetMetric(string name)
    {
        lock (_lock)
        {
            return _metrics.TryGetValue(name, out var value) ? value : null;
        }
    }

    /// <summary>
    /// Gets all collected metrics
    /// </summary>
    public List<MetricValue> GetAllMetrics()
    {
        lock (_lock)
        {
            return _metrics.Values.ToList();
        }
    }

    /// <summary>
    /// Gets aggregated statistics for a metric
    /// </summary>
    public MetricStatistics? GetStatistics(string name)
    {
        var metric = GetMetric(name);
        if (metric is null || metric.Values.Count == 0)
            return null;

        var values = metric.Values.OrderBy(v => v).ToList();

        return new MetricStatistics
        {
            Name = name,
            Count = values.Count,
            Sum = values.Sum(),
            Average = values.Average(),
            Min = values.First(),
            Max = values.Last(),
            Median = values.Count % 2 == 0
                ? (values[values.Count / 2 - 1] + values[values.Count / 2]) / 2
                : values[values.Count / 2],
            Percentile95 = values[(int)(values.Count * 0.95)],
            Percentile99 = values[(int)(values.Count * 0.99)]
        };
    }

    /// <summary>
    /// Clears all metrics
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _metrics.Clear();
            _logger.LogInformation("All metrics cleared");
        }
    }

    /// <summary>
    /// Resets a specific metric
    /// </summary>
    public void ResetMetric(string name)
    {
        lock (_lock)
        {
            if (_metrics.Remove(name))
            {
                _logger.LogDebug("Reset metric: {Name}", name);
            }
        }
    }
}

/// <summary>
/// Represents a single metric value
/// </summary>
public class MetricValue
{
    public string Name { get; set; } = string.Empty;
    public List<double> Values { get; set; } = new();
    public double Count { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdated { get; set; }

    public double Average => Values.Any() ? Values.Average() : 0;
    public double Sum => Values.Sum();
    public int RecordCount => Values.Count;
}

/// <summary>
/// Statistical summary of a metric
/// </summary>
public class MetricStatistics
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Sum { get; set; }
    public double Average { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public double Median { get; set; }
    public double Percentile95 { get; set; }
    public double Percentile99 { get; set; }

    public override string ToString()
    {
        return $"{Name}: Count={Count}, Avg={Average:F2}, Min={Min:F2}, Max={Max:F2}, P95={Percentile95:F2}";
    }
}

/// <summary>
/// Performance monitor for tracking operation metrics
/// </summary>
public class PerformanceMonitor
{
    private readonly MetricsCollector _collector;
    private readonly ILogger<PerformanceMonitor> _logger;

    public PerformanceMonitor(MetricsCollector collector, ILogger<PerformanceMonitor> logger)
    {
        _collector = collector;
        _logger = logger;
    }

    /// <summary>
    /// Measures the execution time of an operation
    /// </summary>
    public async Task<T> MeasureAsync<T>(string operationName, Func<Task<T>> operation)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            var result = await operation().ConfigureAwait(false);
            var duration = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

            _collector.RecordMetric($"{operationName}_duration_ms", duration);
            _collector.IncrementCounter($"{operationName}_success", 1);

            _logger.LogDebug("Operation {Operation} completed in {Duration}ms",
                operationName, duration);

            return result;
        }
        catch (Exception ex)
        {
            _collector.IncrementCounter($"{operationName}_error", 1);
            _logger.LogError(ex, "Operation {Operation} failed", operationName);
            throw;
        }
    }

    /// <summary>
    /// Measures the execution time of a synchronous operation
    /// </summary>
    public T Measure<T>(string operationName, Func<T> operation)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            var result = operation();
            var duration = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

            _collector.RecordMetric($"{operationName}_duration_ms", duration);
            _collector.IncrementCounter($"{operationName}_success", 1);

            return result;
        }
        catch (Exception ex)
        {
            _collector.IncrementCounter($"{operationName}_error", 1);
            throw;
        }
    }

    /// <summary>
    /// Gets performance report for an operation
    /// </summary>
    public PerformanceReport? GetReport(string operationName)
    {
        var durationMetric = _collector.GetMetric($"{operationName}_duration_ms");
        var successCount = _collector.GetMetric($"{operationName}_success")?.Count ?? 0;
        var errorCount = _collector.GetMetric($"{operationName}_error")?.Count ?? 0;

        if (durationMetric is null)
            return null;

        var stats = _collector.GetStatistics($"{operationName}_duration_ms");

        return new PerformanceReport
        {
            OperationName = operationName,
            TotalExecutions = durationMetric.RecordCount,
            SuccessfulExecutions = (int)successCount,
            FailedExecutions = (int)errorCount,
            DurationStatistics = stats
        };
    }
}

/// <summary>
/// Performance report for an operation
/// </summary>
public class PerformanceReport
{
    public string OperationName { get; set; } = string.Empty;
    public int TotalExecutions { get; set; }
    public int SuccessfulExecutions { get; set; }
    public int FailedExecutions { get; set; }
    public MetricStatistics? DurationStatistics { get; set; }

    public double SuccessRate => TotalExecutions > 0
        ? (double)SuccessfulExecutions / TotalExecutions * 100
        : 0;

    public override string ToString()
    {
        return $"{OperationName}: {TotalExecutions} executions, {SuccessRate:F1}% success rate, " +
            $"Avg: {DurationStatistics?.Average:F2}ms";
    }
}
