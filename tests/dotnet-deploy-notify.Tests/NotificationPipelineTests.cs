#nullable enable
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Middleware;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class NotificationPipelineTests
{
    private readonly ILogger<NotificationPipeline> _logger;

    public NotificationPipelineTests()
    {
        _logger = Substitute.For<ILogger<NotificationPipeline>>();
    }

    [Fact]
    public async Task ExecuteAsync_WithNoProcessors_ReturnsSuccessTrue()
    {
        // Arrange
        var pipeline = new NotificationPipeline(_logger);
        var notification = new DeploymentNotification { ProjectName = "Test", Version = "1.0", Channels = new List<NotificationChannel> { NotificationChannel.Slack } };

        // Act
        var result = await pipeline.ExecuteAsync(notification);

        // Assert
        result.Success.Should().BeTrue();
        result.ProcessedNotification.Should().BeEquivalentTo(notification);
    }

    [Fact]
    public async Task ExecuteAsync_WithSuccessfulProcessor_ReturnsSuccessTrue()
    {
        // Arrange
        var pipeline = new NotificationPipeline(_logger);
        var processor = Substitute.For<INotificationProcessor>();
        processor.ProcessAsync(Arg.Any<PipelineContext>()).Returns(Task.CompletedTask);
        
        pipeline.Use(processor);
        var notification = new DeploymentNotification { ProjectName = "Test", Version = "1.0", Channels = new List<NotificationChannel> { NotificationChannel.Slack } };

        // Act
        var result = await pipeline.ExecuteAsync(notification);

        // Assert
        result.Success.Should().BeTrue();
        await processor.Received(1).ProcessAsync(Arg.Any<PipelineContext>());
    }

    [Fact]
    public async Task ExecuteAsync_WithFailingProcessor_ReturnsSuccessFalseAndErrors()
    {
        // Arrange
        var pipeline = new NotificationPipeline(_logger);
        var processor = Substitute.For<INotificationProcessor>();
        processor.ProcessAsync(Arg.Do<PipelineContext>(ctx => {
            ctx.IsValid = false;
            ctx.Errors.Add("Failed");
        })).Returns(Task.CompletedTask);
        
        pipeline.Use(processor);
        var notification = new DeploymentNotification();

        // Act
        var result = await pipeline.ExecuteAsync(notification);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Processor " + nameof(INotificationProcessor) + ": Failed");
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleProcessors_StopsOnFailure()
    {
        // Arrange
        var pipeline = new NotificationPipeline(_logger);
        
        var processor1 = Substitute.For<INotificationProcessor>();
        processor1.ProcessAsync(Arg.Any<PipelineContext>()).Returns(Task.CompletedTask);
        
        var processor2 = Substitute.For<INotificationProcessor>();
        processor2.ProcessAsync(Arg.Do<PipelineContext>(ctx => {
            ctx.IsValid = false;
            ctx.Errors.Add("Failed");
        })).Returns(Task.CompletedTask);

        var processor3 = Substitute.For<INotificationProcessor>();

        pipeline.Use(processor1).Use(processor2).Use(processor3);
        var notification = new DeploymentNotification();

        // Act
        var result = await pipeline.ExecuteAsync(notification);

        // Assert
        result.Success.Should().BeFalse();
        await processor1.Received(1).ProcessAsync(Arg.Any<PipelineContext>());
        await processor2.Received(1).ProcessAsync(Arg.Any<PipelineContext>());
        await processor3.DidNotReceive().ProcessAsync(Arg.Any<PipelineContext>());
    }
}
