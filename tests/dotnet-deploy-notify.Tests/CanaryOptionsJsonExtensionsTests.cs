using System;
using System.Text.Json;
using DotNetDeployNotify.Configuration;
using Xunit;

namespace dotnet_deploy_notify.Tests
{
    public class CanaryOptionsJsonExtensionsTests
    {
        [Fact]
        public void ToJson_WithDefaultOptions_ReturnsNonEmptyString()
        {
            // Arrange
            var options = new CanaryOptions();

            // Act
            string json = CanaryOptionsJsonExtensions.ToJson(options);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(json));
        }

        [Fact]
        public void ToJson_WithIndentation_ContainsWhitespace()
        {
            // Arrange
            var options = new CanaryOptions();

            // Act
            string json = CanaryOptionsJsonExtensions.ToJson(options, true);

            // Assert
            Assert.Contains("\n", json);
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsExpectedOptions()
        {
            // Arrange
            string json = JsonSerializer.Serialize(new CanaryOptions());

            // Act
            CanaryOptions? options = CanaryOptionsJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(options);
        }

        [Fact]
        public void FromJson_NullJson_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => CanaryOptionsJsonExtensions.FromJson(null));
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrueAndOptions()
        {
            // Arrange
            string json = JsonSerializer.Serialize(new CanaryOptions());

            // Act
            bool success = CanaryOptionsJsonExtensions.TryFromJson(json, out CanaryOptions? options);

            // Assert
            Assert.True(success);
            Assert.NotNull(options);
        }

        [Fact]
        public void TryFromJson_NullJson_ReturnsFalse()
        {
            // Act
            bool success = CanaryOptionsJsonExtensions.TryFromJson(null, out CanaryOptions? options);

            // Assert
            Assert.False(success);
            Assert.Null(options);
        }
    }
}
