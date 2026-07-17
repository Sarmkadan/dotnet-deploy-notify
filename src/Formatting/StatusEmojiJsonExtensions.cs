#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using DotNetDeployNotify.Core;

namespace DotNetDeployNotify.Formatting;

/// <summary>
/// Provides System.Text.Json serialization helpers for status emoji mappings.
/// </summary>
public static class StatusEmojiJsonExtensions
{
	/// <summary>
	/// Shared JSON serialization options with camelCase naming policy.
	/// </summary>
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		PropertyNameCaseInsensitive = true,
		DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
	};

	/// <summary>
	/// Serializes a <see cref="StatusEmoji"/> instance to JSON.
	/// </summary>
	/// <param name="value">The status emoji mapping to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation.</param>
	/// <returns>JSON string representation of the status emoji mapping.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
	public static string ToJson(this StatusEmoji value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(JsonOptions) { WriteIndented = true }
			: JsonOptions;
		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a JSON string to a <see cref="StatusEmoji"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>A <see cref="StatusEmoji"/> instance if deserialization succeeded; otherwise null.</returns>
	/// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or empty.</exception>
	public static StatusEmoji? FromJson(string json)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);
		return JsonSerializer.Deserialize<StatusEmoji>(json, JsonOptions);
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to a <see cref="StatusEmoji"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">The deserialized instance if successful; null otherwise.</param>
	/// <returns>True if deserialization succeeded; false otherwise.</returns>
	/// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or empty.</exception>
	public static bool TryFromJson(string json, out StatusEmoji? value)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);
		try
		{
			value = JsonSerializer.Deserialize<StatusEmoji>(json, JsonOptions);
			return value is not null;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}

	/// <summary>
	/// Represents a status emoji mapping for serialization
	/// </summary>
	public sealed record StatusEmoji
	{
		/// <summary>
		/// Gets the build status
		/// </summary>
		public required BuildStatus Status { get; init; }

		/// <summary>
		/// Gets the emoji representation
		/// </summary>
		public required string Emoji { get; init; }

		/// <summary>
		/// Gets the status label
		/// </summary>
		public required string Label { get; init; }
	}
}