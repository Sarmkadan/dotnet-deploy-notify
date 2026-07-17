#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetDeployNotify.Utilities;

/// <summary>
/// Provides System.Text.Json serialization extensions for CollectionExtensions metadata
/// </summary>
public static class CollectionExtensionsJsonExtensions
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
    /// Serializes CollectionExtensions type information to JSON string
    /// </summary>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>JSON string representation of CollectionExtensions metadata</returns>
    public static string ToJson(bool indented = false)
    {
        var options = new JsonSerializerOptions(_jsonOptions)
        {
            WriteIndented = indented
        };

        var metadata = new CollectionExtensionsMetadata
        {
            Type = typeof(CollectionExtensions).Name,
            Namespace = typeof(CollectionExtensions).Namespace ?? "DotNetDeployNotify.Utilities",
            Assembly = typeof(CollectionExtensions).Assembly.GetName().Name ?? "DotNetDeployNotify",
            Methods = GetPublicStaticMethodNames(typeof(CollectionExtensions))
        };

        return JsonSerializer.Serialize(metadata, options);
    }

    /// <summary>
    /// Gets the names of all public static methods in a type
    /// </summary>
    /// <param name="type">The type to inspect</param>
    /// <returns>Array of method names</returns>
    private static string[] GetPublicStaticMethodNames(Type type)
    {
        return type.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => !m.IsSpecialName) // Exclude property accessors, operators, etc.
            .Select(m => m.Name)
            .OrderBy(name => name)
            .ToArray();
    }

    /// <summary>
    /// Deserializes JSON string to CollectionExtensions metadata
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>CollectionExtensions metadata or null if deserialization fails</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null</exception>
    public static CollectionExtensionsMetadata? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            return JsonSerializer.Deserialize<CollectionExtensionsMetadata>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize JSON string to CollectionExtensions metadata
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="value">Output value, null if deserialization fails</param>
    /// <returns>True if deserialization succeeds, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null</exception>
    public static bool TryFromJson(string json, out CollectionExtensionsMetadata? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<CollectionExtensionsMetadata>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Metadata representation of the CollectionExtensions class for JSON serialization
    /// </summary>
    public sealed class CollectionExtensionsMetadata
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