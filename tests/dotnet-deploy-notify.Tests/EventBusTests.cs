#nullable enable
using DotNetDeployNotify.Events;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class EventBusTests
{
    [Fact]
    public async Task PublishAsync_WithSubscribedHandler_InvokesHandlerWithEvent()
    {
        // Arrange
        var eventBus = CreateEventBus();
        var handler = new RecordingEventHandler<NotificationCreatedEvent>();
        var domainEvent = new NotificationCreatedEvent { NotificationId = "notification-1" };
        eventBus.Subscribe(handler);

        // Act
        await eventBus.PublishAsync(domainEvent);

        // Assert
        Assert.Same(domainEvent, Assert.Single(handler.ReceivedEvents));
    }

    [Fact]
    public async Task PublishAsync_WithNoHandlers_CompletesWithoutThrowing()
    {
        // Arrange
        var eventBus = CreateEventBus();
        var domainEvent = new NotificationCreatedEvent();

        // Act
        var exception = await Record.ExceptionAsync(() => eventBus.PublishAsync(domainEvent));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task PublishAsync_WithMultipleHandlers_InvokesAllHandlers()
    {
        // Arrange
        var eventBus = CreateEventBus();
        var firstHandler = new RecordingEventHandler<NotificationCreatedEvent>();
        var secondHandler = new RecordingEventHandler<NotificationCreatedEvent>();
        var domainEvent = new NotificationCreatedEvent();
        eventBus.Subscribe(firstHandler);
        eventBus.Subscribe(secondHandler);

        // Act
        await eventBus.PublishAsync(domainEvent);

        // Assert
        Assert.Same(domainEvent, Assert.Single(firstHandler.ReceivedEvents));
        Assert.Same(domainEvent, Assert.Single(secondHandler.ReceivedEvents));
    }

    [Fact]
    public async Task PublishAsync_AfterHandlerIsUnsubscribed_DoesNotInvokeHandler()
    {
        // Arrange
        var eventBus = CreateEventBus();
        var handler = new RecordingEventHandler<NotificationCreatedEvent>();
        eventBus.Subscribe(handler);
        eventBus.Unsubscribe(handler);

        // Act
        await eventBus.PublishAsync(new NotificationCreatedEvent());

        // Assert
        Assert.Empty(handler.ReceivedEvents);
    }

    [Fact]
    public async Task PublishAsync_WhenHandlerThrows_StillInvokesOtherHandlers()
    {
        // Arrange
        var eventBus = CreateEventBus();
        var throwingHandler = new ThrowingEventHandler<NotificationCreatedEvent>();
        var recordingHandler = new RecordingEventHandler<NotificationCreatedEvent>();
        var domainEvent = new NotificationCreatedEvent();
        eventBus.Subscribe(throwingHandler);
        eventBus.Subscribe(recordingHandler);

        // Act
        var exception = await Record.ExceptionAsync(() => eventBus.PublishAsync(domainEvent));

        // Assert
        Assert.Null(exception);
        Assert.Same(domainEvent, Assert.Single(recordingHandler.ReceivedEvents));
    }

    [Fact]
    public async Task PublishAsync_WithHandlersForDifferentEventTypes_InvokesOnlyMatchingHandler()
    {
        // Arrange
        var eventBus = CreateEventBus();
        var matchingHandler = new RecordingEventHandler<NotificationCreatedEvent>();
        var nonMatchingHandler = new RecordingEventHandler<NotificationProcessedEvent>();
        var domainEvent = new NotificationCreatedEvent();
        eventBus.Subscribe(matchingHandler);
        eventBus.Subscribe(nonMatchingHandler);

        // Act
        await eventBus.PublishAsync(domainEvent);

        // Assert
        Assert.Same(domainEvent, Assert.Single(matchingHandler.ReceivedEvents));
        Assert.Empty(nonMatchingHandler.ReceivedEvents);
    }

    private static InMemoryEventBus CreateEventBus()
        => new(NullLogger<InMemoryEventBus>.Instance);

    private sealed class RecordingEventHandler<TEvent> : IEventHandler<TEvent>
        where TEvent : DomainEvent
    {
        public List<TEvent> ReceivedEvents { get; } = new();

        public Task HandleAsync(TEvent @event)
        {
            ReceivedEvents.Add(@event);
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingEventHandler<TEvent> : IEventHandler<TEvent>
        where TEvent : DomainEvent
    {
        public Task HandleAsync(TEvent @event)
            => throw new InvalidOperationException("Handler failure.");
    }
}
