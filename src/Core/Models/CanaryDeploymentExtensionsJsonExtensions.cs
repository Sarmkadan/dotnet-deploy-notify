#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetDeployNotify.Core.Models;

/// <summary>
/// Provides System.Text.Json serialization helpers for <see cref="CanaryDeploymentExtensions"/> type metadata.
/// </summary>
public static class CanaryDeploymentExtensionsJsonExtensions
{
    /// <summary>
    /// JsonSerializerOptions with camelCase naming policy for consistent serialization.
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.General)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Serializes CanaryDeploymentExtensions type information to JSON string.
    /// </summary>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON string representation of CanaryDeploymentExtensions metadata.</returns>
    public static string ToJson(bool indented = false)
    {
        var options = new JsonSerializerOptions(_jsonOptions) { WriteIndented = indented };

        var metadata = new CanaryDeploymentExtensionsMetadata
        {
            Type = nameof(CanaryDeploymentExtensions),
            Namespace = typeof(CanaryDeploymentExtensions).Namespace ?? "DotNetDeployNotify.Core.Models",
            Assembly = typeof(CanaryDeploymentExtensions).Assembly.GetName().Name ?? "DotNetDeployNotify",
            Methods = [
                "IsActive",
                "IsPromoted",
                "IsFailedOrAborted",
                "GetTrafficSplitDisplay",
                "CalculateHealthScore",
                "GetStatusSummary",
                "CanPromote",
                "GetNextTrafficPercentage",
                "GetCurrentSoakRemaining",
                "IsCurrentSoakComplete"
            ]
        };

        return JsonSerializer.Serialize(metadata, options);
    }

    /// <summary>
    /// Deserializes JSON string to CanaryDeploymentExtensions metadata.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <returns>CanaryDeploymentExtensions metadata or null if deserialization fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static CanaryDeploymentExtensionsMetadata? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            return JsonSerializer.Deserialize<CanaryDeploymentExtensionsMetadata>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize JSON string to CanaryDeploymentExtensions metadata.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <param name="value">Output value, null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static bool TryFromJson(string json, out CanaryDeploymentExtensionsMetadata? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<CanaryDeploymentExtensionsMetadata>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Metadata representation of the CanaryDeploymentExtensions class for JSON serialization.
    /// </summary>
    public sealed class CanaryDeploymentExtensionsMetadata
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