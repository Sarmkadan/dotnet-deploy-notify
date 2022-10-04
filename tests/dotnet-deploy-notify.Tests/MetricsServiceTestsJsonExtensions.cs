using System.Text.Json;

namespace DotNetDeployNotify.Tests;

public static class MetricsServiceTestsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static string ToJson(this MetricsServiceTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (indented)
        {
            _jsonSerializerOptions.WriteIndented = true;
        }
        return JsonSerializer.Serialize(value, _jsonSerializerOptions);
    }

    public static MetricsServiceTests? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        try
        {
            return JsonSerializer.Deserialize<MetricsServiceTests>(json, _jsonSerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static bool TryFromJson(string json, out MetricsServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(json);
        try
        {
            value = JsonSerializer.Deserialize<MetricsServiceTests>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
