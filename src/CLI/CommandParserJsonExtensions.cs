#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetDeployNotify.CLI;

/// <summary>
/// Provides JSON serialization extensions for <see cref="ParsedCommand"/>
/// </summary>
public static class CommandParserJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		ReferenceHandler = ReferenceHandler.IgnoreCycles
	};

	/// <summary>
	/// Serializes the <see cref="ParsedCommand"/> instance to a JSON string
	/// </summary>
	/// <param name="value">The parsed command instance to serialize</param>
	/// <param name="indented">Whether to format the JSON with indentation</param>
	/// <returns>A JSON string representation of the parsed command</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
	public static string ToJson(this ParsedCommand value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
			: _jsonOptions;

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a JSON string to a <see cref="ParsedCommand"/> instance
	/// </summary>
	/// <param name="json">The JSON string to deserialize</param>
	/// <returns>A deserialized <see cref="ParsedCommand"/> instance, or null if parsing fails</returns>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty</exception>
	public static ParsedCommand? FromJson(string json)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);

		try
		{
			return JsonSerializer.Deserialize<ParsedCommand>(json, _jsonOptions);
		}
		catch (JsonException)
		{
			return null;
		}
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to a <see cref="ParsedCommand"/> instance
	/// </summary>
	/// <param name="json">The JSON string to deserialize</param>
	/// <param name="value">The resulting <see cref="ParsedCommand"/> instance, or null if parsing fails</param>
	/// <returns>True if deserialization succeeds; otherwise, false</returns>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty</exception>
	public static bool TryFromJson(string json, out ParsedCommand? value)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);

		try
		{
			value = JsonSerializer.Deserialize<ParsedCommand>(json, _jsonOptions);
			return true;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}
}