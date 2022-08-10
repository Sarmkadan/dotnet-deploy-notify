# INotificationRepository

The `INotificationRepository` interface defines the contract for persisting and retrieving deployment notification records within the `dotnet-deploy-notify` system. It provides asynchronous operations for creating, querying, updating, and deleting `DeploymentNotification` entities, as well as specialized lookups by project, environment, status, and pending state.

## API

### CreateAsync
```csharp
Task CreateAsync(DeploymentNotification notification);
```
Adds a new deployment notification to the underlying store.  
- **Parameters**  
  - `notification`: The `DeploymentNotification` instance to persist. Must not be `null`.  
- **Return Value**  
  - A `Task` that completes when the insert operation finishes.  
- **Exceptions**  
  - `ArgumentNullException` if `notification` is `null`.  
  - May propagate store‑specific exceptions (e.g., `DbUpdateException`) on failure.

### GetByIdAsync
```csharp
Task<DeploymentNotification?> GetByIdAsync(Guid id);
```
Retrieves a single deployment notification by its unique identifier.  
- **Parameters**  
  - `id`: The `Guid` identifying the notification.  
- **Return Value**  
  - A `Task` whose result is the matching `DeploymentNotification` or `null` if none exists.  
- **Exceptions**  
  - `ArgumentException` if `id` is `Guid.Empty`.  
  - May throw store‑specific exceptions on query failure.

### GetByProjectAsync
```csharp
Task<List<DeploymentNotification>> GetByProjectAsync(string projectKey);
```
Returns all notifications associated with a given project.  
- **Parameters**  
  - `projectKey`: The project identifier (case‑sensitive) whose notifications are requested. Must not be `null` or whitespace.  
- **Return Value**  
  - A `Task` yielding a list of `DeploymentNotification` objects; the list may be empty but never `null`.  
- **Exceptions**  
  - `ArgumentNullException` or `ArgumentException` if `projectKey` is invalid.  
  - Store‑specific exceptions may be thrown on error.

### GetPendingAsync
```csharp
Task<List<DeploymentNotification>> GetPendingAsync();
```
Fetches all notifications that have not yet been processed (e.g., awaiting delivery).  
- **Parameters**  
  - None.  
- **Return Value**  
  - A `Task` yielding a list of pending `DeploymentNotification` instances; empty list if none are pending.  
- **Exceptions**  
  - Propagates any store‑related exceptions.

### UpdateAsync
```csharp
Task UpdateAsync(DeploymentNotification notification);
```
Updates an existing deployment notification record.  
- **Parameters**  
  - `notification`: The `DeploymentNotification` with modified values. Must not be `null` and must represent an existing entity (identified by its `Id`).  
- **Return Value**  
  - A `Task` completing when the update succeeds.  
- **Exceptions**  
  - `ArgumentNullException` if `notification` is `null`.  
  - `InvalidOperationException` if no record with the given `Id` exists.  
  - Store‑specific exceptions on concurrency or write failures.

### DeleteAsync
```csharp
Task DeleteAsync(Guid id);
```
Removes a deployment notification from the store.  
- **Parameters**  
  - `id`: The `Guid` of the notification to delete.  
- **Return Value**  
  - A `Task` that completes when the deletion is finished.  
- **Exceptions**  
  - `ArgumentException` if `id` is `Guid.Empty`.  
  - `InvalidOperationException` if no entity with the specified `Id` is found.  
  - Store‑specific exceptions may be raised.

### GetByEnvironmentAsync
```csharp
Task<List<DeploymentNotification>> GetByEnvironmentAsync(string environment);
```
Retrieves all notifications for a specific deployment environment (e.g., "Production", "Staging").  
- **Parameters**  
  - `environment`: The environment name; must not be `null` or whitespace.  
- **Return Value**  
  - A `Task` yielding a list of matching `DeploymentNotification` objects; empty list if none.  
- **Exceptions**  
  - `ArgumentNullException` or `ArgumentException` for invalid `environment`.  
  - Store‑specific exceptions on query failure.

### GetByStatusAsync
```csharp
Task<List<DeploymentNotification>> GetByStatusAsync(NotificationStatus status);
```
Returns notifications filtered by their processing status.  
- **Parameters**  
  - `status`: The `NotificationStatus` enum value to filter by.  
- **Return Value**  
  - A `Task` yielding a list of `DeploymentNotification` entities with the given status; empty list if none.  
- **Exceptions**  
  - May throw store‑related exceptions.

### GetAllAsync
```csharp
Task<List<DeploymentNotification>> GetAllAsync();
```
Obtains every deployment notification stored in the repository.  
- **Parameters**  
  - None.  
- **Return Value**  
  - A `Task` yielding a complete list of `DeploymentNotification` objects; empty list if the store contains no records.  
- **Exceptions**  
  - Propagates any exceptions from the underlying data store.

## Usage

### Example 1: Adding a new notification
```csharp
public class NotificationService
{
    private readonly INotificationRepository _repo;

    public NotificationService(INotificationRepository repo)
    {
        _repo = repo;
    }

    public async Task RecordDeploymentAsync(DeploymentNotification notification)
    {
        if (notification == null) throw new ArgumentNullException(nameof(notification));
        await _repo.CreateAsync(notification);
    }
}
```

### Example 2: Retrieving pending notifications for processing
```csharp
public class NotificationProcessor
{
    private readonly INotificationRepository _repo;

    public NotificationProcessor(INotificationRepository repo)
    {
        _repo = repo;
    }

    public async Task ProcessPendingAsync()
    {
        var pending = await _repo.GetPendingAsync();
        foreach (var note in pending)
        {
            // Attempt to send notification, update status on success/failure
            try
            {
                await SendNotificationAsync(note);
                note.Status = NotificationStatus.Sent;
            }
            catch
            {
                note.Status = NotificationStatus.Failed;
            }
            await _repo.UpdateAsync(note);
        }
    }

    private Task SendNotificationAsync(DeploymentNotification note) => Task.CompletedTask;
}
```

## Notes
- Implementations should treat all methods as thread‑safe; concurrent calls from multiple threads must not corrupt internal state.  
- Methods that return collections (`GetByProjectAsync`, `GetPendingAsync`, `GetByEnvironmentAsync`, `GetByStatusAsync`, `GetAllAsync`) always return a non‑null list; the list may be empty to indicate absence of matching records.  
- Null arguments are validated by the interface contract; passing `null` where not permitted results in `ArgumentNullException`.  
- Update and delete operations assume the entity exists; attempting to modify or remove a non‑identified record yields `InvalidOperationException`.  
- All I/O‑bound operations are asynchronous; callers should `await` the returned `Task` to avoid blocking threads.  
- Specific exception types beyond those mentioned depend on the underlying persistence technology (e.g., Entity Framework, Dapper) and are not defined by the interface itself.
