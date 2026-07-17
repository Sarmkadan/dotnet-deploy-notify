#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetDeployNotify.DependencyInjection;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="ServiceConfigurationBuilder"/>.
/// </summary>
public static class ServiceCollectionExtensionsJsonExtensions
{
    /// <summary>
    /// JSON serialization options with camelCase naming policy.
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the <see cref="ServiceConfigurationBuilder"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="ServiceConfigurationBuilder"/> instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the <see cref="ServiceConfigurationBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this ServiceConfigurationBuilder value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        return JsonSerializer.Serialize(value, CreateJsonSerializerOptions(indented));
    }

    /// <summary>
    /// Creates a new <see cref="JsonSerializerOptions"/> instance based on the provided parameters.
    /// </summary>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A configured <see cref="JsonSerializerOptions"/> instance.</returns>
    private static JsonSerializerOptions CreateJsonSerializerOptions(bool indented) =>
        new(_jsonOptions)
        {
            WriteIndented = indented
        };

    /// <summary>
    /// Deserializes a JSON string to a <see cref="ServiceConfigurationBuilder"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="ServiceConfigurationBuilder"/> instance populated from the JSON string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static ServiceConfigurationBuilder? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return JsonSerializer.Deserialize<ServiceConfigurationBuilder>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="ServiceConfigurationBuilder"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized <see cref="ServiceConfigurationBuilder"/> instance, or <see langword="null"/> if deserialization fails.</param>
    /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out ServiceConfigurationBuilder? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<ServiceConfigurationBuilder>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
