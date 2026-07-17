using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetDeployNotify.Results;

/// <summary>
/// Provides JSON serialization extensions for <see cref="Result"/> and <see cref="Result{T}"/> types.
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
    /// Converts a <see cref="Result"/> to its JSON representation.
    /// </summary>
    /// <param name="value">The result to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON string representation of the result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this Result value, bool indented = false) =>
        ToJson(value, indented, _jsonOptions);

    /// <summary>
    /// Converts a <see cref="Result{T}"/> to its JSON representation.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The result to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON string representation of the result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson<T>(this Result<T> value, bool indented = false) =>
        ToJson(value, indented, _jsonOptions);

    /// <summary>
    /// Parses a JSON string into a <see cref="Result"/>.
    /// </summary>
    /// <param name="json">JSON string to parse.</param>
    /// <returns>The deserialized result, or <see langword="null"/> if parsing fails.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/> or empty.</exception>
    public static Result? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return TryParseJson(json, out var result)
            ? result
            : null;
    }

    /// <summary>
    /// Parses a JSON string into a <see cref="Result{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="json">JSON string to parse.</param>
    /// <returns>The deserialized result, or <see langword="null"/> if parsing fails.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/> or empty.</exception>
    public static Result<T>? FromJson<T>(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return TryParseJson<T>(json, out var result)
            ? result
            : null;
    }

    /// <summary>
    /// Attempts to parse a JSON string into a <see cref="Result"/>.
    /// </summary>
    /// <param name="json">JSON string to parse.</param>
    /// <param name="value">Output parameter containing the deserialized result.</param>
    /// <returns><see langword="true"/> if parsing succeeds; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/> or empty.</exception>
    public static bool TryFromJson(string json, out Result? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return TryParseJson(json, out value);
    }

    /// <summary>
    /// Attempts to parse a JSON string into a <see cref="Result{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="json">JSON string to parse.</param>
    /// <param name="value">Output parameter containing the deserialized result.</param>
    /// <returns><see langword="true"/> if parsing succeeds; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/> or empty.</exception>
    public static bool TryFromJson<T>(string json, out Result<T>? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return TryParseJson(json, out value);
    }

    private static string ToJson(Result value, bool indented, JsonSerializerOptions baseOptions)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = new JsonSerializerOptions(baseOptions)
        {
            WriteIndented = indented
        };

        return JsonSerializer.Serialize(value, options);
    }

    private static string ToJson<T>(Result<T> value, bool indented, JsonSerializerOptions baseOptions)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = new JsonSerializerOptions(baseOptions)
        {
            WriteIndented = indented
        };

        return JsonSerializer.Serialize(value, options);
    }

    private static bool TryParseJson(string json, out Result? result)
    {
        try
        {
            result = JsonSerializer.Deserialize<Result>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            result = null;
            return false;
        }
    }

    private static bool TryParseJson<T>(string json, out Result<T>? result)
    {
        try
        {
            result = JsonSerializer.Deserialize<Result<T>>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            result = null;
            return false;
        }
    }
}