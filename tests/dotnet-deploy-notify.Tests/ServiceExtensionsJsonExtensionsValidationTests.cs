#nullable enable
using DotNetDeployNotify.Infrastructure;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Tests for <see cref="ServiceExtensionsJsonExtensionsValidation"/> validation methods.
/// </summary>
public class ServiceExtensionsJsonExtensionsValidationTests
{
    private readonly ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata _validMetadata = new()
    {
        Type = "ServiceExtensions",
        Namespace = "DotNetDeployNotify.Infrastructure",
        Assembly = "DotNetDeployNotify",
        Methods = ["IsCritical", "IsProduction", "GetDescription"]
    };

    [Fact]
    public void Validate_WithValidMetadata_ReturnsEmptyList()
    {
        // Act
        var result = _validMetadata.Validate();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithNullType_ReturnsError()
    {
        // Arrange
        var metadata = new ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata
        {
            Type = null,
            Namespace = "DotNetDeployNotify.Infrastructure",
            Assembly = "DotNetDeployNotify",
            Methods = ["IsCritical"]
        };

        // Act
        var result = metadata.Validate();

        // Assert
        result.Should().ContainSingle(error => error == "Type must not be null or empty.");
    }

    [Fact]
    public void Validate_WithEmptyType_ReturnsError()
    {
        // Arrange
        var metadata = new ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata
        {
            Type = string.Empty,
            Namespace = "DotNetDeployNotify.Infrastructure",
            Assembly = "DotNetDeployNotify",
            Methods = ["IsCritical"]
        };

        // Act
        var result = metadata.Validate();

        // Assert
        result.Should().ContainSingle(error => error == "Type must not be null or empty.");
    }

    [Fact]
    public void Validate_WithWhitespaceType_ReturnsNoError()
    {
        // Arrange
        var metadata = new ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata
        {
            Type = "   ",
            Namespace = "DotNetDeployNotify.Infrastructure",
            Assembly = "DotNetDeployNotify",
            Methods = ["IsCritical"]
        };

        // Act
        var result = metadata.Validate();

        // Assert - string.IsNullOrEmpty only checks for null or empty, not whitespace
        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithNullNamespace_ReturnsError()
    {
        // Arrange
        var metadata = new ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata
        {
            Type = "ServiceExtensions",
            Namespace = null,
            Assembly = "DotNetDeployNotify",
            Methods = ["IsCritical"]
        };

        // Act
        var result = metadata.Validate();

        // Assert
        result.Should().ContainSingle(error => error == "Namespace must not be null or empty.");
    }

    [Fact]
    public void Validate_WithEmptyNamespace_ReturnsError()
    {
        // Arrange
        var metadata = new ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata
        {
            Type = "ServiceExtensions",
            Namespace = string.Empty,
            Assembly = "DotNetDeployNotify",
            Methods = ["IsCritical"]
        };

        // Act
        var result = metadata.Validate();

        // Assert
        result.Should().ContainSingle(error => error == "Namespace must not be null or empty.");
    }

    [Fact]
    public void Validate_WithNullAssembly_ReturnsError()
    {
        // Arrange
        var metadata = new ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata
        {
            Type = "ServiceExtensions",
            Namespace = "DotNetDeployNotify.Infrastructure",
            Assembly = null,
            Methods = ["IsCritical"]
        };

        // Act
        var result = metadata.Validate();

        // Assert
        result.Should().ContainSingle(error => error == "Assembly must not be null or empty.");
    }

    [Fact]
    public void Validate_WithEmptyAssembly_ReturnsError()
    {
        // Arrange
        var metadata = new ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata
        {
            Type = "ServiceExtensions",
            Namespace = "DotNetDeployNotify.Infrastructure",
            Assembly = string.Empty,
            Methods = ["IsCritical"]
        };

        // Act
        var result = metadata.Validate();

        // Assert
        result.Should().ContainSingle(error => error == "Assembly must not be null or empty.");
    }

    [Fact]
    public void Validate_WithNullMethods_ReturnsError()
    {
        // Arrange
        var metadata = new ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata
        {
            Type = "ServiceExtensions",
            Namespace = "DotNetDeployNotify.Infrastructure",
            Assembly = "DotNetDeployNotify",
            Methods = null
        };

        // Act
        var result = metadata.Validate();

        // Assert
        result.Should().ContainSingle(error => error == "Methods must not be null.");
    }

    [Fact]
    public void Validate_WithEmptyMethodsArray_ReturnsError()
    {
        // Arrange
        var metadata = new ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata
        {
            Type = "ServiceExtensions",
            Namespace = "DotNetDeployNotify.Infrastructure",
            Assembly = "DotNetDeployNotify",
            Methods = []
        };

        // Act
        var result = metadata.Validate();

        // Assert
        result.Should().ContainSingle(error => error == "Methods must not be empty.");
    }

    [Fact]
    public void Validate_WithMethodsContainingNull_ReturnsError()
    {
        // Arrange
        var metadata = new ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata
        {
            Type = "ServiceExtensions",
            Namespace = "DotNetDeployNotify.Infrastructure",
            Assembly = "DotNetDeployNotify",
            Methods = [null!, "IsCritical"]
        };

        // Act
        var result = metadata.Validate();

        // Assert
        result.Should().ContainSingle(error => error == "Methods must not contain null or empty strings.");
    }

    [Fact]
    public void Validate_WithMethodsContainingEmptyString_ReturnsError()
    {
        // Arrange
        var metadata = new ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata
        {
            Type = "ServiceExtensions",
            Namespace = "DotNetDeployNotify.Infrastructure",
            Assembly = "DotNetDeployNotify",
            Methods = ["", "IsCritical"]
        };

        // Act
        var result = metadata.Validate();

        // Assert
        result.Should().ContainSingle(error => error == "Methods must not contain null or empty strings.");
    }

    [Fact]
    public void Validate_WithMethodsContainingWhitespace_ReturnsNoError()
    {
        // Arrange
        var metadata = new ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata
        {
            Type = "ServiceExtensions",
            Namespace = "DotNetDeployNotify.Infrastructure",
            Assembly = "DotNetDeployNotify",
            Methods = ["   ", "IsCritical"]
        };

        // Act
        var result = metadata.Validate();

        // Assert - string.IsNullOrEmpty only checks for null or empty, not whitespace
        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithAllErrors_ReturnsAllErrors()
    {
        // Arrange
        var metadata = new ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata
        {
            Type = null,
            Namespace = null,
            Assembly = null,
            Methods = null
        };

        // Act
        var result = metadata.Validate();

        // Assert
        result.Should().HaveCount(4);
        result.Should().Contain(error => error == "Type must not be null or empty.");
        result.Should().Contain(error => error == "Namespace must not be null or empty.");
        result.Should().Contain(error => error == "Assembly must not be null or empty.");
        result.Should().Contain(error => error == "Methods must not be null.");
    }

    [Fact]
    public void IsValid_WithValidMetadata_ReturnsTrue()
    {
        // Act & Assert
        _validMetadata.IsValid().Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithNullType_ReturnsFalse()
    {
        // Arrange
        var metadata = new ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata
        {
            Type = null,
            Namespace = "DotNetDeployNotify.Infrastructure",
            Assembly = "DotNetDeployNotify",
            Methods = ["IsCritical"]
        };

        // Act & Assert
        metadata.IsValid().Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithEmptyMethodsArray_ReturnsFalse()
    {
        // Arrange
        var metadata = new ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata
        {
            Type = "ServiceExtensions",
            Namespace = "DotNetDeployNotify.Infrastructure",
            Assembly = "DotNetDeployNotify",
            Methods = []
        };

        // Act & Assert
        metadata.IsValid().Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithNullInMethods_ReturnsFalse()
    {
        // Arrange
        var metadata = new ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata
        {
            Type = "ServiceExtensions",
            Namespace = "DotNetDeployNotify.Infrastructure",
            Assembly = "DotNetDeployNotify",
            Methods = [null!, "IsCritical"]
        };

        // Act & Assert
        metadata.IsValid().Should().BeFalse();
    }

    [Fact]
    public void EnsureValid_WithValidMetadata_DoesNotThrow()
    {
        // Act
        Action act = () => _validMetadata.EnsureValid();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureValid_WithNullType_ThrowsArgumentException()
    {
        // Arrange
        var metadata = new ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata
        {
            Type = null,
            Namespace = "DotNetDeployNotify.Infrastructure",
            Assembly = "DotNetDeployNotify",
            Methods = ["IsCritical"]
        };

        // Act
        Action act = () => metadata.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Type must not be null or empty.*")
            .And.ParamName.Should().Be("metadata");
    }

    [Fact]
    public void EnsureValid_WithEmptyMethodsArray_ThrowsArgumentException()
    {
        // Arrange
        var metadata = new ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata
        {
            Type = "ServiceExtensions",
            Namespace = "DotNetDeployNotify.Infrastructure",
            Assembly = "DotNetDeployNotify",
            Methods = []
        };

        // Act
        Action act = () => metadata.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Methods must not be empty.*")
            .And.ParamName.Should().Be("metadata");
    }

    [Fact]
    public void EnsureValid_WithMultipleErrors_ThrowsArgumentExceptionWithAllErrors()
    {
        // Arrange
        var metadata = new ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata
        {
            Type = null,
            Namespace = null,
            Assembly = null,
            Methods = []
        };

        // Act
        Action act = () => metadata.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Type must not be null or empty*; *Namespace must not be null or empty*; *Assembly must not be null or empty*; *Methods must not be empty.*")
            .And.ParamName.Should().Be("metadata");
    }

    [Fact]
    public void Validate_WithNullMetadata_ThrowsArgumentNullException()
    {
        // Arrange
        ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata? metadata = null;

        // Act
        Action act = () => metadata!.Validate();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IsValid_WithNullMetadata_ThrowsArgumentNullException()
    {
        // Arrange
        ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata? metadata = null;

        // Act
        Action act = () => metadata!.IsValid();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EnsureValid_WithNullMetadata_ThrowsArgumentNullException()
    {
        // Arrange
        ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata? metadata = null;

        // Act
        Action act = () => metadata!.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
