#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using FluentAssertions;
using Xunit;

#nullable disable

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Unit tests for ServiceExtensionsValidation class
/// </summary>
public class ServiceExtensionsValidationTests
{
    #region DeploymentNotification.Validate() Tests

    [Fact]
    public void Validate_DeploymentNotification_WithAllValidProperties_ReturnsEmptyList()
    {
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "MyProject",
            Version = "1.0.0",
            Status = BuildStatus.Success,
            BranchName = "main",
            CommitHash = "abc123",
            TargetEnvironment = global::DotNetDeployNotify.Core.Environment.Development,
            Priority = NotificationPriority.Normal,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            Channels = new List<NotificationChannel> { NotificationChannel.Telegram },
            Message = "Test message",
            CommitAuthor = "Test Author",
            RepositoryUrl = "https://github.com/test/repo",
            BuildUrl = "https://ci.example.com/build/123"
        };

        // Act
        var result = notification.Validate();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_DeploymentNotification_WithNullValue_ThrowsArgumentNullException()
    {
        // Arrange
        DeploymentNotification? notification = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => notification!.Validate());
    }

    [Fact]
    public void Validate_DeploymentNotification_WithEmptyProjectName_ReturnsError()
    {
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "",
            Version = "1.0.0",
            Status = BuildStatus.Success,
            BranchName = "main",
            CommitHash = "abc123",
            TargetEnvironment = global::DotNetDeployNotify.Core.Environment.Development,
            Priority = NotificationPriority.Normal,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            Channels = new List<NotificationChannel> { NotificationChannel.Telegram },
            Message = "Test",
            CommitAuthor = "Author",
            RepositoryUrl = "https://github.com/test/repo",
            BuildUrl = "https://ci.example.com/build/123"
        };

        // Act
        var result = notification.Validate();

        // Assert
        result.Should().ContainSingle("ProjectName is required and cannot be empty");
    }

    [Fact]
    public void Validate_DeploymentNotification_WithWhitespaceProjectName_ReturnsError()
    {
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "   ",
            Version = "1.0.0",
            Status = BuildStatus.Success,
            BranchName = "main",
            CommitHash = "abc123",
            TargetEnvironment = global::DotNetDeployNotify.Core.Environment.Development,
            Priority = NotificationPriority.Normal,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            Channels = new List<NotificationChannel> { NotificationChannel.Telegram },
            Message = "Test",
            CommitAuthor = "Author",
            RepositoryUrl = "https://github.com/test/repo",
            BuildUrl = "https://ci.example.com/build/123"
        };

        // Act
        var result = notification.Validate();

        // Assert
        result.Should().ContainSingle("ProjectName is required and cannot be empty");
    }

    [Fact]
    public void Validate_DeploymentNotification_WithEmptyVersion_ReturnsError()
    {
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "MyProject",
            Version = "",
            Status = BuildStatus.Success,
            BranchName = "main",
            CommitHash = "abc123",
            TargetEnvironment = global::DotNetDeployNotify.Core.Environment.Development,
            Priority = NotificationPriority.Normal,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            Channels = new List<NotificationChannel> { NotificationChannel.Telegram }
        };

        // Act
        var result = notification.Validate();

        // Assert
        result.Should().ContainSingle("Version is required and cannot be empty");
    }

    [Fact]
    public void Validate_DeploymentNotification_WithEmptyBranchName_ReturnsError()
    {
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "MyProject",
            Version = "1.0.0",
            Status = BuildStatus.Success,
            BranchName = "",
            CommitHash = "abc123",
            TargetEnvironment = global::DotNetDeployNotify.Core.Environment.Development,
            Priority = NotificationPriority.Normal,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            Channels = new List<NotificationChannel> { NotificationChannel.Telegram }
        };

        // Act
        var result = notification.Validate();

        // Assert
        result.Should().ContainSingle("BranchName is required and cannot be empty");
    }

    [Fact]
    public void Validate_DeploymentNotification_WithEmptyCommitHash_ReturnsError()
    {
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "MyProject",
            Version = "1.0.0",
            Status = BuildStatus.Success,
            BranchName = "main",
            CommitHash = "",
            TargetEnvironment = global::DotNetDeployNotify.Core.Environment.Development,
            Priority = NotificationPriority.Normal,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            Channels = new List<NotificationChannel> { NotificationChannel.Telegram }
        };

        // Act
        var result = notification.Validate();

        // Assert
        result.Should().ContainSingle("CommitHash is required and cannot be empty");
    }

    [Fact]
    public void Validate_DeploymentNotification_WithDefaultStatus_ReturnsError()
    {
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "MyProject",
            Version = "1.0.0",
            Status = default,
            BranchName = "main",
            CommitHash = "abc123",
            TargetEnvironment = global::DotNetDeployNotify.Core.Environment.Development,
            Priority = NotificationPriority.Normal,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            Channels = new List<NotificationChannel> { NotificationChannel.Telegram }
        };

        // Act
        var result = notification.Validate();

        // Assert
        result.Should().ContainSingle("Status must be a valid BuildStatus value");
    }

    [Fact]
    public void Validate_DeploymentNotification_WithDefaultTargetEnvironment_ReturnsError()
    {
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "MyProject",
            Version = "1.0.0",
            Status = BuildStatus.Success,
            BranchName = "main",
            CommitHash = "abc123",
            TargetEnvironment = default,
            Priority = NotificationPriority.Normal,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            Channels = new List<NotificationChannel> { NotificationChannel.Telegram }
        };

        // Act
        var result = notification.Validate();

        // Assert
        result.Should().ContainSingle("TargetEnvironment must be a valid Environment value");
    }

    [Fact]
    public void Validate_DeploymentNotification_WithNullChannels_ReturnsError()
    {
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "MyProject",
            Version = "1.0.0",
            Status = BuildStatus.Success,
            BranchName = "main",
            CommitHash = "abc123",
            TargetEnvironment = global::DotNetDeployNotify.Core.Environment.Development,
            Priority = NotificationPriority.Normal,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            Channels = null!
        };

        // Act
        var result = notification.Validate();

        // Assert
        result.Should().ContainSingle("Channels collection must contain at least one channel");
    }

    [Fact]
    public void Validate_DeploymentNotification_WithEmptyChannels_ReturnsError()
    {
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "MyProject",
            Version = "1.0.0",
            Status = BuildStatus.Success,
            BranchName = "main",
            CommitHash = "abc123",
            TargetEnvironment = global::DotNetDeployNotify.Core.Environment.Development,
            Priority = NotificationPriority.Normal,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            Channels = new List<NotificationChannel>()
        };

        // Act
        var result = notification.Validate();

        // Assert
        result.Should().ContainSingle("Channels collection must contain at least one channel");
    }

    [Fact]
    public void Validate_DeploymentNotification_WithNegativeDurationSeconds_ReturnsError()
    {
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "MyProject",
            Version = "1.0.0",
            Status = BuildStatus.Success,
            BranchName = "main",
            CommitHash = "abc123",
            TargetEnvironment = global::DotNetDeployNotify.Core.Environment.Development,
            Priority = NotificationPriority.Normal,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            DurationSeconds = -1,
            Channels = new List<NotificationChannel> { NotificationChannel.Telegram }
        };

        // Act
        var result = notification.Validate();

        // Assert
        result.Should().ContainSingle("DurationSeconds cannot be negative");
    }

    [Fact]
    public void Validate_DeploymentNotification_WithDefaultCreatedAt_ReturnsError()
    {
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "MyProject",
            Version = "1.0.0",
            Status = BuildStatus.Success,
            BranchName = "main",
            CommitHash = "abc123",
            TargetEnvironment = global::DotNetDeployNotify.Core.Environment.Development,
            Priority = NotificationPriority.Normal,
            CreatedAt = default,
            Channels = new List<NotificationChannel> { NotificationChannel.Telegram }
        };

        // Act
        var result = notification.Validate();

        // Assert
        result.Should().ContainSingle("CreatedAt must be a valid DateTime");
    }

    [Fact]
    public void Validate_DeploymentNotification_WithFutureCreatedAt_ReturnsError()
    {
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "MyProject",
            Version = "1.0.0",
            Status = BuildStatus.Success,
            BranchName = "main",
            CommitHash = "abc123",
            TargetEnvironment = global::DotNetDeployNotify.Core.Environment.Development,
            Priority = NotificationPriority.Normal,
            CreatedAt = DateTime.UtcNow.AddHours(1),
            Channels = new List<NotificationChannel> { NotificationChannel.Telegram }
        };

        // Act
        var result = notification.Validate();

        // Assert
        result.Should().ContainSingle("CreatedAt cannot be in the future");
    }

    [Fact]
    public void Validate_DeploymentNotification_WithDefaultPriority_ReturnsError()
    {
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "MyProject",
            Version = "1.0.0",
            Status = BuildStatus.Success,
            BranchName = "main",
            CommitHash = "abc123",
            TargetEnvironment = global::DotNetDeployNotify.Core.Environment.Development,
            Priority = default,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            Channels = new List<NotificationChannel> { NotificationChannel.Telegram }
        };

        // Act
        var result = notification.Validate();

        // Assert
        result.Should().ContainSingle("Priority must be a valid NotificationPriority value");
    }

    [Fact]
    public void Validate_DeploymentNotification_WithMultipleProblems_ReturnsAllErrors()
    {
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "",
            Version = "",
            Status = default,
            BranchName = "",
            CommitHash = "",
            TargetEnvironment = default,
            Priority = default,
            CreatedAt = DateTime.UtcNow.AddHours(1),
            Channels = new List<NotificationChannel>()
        };

        // Act
        var result = notification.Validate();

        // Assert
        result.Should().HaveCount(8);
        result.Should().Contain("ProjectName is required and cannot be empty");
        result.Should().Contain("Version is required and cannot be empty");
        result.Should().Contain("BranchName is required and cannot be empty");
        result.Should().Contain("CommitHash is required and cannot be empty");
        result.Should().Contain("Status must be a valid BuildStatus value");
        result.Should().Contain("TargetEnvironment must be a valid Environment value");
        result.Should().Contain("Channels collection must contain at least one channel");
        result.Should().Contain("CreatedAt cannot be in the future");
    }

    #endregion

    #region NotificationResult.Validate() Tests

    [Fact]
    public void Validate_NotificationResult_WithAllValidProperties_ReturnsEmptyList()
    {
        // Arrange
        var result = new NotificationResult
        {
            NotificationId = "notification-123",
            ConfigurationId = "config-456",
            Channel = NotificationChannel.Slack,
            Status = DeliveryStatus.Delivered,
            DurationMs = 1000,
            AttemptNumber = 1,
            AttemptedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        // Act
        var validationResult = result.Validate();

        // Assert
        validationResult.Should().BeEmpty();
    }

    [Fact]
    public void Validate_NotificationResult_WithNullValue_ThrowsArgumentNullException()
    {
        // Arrange
        NotificationResult? notificationResult = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => notificationResult!.Validate());
    }

    [Fact]
    public void Validate_NotificationResult_WithEmptyNotificationId_ReturnsError()
    {
        // Arrange
        var result = new NotificationResult
        {
            NotificationId = "",
            ConfigurationId = "config-456",
            Channel = NotificationChannel.Slack,
            Status = DeliveryStatus.Delivered,
            DurationMs = 1000,
            AttemptNumber = 1,
            AttemptedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        // Act
        var validationResult = result.Validate();

        // Assert
        validationResult.Should().ContainSingle("NotificationId is required and cannot be empty");
    }

    [Fact]
    public void Validate_NotificationResult_WithEmptyConfigurationId_ReturnsError()
    {
        // Arrange
        var result = new NotificationResult
        {
            NotificationId = "notification-123",
            ConfigurationId = "",
            Channel = NotificationChannel.Slack,
            Status = DeliveryStatus.Delivered,
            DurationMs = 1000,
            AttemptNumber = 1,
            AttemptedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        // Act
        var validationResult = result.Validate();

        // Assert
        validationResult.Should().ContainSingle("ConfigurationId is required and cannot be empty");
    }

    [Fact]
    public void Validate_NotificationResult_WithDefaultChannel_ReturnsError()
    {
        // Arrange
        var result = new NotificationResult
        {
            NotificationId = "notification-123",
            ConfigurationId = "config-456",
            Channel = default,
            Status = DeliveryStatus.Delivered,
            DurationMs = 1000,
            AttemptNumber = 1,
            AttemptedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        // Act
        var validationResult = result.Validate();

        // Assert
        validationResult.Should().ContainSingle("Channel must be a valid NotificationChannel value");
    }

    [Fact]
    public void Validate_NotificationResult_WithDefaultStatus_ReturnsError()
    {
        // Arrange
        var result = new NotificationResult
        {
            NotificationId = "notification-123",
            ConfigurationId = "config-456",
            Channel = NotificationChannel.Slack,
            Status = default,
            DurationMs = 1000,
            AttemptNumber = 1,
            AttemptedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        // Act
        var validationResult = result.Validate();

        // Assert
        validationResult.Should().ContainSingle("Status must be a valid DeliveryStatus value");
    }

    [Fact]
    public void Validate_NotificationResult_WithNegativeDurationMs_ReturnsError()
    {
        // Arrange
        var result = new NotificationResult
        {
            NotificationId = "notification-123",
            ConfigurationId = "config-456",
            Channel = NotificationChannel.Slack,
            Status = DeliveryStatus.Delivered,
            DurationMs = -100,
            AttemptNumber = 1,
            AttemptedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        // Act
        var validationResult = result.Validate();

        // Assert
        validationResult.Should().ContainSingle("DurationMs cannot be negative");
    }

    [Fact]
    public void Validate_NotificationResult_WithZeroAttemptNumber_ReturnsError()
    {
        // Arrange
        var result = new NotificationResult
        {
            NotificationId = "notification-123",
            ConfigurationId = "config-456",
            Channel = NotificationChannel.Slack,
            Status = DeliveryStatus.Delivered,
            DurationMs = 1000,
            AttemptNumber = 0,
            AttemptedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        // Act
        var validationResult = result.Validate();

        // Assert
        validationResult.Should().ContainSingle("AttemptNumber must be at least 1");
    }

    [Fact]
    public void Validate_NotificationResult_WithDefaultAttemptedAt_ReturnsError()
    {
        // Arrange
        var result = new NotificationResult
        {
            NotificationId = "notification-123",
            ConfigurationId = "config-456",
            Channel = NotificationChannel.Slack,
            Status = DeliveryStatus.Delivered,
            DurationMs = 1000,
            AttemptNumber = 1,
            AttemptedAt = default
        };

        // Act
        var validationResult = result.Validate();

        // Assert
        validationResult.Should().ContainSingle("AttemptedAt must be a valid DateTime");
    }

    [Fact]
    public void Validate_NotificationResult_WithFutureAttemptedAt_ReturnsError()
    {
        // Arrange
        var result = new NotificationResult
        {
            NotificationId = "notification-123",
            ConfigurationId = "config-456",
            Channel = NotificationChannel.Slack,
            Status = DeliveryStatus.Delivered,
            DurationMs = 1000,
            AttemptNumber = 1,
            AttemptedAt = DateTime.UtcNow.AddHours(1)
        };

        // Act
        var validationResult = result.Validate();

        // Assert
        validationResult.Should().ContainSingle("AttemptedAt cannot be in the future");
    }

    [Fact]
    public void Validate_NotificationResult_WithMultipleProblems_ReturnsAllErrors()
    {
        // Arrange
        var result = new NotificationResult
        {
            NotificationId = "",
            ConfigurationId = "",
            Channel = default,
            Status = default,
            DurationMs = -100,
            AttemptNumber = 0,
            AttemptedAt = DateTime.UtcNow.AddHours(1)
        };

        // Act
        var validationResult = result.Validate();

        // Assert
        validationResult.Should().HaveCount(7);
        validationResult.Should().Contain("NotificationId is required and cannot be empty");
        validationResult.Should().Contain("ConfigurationId is required and cannot be empty");
        validationResult.Should().Contain("Channel must be a valid NotificationChannel value");
        validationResult.Should().Contain("Status must be a valid DeliveryStatus value");
        validationResult.Should().Contain("DurationMs cannot be negative");
        validationResult.Should().Contain("AttemptNumber must be at least 1");
        validationResult.Should().Contain("AttemptedAt cannot be in the future");
    }

    #endregion

    #region DeploymentNotification.IsValid() Tests

    [Fact]
    public void IsValid_DeploymentNotification_WithValidProperties_ReturnsTrue()
    {
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "MyProject",
            Version = "1.0.0",
            Status = BuildStatus.Success,
            BranchName = "main",
            CommitHash = "abc123",
            TargetEnvironment = global::DotNetDeployNotify.Core.Environment.Development,
            Priority = NotificationPriority.Normal,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            Channels = new List<NotificationChannel> { NotificationChannel.Telegram }
        };

        // Act
        var isValid = notification.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_DeploymentNotification_WithInvalidProperties_ReturnsFalse()
    {
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "",
            Version = "",
            Status = BuildStatus.Success,
            BranchName = "",
            CommitHash = "",
            TargetEnvironment = global::DotNetDeployNotify.Core.Environment.Development,
            Priority = NotificationPriority.Normal,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            Channels = new List<NotificationChannel>()
        };

        // Act
        var isValid = notification.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_DeploymentNotification_WithNullValue_ThrowsArgumentNullException()
    {
        // Arrange
        DeploymentNotification? notification = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => notification!.IsValid());
    }

    #endregion

    #region NotificationResult.IsValid() Tests

    [Fact]
    public void IsValid_NotificationResult_WithValidProperties_ReturnsTrue()
    {
        // Arrange
        var result = new NotificationResult
        {
            NotificationId = "notification-123",
            ConfigurationId = "config-456",
            Channel = NotificationChannel.Slack,
            Status = DeliveryStatus.Delivered,
            DurationMs = 1000,
            AttemptNumber = 1,
            AttemptedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        // Act
        var isValid = result.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_NotificationResult_WithInvalidProperties_ReturnsFalse()
    {
        // Arrange
        var result = new NotificationResult
        {
            NotificationId = "",
            ConfigurationId = "",
            Channel = NotificationChannel.Slack,
            Status = DeliveryStatus.Delivered,
            DurationMs = -100,
            AttemptNumber = 1,
            AttemptedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        // Act
        var isValid = result.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_NotificationResult_WithNullValue_ThrowsArgumentNullException()
    {
        // Arrange
        NotificationResult? result = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result!.IsValid());
    }

    #endregion

    #region DeploymentNotification.EnsureValid() Tests

    [Fact]
    public void EnsureValid_DeploymentNotification_WithValidProperties_DoesNotThrow()
    {
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "MyProject",
            Version = "1.0.0",
            Status = BuildStatus.Success,
            BranchName = "main",
            CommitHash = "abc123",
            TargetEnvironment = global::DotNetDeployNotify.Core.Environment.Development,
            Priority = NotificationPriority.Normal,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            Channels = new List<NotificationChannel> { NotificationChannel.Telegram }
        };

        // Act
        Action act = () => notification.EnsureValid();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureValid_DeploymentNotification_WithInvalidProperties_ThrowsArgumentException()
    {
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "",
            Version = "",
            Status = BuildStatus.Success,
            BranchName = "main",
            CommitHash = "abc123",
            TargetEnvironment = global::DotNetDeployNotify.Core.Environment.Development,
            Priority = NotificationPriority.Normal,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            Channels = new List<NotificationChannel>()
        };

        // Act
        Action act = () => notification.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EnsureValid_DeploymentNotification_WithNullValue_ThrowsArgumentNullException()
    {
        // Arrange
        DeploymentNotification? notification = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => notification!.EnsureValid());
    }

    #endregion

    #region NotificationResult.EnsureValid() Tests

    [Fact]
    public void EnsureValid_NotificationResult_WithValidProperties_DoesNotThrow()
    {
        // Arrange
        var result = new NotificationResult
        {
            NotificationId = "notification-123",
            ConfigurationId = "config-456",
            Channel = NotificationChannel.Slack,
            Status = DeliveryStatus.Delivered,
            DurationMs = 1000,
            AttemptNumber = 1,
            AttemptedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        // Act
        Action act = () => result.EnsureValid();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureValid_NotificationResult_WithInvalidProperties_ThrowsArgumentException()
    {
        // Arrange
        var result = new NotificationResult
        {
            NotificationId = "",
            ConfigurationId = "",
            Channel = NotificationChannel.Slack,
            Status = DeliveryStatus.Delivered,
            DurationMs = -100,
            AttemptNumber = 1,
            AttemptedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        // Act
        Action act = () => result.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EnsureValid_NotificationResult_WithNullValue_ThrowsArgumentNullException()
    {
        // Arrange
        NotificationResult? result = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result!.EnsureValid());
    }

    #endregion
}
