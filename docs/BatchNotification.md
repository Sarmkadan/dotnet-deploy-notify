# BatchNotification

Represents a batch of deployment notifications to be sent through configured channels, tracking delivery status and metadata.

## API

### Properties

#### `public string Id`
Unique identifier for the batch. Used to correlate notifications and delivery attempts.

#### `public string Name`
Human-readable name for the batch. Optional but recommended for tracking purposes.

#### `public string Description`
Detailed description of the batch content or purpose. Optional.

#### `public List<DeploymentNotification> Notifications`
List of individual deployment notifications included in the batch. Cannot be null; defaults to empty list.

#### `public List<NotificationChannel> Channels`
List of channels through which the batch will be delivered (e.g., email, webhook). Cannot be null; defaults to empty list.

#### `public DateTime CreatedAt`
Timestamp when the batch was created. Set automatically on instantiation; read-only.

#### `public DateTime? ScheduledAt`
Optional timestamp when the batch is scheduled for delivery. Null if not scheduled.

#### `public DateTime? SentAt`
Timestamp when the batch was successfully sent. Null if not yet sent or failed.

#### `public BatchStatus Status`
Current status of the batch (e.g., Pending, Sent, Failed). Defaults to Pending.

#### `public int TotalDeliveryAttempts`
Total number of delivery attempts made for this batch. Increments on each attempt.

#### `public int SuccessfulDeliveries`
Number of successful deliveries across all channels. Resets on failure.

#### `public int FailedDeliveries`
Number of failed deliveries across all channels. Resets on success.

#### `public Dictionary<string, object> Metadata`
Additional key-value pairs for extensibility. Can be used to store custom context.

### Methods

#### `public int GetNotificationCount()`
Returns the number of notifications in the batch.

- **Returns**: Count of items in the `Notifications` list.
- **Throws**: None.

#### `public int GetTotalDeliveryTargets()`
Returns the total number of delivery targets across all channels and notifications.

- **Returns**: Sum of all target counts from each channel in `Channels`.
- **Throws**: None.

#### `public double GetSuccessRate()`
Calculates the success rate of deliveries as a value between 0.0 and 1.0.

- **Returns**: Success rate; 0.0 if no attempts have been made.
- **Throws**: None.

#### `public bool IsValid()`
Checks whether the batch is in a valid state for sending.

- **Returns**: `true` if `Status` is `Pending` and `Notifications` and `Channels` are non-empty; otherwise `false`.
- **Throws**: None.

#### `public bool IsReadyToSend()`
Checks whether the batch is ready to be sent based on scheduling and validity.

- **Returns**: `true` if `IsValid()` is `true` and either `ScheduledAt` is null or `ScheduledAt` <= `DateTime.UtcNow`; otherwise `false`.
- **Throws**: None.

#### `public void MarkAsSent()`
Updates the batch status and timestamps to reflect successful sending.

- **Throws**: `InvalidOperationException` if `Status` is not `Pending` or if `IsValid()` returns `false`.

#### `public void MarkAsFailed()`
Updates the batch status and timestamps to reflect failed sending.

- **Throws**: `InvalidOperationException` if `Status` is not `Pending` or if `IsValid()` returns `false`.

## Usage

### Example 1: Creating and sending a batch
