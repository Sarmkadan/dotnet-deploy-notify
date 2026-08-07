using DotNetDeployNotify.Infrastructure;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Contains unit tests for <see cref="ServiceExtensionsJsonExtensions"/>.
/// </summary>
public class ServiceExtensionsJsonExtensionsTests
{
    /// <summary>
    /// Contains tests for the <see cref="ServiceExtensionsJsonExtensions.ToJson(bool)"/> method.
    /// </summary>
    public class ToJson
    {
        /// <summary>
        /// Tests that <see cref="ServiceExtensionsJsonExtensions.ToJson()"/> serializes metadata correctly.
        /// </summary>
        [Fact]
        public void ToJson_ShouldSerializeMetadataWithCorrectProperties()
        {
            // Act
            var result = ServiceExtensionsJsonExtensions.ToJson();

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().Contain("ServiceExtensions");
            result.Should().Contain("DotNetDeployNotify.Infrastructure");
            result.Should().Contain("DotNetDeployNotify");
            result.Should().Contain("IsCritical");
            result.Should().Contain("IsProduction");
            result.Should().Contain("SupportsStatus");
            result.Should().Contain("GetDescription");
            result.Should().Contain("MergeMetadata");
            result.Should().Contain("Clone");
            result.Should().Contain("ToCompactString");
            result.Should().Contain("GetSeverityLevel");
            result.Should().Contain("ShouldRetry");
            result.Should().Contain("GetRetryDelay");
        }

        /// <summary>
        /// Tests that <see cref="ServiceExtensionsJsonExtensions.ToJson(bool)"/> with <c>indented: true</c> formats the JSON with indentation.
        /// </summary>
        [Fact]
        public void ToJson_WithIndentedTrue_ShouldFormatJsonWithIndentation()
        {
            // Act
            var result = ServiceExtensionsJsonExtensions.ToJson(indented: true);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().Contain("\n"); // Should have newlines for indentation
            result.Should().Contain("  "); // Should have indentation spaces
        }

        /// <summary>
        /// Tests that <see cref="ServiceExtensionsJsonExtensions.ToJson(bool)"/> with <c>indented: false</c> formats the JSON without indentation.
        /// </summary>
        [Fact]
        public void ToJson_WithIndentedFalse_ShouldFormatJsonWithoutIndentation()
        {
            // Act
            var result = ServiceExtensionsJsonExtensions.ToJson(indented: false);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().NotContain("\n"); // Should not have newlines
        }
    }

    /// <summary>
    /// Contains tests for the <see cref="ServiceExtensionsJsonExtensions.FromJson(string)"/> method.
    /// </summary>
    public class FromJson
    {
        /// <summary>
        /// Tests that <see cref="ServiceExtensionsJsonExtensions.FromJson(string)"/> deserializes valid JSON correctly.
        /// </summary>
        [Fact]
        public void FromJson_WithValidJson_ShouldDeserializeCorrectly()
        {
            // Arrange
            var json = ServiceExtensionsJsonExtensions.ToJson();

            // Act
            var result = ServiceExtensionsJsonExtensions.FromJson(json);

            // Assert
            result.Should().NotBeNull();
            result!.Type.Should().Be("ServiceExtensions");
            result.Namespace.Should().Be("DotNetDeployNotify.Infrastructure");
            result.Assembly.Should().Be("DotNetDeployNotify");
            result.Methods.Should().NotBeNull();
            result.Methods.Should().Contain("IsCritical");
            result.Methods.Should().Contain("IsProduction");
        }

        /// <summary>
        /// Tests that <see cref="ServiceExtensionsJsonExtensions.FromJson(string)"/> throws <see cref="ArgumentNullException"/> when the input JSON is <c>null</c>.
        /// </summary>
        [Fact]
        public void FromJson_WithNullJson_ShouldThrowArgumentNullException()
        {
            // Arrange
            string? nullJson = null;

            // Act
            Action act = () => ServiceExtensionsJsonExtensions.FromJson(nullJson);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        /// <summary>
        /// Tests that <see cref="ServiceExtensionsJsonExtensions.FromJson(string)"/> returns <c>null</c> when the input JSON is an empty string.
        /// </summary>
        [Fact]
        public void FromJson_WithEmptyString_ShouldReturnNull()
        {
            // Arrange
            var emptyJson = "";

            // Act
            var result = ServiceExtensionsJsonExtensions.FromJson(emptyJson);

            // Assert
            result.Should().BeNull();
        }

        /// <summary>
        /// Tests that <see cref="ServiceExtensionsJsonExtensions.FromJson(string)"/> returns <c>null</c> when the input JSON is invalid.
        /// </summary>
        [Fact]
        public void FromJson_WithInvalidJson_ShouldReturnNull()
        {
            // Arrange
            var invalidJson = "{ invalid json";

            // Act
            var result = ServiceExtensionsJsonExtensions.FromJson(invalidJson);

            // Assert
            result.Should().BeNull();
        }

        /// <summary>
        /// Tests that <see cref="ServiceExtensionsJsonExtensions.FromJson(string)"/> deserializes partial JSON correctly with <c>null</c> for missing fields.
        /// </summary>
        [Fact]
        public void FromJson_WithPartialJson_ShouldDeserializeWithNulls()
        {
            // Arrange
            var partialJson = "{\"type\":\"ServiceExtensions\"}";

            // Act
            var result = ServiceExtensionsJsonExtensions.FromJson(partialJson);

            // Assert
            result.Should().NotBeNull();
            result!.Type.Should().Be("ServiceExtensions");
            result.Namespace.Should().BeNull();
            result.Assembly.Should().BeNull();
            result.Methods.Should().BeNull();
        }

        /// <summary>
        /// Tests that <see cref="ServiceExtensionsJsonExtensions.FromJson(string)"/> handles camelCase properties correctly.
        /// </summary>
        [Fact]
        public void FromJson_WithCamelCaseProperties_ShouldDeserializeCorrectly()
        {
            // Arrange
            var json = "{\"type\":\"Test\",\"namespace\":\"Test.Namespace\",\"assembly\":\"Test.Assembly\",\"methods\":[\"Method1\",\"Method2\"]}";

            // Act
            var result = ServiceExtensionsJsonExtensions.FromJson(json);

            // Assert
            result.Should().NotBeNull();
            result!.Type.Should().Be("Test");
            result.Namespace.Should().Be("Test.Namespace");
            result.Assembly.Should().Be("Test.Assembly");
            result.Methods.Should().NotBeNull();
            result.Methods.Should().HaveCount(2);
            result.Methods.Should().Contain("Method1");
            result.Methods.Should().Contain("Method2");
        }
    }

    /// <summary>
    /// Contains tests for the <see cref="ServiceExtensionsJsonExtensions.TryFromJson(string, out ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata?)"/> method.
    /// </summary>
    public class TryFromJson
    {
        /// <summary>
        /// Tests that <see cref="ServiceExtensionsJsonExtensions.TryFromJson(string, out ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata?)"/> returns <c>true</c> and deserializes correctly with valid JSON.
        /// </summary>
        [Fact]
        public void TryFromJson_WithValidJson_ShouldReturnTrueAndDeserialize()
        {
            // Arrange
            var json = ServiceExtensionsJsonExtensions.ToJson();

            // Act
            var result = ServiceExtensionsJsonExtensions.TryFromJson(json, out var value);

            // Assert
            result.Should().BeTrue();
            value.Should().NotBeNull();
            value!.Type.Should().Be("ServiceExtensions");
        }

        /// <summary>
        /// Tests that <see cref="ServiceExtensionsJsonExtensions.TryFromJson(string, out ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata?)"/> throws <see cref="ArgumentNullException"/> when the input JSON is <c>null</c>.
        /// </summary>
        [Fact]
        public void TryFromJson_WithNullJson_ShouldThrowArgumentNullException()
        {
            // Arrange
            string? nullJson = null;

            // Act
            Action act = () => ServiceExtensionsJsonExtensions.TryFromJson(nullJson, out _);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        /// <summary>
        /// Tests that <see cref="ServiceExtensionsJsonExtensions.TryFromJson(string, out ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata?)"/> returns <c>false</c> and <c>null</c> when the input JSON is an empty string.
        /// </summary>
        [Fact]
        public void TryFromJson_WithEmptyString_ShouldReturnFalseAndNullValue()
        {
            // Arrange
            var emptyJson = "";

            // Act
            var result = ServiceExtensionsJsonExtensions.TryFromJson(emptyJson, out var value);

            // Assert
            result.Should().BeFalse();
            value.Should().BeNull();
        }

        /// <summary>
        /// Tests that <see cref="ServiceExtensionsJsonExtensions.TryFromJson(string, out ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata?)"/> returns <c>false</c> and <c>null</c> when the input JSON is invalid.
        /// </summary>
        [Fact]
        public void TryFromJson_WithInvalidJson_ShouldReturnFalseAndNullValue()
        {
            // Arrange
            var invalidJson = "{ invalid json";

            // Act
            var result = ServiceExtensionsJsonExtensions.TryFromJson(invalidJson, out var value);

            // Assert
            result.Should().BeFalse();
            value.Should().BeNull();
        }

        /// <summary>
        /// Tests that <see cref="ServiceExtensionsJsonExtensions.TryFromJson(string, out ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata?)"/> returns <c>false</c> and <c>null</c> when the input JSON contains only whitespace.
        /// </summary>
        [Fact]
        public void TryFromJson_WithWhitespaceOnly_ShouldReturnFalseAndNullValue()
        {
            // Arrange
            var whitespaceJson = "   \n\t  ";

            // Act
            var result = ServiceExtensionsJsonExtensions.TryFromJson(whitespaceJson, out var value);

            // Assert
            result.Should().BeFalse();
            value.Should().BeNull();
        }
    }

    /// <summary>
    /// Contains tests for the <see cref="ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata"/> properties.
    /// </summary>
    public class ServiceExtensionsMetadataProperties
    {
        /// <summary>
        /// Tests that the <c>Type</c> property of <see cref="ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata"/> is a nullable string.
        /// </summary>
        [Fact]
        public void TypeProperty_ShouldBeNullableString()
        {
            // Arrange
            var metadata = new ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata
            {
                Type = "TestType",
                Namespace = "Test.Namespace",
                Assembly = "Test.Assembly",
                Methods = ["Method1"]
            };

            // Act & Assert
            metadata.Type.Should().Be("TestType");
            metadata.Type = null;
            metadata.Type.Should().BeNull();
        }

        /// <summary>
        /// Tests that the <c>Namespace</c> property of <see cref="ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata"/> is a nullable string.
        /// </summary>
        [Fact]
        public void NamespaceProperty_ShouldBeNullableString()
        {
            // Arrange
            var metadata = new ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata
            {
                Type = "TestType",
                Namespace = "Test.Namespace",
                Assembly = "Test.Assembly",
                Methods = ["Method1"]
            };

            // Act & Assert
            metadata.Namespace.Should().Be("Test.Namespace");
            metadata.Namespace = null;
            metadata.Namespace.Should().BeNull();
        }

        /// <summary>
        /// Tests that the <c>Assembly</c> property of <see cref="ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata"/> is a nullable string.
        /// </summary>
        [Fact]
        public void AssemblyProperty_ShouldBeNullableString()
        {
            // Arrange
            var metadata = new ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata
            {
                Type = "TestType",
                Namespace = "Test.Namespace",
                Assembly = "Test.Assembly",
                Methods = ["Method1"]
            };

            // Act & Assert
            metadata.Assembly.Should().Be("Test.Assembly");
            metadata.Assembly = null;
            metadata.Assembly.Should().BeNull();
        }

        /// <summary>
        /// Tests that the <c>Methods</c> property of <see cref="ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata"/> is a nullable string array.
        /// </summary>
        [Fact]
        public void MethodsProperty_ShouldBeNullableStringArray()
        {
            // Arrange
            var metadata = new ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata
            {
                Type = "TestType",
                Namespace = "Test.Namespace",
                Assembly = "Test.Assembly",
                Methods = ["Method1", "Method2"]
            };

            // Act & Assert
            metadata.Methods.Should().NotBeNull();
            metadata.Methods.Should().HaveCount(2);
            metadata.Methods.Should().Contain("Method1");
            metadata.Methods.Should().Contain("Method2");

            // Can be set to null
            metadata.Methods = null;
            metadata.Methods.Should().BeNull();
        }

        /// <summary>
        /// Tests that <see cref="ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata"/> with an empty <c>Methods</c> array serializes and deserializes correctly.
        /// </summary>
        [Fact]
        public void MethodsProperty_WithEmptyArray_ShouldSerializeAndDeserialize()
        {
            // Arrange
            var metadata = new ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata
            {
                Type = "TestType",
                Namespace = "Test.Namespace",
                Assembly = "Test.Assembly",
                Methods = Array.Empty<string>()
            };

            // Serialize
            var json = ServiceExtensionsJsonExtensions.ToJson();
            var deserialized = ServiceExtensionsJsonExtensions.FromJson(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Methods.Should().NotBeNull();
        }
    }
}
