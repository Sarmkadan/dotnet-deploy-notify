// Copyright (c) 2024
// SPDX-License-Identifier: MIT

using System;
using DotNetDeployNotify.Events;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class DomainEventExtensionsTests
{
    [Fact]
    public void IsSuccess_ProcessedEventWithSuccessFalse_ReturnsFalse()
    {
        var @event = new NotificationProcessedEvent { Success = false };

        Assert.False(@event.IsSuccess());
    }

    [Fact]
    public void IsSuccess_NonProcessedEvent_ReturnsTrue()
    {
        var @event = new NotificationCreatedEvent();

        Assert.True(@event.IsSuccess());
    }

    [Fact]
    public void IsSuccess_NullEvent_ThrowsArgumentNullException()
    {
        DomainEvent? @event = null;

        Assert.Throws<ArgumentNullException>(() => @event!.IsSuccess());
    }

    [Fact]
    public void GetChannels_CreatedEventWithChannels_ReturnsChannels()
    {
        var @event = new NotificationCreatedEvent { Channels = { "email", "slack" } };

        var channels = @event.GetChannels();

        Assert.Equal(new[] { "email", "slack" }, channels);
    }

    [Fact]
    public void GetChannels_EventWithoutChannelsSupport_ReturnsEmptyList()
    {
        var @event = new ChannelDeliveryFailedEvent();

        var channels = @event.GetChannels();

        Assert.Empty(channels);
    }

    [Fact]
    public void FormatForLog_WithoutDetails_ReturnsToString()
    {
        var @event = new NotificationCreatedEvent { NotificationId = "n1" };

        var result = @event.FormatForLog();

        Assert.Equal(@event.ToString(), result);
    }

    [Fact]
    public void FormatForLog_WithDetailsAndError_IncludesErrorMessage()
    {
        var @event = new NotificationProcessedEvent
        {
            NotificationId = "n1",
            Success = false,
            Error = "boom",
        };

        var result = @event.FormatForLog(includeDetails: true);

        Assert.Contains("Notification: n1", result);
        Assert.Contains("Success: False", result);
        Assert.Contains("Error: boom", result);
    }

    [Fact]
    public void OccurredBetween_EventWithinWindow_ReturnsTrue()
    {
        var @event = new NotificationCreatedEvent();

        var result = @event.OccurredBetween(@event.OccurredAt.AddMinutes(-1), @event.OccurredAt.AddMinutes(1));

        Assert.True(result);
    }

    [Fact]
    public void OccurredBetween_StartAfterEnd_ThrowsArgumentOutOfRangeException()
    {
        var @event = new NotificationCreatedEvent();
        var start = DateTime.UtcNow;
        var end = start.AddMinutes(-1);

        Assert.Throws<ArgumentOutOfRangeException>(() => @event.OccurredBetween(start, end));
    }

    [Fact]
    public void GetErrorMessage_ChannelDeliveryFailedEvent_ReturnsErrorMessage()
    {
        var @event = new ChannelDeliveryFailedEvent { ErrorMessage = "timeout" };

        Assert.Equal("timeout", @event.GetErrorMessage());
    }

    [Fact]
    public void GetErrorMessage_ProcessedEventWithNoError_ReturnsNull()
    {
        var @event = new NotificationProcessedEvent { Success = true, Error = null };

        Assert.Null(@event.GetErrorMessage());
    }

    [Fact]
    public void HasChannels_CreatedEventWithNoChannels_ReturnsFalse()
    {
        var @event = new NotificationCreatedEvent();

        Assert.False(@event.HasChannels());
    }

    [Fact]
    public void HasChannels_ProcessedEventWithChannels_ReturnsTrue()
    {
        var @event = new NotificationProcessedEvent { Channels = { "email" } };

        Assert.True(@event.HasChannels());
    }

    [Fact]
    public void GetNotificationId_ChannelDeliveryFailedEvent_ReturnsId()
    {
        var @event = new ChannelDeliveryFailedEvent { NotificationId = "n42" };

        Assert.Equal("n42", @event.GetNotificationId());
    }
}
