#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;

namespace DotNetDeployNotify.Events;

/// <summary>
/// Extension methods for <see cref="DomainEvent"/> providing common operations and utilities.
/// </summary>
public static class DomainEventExtensions
{
    /// <summary>
    /// Determines whether the event represents a successful operation.
    /// </summary>
    /// <param name="event">The domain event to check.</param>
    /// <returns>True if the event indicates success; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null.</exception>
    public static bool IsSuccess(this DomainEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return @event switch
        {
            NotificationProcessedEvent processed => processed.Success,
            _ => true
        };
    }

    /// <summary>
    /// Gets the channel names associated with the event.
    /// </summary>
    /// <param name="event">The domain event.</param>
    /// <returns>An immutable list of channel names, or empty list if none.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null.</exception>
    public static IReadOnlyList<string> GetChannels(this DomainEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return @event switch
        {
            NotificationCreatedEvent created => created.Channels.AsReadOnly(),
            NotificationProcessedEvent processed => processed.Channels.AsReadOnly(),
            _ => Array.Empty<string>().AsReadOnly()
        };
    }

    /// <summary>
    /// Formats the event for logging purposes with a consistent format.
    /// </summary>
    /// <param name="event">The domain event to format.</param>
    /// <param name="includeDetails">Whether to include detailed properties in the output.</param>
    /// <returns>A formatted string representation of the event.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null.</exception>
    public static string FormatForLog(this DomainEvent @event, bool includeDetails = false)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return !includeDetails
            ? @event.ToString()
            : FormatForLogWithDetails(@event);
    }

    private static string FormatForLogWithDetails(DomainEvent @event)
    {
        var baseInfo = @event.ToString();
        var details = new List<string> { baseInfo };

        switch (@event)
        {
            case NotificationCreatedEvent created:
                details.Add($"Notification: {created.NotificationId}");
                details.Add($"Project: {created.ProjectName}");
                details.Add($"Version: {created.Version}");
                details.Add($"Channels: {created.Channels.Count}");
                break;

            case NotificationProcessedEvent processed:
                details.Add($"Notification: {processed.NotificationId}");
                details.Add($"Success: {processed.Success}");
                details.Add($"Channels: {processed.Channels.Count}");
                if (processed.Error is not null)
                {
                    details.Add($"Error: {processed.Error}");
                }
                break;

            case ChannelDeliveryFailedEvent failed:
                details.Add($"Notification: {failed.NotificationId}");
                details.Add($"Channel: {failed.ChannelName}");
                details.Add($"Attempt: {failed.AttemptNumber}");
                details.Add($"Error: {failed.ErrorMessage}");
                break;
        }

        return string.Join(" | ", details);
    }

    /// <summary>
    /// Determines whether this event occurred within the specified time window.
    /// </summary>
    /// <param name="event">The domain event to check.</param>
    /// <param name="startUtc">The start of the time window (inclusive).</param>
    /// <param name="endUtc">The end of the time window (inclusive).</param>
    /// <returns>True if the event occurred within the window; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="startUtc"/> is after <paramref name="endUtc"/>.</exception>
    public static bool OccurredBetween(
        this DomainEvent @event,
        DateTime startUtc,
        DateTime endUtc)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(startUtc, endUtc);

        return @event.OccurredAt >= startUtc && @event.OccurredAt <= endUtc;
    }

    /// <summary>
    /// Gets the error message from the event if available.
    /// </summary>
    /// <param name="event">The domain event.</param>
    /// <returns>The error message if present; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null.</exception>
    public static string? GetErrorMessage(this DomainEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return @event switch
        {
            NotificationProcessedEvent processed when processed.Error is not null => processed.Error,
            ChannelDeliveryFailedEvent failed => failed.ErrorMessage,
            _ => null
        };
    }

    /// <summary>
    /// Determines whether the event has any associated channels.
    /// </summary>
    /// <param name="event">The domain event.</param>
    /// <returns>True if the event has one or more channels; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null.</exception>
    public static bool HasChannels(this DomainEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return @event switch
        {
            NotificationCreatedEvent created => created.Channels.Count > 0,
            NotificationProcessedEvent processed => processed.Channels.Count > 0,
            _ => false
        };
    }

    /// <summary>
    /// Gets the notification identifier from the event if available.
    /// </summary>
    /// <param name="event">The domain event.</param>
    /// <returns>The notification identifier if present; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null.</exception>
    public static string? GetNotificationId(this DomainEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return @event switch
        {
            NotificationCreatedEvent created => created.NotificationId,
            NotificationProcessedEvent processed => processed.NotificationId,
            ChannelDeliveryFailedEvent failed => failed.NotificationId,
            _ => null
        };
    }
}
