// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetDeployNotify.Core.Models;

namespace DotNetDeployNotify.Serialization;

/// <summary>
/// Custom JsonConverter for handling BuildStatus enum serialization
/// </summary>
public class BuildStatusConverter : JsonConverter<BuildStatus>
{
    public override BuildStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var stringValue = reader.GetString();
        if (string.IsNullOrEmpty(stringValue))
            return BuildStatus.Pending;

        return Enum.Parse<BuildStatus>(stringValue, ignoreCase: true);
    }

    public override void Write(Utf8JsonWriter writer, BuildStatus value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

/// <summary>
/// Custom JsonConverter for handling NotificationChannel enum serialization
/// </summary>
public class NotificationChannelConverter : JsonConverter<NotificationChannel>
{
    public override NotificationChannel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var stringValue = reader.GetString();
        if (string.IsNullOrEmpty(stringValue))
            return NotificationChannel.Slack;

        return Enum.Parse<NotificationChannel>(stringValue, ignoreCase: true);
    }

    public override void Write(Utf8JsonWriter writer, NotificationChannel value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

/// <summary>
/// Serialization helper for JSON operations
/// </summary>
public class JsonSerializationHelper
{
    private readonly JsonSerializerOptions _options;

    public JsonSerializationHelper()
    {
        _options = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new JsonStringEnumConverter(),
                new BuildStatusConverter(),
                new NotificationChannelConverter()
            }
        };
    }

    /// <summary>
    /// Serializes an object to JSON string
    /// </summary>
    public string Serialize<T>(T obj)
    {
        try
        {
            return JsonSerializer.Serialize(obj, _options);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to serialize object: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Deserializes a JSON string to an object
    /// </summary>
    public T? Deserialize<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(json, _options);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to deserialize JSON: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Converts an object to dictionary
    /// </summary>
    public Dictionary<string, object?> ObjectToDictionary<T>(T obj)
    {
        if (obj == null)
            return new Dictionary<string, object?>();

        var json = JsonSerializer.Serialize(obj, _options);
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        return JsonElementToDictionary(jsonElement);
    }

    private Dictionary<string, object?> JsonElementToDictionary(JsonElement element)
    {
        var dict = new Dictionary<string, object?>();

        foreach (var property in element.EnumerateObject())
        {
            dict[property.Name] = GetJsonElementValue(property.Value);
        }

        return dict;
    }

    private object? GetJsonElementValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.GetDecimal(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Array => element.EnumerateArray().Select(GetJsonElementValue).ToList(),
            JsonValueKind.Object => JsonElementToDictionary(element),
            _ => element.ToString()
        };
    }
}

/// <summary>
/// Safe JSON parser that doesn't throw on invalid JSON
/// </summary>
public class SafeJsonParser
{
    /// <summary>
    /// Safely tries to parse JSON without throwing exceptions
    /// </summary>
    public static (bool Success, T? Result) TryParse<T>(string json)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json))
                return (false, default);

            var result = JsonSerializer.Deserialize<T>(json);
            return (true, result);
        }
        catch
        {
            return (false, default);
        }
    }

    /// <summary>
    /// Safely merges multiple JSON objects
    /// </summary>
    public static string MergeJsonObjects(params string[] jsonStrings)
    {
        var merged = new Dictionary<string, object>();

        foreach (var json in jsonStrings.Where(j => !string.IsNullOrWhiteSpace(j)))
        {
            if (TryParse<Dictionary<string, object>>(json).Success &&
                TryParse<Dictionary<string, object>>(json).Result is not null)
            {
                var dict = TryParse<Dictionary<string, object>>(json).Result;
                foreach (var kvp in dict!)
                {
                    merged[kvp.Key] = kvp.Value;
                }
            }
        }

        return JsonSerializer.Serialize(merged);
    }

    /// <summary>
    /// Validates JSON structure
    /// </summary>
    public static bool IsValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
