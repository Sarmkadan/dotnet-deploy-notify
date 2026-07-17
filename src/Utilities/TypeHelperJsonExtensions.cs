#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetDeployNotify.Utilities;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="TypeHelper"/>
/// </summary>
public static class TypeHelperJsonExtensions
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
    /// Serializes TypeHelper type information to JSON string
    /// </summary>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>JSON string representation of TypeHelper metadata</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="indented"/> is invalid</exception>
    public static string ToJson(bool indented = false)
    {
        var options = new JsonSerializerOptions(_jsonOptions)
        {
            WriteIndented = indented
        };

        var typeHelperType = typeof(TypeHelper);
        var metadata = new TypeHelperMetadata
        {
            Type = typeHelperType.Name,
            Namespace = typeHelperType.Namespace ?? "DotNetDeployNotify.Utilities",
            Assembly = typeHelperType.Assembly.GetName().Name ?? "DotNetDeployNotify",
            Methods = GetPublicStaticMethodNames(typeHelperType)
        };

        return JsonSerializer.Serialize(metadata, options);
    }

    /// <summary>
    /// Deserializes JSON string to TypeHelper metadata
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>TypeHelper metadata or null if deserialization fails</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null</exception>
    public static TypeHelperMetadata? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            return JsonSerializer.Deserialize<TypeHelperMetadata>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize JSON string to TypeHelper metadata
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="value">Output value, null if deserialization fails</param>
    /// <returns>True if deserialization succeeds, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null</exception>
    public static bool TryFromJson(string json, out TypeHelperMetadata? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<TypeHelperMetadata>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Gets the names of all public static methods from a type
    /// </summary>
    /// <param name="type">The type to inspect</param>
    /// <returns>Array of method names with generic parameters</returns>
    private static string[] GetPublicStaticMethodNames(Type type)
    {
        return type.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(m => m.IsGenericMethod ? $"{m.Name}<T>" : m.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    /// <summary>
    /// Metadata representation of the TypeHelper class for JSON serialization
    /// </summary>
    public sealed class TypeHelperMetadata
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