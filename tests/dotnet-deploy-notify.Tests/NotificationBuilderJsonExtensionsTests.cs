using Xunit;
using System;
using System.Text.Json;
using DotNetDeployNotify.Services;
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;

namespace dotnet_deploy_notify.Tests
{
    public class NotificationBuilderJsonExtensionsTests
    {
        [Fact]
        public void ToJson_WithValidBuilder_ReturnsNonEmptyString()
        {
            // Arrange
            var builder = new NotificationBuilder()
                .WithProject("TestProject", "1.0.0")
                .WithStatus(BuildStatus.Success, "Deployment successful");

            // Act
            string json = builder.ToJson();

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(json));
            Assert.Contains("testProject", json); // Check camelCase naming policy
        }

        [Fact]
        public void ToJson_WithIndented_ReturnsFormattedString()
        {
            // Arrange
            var builder = new NotificationBuilder()
                .WithProject("P", "1.0")
                .WithStatus(BuildStatus.Started, "Starting");

            // Act
            string json = builder.ToJson(indented: true);

            // Assert
            Assert.Contains("\n", json);
            Assert.Contains("  ", json); // Check for indentation
        }

        [Fact]
        public void ToJson_WithNullBuilder_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ((NotificationBuilder)null!).ToJson());
        }

        [Fact]
        public void FromJson_WithValidJson_ReturnsNotificationBuilder()
        {
            // Arrange
            var notification = new DeploymentNotification
            {
                ProjectName = "MyProject",
                Version = "2.0.0",
                Status = BuildStatus.Failed,
                Message = "Build failed"
            };
            string json = JsonSerializer.Serialize(notification);

            // Act
            var result = NotificationBuilderJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void FromJson_WithNullOrEmptyInput_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => NotificationBuilderJsonExtensions.FromJson(null));
            Assert.Throws<ArgumentException>(() => NotificationBuilderJsonExtensions.FromJson(string.Empty));
        }

        [Fact]
        public void FromJson_WithInvalidJson_ThrowsJsonException()
        {
            // Arrange
            string invalidJson = "{ this is not valid json }";

            // Act & Assert
            Assert.Throws<JsonException>(() => NotificationBuilderJsonExtensions.FromJson(invalidJson));
        }

        [Fact]
        public void TryFromJson_WithValidJson_ReturnsTrueAndBuilder()
        {
            // Arrange
            var notification = new DeploymentNotification
            {
                ProjectName = "ProjectX",
                Status = BuildStatus.Success
            };
            string json = JsonSerializer.Serialize(notification);

            // Act
            bool success = NotificationBuilderJsonExtensions.TryFromJson(json, out var builder);

            // Assert
            Assert.True(success);
            Assert.NotNull(builder);
        }

        [Fact]
        public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
        {
            // Act
            bool success = NotificationBuilderJsonExtensions.TryFromJson("invalid", out var builder);

            // Assert
            Assert.False(success);
            Assert.Null(builder);
        }
    }
}
