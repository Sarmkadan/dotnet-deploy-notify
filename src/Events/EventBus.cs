#nullable enable
using System.Collections.Concurrent;

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Events;

/// <summary>
/// Base class for all domain events
/// </summary>
public abstract class DomainEvent
{
    public string EventId { get; } = Guid.NewGuid().ToString();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string AggregateId { get; protected set; } = string.Empty;

    public override string ToString() => $"{GetType().Name} - {AggregateId} @ {OccurredAt:O}";
}

/// <summary>
/// Event that fires when a notification is created
/// </summary>
public class NotificationCreatedEvent : DomainEvent
{
    public string NotificationId { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public List<string> Channels { get; set; } = new();
}

/// <summary>
/// Event that fires when a notification is processed
/// </summary>
public class NotificationProcessedEvent : DomainEvent
{
    public string NotificationId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public List<string> Channels { get; set; } = new();
    public string? Error { get; set; }
}

/// <summary>
/// Event that fires when a channel delivery fails
/// </summary>
public class ChannelDeliveryFailedEvent : DomainEvent
{
    public string NotificationId { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public int AttemptNumber { get; set; }
}

/// <summary>
/// Handler interface for domain events
/// </summary>
public interface IEventHandler<in TEvent> where TEvent : DomainEvent
{
    Task HandleAsync(TEvent @event);
}

/// <summary>
/// Event bus for publishing and subscribing to domain events
/// </summary>
public interface IEventBus
{
    void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : DomainEvent;
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : DomainEvent;
    void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : DomainEvent;
}

/// <summary>
/// In-memory event bus implementation
/// </summary>
public sealed class InMemoryEventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<object>> _handlers = new();
    private readonly ILogger<InMemoryEventBus> _logger;

    public InMemoryEventBus(ILogger<InMemoryEventBus> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : DomainEvent
    {
        ArgumentNullException.ThrowIfNull(handler);
        var eventType = typeof(TEvent);

        var handlersForEvent = _handlers.GetOrAdd(eventType, static _ => new List<object>());
        lock (handlersForEvent)
        {
            handlersForEvent.Add(handler);
        }
        _logger.LogDebug("Subscribed {HandlerType} to {EventType}",
            handler.GetType().Name, eventType.Name);
    }

    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : DomainEvent
    {
        ArgumentNullException.ThrowIfNull(@event);
        var eventType = typeof(TEvent);

        if (!_handlers.TryGetValue(eventType, out var handlersForEvent))
        {
            _logger.LogDebug("No handlers for event: {EventType}", eventType.Name);
            return;
        }

        object[] handlerSnapshot;
        lock (handlersForEvent)
        {
            handlerSnapshot = handlersForEvent.ToArray();
        }

        _logger.LogDebug("Publishing event: {EventType} with {HandlerCount} handlers",
            eventType.Name, handlerSnapshot.Length);

        var tasks = handlerSnapshot
            .Cast<IEventHandler<TEvent>>()
            .Select(handler => PublishToHandlerAsync(handler, @event));

        await Task.WhenAll(tasks);
    }

    public void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : DomainEvent
    {
        ArgumentNullException.ThrowIfNull(handler);
        var eventType = typeof(TEvent);

        if (_handlers.TryGetValue(eventType, out var handlersForEvent))
        {
            lock (handlersForEvent)
            {
                handlersForEvent.Remove(handler);
            }
            _logger.LogDebug("Unsubscribed {HandlerType} from {EventType}",
                handler.GetType().Name, eventType.Name);
        }
    }

    private async Task PublishToHandlerAsync<TEvent>(IEventHandler<TEvent> handler, TEvent @event)
        where TEvent : DomainEvent
    {
        try
        {
            _logger.LogDebug("Executing handler {HandlerType} for event {EventType}",
                handler.GetType().Name, @event.GetType().Name);

            await handler.HandleAsync(@event);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Handler {HandlerType} failed for event {EventId}",
                handler.GetType().Name, @event.EventId);
        }
    }
}

/// <summary>
/// Example event handler that logs notification creation
/// </summary>
public sealed class NotificationCreatedEventHandler : IEventHandler<NotificationCreatedEvent>
{
    private readonly ILogger<NotificationCreatedEventHandler> _logger;

    public NotificationCreatedEventHandler(ILogger<NotificationCreatedEventHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public Task HandleAsync(NotificationCreatedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        _logger.LogInformation(
            "Notification created: {NotificationId} for {ProjectName} v{Version} to {ChannelCount} channels",
            @event.NotificationId, @event.ProjectName, @event.Version, @event.Channels.Count);

        return Task.CompletedTask;
    }
}

/// <summary>
/// Example event handler that tracks delivery failures
/// </summary>
public sealed class ChannelDeliveryFailedEventHandler : IEventHandler<ChannelDeliveryFailedEvent>
{
    private readonly ILogger<ChannelDeliveryFailedEventHandler> _logger;

    public ChannelDeliveryFailedEventHandler(ILogger<ChannelDeliveryFailedEventHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public Task HandleAsync(ChannelDeliveryFailedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        _logger.LogWarning(
            "Channel delivery failed: Notification {NotificationId} to {Channel} (attempt {Attempt}): {Error}",
            @event.NotificationId, @event.ChannelName, @event.AttemptNumber, @event.ErrorMessage);

        return Task.CompletedTask;
    }
}

/// <summary>
/// Observer pattern implementation for real-time notifications
/// </summary>
public interface INotificationObserver
{
    Task OnNotificationCreatedAsync(string notificationId, string projectName);
    Task OnNotificationDeliveredAsync(string notificationId, string channel);
    Task OnDeliveryFailedAsync(string notificationId, string channel, string error);
}

/// <summary>
/// Subject class for managing observers
/// </summary>
public sealed class NotificationObservable
{
    private readonly List<INotificationObserver> _observers = new();
    private readonly ILogger<NotificationObservable> _logger;

    public NotificationObservable(ILogger<NotificationObservable> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public void Attach(INotificationObserver observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        _observers.Add(observer);
        _logger.LogDebug("Attached observer: {ObserverType}", observer.GetType().Name);
    }

    public void Detach(INotificationObserver observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        _observers.Remove(observer);
        _logger.LogDebug("Detached observer: {ObserverType}", observer.GetType().Name);
    }

    public async Task NotifyNotificationCreatedAsync(string notificationId, string projectName)
    {
        var tasks = _observers.Select(o => o.OnNotificationCreatedAsync(notificationId, projectName));
        await Task.WhenAll(tasks);
    }

    public async Task NotifyNotificationDeliveredAsync(string notificationId, string channel)
    {
        var tasks = _observers.Select(o => o.OnNotificationDeliveredAsync(notificationId, channel));
        await Task.WhenAll(tasks);
    }

    public async Task NotifyDeliveryFailedAsync(string notificationId, string channel, string error)
    {
        var tasks = _observers.Select(o => o.OnDeliveryFailedAsync(notificationId, channel, error));
        await Task.WhenAll(tasks);
    }
}
