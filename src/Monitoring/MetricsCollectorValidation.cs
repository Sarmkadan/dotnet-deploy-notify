#nullable enable

namespace DotNetDeployNotify.Monitoring;

/// <summary>
/// Provides validation helpers for <see cref="MetricsCollector"/> instances.
/// </summary>
public static class MetricsCollectorValidation
{
    /// <summary>
    /// Validates the specified <see cref="MetricsCollector"/> instance.
    /// </summary>
    /// <param name="value">The metrics collector to validate.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> Validate(this MetricsCollector? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate metrics collection state
        var allMetrics = value.GetAllMetrics();
        if (allMetrics is null)
        {
            problems.Add("MetricsCollector.GetAllMetrics() returned null");
        }
        else
        {
            foreach (var metric in allMetrics)
            {
                if (metric is null)
                {
                    problems.Add("MetricsCollector contains null MetricValue");
                    continue;
                }

                // Validate metric name
                if (string.IsNullOrWhiteSpace(metric.Name))
                {
                    problems.Add($"Metric '{metric.Name}' has null, empty, or whitespace Name");
                }

                // Validate values list
                if (metric.Values is null)
                {
                    problems.Add($"Metric '{metric.Name}' has null Values collection");
                }
                else if (metric.Values.Count == 0)
                {
                    // Empty values list is valid (metric exists but has no values recorded yet)
                }

                // Validate dates
                if (metric.CreatedAt == default)
                {
                    problems.Add($"Metric '{metric.Name}' has default CreatedAt date (0001-01-01)");
                }
                else if (metric.CreatedAt > DateTime.UtcNow.AddMinutes(5))
                {
                    problems.Add($"Metric '{metric.Name}' has CreatedAt date in the future");
                }

                if (metric.LastUpdated == default)
                {
                    problems.Add($"Metric '{metric.Name}' has default LastUpdated date (0001-01-01)");
                }
                else if (metric.LastUpdated > DateTime.UtcNow.AddMinutes(5))
                {
                    problems.Add($"Metric '{metric.Name}' has LastUpdated date in the future");
                }

                // Validate Count
                if (double.IsNaN(metric.Count) || double.IsInfinity(metric.Count))
                {
                    problems.Add($"Metric '{metric.Name}' has invalid Count value");
                }
                else if (metric.Count < 0)
                {
                    problems.Add($"Metric '{metric.Name}' has negative Count ({metric.Count})");
                }
            }
        }

        // Validate internal state consistency
        try
        {
            // Test that GetStatistics works for all metrics
            foreach (var metricName in allMetrics?.Select(m => m.Name) ?? [])
            {
                var stats = value.GetStatistics(metricName);
                if (stats is null && allMetrics?.Any(m => m.Name == metricName) == true)
                {
                    problems.Add($"GetStatistics() returned null for valid metric '{metricName}'");
                }
            }
        }
        catch (Exception ex)
        {
            problems.Add($"MetricsCollector internal error during validation: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="MetricsCollector"/> instance is valid.
    /// </summary>
    /// <param name="value">The metrics collector to check.</param>
    /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
    public static bool IsValid(this MetricsCollector? value)
    {
        return value?.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="MetricsCollector"/> instance is valid.
    /// </summary>
    /// <param name="value">The metrics collector to validate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">The metrics collector is invalid; the exception message contains the validation problems.</exception>
    public static void EnsureValid(this MetricsCollector? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"MetricsCollector is invalid. Problems:\n{string.Join("\n", problems)}");
        }
    }
}