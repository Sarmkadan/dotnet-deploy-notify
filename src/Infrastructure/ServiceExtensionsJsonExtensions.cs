#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetDeployNotify.Infrastructure;

/// <summary>
/// Provides System.Text.Json serialization extensions for ServiceExtensions
/// </summary>
public static class ServiceExtensionsJsonExtensions
{
    /// <summary>
    /// JsonSerializerOptions with camelCase naming policy for consistent serialization
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Serializes ServiceExtensions type information to JSON string
    /// </summary>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>JSON string representation of ServiceExtensions metadata</returns>
    public static string ToJson(bool indented = false)
    {
        var options = new JsonSerializerOptions(_jsonOptions)
        {
            WriteIndented = indented
        };
        var metadata = new ServiceExtensionsMetadata
        {
            Type = "ServiceExtensions",
            Namespace = typeof(ServiceExtensions).Namespace ?? "DotNetDeployNotify.Infrastructure",
            Assembly = typeof(ServiceExtensions).Assembly.GetName().Name ?? "DotNetDeployNotify",
            Methods = new[] { "IsCritical", "IsProduction", "SupportsStatus", "SupportsEnvironment",
                            "GetDescription", "MergeMetadata", "Clone", "ToCompactString",
                            "GetSeverityLevel", "ShouldRetry", "GetRetryDelay" }
        };
        return JsonSerializer.Serialize(metadata, options);
    }

    /// <summary>
    /// Deserializes JSON string to ServiceExtensions metadata
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>ServiceExtensions metadata or null if deserialization fails</returns>
    public static ServiceExtensionsMetadata? FromJson(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<ServiceExtensionsMetadata>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize JSON string to ServiceExtensions metadata
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="value">Output value, null if deserialization fails</param>
    /// <returns>True if deserialization succeeds, false otherwise</returns>
    public static bool TryFromJson(string json, out ServiceExtensionsMetadata? value)
    {
        try
        {
            value = JsonSerializer.Deserialize<ServiceExtensionsMetadata>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Metadata representation of the ServiceExtensions class for JSON serialization
    /// </summary>
    public sealed class ServiceExtensionsMetadata
    {
        public string? Type { get; set; }
        public string? Namespace { get; set; }
        public string? Assembly { get; set; }
        public string[]? Methods { get; set; }
    }
}