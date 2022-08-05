# IAuditService

Interface that defines the contract for recording and querying audit information related to notification processing, configuration changes, and validation outcomes within the `dotnet-deploy-notify` system.

## API

### Properties
- **Id** (`string`)  
  Identifier of the audit log entry or the service instance.  
  *Throws:* None.

- **Timestamp** (`DateTime`)  
  Date and time when the audit event occurred or was recorded.  
  *Throws:* None.

- **Operation** (`string`)  
  Name of the operation being audited (e.g., "NotificationCreated", "DeliveryAttempt").  
  *Throws:* None.

- **EntityType** (`string`)  
  Type of the entity the audit entry concerns (e.g., "Notification", "Configuration").  
  *Throws:* None.

- **EntityId** (`string`)  
  Unique identifier of the entity being audited.  
  *Throws:* None.

- **Actor** (`string`)  
  Identifier of the user, system, or process that performed the operation.  
  *Throws:* None.

- **Details** (`string`)  
  Free‑form description or payload associated with the audit event.  
  *Throws:* None.

- **Status** (`string`)  
  Outcome of the operation (e.g., "Success", "Failed", "Pending").  
  *Throws:* None.

- **Metadata** (`Dictionary<string, object>`)  
  Additional key‑value pairs providing contextual information not covered by the fixed properties.  
  *Throws:* None.

### Methods
- **AuditService** (`AuditService`)  
  Returns a concrete implementation of the audit service that fulfills this interface.  
  *Parameters:* None.  
  *Return:* An `AuditService` instance ready for use.  
  *Throws:* `NotImplementedException` if the implementing type does not provide a concrete service.

- **LogNotificationCreatedAsync** (`Task`)  
  Records an audit entry for a newly created notification.  
  *Parameters:* Values required to populate the audit properties (notification identifier, actor, details, etc.).  
  *Return:* A task that completes when the log entry has been persisted.  
  *Throws:* `ArgumentNullException` if any required argument is `null`; may propagate storage‑specific exceptions.

- **LogDeliveryAttemptAsync** (`Task`)  
  Records an audit entry for a delivery attempt of a notification.  
  *Parameters:* Notification identifier, delivery outcome, actor, and any relevant details.  
  *Return:* A task that completes when the log entry has been persisted.  
  *Throws:* `ArgumentNullException` for missing required data; storage errors are bubbled up.

- **LogConfigurationChangeAsync** (`Task`)  
  Records an audit entry when a configuration setting is modified.  
  *Parameters:* Configuration key, old value, new value, actor, and timestamp.  
  *Return:* A task that completes when the log entry has been persisted.  
  *Throws:* `ArgumentNullException` for null arguments; underlying store exceptions are propagated.

- **LogValidationFailureAsync** (`Task`)  
  Records an audit entry when validation of a notification or configuration fails.  
  *Parameters:* Entity being validated, validation error message, actor, and optional details.  
  *Return:* A task that completes when the log entry has been persisted.  
  *Throws:* `ArgumentNullException` if validation context is missing; store‑related exceptions may occur.

- **GetAuditLogsAsync** (`Task<List<AuditLogEntry>>`)  
  Retrieves all audit log entries, optionally filtered by criteria defined by the implementing service.  
  *Parameters:* None (filtering, if supported, is configured via the service implementation).  
  *Return:* A task yielding a read‑only list of `AuditLogEntry` objects.  
  *Throws:* `InvalidOperationException` if the store is unavailable; other storage exceptions are propagated.

- **GetNotificationAuditLogsAsync** (`Task<List<AuditLogEntry>>`)  
  Retrieves audit log entries associated with a specific notification.  
  *Parameters:* Notification identifier (`string`).  
  *Return:* A task yielding a list of `AuditLogEntry` objects for the given notification.  
  *Throws:* `ArgumentNullException` if the notification identifier is `null` or empty; storage exceptions are propagated.

- **ClearOldLogsAsync** (`Task`)  
  Removes audit log entries older than a retention period defined by the service implementation.  
  *Parameters:* None (retention policy is internal to the implementation).  
  *Return:* A task that completes when the cleanup operation finishes.  
  *Throws:* UnauthorizedAccessException if the service lacks permission to delete records; storage exceptions are propagated.

## Usage

```csharp
// Example 1: Logging a notification creation
public async Task HandleNotificationCreated(string notificationId, string actor)
{
    var audit = _auditService; // IAuditService injected via DI
    await audit.LogNotificationCreatedAsync(
        notificationId: notificationId,
        actor: actor,
        details: $"Notification {notificationId} was created.",
        status: "Success");
}

// Example 2: Retrieving audit logs for a specific notification
public async Task<IReadOnlyList<AuditLogEntry>> GetNotificationHistory(string notificationId)
{
    if (string.IsNullOrWhiteSpace(notificationId))
        throw new ArgumentException("Notification ID required.", nameof(notificationId));

    var audit = _auditService;
    return await audit.GetNotificationAuditLogsAsync(notificationId);
}
```

## Notes
- All asynchronous methods are expected to be **thread‑safe**; concurrent calls should not corrupt internal state. Implementations must ensure proper synchronization when accessing shared storage.
- Passing `null` for any required argument will result in an `ArgumentNullException`; callers should validate inputs before invoking the API.
- The `Metadata` dictionary permits extension of the audit record without modifying the interface; however, keys should be documented by the consuming application to avoid collisions.
- Implementations may buffer writes for performance; therefore, a call to `Log*Async` does not guarantee immediate persistence until the returned task completes.
- The `AuditService` method/property provides a way to obtain the concrete service behind the interface; if the interface is used purely for mocking in tests, this member may throw `NotImplementedException`.
- Retention and deletion policies for `ClearOldLogsAsync` are implementation‑specific; invoking this method may permanently remove data, so it should be used with caution in production environments.
