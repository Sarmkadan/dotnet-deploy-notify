using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Exceptions;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Data;
using DotNetDeployNotify.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Tests for the NotificationService class.
/// </summary>
public class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _notificationRepositoryMock = new();
    private readonly Mock<IChannelConfigRepository> _configRepositoryMock = new();
    private readonly Mock<INotificationResultRepository> _resultRepositoryMock = new();
    private readonly Mock<IWebhookDispatcher> _dispatcherMock = new();
    private readonly Mock<IValidationService> _validationServiceMock = new();
    private readonly Mock<ILogger<NotificationService>> _loggerMock = new();

    private readonly NotificationService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationServiceTests"/> class.
    /// </summary>
    public NotificationServiceTests()
    {
        _service = new NotificationService(
            _notificationRepositoryMock.Object,
            _configRepositoryMock.Object,
            _resultRepositoryMock.Object,
            _dispatcherMock.Object,
            _validationServiceMock.Object,
            _loggerMock.Object);
    }

    /// <summary>
    /// Tests that CreateNotificationAsync returns the ID of the created notification when the notification is valid.
    /// </summary>
    [Fact]
    public async Task CreateNotificationAsync_ShouldReturnId_WhenValid()
    {
        // Arrange
        var notification = new DeploymentNotification { Id = "test-id", ProjectName = "test-project" };
        _validationServiceMock.Setup(v => v.ValidateNotification(notification))
            .Returns(new ValidationResult { IsValid = true });

        // Act
        var result = await _service.CreateNotificationAsync(notification);

        // Assert
        result.Should().Be("test-id");
        _notificationRepositoryMock.Verify(r => r.CreateAsync(notification), Times.Once);
    }

    /// <summary>
    /// Tests that CreateNotificationAsync throws a NotificationValidationException when the notification is invalid.
    /// </summary>
    [Fact]
    public async Task CreateNotificationAsync_ShouldThrowException_WhenInvalid()
    {
        // Arrange
        var notification = new DeploymentNotification();
        _validationServiceMock.Setup(v => v.ValidateNotification(notification))
            .Returns(new ValidationResult { IsValid = false, Errors = new List<string> { "error" } });

        // Act
        Func<Task> act = async () => await _service.CreateNotificationAsync(notification);

        // Assert
        await act.Should().ThrowAsync<NotificationValidationException>();
    }

    /// <summary>
    /// Tests that SendNotificationAsync throws a NotificationException when the notification is not found.
    /// </summary>
    [Fact]
    public async Task SendNotificationAsync_ShouldThrowException_WhenNotificationNotFound()
    {
        // Arrange
        _notificationRepositoryMock.Setup(r => r.GetByIdAsync("not-found"))
            .ReturnsAsync((DeploymentNotification?)null);

        // Act
        Func<Task> act = async () => await _service.SendNotificationAsync("not-found");

        // Assert
        await act.Should().ThrowAsync<NotificationException>();
    }

    /// <summary>
    /// Tests that SendNotificationAsync returns an empty list when no channels are specified.
    /// </summary>
    [Fact]
    public async Task SendNotificationAsync_ShouldReturnEmptyList_WhenNoChannelsSpecified()
    {
        // Arrange
        var notification = new DeploymentNotification { Id = "id", Channels = new List<NotificationChannel>() };
        _notificationRepositoryMock.Setup(r => r.GetByIdAsync("id"))
            .ReturnsAsync(notification);

        // Act
        var results = await _service.SendNotificationAsync("id", new List<NotificationChannel>());

        // Assert
        results.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that SendNotificationAsync sends and returns the result when the notification is valid.
    /// </summary>
    [Fact]
    public async Task SendNotificationAsync_ShouldSendAndReturnResult_WhenValid()
    {
        // Arrange
        var notification = new DeploymentNotification
        {
            Id = "id",
            Channels = new List<NotificationChannel> { NotificationChannel.Slack }
        };
        var config = new ChannelConfiguration { DisplayName = "SlackConfig" };
        var result = new NotificationResult();
        result.MarkAsSuccessful(200, "OK");

        _notificationRepositoryMock.Setup(r => r.GetByIdAsync("id"))
            .ReturnsAsync(notification);
        _configRepositoryMock.Setup(r => r.GetByChannelAsync(NotificationChannel.Slack))
            .ReturnsAsync(new List<ChannelConfiguration> { config });
        _dispatcherMock.Setup(d => d.SendToWebhookAsync(config, notification))
            .ReturnsAsync(result);

        // Act
        var results = await _service.SendNotificationAsync("id");

        // Assert
        results.Should().ContainSingle().Which.Should().Be(result);
        _resultRepositoryMock.Verify(r => r.CreateAsync(result), Times.Once);
        _notificationRepositoryMock.Verify(r => r.UpdateAsync(notification), Times.Once);
    }

    /// <summary>
    /// Tests that RetryFailedDeliveriesAsync throws a NotificationException when the notification is not found.
    /// </summary>
    [Fact]
    public async Task RetryFailedDeliveriesAsync_ShouldThrowException_WhenNotificationNotFound()
    {
        // Arrange
        _notificationRepositoryMock.Setup(r => r.GetByIdAsync("not-found"))
            .ReturnsAsync((DeploymentNotification?)null);

        // Act
        Func<Task> act = async () => await _service.RetryFailedDeliveriesAsync("not-found");

        // Assert
        await act.Should().ThrowAsync<NotificationException>();
    }
}
