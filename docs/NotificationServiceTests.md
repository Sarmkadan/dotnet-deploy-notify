# NotificationServiceTests

Unit test suite for the `NotificationService` class, verifying its core behaviours around notification creation, delivery, and retry logic. The tests cover both success paths and expected failure modes, ensuring the service correctly handles valid inputs, invalid data, missing entities, and edge-case configurations.

## API

### NotificationServiceTests

Constructor. Initialises the test fixture, setting up any shared mocks, fakes, or test data required by the individual test methods. No parameters.

### async Task CreateNotificationAsync_ShouldReturnId_WhenValid

Verifies that creating a notification with valid input data succeeds and returns a non‑null, non‑empty identifier.  
**Parameters:** none (test data is arranged internally).  
**Returns:** a completed `Task` once the assertion passes.  
**Throws:** test assertion failure if the returned ID is null, empty, or the operation throws unexpectedly.

### async Task CreateNotificationAsync_ShouldThrowException_WhenInvalid

Verifies that attempting to create a notification with invalid or incomplete data causes the service to throw an appropriate exception.  
**Parameters:** none.  
**Returns:** a completed `Task` once the expected exception is caught.  
**Throws:** test assertion failure if no exception is thrown, or an exception of the wrong type is raised.

### async Task SendNotificationAsync_ShouldThrowException_WhenNotificationNotFound

Verifies that sending a notification that does not exist in the store results in an exception.  
**Parameters:** none.  
**Returns:** a completed `Task` once the expected exception is observed.  
**Throws:** test assertion failure if the call succeeds without throwing, or throws an unexpected exception type.

### async Task SendNotificationAsync_ShouldReturnEmptyList_WhenNoChannelsSpecified

Verifies that sending a valid notification that has no delivery channels configured returns an empty result set rather than throwing or returning null.  
**Parameters:** none.  
**Returns:** a completed `Task` once the empty‑list assertion passes.  
**Throws:** test assertion failure if the result is null, non‑empty, or an exception is thrown.

### async Task SendNotificationAsync_ShouldSendAndReturnResult_WhenValid

Verifies the full happy path: a valid notification with at least one channel is sent successfully, and the returned result list contains one entry per channel with the expected delivery status metadata.  
**Parameters:** none.  
**Returns:** a completed `Task` once all channel‑result assertions pass.  
**Throws:** test assertion failure if the result count mismatches, statuses are incorrect, or an exception is thrown.

### async Task RetryFailedDeliveriesAsync_ShouldThrowException_WhenNotificationNotFound

Verifies that requesting a retry for a notification ID that does not exist causes the service to throw an exception.  
**Parameters:** none.  
**Returns:** a completed `Task` once the expected exception is caught.  
**Throws:** test assertion failure if no exception is thrown, or an exception of the wrong type is raised.

## Usage

```csharp
// Example 1: Testing the happy path for sending a notification
[Fact]
public async Task SendNotificationAsync_ShouldSendAndReturnResult_WhenValid()
{
    // Arrange
    var notificationId = Guid.NewGuid().ToString();
    var channels = new[] { "email", "slack" };
    var service = new NotificationService(/* mocked dependencies */);

    // Act
    var results = await service.SendNotificationAsync(notificationId, channels);

    // Assert
    Assert.Equal(2, results.Count);
    Assert.All(results, r => Assert.Equal(DeliveryStatus.Sent, r.Status));
}
```

```csharp
// Example 2: Testing exception behaviour when notification is missing
[Fact]
public async Task SendNotificationAsync_ShouldThrowException_WhenNotificationNotFound()
{
    // Arrange
    var missingId = "nonexistent-id";
    var service = new NotificationService(/* mocked dependencies */);

    // Act & Assert
    await Assert.ThrowsAsync<NotificationNotFoundException>(
        () => service.SendNotificationAsync(missingId, new[] { "email" }));
}
```

## Notes

- **Edge cases:** The `SendNotificationAsync_ShouldReturnEmptyList_WhenNoChannelsSpecified` test explicitly covers the scenario where a notification exists but has zero delivery channels. Callers should not assume a non‑empty result list; an empty list is a valid, non‑exceptional outcome.
- **Exception types:** Tests that expect exceptions rely on specific exception types (e.g., `NotificationNotFoundException`). Production code should throw documented, distinct types so callers can handle each failure mode separately.
- **Thread safety:** These tests are designed for single‑threaded execution via standard test runners (xUnit, NUnit, or MSTest). The service methods under test are expected to be thread‑safe when used with their real dependencies; the tests themselves do not validate concurrent invocation behaviour.
- **Test isolation:** Each test method arranges its own state and does not depend on execution order. They can be run individually or in parallel without side‑effects.
