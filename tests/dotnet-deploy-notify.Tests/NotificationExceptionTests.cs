#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Exceptions;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Tests for the notification exception hierarchy: constructors, properties and ToString output.
/// </summary>
public class NotificationExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_SetsMessageAndLeavesInnerExceptionNull()
    {
        var exception = new NotificationException("delivery failed");

        exception.Message.Should().Be("delivery failed");
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithInnerException_PreservesMessageAndInnerException()
    {
        var inner = new InvalidOperationException("socket closed");

        var exception = new NotificationException("delivery failed", inner);

        exception.Message.Should().Be("delivery failed");
        exception.InnerException.Should().BeSameAs(inner);
    }

    [Fact]
    public void Constructor_WithNullMessage_FallsBackToDefaultExceptionMessage()
    {
        var exception = new NotificationException(null!);

        exception.Message.Should().Contain("NotificationException");
    }

    [Fact]
    public void ChannelConfigurationException_MessageOnlyConstructor_LeavesChannelAndConfigIdNull()
    {
        var exception = new ChannelConfigurationException("missing webhook url");

        exception.Message.Should().Be("missing webhook url");
        exception.ChannelType.Should().BeNull();
        exception.ConfigurationId.Should().BeNull();
        exception.Should().BeAssignableTo<NotificationException>();
    }

    [Fact]
    public void ChannelConfigurationException_DetailedConstructor_SetsChannelAndConfigurationId()
    {
        var exception = new ChannelConfigurationException("invalid slack config", NotificationChannel.Slack, "cfg-42");

        exception.ChannelType.Should().Be(NotificationChannel.Slack);
        exception.ConfigurationId.Should().Be("cfg-42");
        exception.ToString().Should().Be(
            "ChannelConfigurationException { ChannelType = Slack, ConfigurationId = cfg-42 }");
    }

    [Fact]
    public void WebhookDeliveryException_Constructor_SetsChannelAttemptsAndStatusCode()
    {
        var exception = new WebhookDeliveryException("gave up", NotificationChannel.Discord, 3, 503);

        exception.Message.Should().Be("gave up");
        exception.Channel.Should().Be(NotificationChannel.Discord);
        exception.Attempts.Should().Be(3);
        exception.LastStatusCode.Should().Be(503);
        exception.Should().BeAssignableTo<NotificationException>();
    }

    [Fact]
    public void WebhookDeliveryException_NullStatusCodeAndZeroAttempts_AreStoredAsGiven()
    {
        var exception = new WebhookDeliveryException("no response", NotificationChannel.Webhook, 0, null);

        exception.Attempts.Should().Be(0);
        exception.LastStatusCode.Should().BeNull();
        exception.ToString().Should().Be(
            "WebhookDeliveryException { Channel = Webhook, Attempts = 0, LastStatusCode =  }");
    }
}
