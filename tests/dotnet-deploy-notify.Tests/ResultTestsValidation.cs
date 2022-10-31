using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit;

public static class ResultTestsValidation
{
    public static IReadOnlyList<string> Validate(this ResultTests value)
    {
        var problems = new List<string>();

        if (value.Ok_WithValue_IsSuccessTrueAndContainsValue == null)
        {
            problems.Add("Ok_WithValue_IsSuccessTrueAndContainsValue is null");
        }

        if (value.Fail_WithErrorMessage_IsSuccessFalseAndStoresError == null)
        {
            problems.Add("Fail_WithErrorMessage_IsSuccessFalseAndStoresError is null");
        }

        if (value.Fail_WithMultipleErrors_JoinsAllErrorsIntoSingleMessage == null)
        {
            problems.Add("Fail_WithMultipleErrors_JoinsAllErrorsIntoSingleMessage is null");
        }

        if (value.Map_OnSuccessResult_TransformsValueToNewType == null)
        {
            problems.Add("Map_OnSuccessResult_TransformsValueToNewType is null");
        }

        if (value.Map_OnFailureResult_PropagatesErrorWithoutInvokingMapper == null)
        {
            problems.Add("Map_OnFailureResult_PropagatesErrorWithoutInvokingMapper is null");
        }

        if (value.GetValueOrDefault_OnFailure_ReturnsProvidedDefault == null)
        {
            problems.Add("GetValueOrDefault_OnFailure_ReturnsProvidedDefault is null");
        }

        if (value.Try_WhenFunctionThrows_ReturnsFailureWithExceptionMessage == null)
        {
            problems.Add("Try_WhenFunctionThrows_ReturnsFailureWithExceptionMessage is null");
        }

        if (value.Try_WhenFunctionSucceeds_ReturnsSuccessWithReturnValue == null)
        {
            problems.Add("Try_WhenFunctionSucceeds_ReturnsSuccessWithReturnValue is null");
        }

        return problems;
    }

    public static bool IsValid(this ResultTests value)
    {
        return !Validate(value).Any();
    }

    public static void EnsureValid(this ResultTests value)
    {
        if (!IsValid(value))
        {
            var problems = Validate(value);
            throw new ArgumentException($"Invalid ResultTests: {string.Join(", ", problems)}");
        }
    }
}
