# ResultTests

`ResultTests` is a test class that verifies the behavior of a `Result` type or similar error-handling construct in the `dotnet-deploy-notify` project. It ensures that success and failure cases are correctly handled, including value transformation, error propagation, and default value retrieval. These tests validate the robustness of the `Result` type's API, particularly in scenarios involving mapping, error aggregation, and exception handling.

## API

### `Ok_WithValue_IsSuccessTrueAndContainsValue`
Verifies that a successful `Result` with a value correctly reports `IsSuccess` as `true` and contains the expected value.
- **Purpose**: Confirms basic success-case behavior.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: Assertion exceptions if the test fails.

### `Fail_WithErrorMessage_IsSuccessFalseAndStoresError`
Tests that a failed `Result` correctly reports `IsSuccess` as `false` and stores the provided error message.
- **Purpose**: Validates error storage in failure cases.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: Assertion exceptions if the test fails.

### `Fail_WithMultipleErrors_JoinsAllErrorsIntoSingleMessage`
Ensures that when a `Result` fails with multiple errors, all errors are joined into a single concatenated message.
- **Purpose**: Validates error aggregation behavior.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: Assertion exceptions if the test fails.

### `Map_OnSuccessResult_TransformsValueToNewType`
Verifies that the `Map` operation transforms the value to a new type when the `Result` is successful.
- **Purpose**: Confirms successful value transformation.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: Assertion exceptions if the test fails.

### `Map_OnFailureResult_PropagatesErrorWithoutInvokingMapper`
Tests that the `Map` operation propagates the error without invoking the mapper function when the `Result` is a failure.
- **Purpose**: Ensures error propagation in mapping operations.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: Assertion exceptions if the test fails.

### `GetValueOrDefault_OnFailure_ReturnsProvidedDefault`
Confirms that `GetValueOrDefault` returns the provided default value when the `Result` is a failure.
- **Purpose**: Validates default value fallback behavior.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: Assertion exceptions if the test fails.

### `Try_WhenFunctionThrows_ReturnsFailureWithExceptionMessage`
Tests that the `Try` method returns a failure `Result` containing the exception message when the wrapped function throws.
- **Purpose**: Validates exception handling in `Try` operations.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: Assertion exceptions if the test fails.

### `Try_WhenFunctionSucceeds_ReturnsSuccessWithReturnValue`
Verifies that the `Try` method returns a successful `Result` with the return value when the wrapped function succeeds.
- **Purpose**: Confirms success-case behavior of `Try` operations.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: Assertion exceptions if the test fails.

## Usage

### Example 1: Testing Success and Failure Cases
