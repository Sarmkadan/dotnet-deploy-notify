#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Results;
using FluentAssertions;

namespace DotNetDeployNotify.Tests;

public class ResultTests
{
    [Fact]
    public void Ok_WithValue_IsSuccessTrueAndContainsValue()
    {
        // Act
        var result = Result<int>.Ok(42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Fail_WithErrorMessage_IsSuccessFalseAndStoresError()
    {
        // Act
        var result = Result<string>.Fail("Webhook delivery failed");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Webhook delivery failed");
        result.Value.Should().BeNull();
    }

    [Fact]
    public void Fail_WithMultipleErrors_JoinsAllErrorsIntoSingleMessage()
    {
        // Arrange
        var errors = new List<string> { "Project name is required", "Version is required" };

        // Act
        var result = Result<string>.Fail(errors);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Project name is required");
        result.Error.Should().Contain("Version is required");
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void Map_OnSuccessResult_TransformsValueToNewType()
    {
        // Arrange
        var result = Result<int>.Ok(5);

        // Act
        var mapped = result.Map(x => x * 10);

        // Assert
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(50);
    }

    [Fact]
    public void Map_OnFailureResult_PropagatesErrorWithoutInvokingMapper()
    {
        // Arrange
        var result = Result<int>.Fail("Original error");
        var mapperInvoked = false;

        // Act
        var mapped = result.Map(x => { mapperInvoked = true; return x.ToString(); });

        // Assert
        mapped.IsSuccess.Should().BeFalse();
        mapped.Error.Should().Be("Original error");
        mapperInvoked.Should().BeFalse();
    }

    [Fact]
    public void GetValueOrDefault_OnFailure_ReturnsProvidedDefault()
    {
        // Arrange
        var result = Result<string>.Fail("not found");

        // Act
        var value = result.GetValueOrDefault("fallback");

        // Assert
        value.Should().Be("fallback");
    }

    [Fact]
    public void Try_WhenFunctionThrows_ReturnsFailureWithExceptionMessage()
    {
        // Act
        var result = ResultExtensions.Try<int>(() =>
            throw new InvalidOperationException("channel unavailable"));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("channel unavailable");
    }

    [Fact]
    public void Try_WhenFunctionSucceeds_ReturnsSuccessWithReturnValue()
    {
        // Act
        var result = ResultExtensions.Try(() => 99);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(99);
    }
}
