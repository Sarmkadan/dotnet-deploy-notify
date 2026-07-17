#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetDeployNotify.Validation;

/// <summary>
/// Provides JSON serialization and deserialization extensions for validation rule types
/// </summary>
public static class ValidationRuleJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };


    /// <summary>
    /// Serializes a validation rule to JSON string
    /// </summary>
    /// <param name="value">The validation rule to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>JSON representation of the validation rule</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this ValidationRule<string> value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Serializes a numeric validation rule to JSON string
    /// </summary>
    /// <param name="value">The validation rule to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>JSON representation of the validation rule</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this ValidationRule<int> value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a string validation rule from JSON string
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>The deserialized validation rule</returns>
    /// <exception cref="ArgumentNullException">Thrown when json is null</exception>
    /// <exception cref="JsonException">Thrown when JSON is invalid, empty, or cannot be deserialized</exception>
    public static ValidationRule<string> FromJsonString(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            throw new JsonException("JSON string cannot be empty or whitespace");
        }

        ValidationRule<string>? result = JsonSerializer.Deserialize<NotEmptyRule>(json, _jsonOptions);
        if (result is null)
        {
            result = JsonSerializer.Deserialize<LengthRule>(json, _jsonOptions);
        }
        if (result is null)
        {
            result = JsonSerializer.Deserialize<UrlRule>(json, _jsonOptions);
        }
        if (result is null)
        {
            result = JsonSerializer.Deserialize<EmailRule>(json, _jsonOptions);
        }
        if (result is null)
        {
            result = JsonSerializer.Deserialize<PatternRule>(json, _jsonOptions);
        }

        return result ?? throw new JsonException("Unable to deserialize validation rule from JSON");
    }

    /// <summary>
    /// Deserializes a numeric validation rule from JSON string
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>The deserialized validation rule</returns>
    /// <exception cref="ArgumentNullException">Thrown when json is null</exception>
    /// <exception cref="JsonException">Thrown when JSON is invalid, empty, or cannot be deserialized</exception>
    public static ValidationRule<int> FromJsonInt(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            throw new JsonException("JSON string cannot be empty or whitespace");
        }

        return JsonSerializer.Deserialize<RangeRule>(json, _jsonOptions)
            ?? throw new JsonException("Unable to deserialize validation rule from JSON");
    }

    /// <summary>
    /// Attempts to deserialize a string validation rule from JSON string
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="value">Output parameter for the deserialized validation rule</param>
    /// <returns>True if deserialization succeeded; false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when json is null</exception>
    public static bool TryFromJson(string json, out ValidationRule<string>? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = FromJsonString(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Attempts to deserialize a numeric validation rule from JSON string
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="value">Output parameter for the deserialized validation rule</param>
    /// <returns>True if deserialization succeeded; false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when json is null</exception>
    public static bool TryFromJson(string json, out ValidationRule<int>? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = FromJsonInt(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}