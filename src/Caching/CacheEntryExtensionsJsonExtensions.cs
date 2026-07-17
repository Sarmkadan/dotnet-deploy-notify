#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetDeployNotify.Caching;

/// <summary>
/// System.Text.Json serialization extensions for <see cref="CacheEntryExtensions"/> metadata.
/// </summary>
public static class CacheEntryExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes <see cref="CacheEntryExtensions"/> metadata to JSON string.
    /// </summary>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON string representation of <see cref="CacheEntryExtensionsMetadata"/>.</returns>
    public static string ToJson(bool indented = false)
    {
        var options = indented switch
        {
            true => new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true },
            false => _jsonSerializerOptions
        };

        var metadata = new CacheEntryExtensionsMetadata
        {
            Type = "CacheEntryExtensions",
            Namespace = typeof(CacheEntryExtensions).Namespace ?? "DotNetDeployNotify.Caching",
            Assembly = typeof(CacheEntryExtensions).Assembly.GetName().Name ?? "DotNetDeployNotify",
            Methods = [
                nameof(CacheEntryExtensions.GetTimeToLive),
                nameof(CacheEntryExtensions.IsValid),
                nameof(CacheEntryExtensions.GetAge),
                nameof(CacheEntryExtensions.GetExpirationPercentage)
            ]
        };

        return JsonSerializer.Serialize(metadata, options);
    }

    /// <summary>
    /// Deserializes JSON string to <see cref="CacheEntryExtensions"/> metadata.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <returns><see cref="CacheEntryExtensionsMetadata"/> metadata or null if deserialization fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static CacheEntryExtensionsMetadata? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            return JsonSerializer.Deserialize<CacheEntryExtensionsMetadata>(json, _jsonSerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize JSON string to <see cref="CacheEntryExtensions"/> metadata.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <param name="value">Output value, null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static bool TryFromJson(string json, out CacheEntryExtensionsMetadata? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<CacheEntryExtensionsMetadata>(json, _jsonSerializerOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Metadata representation of the CacheEntryExtensions class for JSON serialization.
    /// </summary>
    public sealed class CacheEntryExtensionsMetadata
    {
        /// <summary>
        /// Gets or sets the type identifier.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the namespace.
        /// </summary>
        public string? Namespace { get; set; }

        /// <summary>
        /// Gets or sets the assembly name.
        /// </summary>
        public string? Assembly { get; set; }

        /// <summary>
        /// Gets or sets the array of method names.
        /// </summary>
        public string[]? Methods { get; set; }
    }
}