# ValidationServiceTests

A test class containing unit tests for the validation logic applied to deployment notifications and channel configurations. It verifies that the validation service correctly identifies both valid and invalid input states, returning appropriate success or failure results with descriptive error messages for each violation.

## API

### ValidationServiceTests

Default constructor for the test class. Initializes any shared test context or dependencies required by the individual test methods.

### void ValidateNotification_WithValidNotification_ReturnsSuccess

Tests that a fully populated, well-formed notification object passes validation without errors.

- **Parameters:** None (arranges a valid notification internally).
- **Return value:** `void` (asserts the validation result indicates success).
- **Exceptions:** None expected from the method itself; test failures surface through the assertion framework.

### void ValidateNotification_WithNullNotification_ReturnsFailure

Verifies that passing a null notification reference produces a failure result, typically with an error indicating the notification cannot be null.

- **Parameters:** None.
- **Return value:** `void`.
- **Exceptions:** None.

### void ValidateNotification_WithMissingProjectName_ReturnsFailure

Ensures that a notification lacking a project name is rejected with an appropriate error message.

- **Parameters:** None.
- **Return value:** `void`.
- **Exceptions:** None.

### void ValidateNotification_WithMissingVersion_ReturnsFailure

Ensures that a notification lacking a version identifier is rejected.

- **Parameters:** None.
- **Return value:** `void`.
- **Exceptions:** None.

### void ValidateNotification_WithMissingBranchName_ReturnsFailure

Ensures that a notification without a branch name fails validation.

- **Parameters:** None.
- **Return value:** `void`.
- **Exceptions:** None.

### void ValidateNotification_WithMissingMessage_ReturnsFailure

Ensures that a notification missing the message body is rejected.

- **Parameters:** None.
- **Return value:** `void`.
- **Exceptions:** None.

### void ValidateNotification_WithNoChannels_ReturnsFailure

Verifies that a notification with an empty or null channel list fails validation, since at least one delivery channel is required.

- **Parameters:** None.
- **Return value:** `void`.
- **Exceptions:** None.

### void ValidateNotification_WithNegativeDeliveryAttempts_ReturnsFailure

Tests that a negative value for delivery attempts triggers a validation error.

- **Parameters:** None.
- **Return value:** `void`.
- **Exceptions:** None.

### void ValidateNotification_WithNegativeDuration_ReturnsFailure

Confirms that a negative duration value is rejected as invalid.

- **Parameters:** None.
- **Return value:** `void`.
- **Exceptions:** None.

### void ValidateNotification_WithPositiveDuration_ReturnsSuccess

Confirms that a positive duration value is accepted as valid.

- **Parameters:** None.
- **Return value:** `void`.
- **Exceptions:** None.

### void ValidateNotification_WithMultipleErrors_ReturnsAllErrors

Validates that when a notification contains several independent violations, the result aggregates all distinct error messages rather than stopping at the first failure.

- **Parameters:** None.
- **Return value:** `void`.
- **Exceptions:** None.

### void ValidateChannelConfiguration_WithValidConfig_ReturnsSuccess

Tests that a correctly populated channel configuration object passes validation.

- **Parameters:** None.
- **Return value:** `void`.
- **Exceptions:** None.

### void ValidateChannelConfiguration_WithNullConfig_ReturnsFailure

Verifies that a null channel configuration is rejected.

- **Parameters:** None.
- **Return value:** `void`.
- **Exceptions:** None.

### void ValidateChannelConfiguration_WithMissingDisplayName_ReturnsFailure

Ensures that a channel configuration without a display name fails validation.

- **Parameters:** None.
- **Return value:** `void`.
- **Exceptions:** None.

### void ValidateChannelConfiguration_WithInvalidWebhookUrl_ReturnsFailure

Tests that a malformed or empty webhook URL causes a validation failure.

- **Parameters:** None.
- **Return value:** `void`.
- **Exceptions:** None.

### void ValidateChannelConfiguration_WithMissingTargetId_ReturnsFailure

Ensures that a channel configuration lacking a target identifier is rejected.

- **Parameters:** None.
- **Return value:** `void`.
- **Exceptions:** None.

### void ValidateChannelConfiguration_WithZeroTimeout_ReturnsFailure

Verifies that a timeout value of zero is treated as invalid.

- **Parameters:** None.
- **Return value:** `void`.
- **Exceptions:** None.

### void ValidateChannelConfiguration_WithNegativeMaxRetries_ReturnsFailure

Confirms that a negative maximum retry count produces a validation error.

- **Parameters:** None.
- **Return value:** `void`.
- **Exceptions:** None.

### void ValidateChannelConfiguration_WithNullCustomHeaders_ReturnsFailure

Tests that a null custom headers collection is rejected, even if all other fields are valid.

- **Parameters:** None.
- **Return value:** `void`.
- **Exceptions:** None.

## Usage

```csharp
// Example 1: Running all notification validation tests in a CI pipeline
[TestFixture]
public class ValidationPipeline
{
    private ValidationServiceTests _tests;

    [SetUp]
    public void SetUp()
    {
        _tests = new ValidationServiceTests();
    }

    [Test]
    public void RunAllNotificationTests()
    {
        _tests.ValidateNotification_WithValidNotification_ReturnsSuccess();
        _tests.ValidateNotification_WithNullNotification_ReturnsFailure();
        _tests.ValidateNotification_WithMissingProjectName_ReturnsFailure();
        _tests.ValidateNotification_WithMissingVersion_ReturnsFailure();
        _tests.ValidateNotification_WithMissingBranchName_ReturnsFailure();
        _tests.ValidateNotification_WithMissingMessage_ReturnsFailure();
        _tests.ValidateNotification_WithNoChannels_ReturnsFailure();
        _tests.ValidateNotification_WithNegativeDeliveryAttempts_ReturnsFailure();
        _tests.ValidateNotification_WithNegativeDuration_ReturnsFailure();
        _tests.ValidateNotification_WithPositiveDuration_ReturnsSuccess();
        _tests.ValidateNotification_WithMultipleErrors_ReturnsAllErrors();
    }
}
```

```csharp
// Example 2: Running channel configuration tests as part of a deployment smoke test suite
[TestFixture]
public class ChannelConfigurationSmokeTests
{
    [Test]
    public void VerifyChannelValidationRules()
    {
        var tests = new ValidationServiceTests();

        tests.ValidateChannelConfiguration_WithValidConfig_ReturnsSuccess();
        tests.ValidateChannelConfiguration_WithNullConfig_ReturnsFailure();
        tests.ValidateChannelConfiguration_WithMissingDisplayName_ReturnsFailure();
        tests.ValidateChannelConfiguration_WithInvalidWebhookUrl_ReturnsFailure();
        tests.ValidateChannelConfiguration_WithMissingTargetId_ReturnsFailure();
        tests.ValidateChannelConfiguration_WithZeroTimeout_ReturnsFailure();
        tests.ValidateChannelConfiguration_WithNegativeMaxRetries_ReturnsFailure();
        tests.ValidateChannelConfiguration_WithNullCustomHeaders_ReturnsFailure();
    }
}
```

## Notes

- Each test method is self-contained and arranges its own input data; no shared mutable state is exposed publicly, making the class inherently thread-safe when tests are executed in parallel by a test runner.
- The `ValidateNotification_WithMultipleErrors_ReturnsAllErrors` test implies that the underlying validation service performs full-object validation rather than short-circuiting on the first error. Consumers should not rely on error ordering unless explicitly documented by the service under test.
- Boundary conditions for numeric fields (e.g., zero timeout, negative retries) are explicitly covered; values at the exact positive boundary (e.g., timeout of 1, zero retries) are exercised indirectly through the valid-configuration success tests.
- The null-check tests for both notifications and channel configurations confirm that the validation layer guards against null references before accessing member properties, preventing `NullReferenceException` propagation.
