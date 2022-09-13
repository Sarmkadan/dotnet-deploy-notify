#nullable enable
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Services;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Tests for the ValidationService class.
/// </summary>
public class ValidationServiceTests
{
    private readonly ValidationService _validationService;

    /// <summary>
    /// Initializes a new instance of the ValidationServiceTests class.
    /// </summary>
    public ValidationServiceTests()
    {
        _validationService = new ValidationService();
    }

    #region ValidateNotification Tests

    /// <summary>
    /// Tests that ValidateNotification returns a successful result when given a valid notification.
    /// </summary>
    [Fact]
    public void ValidateNotification_WithValidNotification_ReturnsSuccess()
    {
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "MyApp",
            Version = "1.0.0",
            BranchName = "main",
            Message = "Deployment successful",
            Channels = [NotificationChannel.Slack],
            DeliveryAttempts = 0
        };

        // Act
        var result = _validationService.ValidateNotification(notification);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that ValidateNotification returns a failed result when given a null notification.
    /// </summary>
    [Fact]
    public void ValidateNotification_WithNullNotification_ReturnsFailure()
    {
        // Act
        var result = _validationService.ValidateNotification(null!);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Notification cannot be null");
    }

    /// <summary>
    /// Tests that ValidateNotification returns a failed result when given a notification with a missing project name.
    /// </summary>
    [Fact]
    public void ValidateNotification_WithMissingProjectName_ReturnsFailure()
    {
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "",
            Version = "1.0.0",
            BranchName = "main",
            Message = "Test",
            Channels = [NotificationChannel.Slack]
        };

        // Act
        var result = _validationService.ValidateNotification(notification);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Project name is required");
    }

    /// <summary>
    /// Tests that ValidateNotification returns a failed result when given a notification with a missing version.
    /// </summary>
    [Fact]
    public void ValidateNotification_WithMissingVersion_ReturnsFailure()
    {
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "MyApp",
            Version = "",
            BranchName = "main",
            Message = "Test",
            Channels = [NotificationChannel.Slack]
        };

        // Act
        var result = _validationService.ValidateNotification(notification);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Version is required");
    }

    /// <summary>
    /// Tests that ValidateNotification returns a failed result when given a notification with a missing branch name.
    /// </summary>
    [Fact]
    public void ValidateNotification_WithMissingBranchName_ReturnsFailure()
    {
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "MyApp",
            Version = "1.0.0",
            BranchName = "",
            Message = "Test",
            Channels = [NotificationChannel.Slack]
        };

        // Act
        var result = _validationService.ValidateNotification(notification);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Branch name is required");
    }

    /// <summary>
    /// Tests that ValidateNotification returns a failed result when given a notification with a missing message.
    /// </summary>
    [Fact]
    public void ValidateNotification_WithMissingMessage_ReturnsFailure()
    {
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "MyApp",
            Version = "1.0.0",
            BranchName = "main",
            Message = "",
            Channels = [NotificationChannel.Slack]
        };

        // Act
        var result = _validationService.ValidateNotification(notification);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Message is required");
    }

    /// <summary>
    /// Tests that ValidateNotification returns a failed result when given a notification with no channels.
    /// </summary>
    [Fact]
    public void ValidateNotification_WithNoChannels_ReturnsFailure()
    {
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "MyApp",
            Version = "1.0.0",
            BranchName = "main",
            Message = "Test",
            Channels = []
        };

        // Act
        var result = _validationService.ValidateNotification(notification);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("At least one notification channel must be specified");
    }

    /// <summary>
    /// Tests that ValidateNotification returns a failed result when given a notification with negative delivery attempts.
    /// </summary>
    [Fact]
    public void ValidateNotification_WithNegativeDeliveryAttempts_ReturnsFailure()
    {
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "MyApp",
            Version = "1.0.0",
            BranchName = "main",
            Message = "Test",
            Channels = [NotificationChannel.Slack],
            DeliveryAttempts = -1
        };

        // Act
        var result = _validationService.ValidateNotification(notification);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Delivery attempts cannot be negative");
    }

    /// <summary>
    /// Tests that ValidateNotification returns a failed result when given a notification with a negative duration.
    /// </summary>
    [Fact]
    public void ValidateNotification_WithNegativeDuration_ReturnsFailure()
    {
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "MyApp",
            Version = "1.0.0",
            BranchName = "main",
            Message = "Test",
            Channels = [NotificationChannel.Slack],
            DurationSeconds = -5
        };

        // Act
        var result = _validationService.ValidateNotification(notification);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Duration cannot be negative");
    }

    /// <summary>
    /// Tests that ValidateNotification returns a successful result when given a notification with a positive duration.
    /// </summary>
    [Fact]
    public void ValidateNotification_WithPositiveDuration_ReturnsSuccess()
    {
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "MyApp",
            Version = "1.0.0",
            BranchName = "main",
            Message = "Test",
            Channels = [NotificationChannel.Slack],
            DurationSeconds = 120
        };

        // Act
        var result = _validationService.ValidateNotification(notification);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Tests that ValidateNotification returns all errors when given a notification with multiple errors.
    /// </summary>
    [Fact]
    public void ValidateNotification_WithMultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "",
            Version = "",
            BranchName = "",
            Message = "",
            Channels = [],
            DurationSeconds = -1
        };

        // Act
        var result = _validationService.ValidateNotification(notification);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(1);
    }

    #endregion

    #region ValidateChannelConfiguration Tests

    /// <summary>
    /// Tests that ValidateChannelConfiguration returns a successful result when given a valid channel configuration.
    /// </summary>
    [Fact]
    public void ValidateChannelConfiguration_WithValidConfig_ReturnsSuccess()
    {
        // Arrange
        var config = new ChannelConfiguration
        {
            DisplayName = "Slack Prod",
            WebhookUrl = "https://hooks.slack.com/services/T00/B00/XXXX",
            TargetId = "C123456",
            TimeoutMs = 5000,
            MaxRetries = 3,
            CustomHeaders = new Dictionary<string, string>()
        };

        // Act
        var result = _validationService.ValidateChannelConfiguration(config);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that ValidateChannelConfiguration returns a failed result when given a null channel configuration.
    /// </summary>
    [Fact]
    public void ValidateChannelConfiguration_WithNullConfig_ReturnsFailure()
    {
        // Act
        var result = _validationService.ValidateChannelConfiguration(null!);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Channel configuration cannot be null");
    }

    /// <summary>
    /// Tests that ValidateChannelConfiguration returns a failed result when given a channel configuration with a missing display name.
    /// </summary>
    [Fact]
    public void ValidateChannelConfiguration_WithMissingDisplayName_ReturnsFailure()
    {
        // Arrange
        var config = new ChannelConfiguration
        {
            DisplayName = "",
            WebhookUrl = "https://example.com/webhook",
            TargetId = "123"
        };

        // Act
        var result = _validationService.ValidateChannelConfiguration(config);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Display name is required");
    }

    /// <summary>
    /// Tests that ValidateChannelConfiguration returns a failed result when given a channel configuration with an invalid webhook URL.
    /// </summary>
    [Fact]
    public void ValidateChannelConfiguration_WithInvalidWebhookUrl_ReturnsFailure()
    {
        // Arrange
        var config = new ChannelConfiguration
        {
            DisplayName = "Test",
            WebhookUrl = "not-a-valid-url",
            TargetId = "123",
            CustomHeaders = new Dictionary<string, string>()
        };

        // Act
        var result = _validationService.ValidateChannelConfiguration(config);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Webhook URL is invalid or missing");
    }

    /// <summary>
    /// Tests that ValidateChannelConfiguration returns a failed result when given a channel configuration with a missing target ID.
    /// </summary>
    [Fact]
    public void ValidateChannelConfiguration_WithMissingTargetId_ReturnsFailure()
    {
        // Arrange
        var config = new ChannelConfiguration
        {
            DisplayName = "Test",
            WebhookUrl = "https://example.com/webhook",
            TargetId = "",
            CustomHeaders = new Dictionary<string, string>()
        };

        // Act
        var result = _validationService.ValidateChannelConfiguration(config);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Target ID (chat ID, channel ID, etc.) is required");
    }

    /// <summary>
    /// Tests that ValidateChannelConfiguration returns a failed result when given a channel configuration with a zero timeout.
    /// </summary>
    [Fact]
    public void ValidateChannelConfiguration_WithZeroTimeout_ReturnsFailure()
    {
        // Arrange
        var config = new ChannelConfiguration
        {
            DisplayName = "Test",
            WebhookUrl = "https://example.com/webhook",
            TargetId = "123",
            TimeoutMs = 0,
            CustomHeaders = new Dictionary<string, string>()
        };

        // Act
        var result = _validationService.ValidateChannelConfiguration(config);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Timeout must be greater than 0");
    }

    /// <summary>
    /// Tests that ValidateChannelConfiguration returns a failed result when given a channel configuration with a negative max retries.
    /// </summary>
    [Fact]
    public void ValidateChannelConfiguration_WithNegativeMaxRetries_ReturnsFailure()
    {
        // Arrange
        var config = new ChannelConfiguration
        {
            DisplayName = "Test",
            WebhookUrl = "https://example.com/webhook",
            TargetId = "123",
            TimeoutMs = 5000,
            MaxRetries = -1,
            CustomHeaders = new Dictionary<string, string>()
        };

        // Act
        var result = _validationService.ValidateChannelConfiguration(config);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Max retries cannot be negative");
    }

    /// <summary>
    /// Tests that ValidateChannelConfiguration returns a failed result when given a channel configuration with null custom headers.
    /// </summary>
    [Fact]
    public void ValidateChannelConfiguration_WithNullCustomHeaders_ReturnsFailure()
    {
        // Arrange
        var config = new ChannelConfiguration
        {
            DisplayName = "Test",
            WebhookUrl = "https://example.com/webhook",
            TargetId = "123",
            TimeoutMs = 5000,
            MaxRetries = 3,
            CustomHeaders = null!
        };

        // Act
        var result = _validationService.ValidateChannelConfiguration(config);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Custom headers cannot be null");
    }

    #endregion

    #region URL and Email Validation Tests

    /// <summary>
    /// Tests that IsValidUrl returns true for a valid HTTPS URL.
    /// </summary>
    [Fact]
    public void IsValidUrl_WithHttpsUrl_ReturnsTrue()
    {
        // Act & Assert
        _validationService.IsValidUrl("https://example.com").Should().BeTrue();
    }

    /// <summary>
    /// Tests that IsValidUrl returns true for a valid HTTP URL.
    /// </summary>
    [Fact]
    public void IsValidUrl_WithHttpUrl_ReturnsTrue()
    {
        // Act & Assert
        _validationService.IsValidUrl("http://example.com").Should().BeTrue();
    }

    /// <summary>
    /// Tests that IsValidUrl returns false for an invalid URL.
    /// </summary>
    [Fact]
    public void IsValidUrl_WithInvalidUrl_ReturnsFalse()
    {
        // Act & Assert
        _validationService.IsValidUrl("not-a-url").Should().BeFalse();
    }

    /// <summary>
    /// Tests that IsValidUrl returns false for an FTP URL.
    /// </summary>
    [Fact]
    public void IsValidUrl_WithFtpUrl_ReturnsFalse()
    {
        // Act & Assert
        _validationService.IsValidUrl("ftp://example.com").Should().BeFalse();
    }

    /// <summary>
    /// Tests that IsValidUrl returns false for an empty string.
    /// </summary>
    [Fact]
    public void IsValidUrl_WithEmptyString_ReturnsFalse()
    {
        // Act & Assert
        _validationService.IsValidUrl("").Should().BeFalse();
    }

    /// <summary>
    /// Tests that IsValidUrl returns false for a null string.
    /// </summary>
    [Fact]
    public void IsValidUrl_WithNullString_ReturnsFalse()
    {
        // Act & Assert
        _validationService.IsValidUrl(null!).Should().BeFalse();
    }

    /// <summary>
    /// Tests that IsValidUrl returns true for a URL with a port.
    /// </summary>
    [Fact]
    public void IsValidUrl_WithUrlWithPort_ReturnsTrue()
    {
        // Act & Assert
        _validationService.IsValidUrl("https://example.com:8080/path").Should().BeTrue();
    }

    /// <summary>
    /// Tests that IsValidEmail returns true for a valid email address.
    /// </summary>
    [Fact]
    public void IsValidEmail_WithValidEmail_ReturnsTrue()
    {
        // Act & Assert
        _validationService.IsValidEmail("test@example.com").Should().BeTrue();
    }

    /// <summary>
    /// Tests that IsValidEmail returns false for an invalid email address.
    /// </summary>
    [Fact]
    public void IsValidEmail_WithInvalidEmail_ReturnsFalse()
    {
        // Act & Assert
        _validationService.IsValidEmail("not-an-email").Should().BeFalse();
    }

    /// <summary>
    /// Tests that IsValidEmail returns false for an empty string.
    /// </summary>
    [Fact]
    public void IsValidEmail_WithEmptyString_ReturnsFalse()
    {
        // Act & Assert
        _validationService.IsValidEmail("").Should().BeFalse();
    }

    /// <summary>
    /// Tests that IsValidEmail returns false for a null string.
    /// </summary>
    [Fact]
    public void IsValidEmail_WithNullString_ReturnsFalse()
    {
        // Act & Assert
        _validationService.IsValidEmail(null!).Should().BeFalse();
    }

    /// <summary>
    /// Tests that IsValidEmail returns false for an email address missing a domain.
    /// </summary>
    [Fact]
    public void IsValidEmail_WithEmailMissingDomain_ReturnsFalse()
    {
        // Act & Assert
        _validationService.IsValidEmail("test@").Should().BeFalse();
    }

    #endregion
}
