using Xunit;
using System.ComponentModel.DataAnnotations;
using DotNetDeployNotify.Configuration;

namespace DotnetDeployNotify.Tests
{
    public class DotnetDeployNotifyOptionsTests
    {
        [Fact]
        public void NotificationConfig_HappyPath()
        {
            // Arrange
            var options = new DotnetDeployNotifyOptions();

            // Act
            var notificationConfig = options.Notification;

            // Assert
            Assert.NotNull(notificationConfig);
            Assert.Equal(3, notificationConfig.MaxRetries);
            Assert.Equal(10000, notificationConfig.WebhookTimeoutMs);
            Assert.Equal(5000, notificationConfig.RetryDelayMs);
            Assert.True(notificationConfig.AutoProcessNotifications);
            Assert.Equal(30, notificationConfig.ProcessingIntervalSeconds);
        }

        [Fact]
        public void NotificationConfig_BoundaryValues()
        {
            // Arrange
            var options = new DotnetDeployNotifyOptions();

            // Act
            options.Notification.MaxRetries = 0;
            options.Notification.WebhookTimeoutMs = 100;
            options.Notification.RetryDelayMs = 100;
            options.Notification.ProcessingIntervalSeconds = 1;

            // Assert
            Assert.Equal(0, options.Notification.MaxRetries);
            Assert.Equal(100, options.Notification.WebhookTimeoutMs);
            Assert.Equal(100, options.Notification.RetryDelayMs);
            Assert.Equal(1, options.Notification.ProcessingIntervalSeconds);
        }

        [Fact]
        public void NotificationConfig_InvalidValues()
        {
            // Arrange
            var options = new DotnetDeployNotifyOptions();

            // Act
            options.Notification.MaxRetries = -1;
            options.Notification.WebhookTimeoutMs = 60001;
            options.Notification.RetryDelayMs = 60001;
            options.Notification.ProcessingIntervalSeconds = 3601;

            // Act - the properties themselves do not throw; validation reports the problems
            var problems = DotnetDeployNotifyOptionsValidation.Validate(options.Notification);

            // Assert
            Assert.Contains(problems, p => p.StartsWith("Notification.MaxRetries must be between 0 and 100"));
            Assert.Contains(problems, p => p.StartsWith("Notification.WebhookTimeoutMs must be between 100 and 60000"));
            Assert.Contains(problems, p => p.StartsWith("Notification.RetryDelayMs must be between 100 and 60000"));
            Assert.Contains(problems, p => p.StartsWith("Notification.ProcessingIntervalSeconds must be between 1 and 3600"));
        }

        [Fact]
        public void CanaryOptions_HappyPath()
        {
            // Arrange
            var options = new DotnetDeployNotifyOptions();

            // Act
            var canaryOptions = options.Canary;

            // Assert
            Assert.NotNull(canaryOptions);
        }

        [Fact]
        public void DotnetDeployNotifyOptions_RequiredProperties()
        {
            // Arrange
            var options = new DotnetDeployNotifyOptions();

            // Act
            options.Notification.StorageType = string.Empty;

            var problems = DotnetDeployNotifyOptionsValidation.Validate(options.Notification);

            // Assert
            Assert.Contains(problems, p => p.StartsWith("Notification.StorageType is required"));
        }
    }
}
