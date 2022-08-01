#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetDeployNotify.Monitoring;

/// <summary>
/// Provides System.Text.Json serialization extensions for MetricsCollector and related types
/// </summary>
public static class MetricsCollectorJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Converts a MetricsCollector to a JSON string
    /// </summary>
    /// <param name="value">The metrics collector to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON representation of the metrics collector</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this MetricsCollector value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Parses a JSON string into a MetricsCollector instance
    /// </summary>
    /// <param name="json">The JSON string to parse</param>
    /// <returns>The deserialized MetricsCollector, or null if parsing fails</returns>
    /// <exception cref="ArgumentException">Thrown when json is null or empty</exception>
    public static MetricsCollector? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<MetricsCollector>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to parse a JSON string into a MetricsCollector instance
    /// </summary>
    /// <param name="json">The JSON string to parse</param>
    /// <param name="value">Receives the deserialized MetricsCollector if successful</param>
    /// <returns>True if parsing succeeds; otherwise, false</returns>
    /// <exception cref="ArgumentException">Thrown when json is null or empty</exception>
    public static bool TryFromJson(string json, out MetricsCollector? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<MetricsCollector>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Converts a MetricValue to a JSON string
    /// </summary>
    /// <param name="value">The metric value to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON representation of the metric value</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this MetricValue value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Parses a JSON string into a MetricValue instance
    /// </summary>
    /// <param name="json">The JSON string to parse</param>
    /// <returns>The deserialized MetricValue, or null if parsing fails</returns>
    /// <exception cref="ArgumentException">Thrown when json is null or empty</exception>
    public static MetricValue? FromJsonToMetricValue(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<MetricValue>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to parse a JSON string into a MetricValue instance
    /// </summary>
    /// <param name="json">The JSON string to parse</param>
    /// <param name="value">Receives the deserialized MetricValue if successful</param>
    /// <returns>True if parsing succeeds; otherwise, false</returns>
    /// <exception cref="ArgumentException">Thrown when json is null or empty</exception>
    public static bool TryFromJsonToMetricValue(string json, out MetricValue? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<MetricValue>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Converts a MetricStatistics to a JSON string
    /// </summary>
    /// <param name="value">The metric statistics to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON representation of the metric statistics</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this MetricStatistics value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Parses a JSON string into a MetricStatistics instance
    /// </summary>
    /// <param name="json">The JSON string to parse</param>
    /// <returns>The deserialized MetricStatistics, or null if parsing fails</returns>
    /// <exception cref="ArgumentException">Thrown when json is null or empty</exception>
    public static MetricStatistics? FromJsonToMetricStatistics(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<MetricStatistics>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to parse a JSON string into a MetricStatistics instance
    /// </summary>
    /// <param name="json">The JSON string to parse</param>
    /// <param name="value">Receives the deserialized MetricStatistics if successful</param>
    /// <returns>True if parsing succeeds; otherwise, false</returns>
    /// <exception cref="ArgumentException">Thrown when json is null or empty</exception>
    public static bool TryFromJsonToMetricStatistics(string json, out MetricStatistics? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<MetricStatistics>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Converts a PerformanceMonitor to a JSON string
    /// </summary>
    /// <param name="value">The performance monitor to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON representation of the performance monitor</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this PerformanceMonitor value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Converts a PerformanceReport to a JSON string
    /// </summary>
    /// <param name="value">The performance report to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON representation of the performance report</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this PerformanceReport value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }
}