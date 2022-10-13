#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace DotNetDeployNotify.Events;

/// <summary>
/// Provides validation helpers for domain events to ensure data integrity
/// </summary>
public static class DomainEventValidation
{
    /// <summary>
    /// Validates a domain event and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The domain event to validate</param>
    /// <returns>An empty list if valid, otherwise a list of human-readable error messages</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this DomainEvent? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate EventId
        if (string.IsNullOrWhiteSpace(value.EventId))
        {
            errors.Add($"EventId cannot be null, empty, or whitespace.");
        }
        else if (!Guid.TryParse(value.EventId, out _))
        {
            errors.Add($"EventId '{value.EventId}' is not a valid GUID.");
        }

        // Validate OccurredAt
        if (value.OccurredAt == default)
        {
            errors.Add("OccurredAt cannot be the default DateTime value.");
        }
        else if (value.OccurredAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("OccurredAt cannot be in the future (more than 5 minutes ahead).");
        }
        else if (value.OccurredAt < DateTime.UtcNow.AddYears(-1))
        {
            errors.Add("OccurredAt cannot be more than one year in the past.");
        }

        // Validate AggregateId
        if (string.IsNullOrWhiteSpace(value.AggregateId))
        {
            errors.Add("AggregateId cannot be null, empty, or whitespace.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates a NotificationCreatedEvent and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The notification created event to validate</param>
    /// <returns>An empty list if valid, otherwise a list of human-readable error messages</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this NotificationCreatedEvent? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate base DomainEvent properties
        errors.AddRange(((DomainEvent)value).Validate());

        // Validate NotificationCreatedEvent specific properties
        if (string.IsNullOrWhiteSpace(value.NotificationId))
        {
            errors.Add("NotificationId cannot be null, empty, or whitespace.");
        }
        else if (!Guid.TryParse(value.NotificationId, out _))
        {
            errors.Add($"NotificationId '{value.NotificationId}' is not a valid GUID.");
        }

        if (string.IsNullOrWhiteSpace(value.ProjectName))
        {
            errors.Add("ProjectName cannot be null, empty, or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(value.Version))
        {
            errors.Add("Version cannot be null, empty, or whitespace.");
        }

        if (value.Channels is null)
        {
            errors.Add("Channels cannot be null.");
        }
        else if (value.Channels.Count == 0)
        {
            errors.Add("Channels cannot be empty.");
        }
        else
        {
            for (var i = 0; i < value.Channels.Count; i++)
            {
                var channel = value.Channels[i];
                if (string.IsNullOrWhiteSpace(channel))
                {
                    errors.Add($"Channels[{i}] cannot be null, empty, or whitespace.");
                }
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates a NotificationProcessedEvent and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The notification processed event to validate</param>
    /// <returns>An empty list if valid, otherwise a list of human-readable error messages</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this NotificationProcessedEvent? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate base DomainEvent properties
        errors.AddRange(((DomainEvent)value).Validate());

        // Validate NotificationProcessedEvent specific properties
        if (string.IsNullOrWhiteSpace(value.NotificationId))
        {
            errors.Add("NotificationId cannot be null, empty, or whitespace.");
        }
        else if (!Guid.TryParse(value.NotificationId, out _))
        {
            errors.Add($"NotificationId '{value.NotificationId}' is not a valid GUID.");
        }

        if (value.Channels is null)
        {
            errors.Add("Channels cannot be null.");
        }
        else
        {
            for (var i = 0; i < value.Channels.Count; i++)
            {
                var channel = value.Channels[i];
                if (string.IsNullOrWhiteSpace(channel))
                {
                    errors.Add($"Channels[{i}] cannot be null, empty, or whitespace.");
                }
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates a ChannelDeliveryFailedEvent and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The channel delivery failed event to validate</param>
    /// <returns>An empty list if valid, otherwise a list of human-readable error messages</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this ChannelDeliveryFailedEvent? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate base DomainEvent properties
        errors.AddRange(((DomainEvent)value).Validate());

        // Validate ChannelDeliveryFailedEvent specific properties
        if (string.IsNullOrWhiteSpace(value.NotificationId))
        {
            errors.Add("NotificationId cannot be null, empty, or whitespace.");
        }
        else if (!Guid.TryParse(value.NotificationId, out _))
        {
            errors.Add($"NotificationId '{value.NotificationId}' is not a valid GUID.");
        }

        if (string.IsNullOrWhiteSpace(value.ChannelName))
        {
            errors.Add("ChannelName cannot be null, empty, or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(value.ErrorMessage))
        {
            errors.Add("ErrorMessage cannot be null, empty, or whitespace.");
        }

        if (value.AttemptNumber <= 0)
        {
            errors.Add("AttemptNumber must be a positive integer.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the domain event is valid.
    /// </summary>
    /// <param name="value">The domain event to check</param>
    /// <returns>True if valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static bool IsValid(this DomainEvent? value) => value?.Validate().Count == 0;

    /// <summary>
    /// Determines whether the NotificationCreatedEvent is valid.
    /// </summary>
    /// <param name="value">The notification created event to check</param>
    /// <returns>True if valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static bool IsValid(this NotificationCreatedEvent? value) => value?.Validate().Count == 0;

    /// <summary>
    /// Determines whether the NotificationProcessedEvent is valid.
    /// </summary>
    /// <param name="value">The notification processed event to check</param>
    /// <returns>True if valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static bool IsValid(this NotificationProcessedEvent? value) => value?.Validate().Count == 0;

    /// <summary>
    /// Determines whether the ChannelDeliveryFailedEvent is valid.
    /// </summary>
    /// <param name="value">The channel delivery failed event to check</param>
    /// <returns>True if valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static bool IsValid(this ChannelDeliveryFailedEvent? value) => value?.Validate().Count == 0;

    /// <summary>
    /// Validates the domain event and throws an ArgumentException if invalid.
    /// </summary>
    /// <param name="value">The domain event to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when the event is invalid with a detailed error message</exception>
    public static void EnsureValid(this DomainEvent? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"DomainEvent is invalid. Validation errors: {string.Join(" ", errors)}");
        }
    }

    /// <summary>
    /// Validates the NotificationCreatedEvent and throws an ArgumentException if invalid.
    /// </summary>
    /// <param name="value">The notification created event to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when the event is invalid with a detailed error message</exception>
    public static void EnsureValid(this NotificationCreatedEvent? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"NotificationCreatedEvent is invalid. Validation errors: {string.Join(" ", errors)}");
        }
    }

    /// <summary>
    /// Validates the NotificationProcessedEvent and throws an ArgumentException if invalid.
    /// </summary>
    /// <param name="value">The notification processed event to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when the event is invalid with a detailed error message</exception>
    public static void EnsureValid(this NotificationProcessedEvent? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"NotificationProcessedEvent is invalid. Validation errors: {string.Join(" ", errors)}");
        }
    }

    /// <summary>
    /// Validates the ChannelDeliveryFailedEvent and throws an ArgumentException if invalid.
    /// </summary>
    /// <param name="value">The channel delivery failed event to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when the event is invalid with a detailed error message</exception>
    public static void EnsureValid(this ChannelDeliveryFailedEvent? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ChannelDeliveryFailedEvent is invalid. Validation errors: {string.Join(" ", errors)}");
        }
    }
}