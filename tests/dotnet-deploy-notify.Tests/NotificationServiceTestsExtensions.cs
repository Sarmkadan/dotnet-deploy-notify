using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Data;
using DotNetDeployNotify.Services;
using FluentAssertions;
using Moq;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Extension methods for <see cref="NotificationServiceTests"/> to provide additional test utilities.
/// </summary>
public static class NotificationServiceTestsExtensions
{
    /// <summary>
    /// Creates a valid deployment notification for testing purposes.
    /// </summary>
    /// <param name="projectName">The project name.</param>
    /// <param name="version">The version number.</param>
    /// <param name="environment">The target environment.</param>
    /// <param name="status">The build status.</param>
    /// <returns>A configured <see cref="DeploymentNotification"/> instance.</returns>
    public static DeploymentNotification CreateTestNotification(
        this NotificationServiceTests _,
        string projectName = "TestProject",
        string version = "1.0.0",
        global::DotNetDeployNotify.Core.Environment environment = global::DotNetDeployNotify.Core.Environment.Development,
        global::DotNetDeployNotify.Core.BuildStatus status = global::DotNetDeployNotify.Core.BuildStatus.Success)
    {
        ArgumentException.ThrowIfNullOrEmpty(projectName);
        ArgumentException.ThrowIfNullOrEmpty(version);

        return new DeploymentNotification
        {
            Id = Guid.NewGuid().ToString(),
            ProjectName = projectName,
            Version = version,
            Status = status,
            TargetEnvironment = environment,
            BranchName = "main",
            CommitHash = "abc123",
            CommitAuthor = "Test Author",
            RepositoryUrl = "https://github.com/test/test-repo",
            BuildUrl = "https://ci.example.com/build/123",
            DurationSeconds = 120,
            Priority = global::DotNetDeployNotify.Core.NotificationPriority.Normal,
            Channels = new List<global::DotNetDeployNotify.Core.NotificationChannel> { global::DotNetDeployNotify.Core.NotificationChannel.Slack }
        };
    }

    /// <summary>
    /// Creates a valid channel configuration for testing purposes.
    /// </summary>
    /// <param name="channelType">The notification channel type.</param>
    /// <param name="webhookUrl">The webhook URL.</param>
    /// <param name="displayName">The display name.</param>
    /// <returns>A configured <see cref="ChannelConfiguration"/> instance.</returns>
    public static ChannelConfiguration CreateTestChannelConfiguration(
        this NotificationServiceTests _,
        global::DotNetDeployNotify.Core.NotificationChannel channelType = global::DotNetDeployNotify.Core.NotificationChannel.Slack,
        string webhookUrl = "https://hooks.slack.com/services/test",
        string displayName = "Test Slack Config")
    {
        ArgumentException.ThrowIfNullOrEmpty(webhookUrl);
        ArgumentException.ThrowIfNullOrEmpty(displayName);

        return new ChannelConfiguration
        {
            Id = Guid.NewGuid().ToString(),
            ChannelType = channelType,
            WebhookUrl = webhookUrl,
            ApiToken = "test-token",
            TargetId = "test-channel",
            DisplayName = displayName,
            IsEnabled = true,
            MinimumPriority = global::DotNetDeployNotify.Core.NotificationPriority.Low,
            AllowedEnvironments = new List<global::DotNetDeployNotify.Core.Environment> { global::DotNetDeployNotify.Core.Environment.Development },
            AllowedStatuses = new List<global::DotNetDeployNotify.Core.BuildStatus> { global::DotNetDeployNotify.Core.BuildStatus.Success },
            MaxRetries = 3,
            TimeoutMs = 10000
        };
    }

    /// <summary>
    /// Creates a successful notification result for testing purposes.
    /// </summary>
    /// <param name="notificationId">The notification ID.</param>
    /// <param name="channel">The notification channel.</param>
    /// <param name="configurationId">The configuration ID.</param>
    /// <returns>A configured <see cref="NotificationResult"/> instance marked as successful.</returns>
    public static NotificationResult CreateSuccessfulResult(
        this NotificationServiceTests _,
        string notificationId,
        global::DotNetDeployNotify.Core.NotificationChannel channel = global::DotNetDeployNotify.Core.NotificationChannel.Slack,
        string configurationId = "config-123")
    {
        ArgumentException.ThrowIfNullOrEmpty(notificationId);
        ArgumentException.ThrowIfNullOrEmpty(configurationId);

        var result = new NotificationResult
        {
            Id = Guid.NewGuid().ToString(),
            NotificationId = notificationId,
            Channel = channel,
            ConfigurationId = configurationId,
            Status = DeliveryStatus.Delivered,
            HttpStatusCode = 200,
            ResponseBody = "{\"ok\": true}",
            DurationMs = 150,
            AttemptNumber = 1,
            AttemptedAt = DateTime.UtcNow
        };

        return result;
    }

    /// <summary>
    /// Creates a failed notification result for testing purposes.
    /// </summary>
    /// <param name="notificationId">The notification ID.</param>
    /// <param name="channel">The notification channel.</param>
    /// <param name="configurationId">The configuration ID.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <returns>A configured <see cref="NotificationResult"/> instance marked as failed.</returns>
    public static NotificationResult CreateFailedResult(
        this NotificationServiceTests _,
        string notificationId,
        global::DotNetDeployNotify.Core.NotificationChannel channel = global::DotNetDeployNotify.Core.NotificationChannel.Slack,
        string configurationId = "config-123",
        string errorMessage = "Webhook endpoint not found")
    {
        ArgumentException.ThrowIfNullOrEmpty(notificationId);
        ArgumentException.ThrowIfNullOrEmpty(configurationId);
        ArgumentException.ThrowIfNullOrEmpty(errorMessage);

        var result = new NotificationResult
        {
            Id = Guid.NewGuid().ToString(),
            NotificationId = notificationId,
            Channel = channel,
            ConfigurationId = configurationId,
            Status = DeliveryStatus.Failed,
            HttpStatusCode = 404,
            ResponseBody = "Not Found",
            ErrorMessage = errorMessage,
            ExceptionType = "System.Net.Http.HttpRequestException",
            DurationMs = 50,
            AttemptNumber = 1,
            AttemptedAt = DateTime.UtcNow
        };

        return result;
    }

    /// <summary>
    /// Verifies that a notification was created with the expected properties.
    /// </summary>
    /// <param name="mock">The mocked notification repository.</param>
    /// <param name="expectedNotification">The expected notification.</param>
    public static void VerifyNotificationCreated(
        this NotificationServiceTests _,
        Mock<INotificationRepository> mock,
        DeploymentNotification expectedNotification)
    {
        ArgumentNullException.ThrowIfNull(mock);
        ArgumentNullException.ThrowIfNull(expectedNotification);

        mock.Verify(r => r.CreateAsync(It.Is<DeploymentNotification>(n =>
            n.ProjectName == expectedNotification.ProjectName &&
            n.Version == expectedNotification.Version &&
            n.Status == expectedNotification.Status &&
            n.TargetEnvironment == expectedNotification.TargetEnvironment
        )), Times.Once);
    }

    /// <summary>
    /// Verifies that a notification was updated with the expected properties.
    /// </summary>
    /// <param name="mock">The mocked notification repository.</param>
    /// <param name="expectedNotification">The expected notification.</param>
    public static void VerifyNotificationUpdated(
        this NotificationServiceTests _,
        Mock<INotificationRepository> mock,
        DeploymentNotification expectedNotification)
    {
        ArgumentNullException.ThrowIfNull(mock);
        ArgumentNullException.ThrowIfNull(expectedNotification);

        mock.Verify(r => r.UpdateAsync(It.Is<DeploymentNotification>(n =>
            n.Id == expectedNotification.Id &&
            n.ProjectName == expectedNotification.ProjectName
        )), Times.Once);
    }

    /// <summary>
    /// Verifies that a notification result was created with the expected properties.
    /// </summary>
    /// <param name="mock">The mocked result repository.</param>
    /// <param name="expectedResult">The expected result.</param>
    public static void VerifyResultCreated(
        this NotificationServiceTests _,
        Mock<INotificationResultRepository> mock,
        NotificationResult expectedResult)
    {
        ArgumentNullException.ThrowIfNull(mock);
        ArgumentNullException.ThrowIfNull(expectedResult);

        mock.Verify(r => r.CreateAsync(It.Is<NotificationResult>(res =>
            res.NotificationId == expectedResult.NotificationId &&
            res.Channel == expectedResult.Channel &&
            res.Status == expectedResult.Status
        )), Times.Once);
    }

    /// <summary>
    /// Sets up the validation service mock to return a specific validation result.
    /// </summary>
    /// <param name="mock">The mocked validation service.</param>
    /// <param name="isValid">Whether the validation should pass.</param>
    /// <param name="errors">Optional error messages if validation should fail.</param>
    public static void SetupValidationResult(
        this NotificationServiceTests _,
        Mock<IValidationService> mock,
        bool isValid,
        IEnumerable<string>? errors = null)
    {
        ArgumentNullException.ThrowIfNull(mock);

        mock.Setup(v => v.ValidateNotification(It.IsAny<DeploymentNotification>()))
            .Returns(new ValidationResult
            {
                IsValid = isValid,
                Errors = errors?.ToList() ?? new List<string>()
            });
    }

    /// <summary>
    /// Sets up the channel configuration repository to return a specific configuration.
    /// </summary>
    /// <param name="mock">The mocked channel config repository.</param>
    /// <param name="configurations">The configurations to return.</param>
    public static void SetupChannelConfigurations(
        this NotificationServiceTests _,
        Mock<IChannelConfigRepository> mock,
        IEnumerable<ChannelConfiguration> configurations)
    {
        ArgumentNullException.ThrowIfNull(mock);
        ArgumentNullException.ThrowIfNull(configurations);

        mock.Setup(r => r.GetByChannelAsync(It.IsAny<global::DotNetDeployNotify.Core.NotificationChannel>()))
            .ReturnsAsync(configurations.ToList());
    }

    /// <summary>
    /// Sets up the webhook dispatcher to return a specific result.
    /// </summary>
    /// <param name="mock">The mocked webhook dispatcher.</param>
    /// <param name="result">The result to return.</param>
    public static void SetupWebhookDispatch(
        this NotificationServiceTests _,
        Mock<IWebhookDispatcher> mock,
        NotificationResult result)
    {
        ArgumentNullException.ThrowIfNull(mock);
        ArgumentNullException.ThrowIfNull(result);

        mock.Setup(d => d.SendToWebhookAsync(
            It.IsAny<ChannelConfiguration>(),
            It.IsAny<DeploymentNotification>()))
            .ReturnsAsync(result);
    }
}
