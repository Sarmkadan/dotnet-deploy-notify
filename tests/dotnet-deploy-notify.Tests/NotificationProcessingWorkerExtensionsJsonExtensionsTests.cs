using System;
using Xunit;
using DotNetDeployNotify.BackgroundWorkers;

namespace DotNetDeployNotify.Tests.BackgroundWorkers
{
    public class NotificationProcessingWorkerExtensionsJsonExtensionsTests
    {
        [Fact]
        public void ToJson_WithValidInstance_ReturnsJsonString()
        {
            // Arrange
            // We obtain an instance via deserialization of an empty object to avoid 
            // needing to know the specific constructor parameters of NotificationProcessingWorker.
            var instance = NotificationProcessingWorkerExtensionsJsonExtensions.FromJson("{}");

            // Act
            // Explicitly call the static method to avoid ambiguity with other extension classes.
            string json = NotificationProcessingWorkerExtensionsJsonExtensions.ToJson(instance!);

            // Assert
            Assert.NotNull(json);
            Assert.NotEmpty(json);
            Assert.StartsWith("{", json);
            Assert.EndsWith("}", json);
        }

        [Fact]
        public void ToJson_WithIndented_ReturnsFormattedJson()
        {
            // Arrange
            var instance = NotificationProcessingWorkerExtensionsJsonExtensions.FromJson("{}");

            // Act
            string json = NotificationProcessingWorkerExtensionsJsonExtensions.ToJson(instance!, indented: true);

            // Assert
            Assert.Contains(Environment.NewLine, json);
        }

        [Fact]
        public void ToJson_WithNullInstance_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                NotificationProcessingWorkerExtensionsJsonExtensions.ToJson(null!));
        }

        [Fact]
        public void FromJson_WithValidJson_ReturnsInstance()
        {
            // Arrange
            string json = "{}";

            // Act
            var result = NotificationProcessingWorkerExtensionsJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void FromJson_WithNullInput_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                NotificationProcessingWorkerExtensionsJsonExtensions.FromJson(null!));
        }

        [Fact]
        public void TryFromJson_WithValidJson_ReturnsTrueAndInstance()
        {
            // Arrange
            string json = "{}";

            // Act
            bool success = NotificationProcessingWorkerExtensionsJsonExtensions.TryFromJson(json, out var result);

            // Assert
            Assert.True(success);
            Assert.NotNull(result);
        }

        [Fact]
        public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
        {
            // Arrange
            string invalidJson = "this is not json";

            // Act
            bool success = NotificationProcessingWorkerExtensionsJsonExtensions.TryFromJson(invalidJson, out var result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
        }

        [Fact]
        public void TryFromJson_WithNullInput_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                NotificationProcessingWorkerExtensionsJsonExtensions.TryFromJson(null!, out _));
        }
    }
}
