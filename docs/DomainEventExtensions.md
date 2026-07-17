# DomainEventExtensions
The `DomainEventExtensions` class provides a set of static methods for working with domain events in the context of deployment notifications. These methods enable the evaluation and manipulation of domain events, allowing for more informed decision-making and streamlined notification processes.

## API
* `public static bool IsSuccess`: Evaluates whether a domain event represents a successful outcome. Returns `true` if the event indicates success, `false` otherwise. This method does not throw any exceptions.
* `public static IReadOnlyList<string> GetChannels`: Retrieves the list of channels associated with a domain event. Returns an empty list if no channels are specified. This method does not throw any exceptions.
* `public static string FormatForLog`: Formats a domain event for logging purposes. Returns a string representation of the event. This method does not throw any exceptions.
* `public static bool OccurredBetween`: Determines whether a domain event occurred within a specified time range. Returns `true` if the event falls within the range, `false` otherwise. This method does not throw any exceptions.
* `public static string? GetErrorMessage`: Retrieves the error message associated with a domain event, if any. Returns `null` if no error message is available. This method does not throw any exceptions.
* `public static bool HasChannels`: Checks whether a domain event has any associated channels. Returns `true` if channels are present, `false` otherwise. This method does not throw any exceptions.
* `public static string? GetNotificationId`: Retrieves the notification ID associated with a domain event, if any. Returns `null` if no notification ID is available. This method does not throw any exceptions.

## Usage
The following examples demonstrate how to use the `DomainEventExtensions` class:
```csharp
// Example 1: Evaluating domain event success
var @event = new DomainEvent { /* initialization */ };
if (DomainEventExtensions.IsSuccess(@event))
{
    Console.WriteLine("Domain event was successful.");
}

// Example 2: Retrieving channels and error messages
var channels = DomainEventExtensions.GetChannels(@event);
var errorMessage = DomainEventExtensions.GetErrorMessage(@event);
if (channels.Count > 0)
{
    Console.WriteLine("Channels: " + string.Join(", ", channels));
}
if (errorMessage != null)
{
    Console.WriteLine("Error message: " + errorMessage);
}
```

## Notes
When using the `DomainEventExtensions` class, consider the following edge cases and thread-safety remarks:
* The `GetChannels` and `GetErrorMessage` methods return `null` or empty collections if no data is available, so be sure to check for these cases when processing the results.
* The `OccurredBetween` method relies on the domain event's timestamp, so ensure that the event's timestamp is accurately set before calling this method.
* The `DomainEventExtensions` class is designed to be thread-safe, as all methods are static and do not modify any shared state. However, the thread-safety of the underlying domain event objects is not guaranteed, so be cautious when accessing or modifying these objects in a multithreaded environment.
