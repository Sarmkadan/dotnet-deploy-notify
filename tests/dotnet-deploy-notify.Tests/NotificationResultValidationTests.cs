#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class NotificationResultValidationTests
{
    private static NotificationResult CreateValidResult()
    {
        return new NotificationResult
        {
            NotificationId = "notification-1",
            ConfigurationId = "config-1",
            ResponseBody = "OK",
            Channel = NotificationChannel.Slack,
            Status = DeliveryStatus.Delivered,
            AttemptNumber = 1,
            DurationMs = 100,
            AttemptedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public void Validate_WithValidResult_ReturnsEmptyList()
    {
        var result = CreateValidResult();

        var problems = result.Validate();

        problems.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithInvalidResult_ReturnsProblems()
    {
        var result = new NotificationResult(); // All defaults, should be invalid

        var problems = result.Validate();

        problems.Should().NotBeEmpty();
        problems.Should().Contain(p => p.Contains("NotificationId"));
        problems.Should().Contain(p => p.Contains("ConfigurationId"));
        problems.Should().Contain(p => p.Contains("ResponseBody"));
        problems.Should().Contain(p => p.Contains("Channel"));
        problems.Should().Contain(p => p.Contains("Status"));
    }

    [Fact]
    public void IsValid_WithValidResult_ReturnsTrue()
    {
        var result = CreateValidResult();

        result.IsValid().Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithInvalidResult_ReturnsFalse()
    {
        var result = new NotificationResult();

        result.IsValid().Should().BeFalse();
    }

    [Fact]
    public void EnsureValid_WithValidResult_DoesNotThrow()
    {
        var result = CreateValidResult();

        var act = () => result.EnsureValid();

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureValid_WithInvalidResult_ThrowsArgumentException()
    {
        var result = new NotificationResult();

        var act = () => result.EnsureValid();

        act.Should().Throw<ArgumentException>()
            .WithMessage("*NotificationResult is invalid*");
    }

    [Fact]
    public void EnsureValid_WithNullResult_ThrowsArgumentNullException()
    {
        NotificationResult? result = null;

        var act = () => result!.EnsureValid();

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Validate_WithInvalidHttpStatusCode_ReturnsProblem()
    {
        var result = CreateValidResult();
        result.HttpStatusCode = 99; // Invalid

        var problems = result.Validate();

        problems.Should().Contain("HttpStatusCode must be a valid HTTP status code (100-599)");
    }
}
