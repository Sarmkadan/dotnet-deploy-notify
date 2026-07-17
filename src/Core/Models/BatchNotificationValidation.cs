using System;
using System.Collections.Generic;
using System.Globalization;

namespace DotNetDeployNotify.Core.Models;

/// <summary>
/// Provides validation helpers for <see cref="BatchNotification"/> instances
/// </summary>
public static class BatchNotificationValidation
{
    /// <summary>
    /// Validates the <see cref="BatchNotification"/> instance for common problems.
    /// </summary>
    /// <param name="value">The batch notification to validate.</param>
    /// <returns>A list of human-readable validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this BatchNotification value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (string.IsNullOrWhiteSpace(value.Id))
        {
            errors.Add($"{nameof(BatchNotification)}.{nameof(value.Id)} cannot be null or whitespace.");
        }
        else if (!IsValidGuid(value.Id))
        {
            errors.Add($"{nameof(BatchNotification)}.{nameof(value.Id)} must be a valid GUID, but was '{value.Id}'.");
        }

        // Validate Name
        if (string.IsNullOrWhiteSpace(value.Name))
        {
            errors.Add($"{nameof(BatchNotification)}.{nameof(value.Name)} cannot be null or whitespace.");
        }
        else if (value.Name.Length > 256)
        {
            errors.Add($"{nameof(BatchNotification)}.{nameof(value.Name)} cannot exceed 256 characters, but was {value.Name.Length}.");
        }

        // Validate Description
        if (value.Description?.Length > 2048)
        {
            errors.Add($"{nameof(BatchNotification)}.{nameof(value.Description)} cannot exceed 2048 characters, but was {value.Description?.Length}.");
        }

        // Validate Notifications collection
        ArgumentNullException.ThrowIfNull(value.Notifications);

        if (value.Notifications.Count == 0)
        {
            errors.Add($"{nameof(BatchNotification)}.{nameof(value.Notifications)} collection cannot be empty.");
        }
        else
        {
            for (var i = 0; i < value.Notifications.Count; i++)
            {
                var notification = value.Notifications[i];
                if (notification is null)
                {
                    errors.Add($"{nameof(BatchNotification)}.{nameof(value.Notifications)}[{i}]: Notification cannot be null.");
                }
                else if (!notification.IsValid())
                {
                    errors.Add($"{nameof(BatchNotification)}.{nameof(value.Notifications)}[{i}]: Invalid notification.");
                }
            }
        }

        // Validate Channels collection
        ArgumentNullException.ThrowIfNull(value.Channels);

        if (value.Channels.Count == 0)
        {
            errors.Add($"{nameof(BatchNotification)}.{nameof(value.Channels)} collection cannot be empty.");
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            errors.Add($"{nameof(BatchNotification)}.{nameof(value.CreatedAt)} cannot be the default DateTime value.");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add($"{nameof(BatchNotification)}.{nameof(value.CreatedAt)} cannot be in the future (was {value.CreatedAt:yyyy-MM-dd HH:mm:ss}).");
        }

        // Validate ScheduledAt
        if (value.ScheduledAt.HasValue)
        {
            if (value.ScheduledAt.Value == default)
            {
                errors.Add($"{nameof(BatchNotification)}.{nameof(value.ScheduledAt)} cannot be the default DateTime value when set.");
            }
            else if (value.ScheduledAt.Value < value.CreatedAt)
            {
                errors.Add($"{nameof(BatchNotification)}.{nameof(value.ScheduledAt)} cannot be earlier than {nameof(value.CreatedAt)}.");
            }
            else if (value.ScheduledAt.Value > DateTime.UtcNow.AddYears(1))
            {
                errors.Add($"{nameof(BatchNotification)}.{nameof(value.ScheduledAt)} cannot be more than 1 year in the future.");
            }
        }

        // Validate SentAt
        if (value.SentAt.HasValue)
        {
            if (value.SentAt.Value == default)
            {
                errors.Add($"{nameof(BatchNotification)}.{nameof(value.SentAt)} cannot be the default DateTime value when set.");
            }
            else if (value.SentAt.Value < value.CreatedAt)
            {
                errors.Add($"{nameof(BatchNotification)}.{nameof(value.SentAt)} cannot be earlier than {nameof(value.CreatedAt)}.");
            }
            else if (value.SentAt.Value > DateTime.UtcNow.AddMinutes(5))
            {
                errors.Add($"{nameof(BatchNotification)}.{nameof(value.SentAt)} cannot be more than 5 minutes in the future.");
            }

            // If SentAt is set, Status should not be Pending
            if (value.Status == BatchStatus.Pending)
            {
                errors.Add($"{nameof(BatchNotification)}.{nameof(value.Status)} cannot be Pending when {nameof(value.SentAt)} is set.");
            }
        }

        // Validate Status
        if (!Enum.IsDefined(typeof(BatchStatus), value.Status))
        {
            errors.Add($"{nameof(BatchNotification)}.{nameof(value.Status)} must be a valid BatchStatus value, but was {(int)value.Status}.");
        }

        // Validate delivery statistics
        if (value.TotalDeliveryAttempts < 0)
        {
            errors.Add($"{nameof(BatchNotification)}.{nameof(value.TotalDeliveryAttempts)} cannot be negative, but was {value.TotalDeliveryAttempts}.");
        }

        if (value.SuccessfulDeliveries < 0)
        {
            errors.Add($"{nameof(BatchNotification)}.{nameof(value.SuccessfulDeliveries)} cannot be negative, but was {value.SuccessfulDeliveries}.");
        }

        if (value.FailedDeliveries < 0)
        {
            errors.Add($"{nameof(BatchNotification)}.{nameof(value.FailedDeliveries)} cannot be negative, but was {value.FailedDeliveries}.");
        }

        // Validate delivery statistics consistency
        if (value.SuccessfulDeliveries + value.FailedDeliveries > value.TotalDeliveryAttempts)
        {
            errors.Add($"{nameof(BatchNotification)}: SuccessfulDeliveries + FailedDeliveries ({value.SuccessfulDeliveries} + {value.FailedDeliveries} = {value.SuccessfulDeliveries + value.FailedDeliveries}) cannot exceed TotalDeliveryAttempts ({value.TotalDeliveryAttempts}).");
        }

        // Validate Metadata collection
        ArgumentNullException.ThrowIfNull(value.Metadata);

        foreach (var kvp in value.Metadata)
        {
            if (string.IsNullOrWhiteSpace(kvp.Key))
            {
                errors.Add($"{nameof(BatchNotification)}.{nameof(value.Metadata)}: Keys cannot be null or whitespace.");
                break;
            }

            if (kvp.Value is null)
            {
                errors.Add($"{nameof(BatchNotification)}.{nameof(value.Metadata)}['{kvp.Key}'] cannot be null.");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="BatchNotification"/> instance is valid.
    /// </summary>
    /// <param name="value">The batch notification to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this BatchNotification value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="BatchNotification"/> instance is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The batch notification to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid, containing a list of validation errors.</exception>
    public static void EnsureValid(this BatchNotification value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"BatchNotification is invalid. Problems:\n{string.Join("\n", errors)}");
        }
    }

    /// <summary>
    /// Checks if a string is a valid GUID
    /// </summary>
    /// <param name="value">The string to check</param>
    /// <returns>True if valid GUID; otherwise, false</returns>
    private static bool IsValidGuid(string value)
    {
        return Guid.TryParse(value, out _);
    }
}
