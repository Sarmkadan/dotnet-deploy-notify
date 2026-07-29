// tests/dotnet-deploy-notify.Tests/DeploymentNotificationExtensionsTests.cs

using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using Xunit;

namespace DotNetDeployNotify.Tests.Core.Models
{
    public class DeploymentNotificationExtensionsTests
    {
        [Fact]
        public void IsSuccessful_HappyPath_ReturnsTrue()
        {
            // Arrange
            var notification = new DeploymentNotification
            {
                Status = BuildStatus.Success
            };

            // Act
            var result = DeploymentNotificationExtensions.IsSuccessful(notification);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsSuccessful_NullNotification_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => DeploymentNotificationExtensions.IsSuccessful(null));
        }

        [Fact]
        public void IsFailed_HappyPath_ReturnsTrue()
        {
            // Arrange
            var notification = new DeploymentNotification
            {
                Status = BuildStatus.Failed
            };

            // Act
            var result = DeploymentNotificationExtensions.IsFailed(notification);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsFailed_NullNotification_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => DeploymentNotificationExtensions.IsFailed(null));
        }

        [Fact]
        public void GetDeploymentUrl_HappyPath_ReturnsDeploymentUrl()
        {
            // Arrange
            var notification = new DeploymentNotification
            {
                RepositoryUrl = "https://github.com/user/repo",
                CommitHash = "abc123"
            };

            // Act
            var result = DeploymentNotificationExtensions.GetDeploymentUrl(notification);

            // Assert
            Assert.Equal("https://github.com/user/repo/commit/abc123", result);
        }

        [Fact]
        public void GetDeploymentUrl_NullNotification_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => DeploymentNotificationExtensions.GetDeploymentUrl(null));
        }

        [Fact]
        public void GetDeploymentUrl_EmptyRepositoryUrl_ReturnsNull()
        {
            // Arrange
            var notification = new DeploymentNotification
            {
                RepositoryUrl = string.Empty,
                CommitHash = "abc123"
            };

            // Act
            var result = DeploymentNotificationExtensions.GetDeploymentUrl(notification);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetFormattedDuration_HappyPath_ReturnsFormattedDuration()
        {
            // Arrange
            var notification = new DeploymentNotification
            {
                DurationSeconds = 3600
            };

            // Act
            var result = DeploymentNotificationExtensions.GetFormattedDuration(notification);

            // Assert
            Assert.Equal("1h", result);
        }

        [Fact]
        public void GetFormattedDuration_NullNotification_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => DeploymentNotificationExtensions.GetFormattedDuration(null));
        }

        [Fact]
        public void GetFormattedDuration_ZeroDuration_ReturnsNA()
        {
            // Arrange
            var notification = new DeploymentNotification
            {
                DurationSeconds = 0
            };

            // Act
            var result = DeploymentNotificationExtensions.GetFormattedDuration(notification);

            // Assert
            Assert.Equal("N/A", result);
        }
    }
}
