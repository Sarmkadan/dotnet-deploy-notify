using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Provides JSON serialization helpers for <see cref="ValidationServiceTests"/> instances.
/// </summary>
public static class ValidationServiceTestsJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		TypeInfoResolver = new DefaultJsonTypeInfoResolver()
	};

	/// <summary>
	/// Serializes the specified <see cref="ValidationServiceTests"/> value to a JSON string.
	/// </summary>
	/// <param name="value">The value to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation.</param>
	/// <returns>A JSON string representation of the value.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
	public static string ToJson(this ValidationServiceTests value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		return JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } : _jsonOptions);
	}

	/// <summary>
	/// Deserializes a JSON string to a <see cref="ValidationServiceTests"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>The deserialized <see cref="ValidationServiceTests"/> instance, or <see langword="null"/> if the JSON is empty or whitespace.</returns>
	/// <exception cref="JsonException">The JSON is invalid or cannot be deserialized.</exception>
	public static ValidationServiceTests? FromJson(string json)
	{
		if (string.IsNullOrWhiteSpace(json))
		{
			return null;
		}

		return JsonSerializer.Deserialize<ValidationServiceTests>(json, _jsonOptions);
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to a <see cref="ValidationServiceTests"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized value on success.</param>
	/// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
	public static bool TryFromJson(string json, out ValidationServiceTests? value)
	{
		ArgumentNullException.ThrowIfNull(json);

		if (string.IsNullOrWhiteSpace(json))
		{
			value = null;
			return false;
		}

		try
		{
			value = JsonSerializer.Deserialize<ValidationServiceTests>(json, _jsonOptions);
			return value is not null;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}
}