#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using DotNetDeployNotify.Core.Models;

namespace DotNetDeployNotify.Configuration;

/// <summary>
/// Provides JSON serialization extensions for <see cref="ChannelConfigurationBuilder"/>.
/// </summary>
public static class ChannelConfigurationBuilderJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the <see cref="ChannelConfigurationBuilder"/> to a JSON string.
    /// </summary>
    /// <param name="value">The builder to serialize.</param>
    /// <param name="indented">Whether to indent the JSON for readability.</param>
    /// <returns>A JSON string representation of the built configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this ChannelConfigurationBuilder value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var config = value.Build();

        return JsonSerializer.Serialize(config, indented ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } : _jsonOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="ChannelConfigurationBuilder"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized builder, or null if the JSON is null or empty.
    /// Note: Due to internal implementation constraints of <see cref="ChannelConfigurationBuilder"/>,
    /// deserialization from JSON is not supported. This method always returns null.</returns>
    /// <exception cref="NotSupportedException">Always thrown to indicate deserialization is not supported.</exception>
    public static ChannelConfigurationBuilder? FromJson(string json)
    {
        throw new NotSupportedException(
            "Deserialization from JSON to ChannelConfigurationBuilder is not supported due to internal implementation constraints. " +
            "Use ToJson() for serialization only.");
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="ChannelConfigurationBuilder"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized builder if successful.</param>
    /// <returns>Always false since deserialization is not supported.</returns>
    public static bool TryFromJson(string json, out ChannelConfigurationBuilder? value)
    {
        value = null;
        return false;
    }
}