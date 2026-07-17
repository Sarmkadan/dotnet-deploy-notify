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
        /// <param name="indented">Whether to format the JSON with indentation for better readability</param>
        /// <returns>JSON string representation of DateTimeExtensions metadata with camelCase property names</returns>
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
    /// <remarks>
    /// This class is sealed to prevent inheritance, as it's a simple DTO for JSON serialization.
    /// All properties are nullable to handle missing data during deserialization scenarios.
    /// </remarks>
    public sealed class DateTimeExtensionsMetadata
    {
        private string? _type;
        private string? _namespace;
        private string? _assembly;
        private string[]? _methods;

        /// <summary>
        /// Gets or sets the type identifier
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when value is empty or whitespace</exception>
        public string? Type
        {
            get => _type;
            set => _type = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        /// <summary>
        /// Gets or sets the namespace
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when value is empty or whitespace</exception>
        public string? Namespace
        {
            get => _namespace;
            set => _namespace = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        /// <summary>
        /// Gets or sets the assembly name
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when value is empty or whitespace</exception>
        public string? Assembly
        {
            get => _assembly;
            set => _assembly = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        /// <summary>
        /// Gets or sets the array of method names
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
        public string[]? Methods
        {
            get => _methods;
            set => _methods = value is { Length: 0 } ? null : value;
        }
    }
}