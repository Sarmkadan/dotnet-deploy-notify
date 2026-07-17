using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="TemplateServiceTests"/> instances.
/// </summary>
public static class TemplateServiceTestsJsonExtensions
{
 private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
 {
 PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
 WriteIndented = false
 };

 /// <summary>
 /// Serializes a <see cref="TemplateServiceTests"/> instance to a JSON string.
 /// </summary>
 /// <param name="value">The instance to serialize.</param>
 /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
 /// <returns>A JSON string representation of the instance.</returns>
 /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
 public static string ToJson(this TemplateServiceTests value, bool indented = false)
 {
 ArgumentNullException.ThrowIfNull(value);
 if (indented)
 {
 _jsonSerializerOptions.WriteIndented = true;
 }
 return JsonSerializer.Serialize(value, _jsonSerializerOptions);
 }

 /// <summary>
 /// Deserializes a JSON string to a <see cref="TemplateServiceTests"/> instance.
 /// </summary>
 /// <param name="json">The JSON string to deserialize.</param>
 /// <returns>The deserialized instance, or <see langword="null"/> if deserialization fails.</returns>
 /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
 /// <exception cref="JsonException">Thrown when the JSON is malformed.</exception>
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

 /// <summary>
 /// Attempts to deserialize a JSON string to a <see cref="TemplateServiceTests"/> instance.
 /// </summary>
 /// <param name="json">The JSON string to deserialize.</param>
 /// <param name="value">Receives the deserialized instance if successful; otherwise, <see langword="null"/>.</param>
 /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
 /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
 /// <exception cref="JsonException">Thrown when the JSON is malformed.</exception>
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