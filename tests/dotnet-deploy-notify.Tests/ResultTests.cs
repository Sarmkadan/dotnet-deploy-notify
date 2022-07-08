#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Results;
using FluentAssertions;
using Xunit;

/// <summary>
/// Tests for the Result class.
/// </summary>
public class ResultTests
{
    /// <summary>
    /// Verifies that a successful result with a value is correctly represented.
    /// </summary>
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

    /// <summary>
    /// Verifies that a failed result with an error message is correctly represented.
    /// </summary>
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

    /// <summary>
    /// Verifies that a failed result with multiple errors is correctly represented.
    /// </summary>
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

    /// <summary>
    /// Verifies that mapping a successful result to a new type works correctly.
    /// </summary>
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

    /// <summary>
    /// Verifies that mapping a failed result does not invoke the mapper.
    /// </summary>
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

    /// <summary>
    /// Verifies that getting the value of a failed result returns the provided default.
    /// </summary>
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

    /// <summary>
    /// Verifies that the Try method returns a failed result when the provided function throws an exception.
    /// </summary>
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

    /// <summary>
    /// Verifies that the Try method returns a successful result when the provided function succeeds.
    /// </summary>
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
