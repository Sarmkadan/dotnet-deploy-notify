using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetDeployNotify.Benchmarks.Benchmarks;

/// <summary>
/// Provides System.Text.Json serialization helpers for <see cref="StringExtensionsBenchmarks"/>.
/// </summary>
public static class StringExtensionsBenchmarksJsonExtensions
{
	/// <summary>
	/// Shared JSON serialization options with camelCase naming policy.
	/// </summary>
	private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false
	};

	/// <summary>
	/// Serializes a <see cref="StringExtensionsBenchmarks"/> instance to JSON.
	/// </summary>
	/// <param name="value">The instance to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation.</param>
	/// <returns>JSON string representation of the instance.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
	public static string ToJson(this StringExtensionsBenchmarks value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
			: _jsonSerializerOptions;
		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a JSON string to a <see cref="StringExtensionsBenchmarks"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>A <see cref="StringExtensionsBenchmarks"/> instance, or null if deserialization failed.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is empty or whitespace.</exception>
	public static StringExtensionsBenchmarks? FromJson(string json)
	{
		ArgumentNullException.ThrowIfNull(json);

		if (string.IsNullOrWhiteSpace(json))
		{
			throw new ArgumentException("JSON string cannot be empty or whitespace.", nameof(json));
		}

		try
		{
			return JsonSerializer.Deserialize<StringExtensionsBenchmarks>(json, _jsonSerializerOptions);
		}
		catch (JsonException)
		{
			return null;
		}
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to a <see cref="StringExtensionsBenchmarks"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">The deserialized instance if successful; null otherwise.</param>
	/// <returns>True if deserialization succeeded; false otherwise.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is empty or whitespace.</exception>
	public static bool TryFromJson(string json, out StringExtensionsBenchmarks? value)
	{
		ArgumentNullException.ThrowIfNull(json);

		if (string.IsNullOrWhiteSpace(json))
		{
			throw new ArgumentException("JSON string cannot be empty or whitespace.", nameof(json));
		}

		try
		{
			value = JsonSerializer.Deserialize<StringExtensionsBenchmarks>(json, _jsonSerializerOptions);
			return value is not null;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}
}
