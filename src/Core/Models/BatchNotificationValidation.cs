using System;
using System.Collections.Generic;
using System.Globalization;
using DotNetDeployNotify.Validation;

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
        var prefix = nameof(BatchNotification);

        // Validate Id
        ValidationGuard.CheckRequired(errors, value.Id, $"{prefix}.{nameof(value.Id)}");

        if (!string.IsNullOrWhiteSpace(value.Id) && !IsValidGuid(value.Id))
        {
            errors.Add($"{prefix}.{nameof(value.Id)} must be a valid GUID, but was '{value.Id}'.");
        }

        // Validate Name
        ValidationGuard.CheckRequired(errors, value.Name, $"{prefix}.{nameof(value.Name)}");
        ValidationGuard.CheckMaxLength(errors, value.Name, 256, $"{prefix}.{nameof(value.Name)}");

        // Validate Description
        ValidationGuard.CheckMaxLength(errors, value.Description, 2048, $"{prefix}.{nameof(value.Description)}");

        // Validate Notifications collection
        ValidationGuard.CheckNotNull(errors, value.Notifications, $"{prefix}.{nameof(value.Notifications)}");

        if (value.Notifications is { Count: 0 })
        {
            errors.Add($"{prefix}.{nameof(value.Notifications)} collection cannot be empty.");
        }
        else if (value.Notifications is { } notifications)
        {
            for (var i = 0; i < notifications.Count; i++)
            {
                var notification = notifications[i];
                if (notification is null)
                {
                    errors.Add($"{prefix}.{nameof(value.Notifications)}[{i}]: Notification cannot be null.");
                }
                else if (!notification.IsValid())
                {
                    errors.Add($"{prefix}.{nameof(value.Notifications)}[{i}]: Invalid notification.");
                }
            }
        }

        // Validate Channels collection
        ValidationGuard.CheckNotNull(errors, value.Channels, $"{prefix}.{nameof(value.Channels)}");

        if (value.Channels is { Count: 0 })
        {
            errors.Add($"{prefix}.{nameof(value.Channels)} collection cannot be empty.");
        }
        else if (value.Channels is { } channels)
        {
            foreach (var (index, channel) in channels.Index())
            {
                ValidationGuard.CheckDefinedEnum(errors, channel, $"{prefix}.{nameof(value.Channels)}[{index}]");
            }
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            errors.Add($"{prefix}.{nameof(value.CreatedAt)} cannot be the default DateTime value.");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add($"{prefix}.{nameof(value.CreatedAt)} cannot be in the future (was {value.CreatedAt:yyyy-MM-dd HH:mm:ss}).");
        }

        // Validate ScheduledAt
        if (value.ScheduledAt.HasValue)
        {
            if (value.ScheduledAt.Value == default)
            {
                errors.Add($"{prefix}.{nameof(value.ScheduledAt)} cannot be the default DateTime value when set.");
            }
            else if (value.ScheduledAt.Value < value.CreatedAt)
            {
                errors.Add($"{prefix}.{nameof(value.ScheduledAt)} cannot be earlier than {nameof(value.CreatedAt)}.");
            }
            else if (value.ScheduledAt.Value > DateTime.UtcNow.AddYears(1))
            {
                errors.Add($"{prefix}.{nameof(value.ScheduledAt)} cannot be more than 1 year in the future.");
            }
        }

        // Validate SentAt
        if (value.SentAt.HasValue)
        {
            if (value.SentAt.Value == default)
            {
                errors.Add($"{prefix}.{nameof(value.SentAt)} cannot be the default DateTime value when set.");
            }
            else if (value.SentAt.Value < value.CreatedAt)
            {
                errors.Add($"{prefix}.{nameof(value.SentAt)} cannot be earlier than {nameof(value.CreatedAt)}.");
            }
            else if (value.SentAt.Value > DateTime.UtcNow.AddMinutes(5))
            {
                errors.Add($"{prefix}.{nameof(value.SentAt)} cannot be more than 5 minutes in the future.");
            }

            // If SentAt is set, Status should not be Pending
            if (value.Status == BatchStatus.Pending)
            {
                errors.Add($"{prefix}.{nameof(value.Status)} cannot be Pending when {nameof(value.SentAt)} is set.");
            }
        }

        // Validate Status
        ValidationGuard.CheckDefinedEnum(errors, value.Status, $"{prefix}.{nameof(value.Status)}");

        // Validate delivery statistics
        if (value.TotalDeliveryAttempts < 0)
        {
            errors.Add($"{prefix}.{nameof(value.TotalDeliveryAttempts)} cannot be negative, but was {value.TotalDeliveryAttempts}.");
        }

        if (value.SuccessfulDeliveries < 0)
        {
            errors.Add($"{prefix}.{nameof(value.SuccessfulDeliveries)} cannot be negative, but was {value.SuccessfulDeliveries}.");
        }

        if (value.FailedDeliveries < 0)
        {
            errors.Add($"{prefix}.{nameof(value.FailedDeliveries)} cannot be negative, but was {value.FailedDeliveries}.");
        }

        // Validate delivery statistics consistency
        if (value.SuccessfulDeliveries + value.FailedDeliveries > value.TotalDeliveryAttempts)
        {
            errors.Add($"{prefix}: SuccessfulDeliveries + FailedDeliveries ({value.SuccessfulDeliveries} + {value.FailedDeliveries} = {value.SuccessfulDeliveries + value.FailedDeliveries}) cannot exceed TotalDeliveryAttempts ({value.TotalDeliveryAttempts}).");
        }

        // Validate Metadata collection
        ValidationGuard.CheckNotNull(errors, value.Metadata, $"{prefix}.{nameof(value.Metadata)}");

        if (value.Metadata is { } metadata)
        {
            foreach (var kvp in metadata)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    errors.Add($"{prefix}.{nameof(value.Metadata)}: Keys cannot be null or whitespace.");
                    break;
                }

                if (kvp.Value is null)
                {
                    errors.Add($"{prefix}.{nameof(value.Metadata)}['{kvp.Key}'] cannot be null.");
                }
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
    /// Ensures that the specified <see cref="BatchNotification"/> instance is valid, throwing a <see cref="ValidationFailedException"/> if it is not.
    /// </summary>
    /// <param name="value">The batch notification to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ValidationFailedException">Thrown if <paramref name="value"/> is not valid, containing a list of validation errors.</exception>
    public static void EnsureValid(this BatchNotification value)
    {
        ArgumentNullException.ThrowIfNull(value);

        ValidationGuard.ThrowIfAny(nameof(value), nameof(BatchNotification), value.Validate());
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
