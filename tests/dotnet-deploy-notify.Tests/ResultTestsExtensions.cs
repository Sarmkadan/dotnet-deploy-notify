#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using DotNetDeployNotify.Results;
using FluentAssertions;
using Xunit;

/// <summary>
/// Extension methods for <see cref="ResultTests"/> that provide additional test utilities
/// for working with Result types in test scenarios.
/// </summary>
public static class ResultTestsExtensions
{
    /// <summary>
    /// Asserts that a result is successful and contains the expected value.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to assert.</param>
    /// <param name="expectedValue">The expected value.</param>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
    public static void ShouldBeSuccessWithValue<T>(this Result<T> result, T expectedValue)
    {
        ArgumentNullException.ThrowIfNull(result);

        result.IsSuccess.Should().BeTrue("Expected result to be successful");
        result.Value.Should().Be(expectedValue, "Expected result to contain the specified value");
        result.Error.Should().BeNull("Expected successful result to have no error");
    }

    /// <summary>
    /// Asserts that a result is failed and contains the expected error message.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to assert.</param>
    /// <param name="expectedError">The expected error message.</param>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
    public static void ShouldBeFailureWithError<T>(this Result<T> result, string expectedError)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentException.ThrowIfNullOrEmpty(expectedError);

        result.IsSuccess.Should().BeFalse("Expected result to be a failure");
        result.Error.Should().Be(expectedError, "Expected result to contain the specified error message");
    }

    /// <summary>
    /// Asserts that a result contains all expected errors.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to assert.</param>
    /// <param name="expectedErrors">The expected error messages.</param>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> or <paramref name="expectedErrors"/> is <see langword="null"/>.</exception>
    public static void ShouldContainErrors<T>(this Result<T> result, IReadOnlyList<string> expectedErrors)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(expectedErrors);

        result.IsSuccess.Should().BeFalse("Expected result to be a failure");
        result.Errors.Should().HaveCount(expectedErrors.Count, "Expected result to contain the same number of errors");

        foreach (var expectedError in expectedErrors)
        {
            result.Errors.Should().Contain(expectedError, "Expected error message to contain: {0}", expectedError);
        }
    }

    /// <summary>
    /// Asserts that a result is successful and the value matches a predicate.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to assert.</param>
    /// <param name="predicate">The predicate that the value should satisfy.</param>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> or <paramref name="predicate"/> is <see langword="null"/>.</exception>
    public static void ShouldBeSuccessAndSatisfy<T>(this Result<T> result, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(predicate);

        result.IsSuccess.Should().BeTrue("Expected result to be successful");
        predicate(result.Value!).Should().BeTrue("Expected result value to satisfy the predicate");
    }
}