#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================
/// <summary>
/// Tests for the notification functionality.
/// </summary>
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Services;
using FluentAssertions;
using Moq;
using Xunit;
using Environment = DotNetDeployNotify.Core.Environment;

namespace DotNetDeployNotify.Tests;

public class NotificationTests
{
    /// <summary>
    /// Tests that a notification builder with all required fields builds a valid notification.
    /// </summary>
    [Fact]
    public void NotificationBuilder_WithAllRequiredFields_BuildsValidNotification()
    {
        // Arrange & Act
        var notification = new NotificationBuilder()
            .WithProject("ApiGateway", "3.1.0")
            .WithStatus(BuildStatus.Success, "All checks passed")
            .WithBranch("main", "abc1234", "v.zaiets")
            .WithChannels(NotificationChannel.Slack, NotificationChannel.Telegram)
            .WithEnvironment(Environment.Production)
            .Build();

        // Assert
        notification.ProjectName.Should().Be("ApiGateway");
        notification.Version.Should().Be("3.1.0");
        notification.BranchName.Should().Be("main");
        notification.CommitHash.Should().Be("abc1234");
        notification.Channels.Should().HaveCount(2);
        notification.Channels.Should().Contain(NotificationChannel.Slack);
        notification.Id.Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Tests that a notification builder as a failure sets the critical priority and failed status.
    /// </summary>
    [Fact]
    public void NotificationBuilder_AsFailure_SetsCriticalPriorityAndFailedStatus()
    {
        // Arrange & Act
        var notification = new NotificationBuilder()
            .WithProject("PaymentService", "1.5.0")
            .WithBranch("hotfix/payment-crash")
            .WithChannels(NotificationChannel.Telegram)
            .AsFailure()
            .Build();

        // Assert
        notification.Status.Should().Be(BuildStatus.Failed);
        notification.Priority.Should().Be(NotificationPriority.Critical);
    }

    /// <summary>
    /// Tests that a notification builder as a deployment success sets the high priority and correct status.
    /// </summary>
    [Fact]
    public void NotificationBuilder_AsDeploymentSuccess_SetsHighPriorityAndCorrectStatus()
    {
        // Arrange & Act
        var notification = new NotificationBuilder()
            .WithProject("CatalogService", "2.0.0")
            .WithBranch("release/2.0")
            .WithChannels(NotificationChannel.Discord)
            .AsDeploymentSuccess()
            .Build();

        // Assert
        notification.Status.Should().Be(BuildStatus.DeploymentSuccess);
        notification.Priority.Should().Be(NotificationPriority.High);
    }

    /// <summary>
    /// Tests that a notification builder build with a missing project name throws an InvalidOperationException.
    /// </summary>
    [Fact]
    public void NotificationBuilder_Build_WithMissingProjectName_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = new NotificationBuilder()
            .WithBranch("main")
            .WithChannels(NotificationChannel.Slack);

        // Act
        var act = () => builder.Build();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not valid*");
    }

    /// <summary>
    /// Tests that a notification builder build with no channels throws an InvalidOperationException.
    /// </summary>
    [Fact]
    public void NotificationBuilder_Build_WithNoChannels_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = new NotificationBuilder()
            .WithProject("MyService", "1.0.0")
            .WithBranch("main");

        // Act
        var act = () => builder.Build();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    /// <summary>
    /// Tests that a mocked IValidationService returns configured errors and is verifiable.
    /// </summary>
    [Fact]
    public void IValidationService_WhenMocked_ReturnsConfiguredErrorsAndIsVerifiable()
    {
        // Arrange
        var mockValidation = new Mock<IValidationService>();
        mockValidation
            .Setup(s => s.ValidateNotification(It.IsAny<DeploymentNotification>()))
            .Returns(ValidationResult.Failure("Project name is required", "Version is required"));

        var incompleteNotification = new DeploymentNotification();

        // Act
        var result = mockValidation.Object.ValidateNotification(incompleteNotification);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain("Project name is required");
        result.Errors.Should().Contain("Version is required");
        mockValidation.Verify(s => s.ValidateNotification(incompleteNotification), Times.Once);
    }

    /// <summary>
    /// Tests that a channel configuration should send a notification when the channel is disabled returns false.
    /// </summary>
    [Fact]
    public void ChannelConfiguration_ShouldSendNotification_WhenChannelDisabled_ReturnsFalse()
    {
        // Arrange
        var config = new ChannelConfiguration
        {
            IsEnabled = false,
            DisplayName = "Slack Production",
            WebhookUrl = "https://hooks.slack.com/services/test"
        };
        var notification = new DeploymentNotification
        {
            Priority = NotificationPriority.Critical,
            Status = BuildStatus.Failed
        };

        // Act
        var shouldSend = config.ShouldSendNotification(notification);

        // Assert
        shouldSend.Should().BeFalse();
    }

    /// <summary>
    /// Tests that a channel configuration should send a notification when the priority is below the minimum returns false.
    /// </summary>
    [Fact]
    public void ChannelConfiguration_ShouldSendNotification_WhenPriorityBelowMinimum_ReturnsFalse()
    {
        // Arrange
        var config = new ChannelConfiguration
        {
            IsEnabled = true,
            MinimumPriority = NotificationPriority.High
        };
        var notification = new DeploymentNotification
        {
            Priority = NotificationPriority.Low,
            Status = BuildStatus.Success
        };

        // Act
        var shouldSend = config.ShouldSendNotification(notification);

        // Assert
        shouldSend.Should().BeFalse();
    }

    /// <summary>
    /// Tests that a notification result marks as successful sets the delivered status and clears the error.
    /// </summary>
    [Fact]
    public void NotificationResult_MarkAsSuccessful_SetsDeliveredStatusAndClearsError()
    {
        // Arrange
        var result = new NotificationResult
        {
            NotificationId = "notif-001",
            ConfigurationId = "cfg-slack-prod",
            Channel = NotificationChannel.Slack,
            DurationMs = 142
        };

        // Act
        result.MarkAsSuccessful(200, "{\"ok\":true}");

        // Assert
        result.Status.Should().Be(DeliveryStatus.Delivered);
        result.HttpStatusCode.Should().Be(200);
        result.ResponseBody.Should().Be("{\"ok\":true}");
        result.IsSuccessful.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    /// <summary>
    /// Tests that a notification result marks as failed sets the failed status with error details.
    /// </summary>
    [Fact]
    public void NotificationResult_MarkAsFailed_SetsFailedStatusWithErrorDetails()
    {
        // Arrange
        var result = new NotificationResult
        {
            NotificationId = "notif-002",
            ConfigurationId = "cfg-telegram"
        };

        // Act
        result.MarkAsFailed("Connection refused", "HttpRequestException", 503);

        // Assert
        result.Status.Should().Be(DeliveryStatus.Failed);
        result.ErrorMessage.Should().Be("Connection refused");
        result.ExceptionType.Should().Be("HttpRequestException");
        result.HttpStatusCode.Should().Be(503);
        result.IsSuccessful.Should().BeFalse();
    }

    /// <summary>
    /// Tests that a deployment notification sets and gets metadata with round-tripped typed values.
    /// </summary>
    [Fact]
    public void DeploymentNotification_SetAndGetMetadata_RoundTripsTypedValues()
    {
        // Arrange
        var notification = new DeploymentNotification();

        // Act
        notification.SetMetadata("build_number", 42);
        notification.SetMetadata("triggered_by", "ci-pipeline");

        var buildNumber = notification.GetMetadata<int>("build_number");
        var triggeredBy = notification.GetMetadata<string>("triggered_by");
        var missing = notification.GetMetadata<string>("nonexistent_key");

        // Assert
        buildNumber.Should().Be(42);
        triggeredBy.Should().Be("ci-pipeline");
        missing.Should().BeNull();
    }

    /// <summary>
    /// Tests that a deployment notification increments the delivery attempt counter each call.
    /// </summary>
    [Fact]
    public void DeploymentNotification_IncrementDeliveryAttempt_IncrementsCounterEachCall()
    {
        // Arrange
        var notification = new DeploymentNotification();
        notification.DeliveryAttempts.Should().Be(0);

        // Act
        notification.IncrementDeliveryAttempt();
        notification.IncrementDeliveryAttempt();
        notification.IncrementDeliveryAttempt();

        // Assert
        notification.DeliveryAttempts.Should().Be(3);
    }

    /// <summary>
    /// Tests that a deployment notification marks as processed sets the is processed flag to true.
    /// </summary>
    [Fact]
    public void DeploymentNotification_MarkAsProcessed_SetsIsProcessedTrue()
    {
        // Arrange
        var notification = new DeploymentNotification();

        // Act
        notification.MarkAsProcessed();

        // Assert
        notification.IsProcessed.Should().BeTrue();
    }
}
