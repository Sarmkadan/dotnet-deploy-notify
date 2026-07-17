using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetDeployNotify.Search;

namespace DotNetDeployNotify.Search;

/// <summary>
/// Provides System.Text.Json serialization helpers for <see cref="SearchCriteria"/>.
/// </summary>
public static class SearchCriteriaJsonExtensions
{
	/// <summary>
	/// Shared JSON serialization options with camelCase naming policy.
	/// </summary>
	private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false
	};

	/// <summary>
	/// Serializes a <see cref="SearchCriteria"/> instance to JSON.
	/// </summary>
	/// <param name="value">The instance to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation.</param>
	/// <returns>JSON string representation of the instance.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
	public static string ToJson(this SearchCriteria value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented ? new JsonSerializerOptions(JsonOptions) { WriteIndented = true } : JsonOptions;
		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a JSON string to a <see cref="SearchCriteria"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>A <see cref="SearchCriteria"/> instance, or null if deserialization failed.</returns>
	/// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or empty.</exception>
	public static SearchCriteria? FromJson(string json)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);
		return JsonSerializer.Deserialize<SearchCriteria>(json, JsonOptions);
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to a <see cref="SearchCriteria"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">The deserialized instance if successful; null otherwise.</param>
	/// <returns>True if deserialization succeeded; false otherwise.</returns>
	/// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or empty.</exception>
	public static bool TryFromJson(string json, out SearchCriteria? value)
	{
		ArgumentNullException.ThrowIfNull(json);
		value = JsonSerializer.Deserialize<SearchCriteria>(json, JsonOptions);
		return value is not null;
	}
}