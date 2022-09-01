#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetDeployNotify.Context;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for <see cref="RequestContext"/>
/// </summary>
public static class RequestContextJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		ReferenceHandler = ReferenceHandler.IgnoreCycles,
		PropertyNameCaseInsensitive = true
	};

	/// <summary>
	/// Serializes the <see cref="RequestContext"/> to a JSON string
	/// </summary>
	/// <param name="value">The <see cref="RequestContext"/> to serialize</param>
	/// <param name="indented">Whether to format the JSON with indentation</param>
	/// <returns>A JSON string representation of the <see cref="RequestContext"/></returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
	public static string ToJson(this RequestContext value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
			: _jsonOptions;

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a <see cref="RequestContext"/> from a JSON string
	/// </summary>
	/// <param name="json">The JSON string to deserialize</param>
	/// <returns>The deserialized <see cref="RequestContext"/>, or null if JSON is null, empty, or whitespace</returns>
	/// <exception cref="JsonException">Thrown when the JSON is malformed and cannot be deserialized</exception>
	public static RequestContext? FromJson(string json)
	{
		if (string.IsNullOrWhiteSpace(json))
		{
			return null;
		}

		try
		{
			return JsonSerializer.Deserialize<RequestContext>(json, _jsonOptions);
		}
		catch (JsonException ex)
		{
			throw new JsonException("Failed to deserialize RequestContext from JSON", ex);
		}
	}

	/// <summary>
	/// Attempts to deserialize a <see cref="RequestContext"/> from a JSON string
	/// </summary>
	/// <param name="json">The JSON string to deserialize</param>
	/// <param name="value">Receives the deserialized <see cref="RequestContext"/> if successful</param>
	/// <returns>True if deserialization succeeded; otherwise, false</returns>
	public static bool TryFromJson(string json, out RequestContext? value)
	{
		value = null;

		if (string.IsNullOrWhiteSpace(json))
		{
			return false;
		}

		try
		{
			value = JsonSerializer.Deserialize<RequestContext>(json, _jsonOptions);
			return true;
		}
		catch (JsonException)
		{
			return false;
		}
	}
}