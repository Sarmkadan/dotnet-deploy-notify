#nullable enable

using DotNetDeployNotify.Context;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class RequestContextValidationTests
{
    [Fact]
    public void Validate_ValidContext_ReturnsEmptyList()
    {
        // Arrange
        var context = new RequestContext
        {
            CorrelationId = Guid.NewGuid().ToString(),
            RequestId = Guid.NewGuid().ToString(),
            RequestTime = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>(),
            ExecutionTimeMs = 100
        };

        // Act
        var errors = context.Validate();

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_InvalidCorrelationId_ReturnsErrors()
    {
        // Arrange
        var context = new RequestContext
        {
            CorrelationId = "invalid-guid",
            RequestId = Guid.NewGuid().ToString(),
            RequestTime = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>(),
            ExecutionTimeMs = 100
        };

        // Act
        var errors = context.Validate();

        // Assert
        errors.Should().ContainSingle().Which.Should().Contain("CorrelationId");
    }

    [Fact]
    public void Validate_InvalidRequestTime_ReturnsErrors()
    {
        // Arrange
        var context = new RequestContext
        {
            CorrelationId = Guid.NewGuid().ToString(),
            RequestId = Guid.NewGuid().ToString(),
            RequestTime = DateTime.UtcNow.AddYears(2), // Too far in future
            Metadata = new Dictionary<string, object>(),
            ExecutionTimeMs = 100
        };

        // Act
        var errors = context.Validate();

        // Assert
        errors.Should().ContainSingle().Which.Should().Contain("RequestTime");
    }

    [Fact]
    public void Validate_NullMetadata_ReturnsErrors()
    {
        // Arrange
        var context = new RequestContext
        {
            CorrelationId = Guid.NewGuid().ToString(),
            RequestId = Guid.NewGuid().ToString(),
            RequestTime = DateTime.UtcNow,
            Metadata = null!,
            ExecutionTimeMs = 100
        };

        // Act
        var errors = context.Validate();

        // Assert
        errors.Should().ContainSingle().Which.Should().Contain("Metadata");
    }

    [Fact]
    public void EnsureValid_InvalidContext_ThrowsArgumentException()
    {
        // Arrange
        var context = new RequestContext
        {
            CorrelationId = "invalid",
            RequestId = Guid.NewGuid().ToString(),
            RequestTime = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>(),
            ExecutionTimeMs = 100
        };

        // Act
        var act = () => context.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IsValid_ValidContext_ReturnsTrue()
    {
        // Arrange
        var context = new RequestContext
        {
            CorrelationId = Guid.NewGuid().ToString(),
            RequestId = Guid.NewGuid().ToString(),
            RequestTime = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>(),
            ExecutionTimeMs = 100
        };

        // Act
        var isValid = context.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_InvalidContext_ReturnsFalse()
    {
        // Arrange
        var context = new RequestContext
        {
            CorrelationId = "invalid",
            RequestId = Guid.NewGuid().ToString(),
            RequestTime = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>(),
            ExecutionTimeMs = 100
        };

        // Act
        var isValid = context.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }
}
