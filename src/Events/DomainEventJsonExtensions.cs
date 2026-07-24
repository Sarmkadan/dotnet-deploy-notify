#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using DotNetDeployNotify.Serialization;

namespace DotNetDeployNotify.Events;

/// <summary>
/// Provides JSON serialization extensions for <see cref="DomainEvent"/> types using System.Text.Json
/// </summary>
/// <remarks>
/// SECURITY: This class handles polymorphic deserialization which can be dangerous.
/// The DomainEventTypeInfoResolver is a controlled, safe polymorphic resolver that
/// only allows known DomainEvent subtypes through explicit configuration.
/// </remarks>
public static class DomainEventJsonExtensions
{
    /// <summary>
    /// Custom type info resolver that handles polymorphic deserialization of <see cref="DomainEvent"/> types
    /// </summary>
    /// <remarks>
    /// SECURITY: This is a controlled polymorphic resolver that only allows known DomainEvent subtypes.
    /// It does NOT enable arbitrary type resolution from untrusted input.
    /// </remarks>
    private sealed class DomainEventTypeInfoResolver : DefaultJsonTypeInfoResolver
    {
        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            var jsonTypeInfo = base.GetTypeInfo(type, options);

            // Configure polymorphic deserialization for DomainEvent and all derived types
            // This is safe because it only allows known DomainEvent subtypes that are part of the application
            if (type == typeof(DomainEvent) || type.IsAssignableTo(typeof(DomainEvent)))
            {
                jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
                {
                    TypeDiscriminatorPropertyName = "$type"
                };
            }

            return jsonTypeInfo;
        }
    }

    /// <summary>
    /// Serializes a <see cref="DomainEvent"/> to a JSON string
    /// </summary>
    /// <param name="value">The domain event to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability</param>
    /// <returns>A JSON string representation of the domain event</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static string ToJson(this DomainEvent value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        return JsonSerializationUtilities.Serialize(value, indented);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="DomainEvent"/> instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>The deserialized domain event, or null if the JSON is empty or whitespace</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null</exception>
    public static DomainEvent? FromJson(string json)
    {
        return JsonSerializationUtilities.SafeDeserialize<DomainEvent>(json, JsonSerializationUtilities.DefaultInternalOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="DomainEvent"/> instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized domain event if successful</param>
    /// <returns>True if deserialization succeeded; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null</exception>
    public static bool TryFromJson(string json, out DomainEvent? value)
    {
        return JsonSerializationUtilities.TryDeserialize(json, JsonSerializationUtilities.DefaultInternalOptions, out value);
    }
}