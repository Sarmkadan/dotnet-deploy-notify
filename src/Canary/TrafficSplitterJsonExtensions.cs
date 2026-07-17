#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using DotNetDeployNotify.Core.Models;

namespace DotNetDeployNotify.Canary;

/// <summary>
/// Provides System.Text.Json serialization helpers for <see cref="TrafficSplit"/> to enable
/// easy serialization and deserialization of traffic split configurations.
/// </summary>
public static class TrafficSplitterJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes a <see cref="TrafficSplit"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The traffic split to serialize. Must not be <see langword="null"/>.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the traffic split.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this TrafficSplit value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="TrafficSplit"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Must not be <see langword="null"/>.</param>
    /// <returns>A deserialized <see cref="TrafficSplit"/> instance, or <see langword="null"/> if the JSON is invalid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is malformed or cannot be deserialized to <see cref="TrafficSplit"/>.</exception>
    public static TrafficSplit? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return JsonSerializer.Deserialize<TrafficSplit>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="TrafficSplit"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Must not be <see langword="null"/>.</param>
    /// <param name="value">Receives the deserialized traffic split if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out TrafficSplit? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<TrafficSplit>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = default;
            return false;
        }
    }
}