using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/// <summary>
/// Provides validation for <see cref="ResultTests"/> test class to ensure all expected test methods are properly implemented.
/// </summary>
public static class ResultTestsValidation
{
    /// <summary>
    /// Validates that the <see cref="ResultTests"/> class contains all expected test methods.
    /// </summary>
    /// <param name="value">The <see cref="ResultTests"/> instance to validate (unused, kept for API compatibility)</param>
    /// <returns>A list of validation problems; empty if validation succeeds</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this ResultTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();
        var testType = typeof(ResultTests);
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        var methods = testType.GetMethods(flags);

        if (!HasTestMethod(methods, nameof(ResultTests.Ok_WithValue_IsSuccessTrueAndContainsValue)))
        {
            problems.Add("Missing test method: Ok_WithValue_IsSuccessTrueAndContainsValue");
        }

        if (!HasTestMethod(methods, nameof(ResultTests.Fail_WithErrorMessage_IsSuccessFalseAndStoresError)))
        {
            problems.Add("Missing test method: Fail_WithErrorMessage_IsSuccessFalseAndStoresError");
        }

        if (!HasTestMethod(methods, nameof(ResultTests.Fail_WithMultipleErrors_JoinsAllErrorsIntoSingleMessage)))
        {
            problems.Add("Missing test method: Fail_WithMultipleErrors_JoinsAllErrorsIntoSingleMessage");
        }

        if (!HasTestMethod(methods, nameof(ResultTests.Map_OnSuccessResult_TransformsValueToNewType)))
        {
            problems.Add("Missing test method: Map_OnSuccessResult_TransformsValueToNewType");
        }

        if (!HasTestMethod(methods, nameof(ResultTests.Map_OnFailureResult_PropagatesErrorWithoutInvokingMapper)))
        {
            problems.Add("Missing test method: Map_OnFailureResult_PropagatesErrorWithoutInvokingMapper");
        }

        if (!HasTestMethod(methods, nameof(ResultTests.GetValueOrDefault_OnFailure_ReturnsProvidedDefault)))
        {
            problems.Add("Missing test method: GetValueOrDefault_OnFailure_ReturnsProvidedDefault");
        }

        if (!HasTestMethod(methods, nameof(ResultTests.Try_WhenFunctionThrows_ReturnsFailureWithExceptionMessage)))
        {
            problems.Add("Missing test method: Try_WhenFunctionThrows_ReturnsFailureWithExceptionMessage");
        }

        if (!HasTestMethod(methods, nameof(ResultTests.Try_WhenFunctionSucceeds_ReturnsSuccessWithReturnValue)))
        {
            problems.Add("Missing test method: Try_WhenFunctionSucceeds_ReturnsSuccessWithReturnValue");
        }

        return problems;
    }

    /// <summary>
    /// Checks if a test method with the specified name exists.
    /// </summary>
    /// <param name="methods">Array of methods to search</param>
    /// <param name="methodName">Name of the method to find</param>
    /// <returns>True if the method exists; otherwise false</returns>
    private static bool HasTestMethod(MethodInfo[] methods, string methodName)
    {
        return methods.Any(m => string.Equals(m.Name, methodName, StringComparison.Ordinal));
    }

    /// <summary>
    /// Determines whether the <see cref="ResultTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="ResultTests"/> instance to validate</param>
    /// <returns>True if validation succeeds; otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static bool IsValid(this ResultTests value) => !Validate(value).Any();

    /// <summary>
    /// Validates the <see cref="ResultTests"/> instance and throws an exception if validation fails.
    /// </summary>
    /// <param name="value">The <see cref="ResultTests"/> instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static void EnsureValid(this ResultTests value)
    {
        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException($"Invalid ResultTests: {string.Join(", ", problems)}");
        }
    }
}
