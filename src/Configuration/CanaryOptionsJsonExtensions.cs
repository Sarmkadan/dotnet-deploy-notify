using System;
using System.Text.Json;

namespace DotNetDeployNotify.Configuration
{
    /// <summary>
    /// Provides JSON serialization and deserialization extensions for <see cref="CanaryOptions"/>.
    /// </summary>
    public static class CanaryOptionsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };

        /// <summary>
        /// Serializes the specified <see cref="CanaryOptions"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The options to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the options.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
        public static string ToJson(this CanaryOptions value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
                : _jsonSerializerOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string to a <see cref="CanaryOptions"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>A <see cref="CanaryOptions"/> instance, or <see langword="null"/> if deserialization fails.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
        public static CanaryOptions? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);

            return JsonSerializer.Deserialize<CanaryOptions>(json, _jsonSerializerOptions);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a <see cref="CanaryOptions"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">When this method returns, contains the deserialized options or <see langword="null"/> if deserialization failed.</param>
        /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
        public static bool TryFromJson(string json, out CanaryOptions? value)
        {
            ArgumentNullException.ThrowIfNull(json);

            value = default;
            try
            {
                value = FromJson(json);
                return value is not null;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}