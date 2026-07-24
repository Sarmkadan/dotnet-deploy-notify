#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetDeployNotify.Serialization;

namespace DotNetDeployNotify.Infrastructure;

/// <summary>
/// Provides System.Text.Json serialization extensions for CanaryServiceExtensions metadata.
/// </summary>
public static class CanaryServiceExtensionsJsonExtensions
{
    /// <summary>
    /// JsonSerializerOptions with camelCase naming policy for consistent serialization.
    /// Uses SecureJsonSerializerOptions base configuration.
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptions = new(SecureJsonSerializerOptions.InternalData)
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Serializes CanaryServiceExtensions metadata to JSON string
    /// </summary>
    /// <param name="metadata">The metadata to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>JSON string representation of CanaryServiceExtensions metadata</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metadata"/> is null</exception>
    public static string ToJson(CanaryServiceExtensionsMetadata metadata, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var options = new JsonSerializerOptions(_jsonOptions)
        {
            WriteIndented = indented
        };

        return JsonSerializer.Serialize(metadata, options);
    }

    /// <summary>
    /// Deserializes JSON string to CanaryServiceExtensions metadata
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>CanaryServiceExtensions metadata or null if deserialization fails</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null</exception>
    public static CanaryServiceExtensionsMetadata? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            return JsonSerializer.Deserialize<CanaryServiceExtensionsMetadata>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize JSON string to CanaryServiceExtensions metadata
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="value">Output value, null if deserialization fails</param>
    /// <returns>True if deserialization succeeds, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null</exception>
    public static bool TryFromJson(string json, out CanaryServiceExtensionsMetadata? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<CanaryServiceExtensionsMetadata>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Metadata representation of the CanaryServiceExtensions class for JSON serialization
    /// </summary>
    public sealed class CanaryServiceExtensionsMetadata
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