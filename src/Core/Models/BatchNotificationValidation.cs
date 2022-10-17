#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace DotNetDeployNotify.Core.Models;

/// <summary>
/// Provides validation helpers for <see cref="BatchNotification"/> instances
/// </summary>
public static class BatchNotificationValidation
{
    /// <summary>
    /// Validates the batch notification and returns a list of validation problems
    /// </summary>
    /// <param name="value">The batch notification to validate</param>
    /// <returns>An empty list if valid; otherwise, a list of human-readable problems</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this BatchNotification value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Id
        if (string.IsNullOrWhiteSpace(value.Id))
        {
            problems.Add("Id cannot be null or whitespace.");
        }
        else if (!IsValidGuid(value.Id))
        {
            problems.Add("Id must be a valid GUID.");
        }

        // Validate Name
        if (string.IsNullOrWhiteSpace(value.Name))
        {
            problems.Add("Name cannot be null or whitespace.");
        }
        else if (value.Name.Length > 256)
        {
            problems.Add("Name cannot exceed 256 characters.");
        }

        // Validate Description
        if (value.Description.Length > 2048)
        {
            problems.Add("Description cannot exceed 2048 characters.");
        }

        // Validate Notifications
        if (value.Notifications is null)
        {
            problems.Add("Notifications collection cannot be null.");
        }
        else if (value.Notifications.Count == 0)
        {
            problems.Add("Notifications collection cannot be empty.");
        }
        else
        {
            for (var i = 0; i < value.Notifications.Count; i++)
            {
                var notification = value.Notifications[i];
                if (notification is null)
                {
                    problems.Add($"Notifications[{i}]: Notification cannot be null.");
                }
                else if (!notification.IsValid())
                {
                    problems.Add($"Notifications[{i}]: Invalid notification.");
                }
            }
        }

        // Validate Channels
        if (value.Channels is null)
        {
            problems.Add("Channels collection cannot be null.");
        }
        else if (value.Channels.Count == 0)
        {
            problems.Add("Channels collection cannot be empty.");
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            problems.Add("CreatedAt cannot be the default DateTime value.");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("CreatedAt cannot be in the future.");
        }

        // Validate ScheduledAt
        if (value.ScheduledAt.HasValue)
        {
            if (value.ScheduledAt.Value == default)
            {
                problems.Add("ScheduledAt cannot be the default DateTime value when set.");
            }
            else if (value.ScheduledAt.Value < value.CreatedAt)
            {
                problems.Add("ScheduledAt cannot be earlier than CreatedAt.");
            }
            else if (value.ScheduledAt.Value > DateTime.UtcNow.AddYears(1))
            {
                problems.Add("ScheduledAt cannot be more than 1 year in the future.");
            }
        }

        // Validate SentAt
        if (value.SentAt.HasValue)
        {
            if (value.SentAt.Value == default)
            {
                problems.Add("SentAt cannot be the default DateTime value when set.");
            }
            else if (value.SentAt.Value < value.CreatedAt)
            {
                problems.Add("SentAt cannot be earlier than CreatedAt.");
            }
            else if (value.SentAt.Value > DateTime.UtcNow.AddMinutes(5))
            {
                problems.Add("SentAt cannot be more than 5 minutes in the future.");
            }

            // If SentAt is set, Status should not be Pending
            if (value.Status == BatchStatus.Pending)
            {
                problems.Add("Status cannot be Pending when SentAt is set.");
            }
        }

        // Validate Status
        if (!Enum.IsDefined(typeof(BatchStatus), value.Status))
        {
            problems.Add("Status must be a valid BatchStatus value.");
        }

        // Validate delivery statistics
        if (value.TotalDeliveryAttempts < 0)
        {
            problems.Add("TotalDeliveryAttempts cannot be negative.");
        }

        if (value.SuccessfulDeliveries < 0)
        {
            problems.Add("SuccessfulDeliveries cannot be negative.");
        }

        if (value.FailedDeliveries < 0)
        {
            problems.Add("FailedDeliveries cannot be negative.");
        }

        // Validate delivery statistics consistency
        if (value.SuccessfulDeliveries + value.FailedDeliveries > value.TotalDeliveryAttempts)
        {
            problems.Add("SuccessfulDeliveries + FailedDeliveries cannot exceed TotalDeliveryAttempts.");
        }

        // Validate Metadata
        if (value.Metadata is null)
        {
            problems.Add("Metadata collection cannot be null.");
        }
        else
        {
            foreach (var kvp in value.Metadata)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    problems.Add("Metadata keys cannot be null or whitespace.");
                    break;
                }

                if (kvp.Value is null)
                {
                    problems.Add($"Metadata['{kvp.Key}'] cannot be null.");
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the batch notification is valid
    /// </summary>
    /// <param name="value">The batch notification to check</param>
    /// <returns>True if valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static bool IsValid(this BatchNotification value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures the batch notification is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The batch notification to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when the batch notification is invalid, containing a list of problems</exception>
    public static void EnsureValid(this BatchNotification value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"BatchNotification is invalid. Problems: {string.Join(" ", problems)}");
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