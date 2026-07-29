using Xunit;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetDeployNotify.Results;

namespace DotNetDeployNotify.Tests
{
    public class ResultJsonExtensionsTests
    {
        [Fact]
        public void ToJson_HappyPath_ReturnsJsonString()
        {
            // Arrange
            var result = Result.Ok();

            // Act
            var json = ResultJsonExtensions.ToJson(result);

            // Assert
            Assert.NotNull(json);
        }

        [Fact]
        public void ToJson_T_HappyPath_ReturnsJsonString()
        {
            // Arrange
            var result = Result<int>.Ok(42);

            // Act
            var json = ResultJsonExtensions.ToJson(result);

            // Assert
            Assert.NotNull(json);
        }

        [Fact]
        public void FromJson_HappyPath_ReturnsResult()
        {
            // Arrange
            var json = "{\"IsSuccess\": true}";

            // Act
            var result = ResultJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void FromJson_T_HappyPath_ReturnsResultT()
        {
            // Arrange
            var json = "{\"IsSuccess\": true, \"Value\": 42}";

            // Act
            var result = ResultJsonExtensions.FromJson<int>(json);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal(42, result.Value);
        }

        [Fact]
        public void TryFromJson_HappyPath_ReturnsTrue()
        {
            // Arrange
            var json = "{\"IsSuccess\": true}";

            // Act
            var success = ResultJsonExtensions.TryFromJson(json, out _);

            // Assert
            Assert.True(success);
        }

        [Fact]
        public void TryFromJson_T_HappyPath_ReturnsTrue()
        {
            // Arrange
            var json = "{\"IsSuccess\": true, \"Value\": 42}";

            // Act
            var success = ResultJsonExtensions.TryFromJson<int>(json, out _);

            // Assert
            Assert.True(success);
        }

        [Fact]
        public void ToJson_NullResult_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ResultJsonExtensions.ToJson(null));
        }

        [Fact]
        public void ToJson_T_NullResult_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ResultJsonExtensions.ToJson<int>(null));
        }

        [Fact]
        public void FromJson_NullJson_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ResultJsonExtensions.FromJson(null));
        }

        [Fact]
        public void FromJson_T_NullJson_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ResultJsonExtensions.FromJson<int>(null));
        }
    }
}
