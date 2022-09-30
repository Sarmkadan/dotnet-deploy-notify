#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetDeployNotify.Utilities;

/// <summary>
/// Provides System.Text.Json serialization helpers for <see cref="DateTimeExtensions"/> metadata
/// </summary>
public static class DateTimeExtensionsJsonExtensions
{
    /// <summary>
    /// JsonSerializerOptions with camelCase naming policy for consistent serialization
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.General)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Serializes DateTimeExtensions type information to JSON string
    /// </summary>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>JSON string representation of DateTimeExtensions metadata</returns>
    public static string ToJson(bool indented = false)
    {
        var options = new JsonSerializerOptions(_jsonOptions)
        {
            WriteIndented = indented
        };

        var metadata = new DateTimeExtensionsMetadata
        {
            Type = nameof(DateTimeExtensions),
            Namespace = typeof(DateTimeExtensions).Namespace ?? "DotNetDeployNotify.Utilities",
            Assembly = typeof(DateTimeExtensions).Assembly.GetName().Name ?? "DotNetDeployNotify",
            Methods = [
                "ToRelativeTimeString",
                "ToIsoString",
                "ToFormattedString",
                "IsPast",
                "IsFuture",
                "GetMinutesElapsed",
                "GetSecondsElapsed",
                "RoundToNearestMinute",
                "RoundToNearestHour",
                "GetStartOfDay",
                "GetEndOfDay",
                "GetStartOfWeek",
                "GetStartOfMonth",
                "GetEndOfMonth",
                "IsToday",
                "IsYesterday",
                "GetBusinessDaysBetween",
                "FromUnixTimestamp",
                "ToUnixTimestamp"
            ]
        };

        return JsonSerializer.Serialize(metadata, options);
    }

    /// <summary>
    /// Deserializes JSON string to DateTimeExtensions metadata
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>DateTimeExtensions metadata or null if deserialization fails</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null</exception>
    public static DateTimeExtensionsMetadata? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            return JsonSerializer.Deserialize<DateTimeExtensionsMetadata>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize JSON string to DateTimeExtensions metadata
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="value">Output value, null if deserialization fails</param>
    /// <returns>True if deserialization succeeds, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null</exception>
    public static bool TryFromJson(string json, out DateTimeExtensionsMetadata? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<DateTimeExtensionsMetadata>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Metadata representation of the DateTimeExtensions class for JSON serialization
    /// </summary>
    public sealed class DateTimeExtensionsMetadata
    {
        /// <summary>
        /// Gets or sets the type identifier
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the namespace
        /// </summary>
        public string? Namespace { get; set; }

        /// <summary>
        /// Gets or sets the assembly name
        /// </summary>
        public string? Assembly { get; set; }

        /// <summary>
        /// Gets or sets the array of method names
        /// </summary>
        public string[]? Methods { get; set; }
    }
}