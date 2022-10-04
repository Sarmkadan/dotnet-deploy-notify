using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetDeployNotify.Tests;

public static class TemplateServiceTestsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static string ToJson(this TemplateServiceTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (indented)
        {
            _jsonSerializerOptions.WriteIndented = true;
        }
        return JsonSerializer.Serialize(value, _jsonSerializerOptions);
    }

    public static TemplateServiceTests? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        try
        {
            return JsonSerializer.Deserialize<TemplateServiceTests>(json, _jsonSerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static bool TryFromJson(string json, out TemplateServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(json);
        try
        {
            value = JsonSerializer.Deserialize<TemplateServiceTests>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
