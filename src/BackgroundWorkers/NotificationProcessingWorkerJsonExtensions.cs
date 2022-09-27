#nullable enable
using System;
using System.Text.Json;
using DotNetDeployNotify.BackgroundWorkers;

namespace DotNetDeployNotify.BackgroundWorkers;

/// <summary>
/// Provides JSON (de)serialization extensions for <see cref="NotificationProcessingWorker"/>.
/// </summary>
public static class NotificationProcessingWorkerJsonExtensions
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serializes the <see cref="NotificationProcessingWorker"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The worker instance to serialize.</param>
    /// <param name="indented">If <c>true</c>, the output JSON will be formatted with indentation.</param>
    /// <returns>A JSON representation of <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this NotificationProcessingWorker value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        var opts = new JsonSerializerOptions(Options) { WriteIndented = indented };
        return JsonSerializer.Serialize(value, opts);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="NotificationProcessingWorker"/> instance.
    /// </summary>
    /// <param name="json">The JSON string representing a <see cref="NotificationProcessingWorker"/>.</param>
    /// <returns>The deserialized <see cref="NotificationProcessingWorker"/>, or <c>null</c> if the JSON is empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <c>null</c>.</exception>
    /// <exception cref="JsonException">The JSON is invalid or cannot be deserialized into a <see cref="NotificationProcessingWorker"/>.</exception>
    public static NotificationProcessingWorker? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return JsonSerializer.Deserialize<NotificationProcessingWorker>(json, Options);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="NotificationProcessingWorker"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">When this method returns, contains the deserialized <see cref="NotificationProcessingWorker"/> if the operation succeeded; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <c>null</c>.</exception>
    public static bool TryFromJson(string json, out NotificationProcessingWorker? value)
    {
        ArgumentNullException.ThrowIfNull(json);
        try
        {
            value = FromJson(json);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
