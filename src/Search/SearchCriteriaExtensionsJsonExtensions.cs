#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetDeployNotify.Search;

/// <summary>
/// Provides System.Text.Json serialization helpers for <see cref="SearchCriteriaExtensions"/> type metadata.
/// </summary>
public static class SearchCriteriaExtensionsJsonExtensions
{
    /// <summary>
    /// JsonSerializerOptions with camelCase naming policy for consistent serialization.
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Serializes SearchCriteriaExtensions type information to JSON string.
    /// </summary>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON string representation of SearchCriteriaExtensions metadata.</returns>
    public static string ToJson(bool indented = false)
    {
        var options = new JsonSerializerOptions(_jsonOptions) { WriteIndented = indented };
        var metadata = new SearchCriteriaExtensionsMetadata
        {
            Type = "SearchCriteriaExtensions",
            Namespace = typeof(SearchCriteriaExtensions).Namespace ?? "DotNetDeployNotify.Search",
            Assembly = typeof(SearchCriteriaExtensions).Assembly.GetName().Name ?? "DotNetDeployNotify",
            Methods = [
                "Combine",
                "ClearFilters",
                "WithPagination",
                "FilterByPriority"
            ]
        };
        return JsonSerializer.Serialize(metadata, options);
    }

    /// <summary>
    /// Deserializes JSON string to SearchCriteriaExtensions metadata.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <returns>SearchCriteriaExtensions metadata or null if deserialization fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static SearchCriteriaExtensionsMetadata? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        try
        {
            return JsonSerializer.Deserialize<SearchCriteriaExtensionsMetadata>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize JSON string to SearchCriteriaExtensions metadata.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <param name="value">Output value, null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static bool TryFromJson(string json, out SearchCriteriaExtensionsMetadata? value)
    {
        ArgumentNullException.ThrowIfNull(json);
        try
        {
            value = JsonSerializer.Deserialize<SearchCriteriaExtensionsMetadata>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Metadata representation of the SearchCriteriaExtensions class for JSON serialization.
    /// </summary>
    public sealed class SearchCriteriaExtensionsMetadata
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