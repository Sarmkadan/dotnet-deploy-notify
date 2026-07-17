using System;
using System.Text.Json;

namespace DotNetDeployNotify.Core.Models;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="WebhookPayload"/>.
/// </summary>
public static class WebhookPayloadJsonExtensionsJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false
	};

	/// <summary>
	/// Serializes a <see cref="WebhookPayload"/> instance to a JSON string.
	/// </summary>
	/// <param name="value">The payload to serialize. Cannot be <see langword="null"/>.</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
	/// <returns>The JSON string representation of the payload.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
	public static string ToJson(this WebhookPayload value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		return JsonSerializer.Serialize(
			value,
			indented
				? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
				: _jsonSerializerOptions);
	}

	/// <summary>
	/// Deserializes a JSON string to a <see cref="WebhookPayload"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize. Cannot be <see langword="null"/> or whitespace-only.</param>
	/// <returns>
	/// The deserialized <see cref="WebhookPayload"/> instance if successful; otherwise, <see langword="null"/>.
	/// </returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
	public static WebhookPayload? FromJson(string json)
	{
		ArgumentNullException.ThrowIfNull(json);

		if (string.IsNullOrWhiteSpace(json))
		{
			return null;
		}

		try
		{
			return JsonSerializer.Deserialize<WebhookPayload>(json, _jsonSerializerOptions);
		}
		catch (JsonException)
		{
			return null;
		}
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to a <see cref="WebhookPayload"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize. Cannot be <see langword="null"/> or whitespace-only.</param>
	/// <param name="value">
	/// When this method returns, contains the deserialized <see cref="WebhookPayload"/> if successful,
	/// or <see langword="null"/> if deserialization failed.
	/// </param>
	/// <returns><see langword="true"/> if the JSON was successfully deserialized; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
	public static bool TryFromJson(string json, out WebhookPayload? value)
	{
		ArgumentNullException.ThrowIfNull(json);

		if (string.IsNullOrWhiteSpace(json))
		{
			value = null;
			return false;
		}

		try
		{
			value = JsonSerializer.Deserialize<WebhookPayload>(json, _jsonSerializerOptions);
			return value is not null;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}
}
