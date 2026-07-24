#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

// Provides JSON serialization extensions for NotificationResult objects

using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetDeployNotify.Serialization;

namespace DotNetDeployNotify.Core.Models;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="NotificationResult"/> objects
/// </summary>
/// <remarks>
/// SECURITY: Uses SecureJsonSerializerOptions to ensure safe deserialization of untrusted input.
/// </remarks>
public static class NotificationResultJsonExtensions
{
    /// <summary>
    /// Converts a <see cref="NotificationResult"/> object to its JSON representation
    /// </summary>
    /// <param name="value">The notification result to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability</param>
    /// <returns>JSON string representation of the notification result</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static string ToJson(this NotificationResult value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        return JsonSerializationUtilities.Serialize(value, indented);
    }

    /// <summary>
    /// Parses a JSON string into a <see cref="NotificationResult"/> object
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>Deserialized notification result, or null if JSON is empty or whitespace</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null</exception>
    public static NotificationResult? FromJson(string json)
    {
        return JsonSerializationUtilities.SafeDeserialize<NotificationResult>(json, JsonSerializationUtilities.DefaultInternalOptions);
    }

    /// <summary>
    /// Attempts to parse a JSON string into a <see cref="NotificationResult"/> object
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized notification result if successful</param>
    /// <returns>True if deserialization succeeded; false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null</exception>
    public static bool TryFromJson(string json, out NotificationResult? value)
    {
        return JsonSerializationUtilities.TryDeserialize(json, JsonSerializationUtilities.DefaultInternalOptions, out value);
    }
}