using System.Text.Json;
using DotNetDeployNotify.Configuration;

namespace DotNetDeployNotify.Configuration
{
    public static class CanaryOptionsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };

        public static string ToJson(this CanaryOptions value, bool indented = false)
        {
            return JsonSerializer.Serialize(value, _jsonSerializerOptions);
        }

        public static CanaryOptions? FromJson(string json)
        {
            return JsonSerializer.Deserialize<CanaryOptions>(json, _jsonSerializerOptions);
        }

        public static bool TryFromJson(string json, out CanaryOptions? value)
        {
            value = default;
            try
            {
                value = FromJson(json);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}