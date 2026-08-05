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

public class BatchNotificationValidationTests
{
    private static DeploymentNotification CreateValidNotification()
    {
        return new DeploymentNotification
        {
            ProjectName = "SampleProject",
            Version = "1.0.0",
            BranchName = "main",
            Channels = new List<NotificationChannel> { NotificationChannel.Slack },
        };
    }

    private static BatchNotification CreateValidBatch()
    {
        return new BatchNotification
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Nightly batch",
            Notifications = new List<DeploymentNotification> { CreateValidNotification() },
            Channels = new List<NotificationChannel> { NotificationChannel.Slack },
            CreatedAt = DateTime.UtcNow,
        };
    }

    [Fact]
    public void Validate_Null_ThrowsArgumentNullException()
    {
        Action act = () => ((BatchNotification)null!).Validate();
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Validate_ValidBatch_ReturnsEmptyList()
    {
        var batch = CreateValidBatch();
        var result = batch.Validate();
        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_InvalidId_ReturnsErrorForNonGuid()
    {
        var batch = CreateValidBatch();
        batch.Id = "not-a-guid";

        var result = batch.Validate();

        result.Should().Contain(e => e.Contains("Id") && e.Contains("valid GUID"));
    }

    [Fact]
    public void Validate_EmptyNotificationsAndChannels_ReturnsErrors()
    {
        var batch = CreateValidBatch();
        batch.Notifications = new List<DeploymentNotification>();
        batch.Channels = new List<NotificationChannel>();

        var result = batch.Validate();

        result.Should().Contain(e => e.Contains(nameof(BatchNotification.Notifications)) && e.Contains("empty"));
        result.Should().Contain(e => e.Contains(nameof(BatchNotification.Channels)) && e.Contains("empty"));
    }

    [Fact]
    public void Validate_SentAtBeforeCreatedAtOrPendingStatus_ReturnsErrors()
    {
        var batch = CreateValidBatch();
        batch.CreatedAt = DateTime.UtcNow;
        batch.SentAt = batch.CreatedAt.AddMinutes(-10);
        batch.Status = BatchStatus.Pending;

        var result = batch.Validate();

        result.Should().Contain(e => e.Contains(nameof(BatchNotification.SentAt)) && e.Contains("earlier"));
        result.Should().Contain(e => e.Contains(nameof(BatchNotification.Status)) && e.Contains("Pending"));
    }

    [Fact]
    public void Validate_DeliveryStatisticsExceedTotal_ReturnsError()
    {
        var batch = CreateValidBatch();
        batch.TotalDeliveryAttempts = 1;
        batch.SuccessfulDeliveries = 1;
        batch.FailedDeliveries = 1;

        var result = batch.Validate();

        result.Should().Contain(e => e.Contains("cannot exceed TotalDeliveryAttempts"));
    }

    [Fact]
    public void IsValid_ValidBatch_ReturnsTrue()
    {
        var batch = CreateValidBatch();
        batch.IsValid().Should().BeTrue();
    }

    [Fact]
    public void EnsureValid_InvalidBatch_ThrowsArgumentExceptionWithDetails()
    {
        var batch = CreateValidBatch();
        batch.Name = string.Empty;

        batch.Invoking(b => b.EnsureValid())
            .Should().Throw<ArgumentException>()
            .WithMessage("*Name*");
    }
}
