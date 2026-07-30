using System;
using System.Text.Json;
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Serialization;
using Xunit;

namespace dotnet_deploy_notify.Tests
{
    public class BuildStatusConverterJsonExtensionsTests
    {
        private readonly BuildStatusConverter _converter = new();

        [Fact]
        public void ToJson_WithDefaultOptions_ReturnsNonEmptyString()
        {
            // Act
            string json = _converter.ToJson();

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(json));
        }

        [Fact]
        public void ToJson_WithIndentation_ContainsWhitespace()
        {
            // Act
            string json = _converter.ToJson(indented: true);

            // Assert
            // When indented, the JSON should contain at least one newline or space character.
            Assert.Contains("\n", json);
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsExpectedEnum()
        {
            // Arrange
            string json = JsonSerializer.Serialize(BuildStatus.Started);

            // Act
            BuildStatus result = BuildStatusConverterJsonExtensions.FromJson(json);

            // Assert
            Assert.Equal(BuildStatus.Started, result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void FromJson_NullOrEmpty_ThrowsArgumentException(string input)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => BuildStatusConverterJsonExtensions.FromJson(input));
        }

        [Fact]
        public void FromJson_InvalidJson_ThrowsJsonException()
        {
            // Arrange
            string invalidJson = "\"NotAValidStatus\"";

            // Act & Assert
            Assert.Throws<JsonException>(() => BuildStatusConverterJsonExtensions.FromJson(invalidJson));
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrueAndValue()
        {
            // Arrange
            string json = JsonSerializer.Serialize(BuildStatus.Failed);

            // Act
            bool success = BuildStatusConverterJsonExtensions.TryFromJson(json, out BuildStatus value);

            // Assert
            Assert.True(success);
            Assert.Equal(BuildStatus.Failed, value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\"Unknown\"")]
        public void TryFromJson_InvalidOrEmptyJson_ReturnsFalse(string json)
        {
            // Act
            bool success = BuildStatusConverterJsonExtensions.TryFromJson(json, out BuildStatus value);

            // Assert
            Assert.False(success);
            // The method sets a default value of BuildStatus.Started before attempting deserialization.
            Assert.Equal(BuildStatus.Started, value);
        }
    }
}
