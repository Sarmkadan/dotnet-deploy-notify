using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetDeployNotify.Results;

/// <summary>
/// Provides JSON serialization extensions for <see cref="Result"/> and <see cref="Result{T}"/> types
/// </summary>
public static class ResultJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Converts a <see cref="Result"/> to its JSON representation
    /// </summary>
    /// <param name="value">The result to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>JSON string representation of the result</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static string ToJson(this Result value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Converts a <see cref="Result{T}"/> to its JSON representation
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="value">The result to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>JSON string representation of the result</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static string ToJson<T>(this Result<T> value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Parses a JSON string into a <see cref="Result"/>
    /// </summary>
    /// <param name="json">JSON string to parse</param>
    /// <returns>The deserialized result, or null if parsing fails</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null or empty</exception>
    public static Result? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<Result>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Parses a JSON string into a <see cref="Result{T}"/>
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="json">JSON string to parse</param>
    /// <returns>The deserialized result, or null if parsing fails</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null or empty</exception>
    public static Result<T>? FromJson<T>(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<Result<T>>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to parse a JSON string into a <see cref="Result"/>
    /// </summary>
    /// <param name="json">JSON string to parse</param>
    /// <param name="value">Output parameter containing the deserialized result</param>
    /// <returns>True if parsing succeeds, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null or empty</exception>
    public static bool TryFromJson(string json, out Result? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<Result>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Attempts to parse a JSON string into a <see cref="Result{T}"/>
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="json">JSON string to parse</param>
    /// <param name="value">Output parameter containing the deserialized result</param>
    /// <returns>True if parsing succeeds, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null or empty</exception>
    public static bool TryFromJson<T>(string json, out Result<T>? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<Result<T>>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}