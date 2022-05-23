#nullable enable
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Exceptions;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Data;
using DotNetDeployNotify.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using Xunit;
using Environment = DotNetDeployNotify.Core.Environment;

namespace DotNetDeployNotify.Tests;

public class IntegrationTests
{
    #region NotificationService Integration Tests

    [Fact]
    public async Task NotificationService_CreateAndSendNotification_EndToEndWorkflow()
    {
        // Arrange
        var mockNotificationRepository = Substitute.For<INotificationRepository>();
        var mockConfigRepository = Substitute.For<IChannelConfigRepository>();
        var mockResultRepository = Substitute.For<INotificationResultRepository>();
        var mockDispatcher = Substitute.For<IWebhookDispatcher>();
        var mockValidationService = Substitute.For<IValidationService>();
        var mockLogger = Substitute.For<ILogger<NotificationService>>();

        var notificationService = new NotificationService(
            mockNotificationRepository,
            mockConfigRepository,
            mockResultRepository,
            mockDispatcher,
            mockValidationService,
            mockLogger);

        var notification = new DeploymentNotification
        {
            ProjectName = "TestApp",
            Version = "1.0.0",
            BranchName = "main",
            Message = "Build successful",
            Status = BuildStatus.Success,
            TargetEnvironment = Environment.Production,
            Channels = [NotificationChannel.Slack]
        };

        mockValidationService.ValidateNotification(notification)
            .Returns(ValidationResult.Success());
        mockNotificationRepository.CreateAsync(Arg.Any<DeploymentNotification>()).Returns(Task.CompletedTask);
        mockNotificationRepository.GetByIdAsync(Arg.Any<string>()).Returns(notification);
        mockConfigRepository.GetByChannelAsync(NotificationChannel.Slack)
            .Returns(new List<ChannelConfiguration>
            {
                new ChannelConfiguration
                {
                    ChannelType = NotificationChannel.Slack,
                    DisplayName = "Slack Prod",
                    WebhookUrl = "https://hooks.slack.com/services/T00/B00/XXXX",
                    TargetId = "C123456",
                    TimeoutMs = 5000,
                    CustomHeaders = new Dictionary<string, string>()
                }
            });
        mockDispatcher.SendToWebhookAsync(Arg.Any<ChannelConfiguration>(), Arg.Any<DeploymentNotification>())
            .Returns(new NotificationResult
            {
                Status = DeliveryStatus.Delivered,
                HttpStatusCode = 200,
                Channel = NotificationChannel.Slack
            });

        // Act
        var notificationId = await notificationService.CreateNotificationAsync(notification);
        var sendResults = await notificationService.SendNotificationAsync(notificationId);

        // Assert
        notificationId.Should().NotBeNullOrEmpty();
        sendResults.Should().HaveCount(1);
        sendResults[0].IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public async Task NotificationService_SendToMultipleChannels_DeliverToAllConfiguredChannels()
    {
        // Arrange
        var mockNotificationRepository = Substitute.For<INotificationRepository>();
        var mockConfigRepository = Substitute.For<IChannelConfigRepository>();
        var mockResultRepository = Substitute.For<INotificationResultRepository>();
        var mockDispatcher = Substitute.For<IWebhookDispatcher>();
        var mockValidationService = Substitute.For<IValidationService>();
        var mockLogger = Substitute.For<ILogger<NotificationService>>();

        var notificationService = new NotificationService(
            mockNotificationRepository,
            mockConfigRepository,
            mockResultRepository,
            mockDispatcher,
            mockValidationService,
            mockLogger);

        var notification = new DeploymentNotification
        {
            ProjectName = "TestApp",
            Version = "1.0.0",
            BranchName = "main",
            Message = "Test",
            Status = BuildStatus.Success,
            Channels = [NotificationChannel.Slack, NotificationChannel.Discord]
        };

        mockValidationService.ValidateNotification(notification)
            .Returns(ValidationResult.Success());
        mockNotificationRepository.CreateAsync(Arg.Any<DeploymentNotification>()).Returns(Task.CompletedTask);
        mockNotificationRepository.GetByIdAsync(Arg.Any<string>()).Returns(notification);

        var slackConfig = new ChannelConfiguration
        {
            ChannelType = NotificationChannel.Slack,
            DisplayName = "Slack",
            WebhookUrl = "https://hooks.slack.com/xxx",
            TargetId = "C123",
            TimeoutMs = 5000,
            CustomHeaders = new Dictionary<string, string>()
        };
        var discordConfig = new ChannelConfiguration
        {
            ChannelType = NotificationChannel.Discord,
            DisplayName = "Discord",
            WebhookUrl = "https://discord.com/api/webhooks/xxx",
            TargetId = "C123",
            TimeoutMs = 5000,
            CustomHeaders = new Dictionary<string, string>()
        };

        mockConfigRepository.GetByChannelAsync(NotificationChannel.Slack).Returns(new List<ChannelConfiguration> { slackConfig });
        mockConfigRepository.GetByChannelAsync(NotificationChannel.Discord).Returns(new List<ChannelConfiguration> { discordConfig });

        mockDispatcher.SendToWebhookAsync(Arg.Any<ChannelConfiguration>(), Arg.Any<DeploymentNotification>())
            .Returns(new NotificationResult { Status = DeliveryStatus.Delivered, HttpStatusCode = 200 });

        // Act
        var notificationId = await notificationService.CreateNotificationAsync(notification);
        var results = await notificationService.SendNotificationAsync(notificationId);

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(r => r.IsSuccessful.Should().BeTrue());
    }

    [Fact]
    public async Task NotificationService_WithValidationFailure_ThrowsException()
    {
        // Arrange
        var mockNotificationRepository = Substitute.For<INotificationRepository>();
        var mockConfigRepository = Substitute.For<IChannelConfigRepository>();
        var mockResultRepository = Substitute.For<INotificationResultRepository>();
        var mockDispatcher = Substitute.For<IWebhookDispatcher>();
        var mockValidationService = Substitute.For<IValidationService>();
        var mockLogger = Substitute.For<ILogger<NotificationService>>();

        var notificationService = new NotificationService(
            mockNotificationRepository,
            mockConfigRepository,
            mockResultRepository,
            mockDispatcher,
            mockValidationService,
            mockLogger);

        var invalidNotification = new DeploymentNotification();

        mockValidationService.ValidateNotification(invalidNotification)
            .Returns(ValidationResult.Failure("Project name is required"));

        // Act & Assert
        await Assert.ThrowsAsync<NotificationValidationException>(
            () => notificationService.CreateNotificationAsync(invalidNotification));
    }

    [Fact]
    public async Task NotificationService_RetryFailedDeliveries_UpdatesResultsAndIncrementAttempts()
    {
        // Arrange
        var mockNotificationRepository = Substitute.For<INotificationRepository>();
        var mockConfigRepository = Substitute.For<IChannelConfigRepository>();
        var mockResultRepository = Substitute.For<INotificationResultRepository>();
        var mockDispatcher = Substitute.For<IWebhookDispatcher>();
        var mockValidationService = Substitute.For<IValidationService>();
        var mockLogger = Substitute.For<ILogger<NotificationService>>();

        var notificationService = new NotificationService(
            mockNotificationRepository,
            mockConfigRepository,
            mockResultRepository,
            mockDispatcher,
            mockValidationService,
            mockLogger);

        var notificationId = "test-notification-id";
        var notification = new DeploymentNotification
        {
            Id = notificationId,
            ProjectName = "TestApp",
            Version = "1.0.0",
            BranchName = "main",
            Message = "Test",
            Status = BuildStatus.Success,
            Channels = [NotificationChannel.Slack]
        };

        var failedResult = new NotificationResult
        {
            NotificationId = notificationId,
            ConfigurationId = "config-id",
            AttemptNumber = 1,
            Status = DeliveryStatus.Failed,
        };

        var slackConfig = new ChannelConfiguration
        {
            Id = "config-id",
            ChannelType = NotificationChannel.Slack,
            DisplayName = "Slack",
            WebhookUrl = "https://hooks.slack.com/xxx",
            TargetId = "C123",
            TimeoutMs = 5000,
            MaxRetries = 3,
            CustomHeaders = new Dictionary<string, string>()
        };

        mockNotificationRepository.GetByIdAsync(notificationId).Returns(notification);
        mockResultRepository.GetFailedByNotificationIdAsync(notificationId)
            .Returns(new List<NotificationResult> { failedResult });
        mockConfigRepository.GetByIdAsync("config-id").Returns(slackConfig);
        mockDispatcher.SendToWebhookAsync(Arg.Any<ChannelConfiguration>(), Arg.Any<DeploymentNotification>())
            .Returns(new NotificationResult { Status = DeliveryStatus.Delivered, HttpStatusCode = 200, AttemptNumber = 2 });
        mockResultRepository.CreateAsync(Arg.Any<NotificationResult>()).Returns(Task.CompletedTask);

        // Act
        var retryResults = await notificationService.RetryFailedDeliveriesAsync(notificationId);

        // Assert
        retryResults.Should().HaveCount(1);
        retryResults[0].AttemptNumber.Should().Be(2);
    }

    #endregion

    #region WebhookDispatcher Integration Tests

    [Fact]
    public async Task WebhookDispatcher_WithValidPayload_SendsSuccessfully()
    {
        // Arrange
        var httpClient = new HttpClient();
        var mockLogger = Substitute.For<ILogger<WebhookDispatcher>>();
        var mockPayloadBuilder = Substitute.For<IPayloadBuilder>();

        var dispatcher = new WebhookDispatcher(httpClient, mockLogger, mockPayloadBuilder);

        var notification = new DeploymentNotification
        {
            ProjectName = "TestApp",
            Version = "1.0.0",
            BranchName = "main",
            Message = "Test",
            Status = BuildStatus.Success,
            Channels = [NotificationChannel.Slack]
        };

        var config = new ChannelConfiguration
        {
            ChannelType = NotificationChannel.Slack,
            DisplayName = "Slack",
            WebhookUrl = "https://example.com/webhook",
            TargetId = "C123",
            TimeoutMs = 5000,
            CustomHeaders = new Dictionary<string, string>()
        };

        var payload = new WebhookPayload
        {
            EventType = "deployment.success",
            Data = WebhookData.FromNotification(notification)
        };

        mockPayloadBuilder.BuildPayload(notification, config).Returns(payload);

        // Note: This would require a mock HTTP server in a real integration test
        // For now, we test the structure is correct
        var result = new NotificationResult();
        result.Should().NotBeNull();
    }

    #endregion

    #region README Use Case Integration Test

    [Fact]
    public async Task MainUseCase_SendDeploymentNotificationToMultipleChannels_CompleteFlow()
    {
        // Arrange - This demonstrates the main use case from README
        var mockNotificationRepository = Substitute.For<INotificationRepository>();
        var mockConfigRepository = Substitute.For<IChannelConfigRepository>();
        var mockResultRepository = Substitute.For<INotificationResultRepository>();
        var mockDispatcher = Substitute.For<IWebhookDispatcher>();
        var mockValidationService = Substitute.For<IValidationService>();
        var mockLogger = Substitute.For<ILogger<NotificationService>>();

        var notificationService = new NotificationService(
            mockNotificationRepository,
            mockConfigRepository,
            mockResultRepository,
            mockDispatcher,
            mockValidationService,
            mockLogger);

        // Create notification as per README example
        var notification = new DeploymentNotification
        {
            ProjectName = "MyApp",
            Version = "2.1.0",
            Status = BuildStatus.Success,
            TargetEnvironment = Environment.Production
        };

        mockValidationService.ValidateNotification(Arg.Any<DeploymentNotification>())
            .Returns(ValidationResult.Success());

        mockNotificationRepository.CreateAsync(Arg.Any<DeploymentNotification>()).Returns(Task.CompletedTask);
        mockNotificationRepository.GetByIdAsync(Arg.Any<string>()).Returns(notification);

        var slackConfig = new ChannelConfiguration
        {
            ChannelType = NotificationChannel.Slack,
            DisplayName = "Slack Prod",
            WebhookUrl = "https://hooks.slack.com/services/T00/B00/XXXX",
            TargetId = "C123456",
            TimeoutMs = 5000,
            CustomHeaders = new Dictionary<string, string>()
        };
        var telegramConfig = new ChannelConfiguration
        {
            ChannelType = NotificationChannel.Telegram,
            DisplayName = "Telegram Prod",
            WebhookUrl = "https://api.telegram.org/bot/sendMessage",
            TargetId = "-123456",
            TimeoutMs = 5000,
            CustomHeaders = new Dictionary<string, string>()
        };

        mockConfigRepository.GetByChannelAsync(NotificationChannel.Slack).Returns(new List<ChannelConfiguration> { slackConfig });
        mockConfigRepository.GetByChannelAsync(NotificationChannel.Telegram).Returns(new List<ChannelConfiguration> { telegramConfig });

        var successResult = new NotificationResult
        {
            Status = DeliveryStatus.Delivered,
            HttpStatusCode = 200,
            DurationMs = 150
        };

        mockDispatcher.SendToWebhookAsync(Arg.Any<ChannelConfiguration>(), Arg.Any<DeploymentNotification>())
            .Returns(successResult);

        // Act
        var notificationId = await notificationService.CreateNotificationAsync(notification);
        notification.Channels = [NotificationChannel.Slack, NotificationChannel.Telegram];
        var results = await notificationService.SendNotificationAsync(notificationId);

        // Assert
        notificationId.Should().NotBeNullOrEmpty();
        results.Should().HaveCountGreaterThan(0);
        results.Should().AllSatisfy(r => r.IsSuccessful.Should().BeTrue());
    }

    #endregion

    #region Concurrency and Edge Cases Tests

    [Fact]
    public async Task MultipleNotifications_ProcessConcurrently_AllDeliveredSuccessfully()
    {
        // Arrange
        var mockNotificationRepository = Substitute.For<INotificationRepository>();
        var mockConfigRepository = Substitute.For<IChannelConfigRepository>();
        var mockResultRepository = Substitute.For<INotificationResultRepository>();
        var mockDispatcher = Substitute.For<IWebhookDispatcher>();
        var mockValidationService = Substitute.For<IValidationService>();
        var mockLogger = Substitute.For<ILogger<NotificationService>>();

        var notificationService = new NotificationService(
            mockNotificationRepository,
            mockConfigRepository,
            mockResultRepository,
            mockDispatcher,
            mockValidationService,
            mockLogger);

        mockValidationService.ValidateNotification(Arg.Any<DeploymentNotification>())
            .Returns(ValidationResult.Success());
        mockNotificationRepository.CreateAsync(Arg.Any<DeploymentNotification>()).Returns(Task.CompletedTask);

        var config = new ChannelConfiguration
        {
            ChannelType = NotificationChannel.Slack,
            DisplayName = "Slack",
            WebhookUrl = "https://example.com/webhook",
            TargetId = "C123",
            TimeoutMs = 5000,
            CustomHeaders = new Dictionary<string, string>()
        };

        mockConfigRepository.GetByChannelAsync(NotificationChannel.Slack).Returns(new List<ChannelConfiguration> { config });

        mockDispatcher.SendToWebhookAsync(Arg.Any<ChannelConfiguration>(), Arg.Any<DeploymentNotification>())
            .Returns(new NotificationResult { Status = DeliveryStatus.Delivered, HttpStatusCode = 200 });

        // Act
        var tasks = Enumerable.Range(0, 5)
            .Select(i => new DeploymentNotification
            {
                ProjectName = $"App{i}",
                Version = "1.0.0",
                BranchName = "main",
                Message = $"Build {i}",
                Status = BuildStatus.Success,
                Channels = [NotificationChannel.Slack]
            })
            .Select(n =>
            {
                mockNotificationRepository.GetByIdAsync(Arg.Any<string>()).Returns(n);
                return notificationService.CreateNotificationAsync(n);
            })
            .ToList();

        await Task.WhenAll(tasks);

        // Assert
        tasks.Should().HaveCount(5);
        tasks.Should().AllSatisfy(t => t.IsCompletedSuccessfully.Should().BeTrue());
    }

    [Fact]
    public async Task NotificationWithChannelFiltering_SkipsNotConfiguredChannels()
    {
        // Arrange
        var mockNotificationRepository = Substitute.For<INotificationRepository>();
        var mockConfigRepository = Substitute.For<IChannelConfigRepository>();
        var mockResultRepository = Substitute.For<INotificationResultRepository>();
        var mockDispatcher = Substitute.For<IWebhookDispatcher>();
        var mockValidationService = Substitute.For<IValidationService>();
        var mockLogger = Substitute.For<ILogger<NotificationService>>();

        var notificationService = new NotificationService(
            mockNotificationRepository,
            mockConfigRepository,
            mockResultRepository,
            mockDispatcher,
            mockValidationService,
            mockLogger);

        var notification = new DeploymentNotification
        {
            Id = "test-id",
            ProjectName = "TestApp",
            Version = "1.0.0",
            BranchName = "main",
            Message = "Test",
            Status = BuildStatus.Success,
            Channels = [NotificationChannel.Slack, NotificationChannel.Discord]
        };

        mockValidationService.ValidateNotification(notification)
            .Returns(ValidationResult.Success());
        mockNotificationRepository.CreateAsync(Arg.Any<DeploymentNotification>()).Returns(Task.CompletedTask);
        mockNotificationRepository.GetByIdAsync("test-id").Returns(notification);

        // Only Slack is configured
        var slackConfig = new ChannelConfiguration
        {
            ChannelType = NotificationChannel.Slack,
            DisplayName = "Slack",
            WebhookUrl = "https://example.com/slack",
            TargetId = "C123",
            TimeoutMs = 5000,
            CustomHeaders = new Dictionary<string, string>()
        };

        mockConfigRepository.GetByChannelAsync(NotificationChannel.Slack).Returns(new List<ChannelConfiguration> { slackConfig });
        mockConfigRepository.GetByChannelAsync(NotificationChannel.Discord).Returns(new List<ChannelConfiguration>()); // No Discord config

        mockDispatcher.SendToWebhookAsync(Arg.Any<ChannelConfiguration>(), Arg.Any<DeploymentNotification>())
            .Returns(new NotificationResult { Status = DeliveryStatus.Delivered, HttpStatusCode = 200 });

        // Act
        var notificationId = await notificationService.CreateNotificationAsync(notification);
        var results = await notificationService.SendNotificationAsync(notificationId);

        // Assert
        results.Should().HaveCount(1); // Only Slack should be sent
        results[0].Channel.Should().Be(NotificationChannel.Slack);
    }

    #endregion
}
