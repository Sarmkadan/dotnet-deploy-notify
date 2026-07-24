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
/// Provides System.Text.Json serialization extensions for ServiceExtensions
/// </summary>
public static class ServiceExtensionsJsonExtensions
{
    /// <summary>
    /// Serializes ServiceExtensions type information to JSON string
    /// </summary>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>JSON string representation of ServiceExtensions metadata</returns>
    public static string ToJson(bool indented = false)
    {
        var metadata = new ServiceExtensionsMetadata
        {
            Type = "ServiceExtensions",
            Namespace = typeof(ServiceExtensions).Namespace ?? "DotNetDeployNotify.Infrastructure",
            Assembly = typeof(ServiceExtensions).Assembly.GetName().Name ?? "DotNetDeployNotify",
            Methods = [
                "IsCritical",
                "IsProduction",
                "SupportsStatus",
                "SupportsEnvironment",
                "GetDescription",
                "MergeMetadata",
                "Clone",
                "ToCompactString",
                "GetSeverityLevel",
                "ShouldRetry",
                "GetRetryDelay"
            ]
        };
        return JsonSerializationUtilities.Serialize(metadata, indented);
    }

    /// <summary>
    /// Deserializes JSON string to ServiceExtensions metadata
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>ServiceExtensions metadata or null if deserialization fails</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null</exception>
    public static ServiceExtensionsMetadata? FromJson(string json)
    {
        return JsonSerializationUtilities.SafeDeserialize<ServiceExtensionsMetadata>(json, JsonSerializationUtilities.DefaultInternalOptions);
    }

    /// <summary>
    /// Attempts to deserialize JSON string to ServiceExtensions metadata
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="value">Output value, null if deserialization fails</param>
    /// <returns>True if deserialization succeeds, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null</exception>
    public static bool TryFromJson(string json, out ServiceExtensionsMetadata? value)
    {
        return JsonSerializationUtilities.TryDeserialize(json, JsonSerializationUtilities.DefaultInternalOptions, out value);
    }

    /// <summary>
    /// Metadata representation of the ServiceExtensions class for JSON serialization
    /// </summary>
    public sealed class ServiceExtensionsMetadata
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
