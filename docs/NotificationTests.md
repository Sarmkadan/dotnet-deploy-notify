# NotificationTests

Unit tests for the notification system in `dotnet-deploy-notify`, covering builder validation, channel configuration, and notification result handling.

## API

### `void NotificationBuilder_WithAllRequiredFields_BuildsValidNotification()`

Verifies that a notification is successfully built when all required fields are provided. No parameters or return value. Does not throw.

### `void NotificationBuilder_AsFailure_SetsCriticalPriorityAndFailedStatus()`

Ensures that a failure notification is assigned `Critical` priority and `Failed` status. No parameters or return value. Does not throw.

### `void NotificationBuilder_AsDeploymentSuccess_SetsHighPriorityAndCorrectStatus()`

Validates that a successful deployment notification is assigned `High` priority and the correct `Succeeded` status. No parameters or return value. Does not throw.

### `void NotificationBuilder_Build_WithMissingProjectName_ThrowsInvalidOperationException()`

Confirms that building a notification without a project name throws an `InvalidOperationException`. No parameters or return value.

### `void NotificationBuilder_Build_WithNoChannels_ThrowsInvalidOperationException()`

Confirms that building a notification without any channels throws an `InvalidOperationException`. No parameters or return value.

### `void IValidationService_WhenMocked_ReturnsConfiguredErrorsAndIsVerifiable()`

Validates that a mocked `IValidationService` returns configured validation errors and supports verification. No parameters or return value. Does not throw.

### `void ChannelConfiguration_ShouldSendNotification_WhenChannelDisabled_ReturnsFalse()`

Ensures that `ShouldSendNotification` returns `false` when the channel is disabled. No parameters or return value. Does not throw.

### `void ChannelConfiguration_ShouldSendNotification_WhenPriorityBelowMinimum_ReturnsFalse()`

Ensures that `ShouldSendNotification` returns `false` when the notification priority is below the channel’s minimum. No parameters or return value. Does not throw.

### `void NotificationResult_MarkAsSuccessful_SetsDeliveredStatusAndClearsError()`

Verifies that marking a notification result as successful sets its status to `Delivered` and clears any error details. No parameters or return value. Does not throw.

### `void NotificationResult_MarkAsFailed_SetsFailedStatusWithErrorDetails()`

Validates that marking a notification result as failed sets its status to `Failed` and preserves the error details. No parameters or return value. Does not throw.

### `void DeploymentNotification_SetAndGetMetadata_RoundTripsTypedValues()`

Ensures that typed metadata values are correctly stored and retrieved without loss of type fidelity. No parameters or return value. Does not throw.

### `void DeploymentNotification_IncrementDeliveryAttempt_IncrementsCounterEachCall()`

Confirms that each call to `IncrementDeliveryAttempt` increases the delivery attempt counter by one. No parameters or return value. Does not throw.

### `void DeploymentNotification_MarkAsProcessed_SetsIsProcessedTrue()`

Validates that calling `MarkAsProcessed` sets the `IsProcessed` flag to `true`. No parameters or return value. Does not throw.

## Usage
