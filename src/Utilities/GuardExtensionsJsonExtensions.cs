using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetDeployNotify.Utilities
{
    /// <summary>
    /// Provides System.Text.Json serialization extensions for <see cref="GuardExtensions"/> guard methods
    /// </summary>
    public static class GuardExtensionsJsonExtensions
    {
        /// <summary>
        /// JsonSerializerOptions with camelCase naming policy for consistent serialization
        /// </summary>
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.General)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            IncludeFields = false
        };

        /// <summary>
        /// Serializes GuardExtensions type information to JSON string
        /// </summary>
        /// <param name="indented">Whether to format the JSON with indentation</param>
        /// <returns>JSON string representation of GuardExtensions metadata</returns>
        public static string ToJson(bool indented = false)
        {
            var options = new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = indented
            };

            var metadata = new GuardExtensionsMetadata
            {
                Type = nameof(GuardExtensions),
                Namespace = typeof(GuardExtensions).Namespace ?? "DotNetDeployNotify.Utilities",
                Assembly = typeof(GuardExtensions).Assembly.GetName().Name ?? "DotNetDeployNotify",
                Methods = [
                    nameof(GuardExtensions.ThrowIfNull),
                    nameof(GuardExtensions.ThrowIfNullOrEmpty),
                    nameof(GuardExtensions.ThrowIfFalse),
                    nameof(GuardExtensions.ThrowIfLessThan),
                    nameof(GuardExtensions.ThrowIfLongerThan),
                    nameof(GuardExtensions.ThrowIfInvalidUrl),
                    nameof(GuardExtensions.GetValueOrThrow),
                    nameof(GuardExtensions.IsInRange),
                    nameof(GuardExtensions.MatchesPattern)
                ]
            };

            return JsonSerializer.Serialize(metadata, options);
        }

        /// <summary>
        /// Deserializes JSON string to GuardExtensions metadata
        /// </summary>
        /// <param name="json">JSON string to deserialize</param>
        /// <returns>GuardExtensions metadata or null if deserialization fails</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null</exception>
        public static GuardExtensionsMetadata? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);

            try
            {
                return JsonSerializer.Deserialize<GuardExtensionsMetadata>(json, _jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to deserialize JSON string to GuardExtensions metadata
        /// </summary>
        /// <param name="json">JSON string to deserialize</param>
        /// <param name="value">Output value, null if deserialization fails</param>
        /// <returns>True if deserialization succeeds, false otherwise</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null</exception>
        public static bool TryFromJson(string json, out GuardExtensionsMetadata? value)
        {
            ArgumentNullException.ThrowIfNull(json);

            try
            {
                value = JsonSerializer.Deserialize<GuardExtensionsMetadata>(json, _jsonOptions);
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// Metadata representation of the GuardExtensions class for JSON serialization
        /// </summary>
        [Serializable]
        public sealed class GuardExtensionsMetadata
        {
            /// <summary>
            /// Gets or sets the type identifier
            /// </summary>
            public string? Type { get; set; }

            /// <summary>
            /// Gets or sets the namespace
            /// </summary>
            public string? Namespace { get; set; }

            /// <summary>
            /// Gets or sets the assembly name
            /// </summary>
            public string? Assembly { get; set; }

            /// <summary>
            /// Gets or sets the array of method names
            /// </summary>
            public string[]? Methods { get; set; }
        }
    }
}