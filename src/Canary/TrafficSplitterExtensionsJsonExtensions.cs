#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Text.Json;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Serialization;

namespace DotNetDeployNotify.Canary;

/// <summary>
/// Provides System.Text.Json serialization helpers for <see cref="TrafficSplit"/> type.
/// </summary>
public static class TrafficSplitterExtensionsJsonExtensions
{
	/// <summary>
	/// Shared JSON serialization options with camelCase naming policy and secure settings.
	/// </summary>
	private static readonly JsonSerializerOptions JsonOptions = new(SecureJsonSerializerOptions.UntrustedInput);

	/// <summary>
	/// Serializes a <see cref="TrafficSplit"/> instance to a compact JSON string.
	/// </summary>
	/// <param name="value">The traffic split to serialize. Must not be null.</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
	/// <returns>JSON string representation of the traffic split.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
	public static string ToJson(this TrafficSplit value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(JsonOptions) { WriteIndented = true }
			: JsonOptions;
		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a JSON string to a <see cref="TrafficSplit"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize. Must not be null or empty.</param>
	/// <returns>A <see cref="TrafficSplit"/> instance if deserialization succeeds; otherwise null.</returns>
	/// <exception cref="ArgumentException"><paramref name="json"/> is null or empty.</exception>
	/// <remarks>
	/// Returns null if the JSON is malformed or cannot be deserialized to a <see cref="TrafficSplit"/>.
	/// </remarks>
	public static TrafficSplit? FromJson(string json)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);
		return JsonSerializer.Deserialize<TrafficSplit>(json, JsonOptions);
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to a <see cref="TrafficSplit"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize. Must not be null or empty.</param>
	/// <param name="value">
	/// When this method returns, contains the deserialized <see cref="TrafficSplit"/> instance if successful,
	/// or null if deserialization failed.
	/// </param>
	/// <returns>True if deserialization succeeded; false otherwise.</returns>
	/// <exception cref="ArgumentException"><paramref name="json"/> is null or empty.</exception>
	/// <remarks>
	/// This method suppresses <see cref="JsonException"/> and returns false rather than throwing,
	/// making it suitable for defensive programming scenarios.
	/// </remarks>
	public static bool TryFromJson(string json, out TrafficSplit? value)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);

		try
		{
			value = JsonSerializer.Deserialize<TrafficSplit>(json, JsonOptions);
			return value is not null;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}
}
