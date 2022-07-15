# DomainEvent

`DomainEvent` is a base class representing domain events in the `dotnet-deploy-notify` system. It serves as a foundational type for event-driven notifications, tracking occurrences within the domain, and facilitating communication between system components via an in-memory event bus.

## API

### `public string EventId`
A unique identifier for the event. Used to distinguish between different events and ensure idempotency in event processing.

### `public DateTime OccurredAt`
The timestamp when the event occurred. Provides temporal context for event ordering and processing.

### `public string AggregateId`
The identifier of the aggregate root associated with the event. Used to correlate events with specific domain entities.

### `public override string ToString()`
Returns a string representation of the event, including its `EventId`, `OccurredAt`, and `AggregateId`. Useful for logging and debugging.

### `public string NotificationId`
A unique identifier for the notification derived from the event. Used to track notifications through the system.

### `public string ProjectName`
The name of the project associated with the event. Used to scope notifications to specific projects.

### `public string Version`
The version of the project or system component that generated the event. Helps track compatibility and changes over time.

### `public List<string> Channels`
A list of channels (e.g., email, Slack, Teams) to which the event should be published. Determines where notifications are sent.

### `public bool Success`
Indicates whether the operation associated with the event completed successfully. Used to determine if follow-up actions are needed.

### `public string? Error`
An optional error message describing any failure associated with the event. `null` if the operation succeeded.

### `public string ChannelName`
The name of the channel to which an error notification is being sent. Used in error-specific notifications.

### `public string ErrorMessage`
The message describing an error condition. Used in error notifications to provide context.

### `public int AttemptNumber`
The number of attempts made to process the event or notification. Used for retry logic and tracking.

### `public InMemoryEventBus`
A static instance of an in-memory event bus for publishing and subscribing to domain events. Facilitates decoupled communication between components.

### `public void Subscribe<TEvent>(Action<TEvent> handler)`
Subscribes a handler to events of type `TEvent`. The handler is invoked when an event of the specified type is published.

- **Parameters**:
  - `handler`: The action to invoke when the event is published.
- **Throws**: `ArgumentNullException` if `handler` is `null`.

### `public async Task PublishAsync<TEvent>(TEvent @event)`
Publishes an event of type `TEvent` to all subscribed handlers asynchronously.

- **Parameters**:
  - `@event`: The event to publish.
- **Returns**: A `Task` representing the asynchronous operation.
- **Throws**: `ArgumentNullException` if `@event` is `null`.

### `public void Unsubscribe<TEvent>(Action<TEvent> handler)`
Unsubscribes a previously registered handler for events of type `TEvent`.

- **Parameters**:
  - `handler`: The handler to unsubscribe.
- **Throws**: `ArgumentNullException` if `handler` is `null`.

## Usage

### Publishing and Subscribing to Events
