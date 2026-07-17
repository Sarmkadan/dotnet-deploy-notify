#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

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
}
