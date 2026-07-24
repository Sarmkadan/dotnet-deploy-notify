#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetDeployNotify.Serialization;

namespace DotNetDeployNotify.Canary;

/// <summary>
/// Provides System.Text.Json serialization helpers for <see cref="CanaryDeploymentEngineExtensions"/> type metadata.
/// </summary>
public static class CanaryDeploymentEngineExtensionsJsonExtensions
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
    /// Serializes CanaryDeploymentEngineExtensions type information to JSON string.
    /// </summary>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON string representation of CanaryDeploymentEngineExtensions metadata.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="indented"/> is outside valid range.</exception>
    public static string ToJson(bool indented = false)
    {
        var options = new JsonSerializerOptions(_jsonOptions) { WriteIndented = indented };
        var metadata = new CanaryDeploymentEngineExtensionsMetadata
        {
            Type = "CanaryDeploymentEngineExtensions",
            Namespace = typeof(CanaryDeploymentEngineExtensions).Namespace ?? "DotNetDeployNotify.Canary",
            Assembly = typeof(CanaryDeploymentEngineExtensions).Assembly.GetName().Name ?? "DotNetDeployNotify",
            Methods = [
                "TryAdvanceRolloutAsync",
                "TryPromoteAsync",
                "TryAbortAsync",
                "GetCanaryPercentageNormalizedAsync"
            ]
        };
        return JsonSerializer.Serialize(metadata, options);
    }

    /// <summary>
    /// Deserializes JSON string to CanaryDeploymentEngineExtensions metadata.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <returns>CanaryDeploymentEngineExtensions metadata or null if deserialization fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static CanaryDeploymentEngineExtensionsMetadata? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        try
        {
            return JsonSerializer.Deserialize<CanaryDeploymentEngineExtensionsMetadata>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize JSON string to CanaryDeploymentEngineExtensions metadata.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <param name="value">Output value, null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static bool TryFromJson(string json, out CanaryDeploymentEngineExtensionsMetadata? value)
    {
        ArgumentNullException.ThrowIfNull(json);
        try
        {
            value = JsonSerializer.Deserialize<CanaryDeploymentEngineExtensionsMetadata>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Metadata representation of the CanaryDeploymentEngineExtensions class for JSON serialization.
    /// </summary>
    public sealed class CanaryDeploymentEngineExtensionsMetadata
    {
        /// <summary>
        /// Gets or sets the type identifier.
        /// </summary>
        public string Type { get; set; } = string.Empty;

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
