# IntegrationTests

Integration test suite for the deployment notification system, validating end-to-end workflows, channel delivery, validation, retries, and concurrency scenarios.

## API

### `NotificationService_CreateAndSendNotification_EndToEndWorkflow`

Validates the complete notification creation and delivery pipeline, including persistence, validation, and delivery to configured channels. The test constructs a notification, triggers the service, and asserts successful delivery across all enabled channels.

Parameters:
- None

Return value:
- `Task`: Completes when the end-to-end workflow finishes.

Throws:
- `Exception`: If any step in the pipeline fails, including validation or delivery errors.

---

### `NotificationService_SendToMultipleChannels_DeliverToAllConfiguredChannels`

Ensures that a single notification is delivered to all channels configured in the system. The test configures multiple channel providers, creates a notification, and verifies that each provider receives the payload.

Parameters:
- None

Return value:
- `Task`: Completes when delivery to all channels is confirmed.

Throws:
- `Exception`: If any channel fails to receive the notification.

---

### `NotificationService_WithValidationFailure_ThrowsException`

Confirms that invalid notifications are rejected before delivery. The test constructs a notification with invalid data (e.g., missing required fields), triggers the service, and asserts that an exception is thrown during validation.

Parameters:
- None

Return value:
- `Task`: Completes when validation fails and the exception is thrown.

Throws:
- `Exception`: With details of the validation failure.

---
### `NotificationService_RetryFailedDeliveries_UpdatesResultsAndIncrementAttempts`

Validates that failed deliveries are retried according to configured policies, with attempt counters incremented and results updated. The test simulates transient failures, triggers the service, and asserts that retries occur and results are persisted.

Parameters:
- None

Return value:
- `Task`: Completes when retries are exhausted or successful.

Throws:
- `Exception`: If retries are exhausted without success beyond configured limits.

---
### `WebhookDispatcher_WithValidPayload_SendsSuccessfully`

Tests the webhook dispatcher’s ability to send valid payloads to configured endpoints. The test constructs a valid payload, triggers the dispatcher, and verifies successful HTTP delivery.

Parameters:
- None

Return value:
- `Task`: Completes when the webhook is sent and acknowledged.

Throws:
- `Exception`: If the webhook request fails or the endpoint rejects the payload.

---
### `MainUseCase_SendDeploymentNotificationToMultipleChannels_CompleteFlow`

Covers the primary use case: sending a deployment notification to multiple channels as part of a CI/CD pipeline. The test simulates a deployment event, constructs a notification, triggers the service, and asserts delivery to all configured channels.

Parameters:
- None

Return value:
- `Task`: Completes when the deployment notification is fully processed.

Throws:
- `Exception`: If any step in the flow fails.

---
### `MultipleNotifications_ProcessConcurrently_AllDeliveredSuccessfully`

Ensures that the system can handle multiple concurrent notifications without interference. The test dispatches several notifications in parallel and asserts that all are delivered successfully.

Parameters:
- None

Return value:
- `Task`: Completes when all concurrent notifications are processed.

Throws:
- `Exception`: If any notification fails to deliver due to concurrency issues.

---
### `NotificationWithChannelFiltering_SkipsNotConfiguredChannels`

Validates that notifications are only delivered to configured channels. The test creates a notification with filtering rules, triggers the service, and asserts that only enabled channels receive the payload.

Parameters:
- None

Return value:
- `Task`: Completes when delivery decisions are finalized.

Throws:
- `Exception`: If delivery occurs to a disabled or unconfigured channel.

## Usage

### Example 1: End-to-end notification workflow
