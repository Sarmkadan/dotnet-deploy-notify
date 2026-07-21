#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Threading.Tasks;
using Xunit;

namespace DotNetDeployNotify.Context.Tests;

/// <summary>
/// Tests for <see cref="AmbientRequestContext"/> to verify proper async context flow and isolation.
/// </summary>
public class AmbientRequestContextTests
{
    [Fact]
    public void Current_Getter_CreatesNewContext_WhenNoContextSet()
    {
        // Arrange
        AmbientRequestContext.ClearContext();

        // Act
        var context = AmbientRequestContext.Current;

        // Assert
        Assert.NotNull(context);
        Assert.NotNull(context.CorrelationId);
        Assert.NotNull(context.RequestId);
        Assert.Equal(DateTimeKind.Utc, context.RequestTime.Kind);
    }

    [Fact]
    public void Current_Getter_ReturnsSameContext_WithinSameLogicalCallContext()
    {
        // Arrange
        AmbientRequestContext.ClearContext();

        // Act
        var context1 = AmbientRequestContext.Current;
        var context2 = AmbientRequestContext.Current;

        // Assert
        Assert.Same(context1, context2);
    }

    [Fact]
    public void Current_Setter_Throws_WhenContextAlreadyExists()
    {
        // Arrange
        AmbientRequestContext.ClearContext();
        var initialContext = new RequestContext
        {
            CorrelationId = "test-correlation",
            RequestId = "test-request"
        };
        AmbientRequestContext.SetContext(initialContext);

        // Act & Assert
        var existingContext = new RequestContext
        {
            CorrelationId = "another-correlation",
            RequestId = "another-request"
        };

        var exception = Assert.Throws<InvalidOperationException>(() => AmbientRequestContext.Current = existingContext);
        Assert.Contains("already set", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SetContext_Throws_WhenContextAlreadyExists()
    {
        // Arrange
        AmbientRequestContext.ClearContext();
        var initialContext = new RequestContext
        {
            CorrelationId = "test-correlation",
            RequestId = "test-request"
        };
        AmbientRequestContext.SetContext(initialContext);

        // Act & Assert
        var anotherContext = new RequestContext
        {
            CorrelationId = "another-correlation",
            RequestId = "another-request"
        };

        var exception = Assert.Throws<InvalidOperationException>(() => AmbientRequestContext.SetContext(anotherContext));
        Assert.Contains("already set", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ClearContext_RemovesCurrentContext()
    {
        // Arrange
        var context = new RequestContext
        {
            CorrelationId = "test-correlation",
            RequestId = "test-request"
        };
        AmbientRequestContext.SetContext(context);

        // Act
        AmbientRequestContext.ClearContext();

        // Assert
        Assert.Null(AmbientRequestContext.Current.Metadata["test-key"]);
    }

    [Fact]
    public void Reset_CreatesNewContext()
    {
        // Arrange
        var initialContext = AmbientRequestContext.Current;
        initialContext.SetMetadata("test-key", "test-value");

        // Act
        AmbientRequestContext.Reset();

        // Assert
        var newContext = AmbientRequestContext.Current;
        Assert.NotSame(initialContext, newContext);
        Assert.Empty(newContext.Metadata);
    }

    [Fact]
    public async Task Current_ContextFlowsAcrossAwaitBoundary()
    {
        // Arrange
        AmbientRequestContext.ClearContext();
        var initialContext = new RequestContext
        {
            CorrelationId = "test-correlation",
            RequestId = "test-request"
        };
        initialContext.SetMetadata("test-key", "test-value");
        AmbientRequestContext.SetContext(initialContext);

        RequestContext? capturedContext = null;

        // Act
        await Task.Run(async () =>
        {
            // This runs on a different thread, but AsyncLocal should still flow the context
            capturedContext = AmbientRequestContext.Current;
            await Task.Delay(10); // Ensure we cross await boundaries
            capturedContext = AmbientRequestContext.Current; // Verify it still flows
        });

        // Assert
        Assert.NotNull(capturedContext);
        Assert.Equal("test-correlation", capturedContext.CorrelationId);
        Assert.Equal("test-value", capturedContext.GetMetadata<string>("test-key"));
    }

    [Fact]
    public async Task RequestContextScope_ProperlyRestoresPreviousContext()
    {
        // Arrange
        AmbientRequestContext.ClearContext();
        var outerContext = new RequestContext
        {
            CorrelationId = "outer-correlation",
            RequestId = "outer-request"
        };
        outerContext.SetMetadata("outer-key", "outer-value");
        AmbientRequestContext.SetContext(outerContext);

        var outerContextBefore = AmbientRequestContext.Current;

        RequestContext? innerContext = null;
        RequestContext? restoredContext = null;

        // Act
        using (var scope = new RequestContextScope())
        {
            var context = AmbientRequestContext.Current;
            context.CorrelationId = "inner-correlation";
            context.SetMetadata("inner-key", "inner-value");
            innerContext = AmbientRequestContext.Current;

            await Task.Delay(10); // Cross await boundary
            innerContext = AmbientRequestContext.Current; // Verify context still flows
        }

        restoredContext = AmbientRequestContext.Current;

        // Assert
        Assert.NotSame(outerContextBefore, innerContext);
        Assert.Equal("inner-correlation", innerContext.CorrelationId);
        Assert.Equal("inner-value", innerContext.GetMetadata<string>("inner-key"));

        Assert.Same(outerContextBefore, restoredContext);
        Assert.Equal("outer-correlation", restoredContext.CorrelationId);
        Assert.Equal("outer-value", restoredContext.GetMetadata<string>("outer-key"));
    }

    [Fact]
    public async Task Context_IsIsolatedBetweenParallelTasks()
    {
        // Arrange
        AmbientRequestContext.ClearContext();
        var sharedContext = new RequestContext
        {
            CorrelationId = "shared-correlation",
            RequestId = "shared-request"
        };
        sharedContext.SetMetadata("shared-key", "shared-value");
        AmbientRequestContext.SetContext(sharedContext);

        RequestContext? task1Context = null;
        RequestContext? task2Context = null;
        RequestContext? task3Context = null;

        // Act
        await Task.WhenAll(
            Task.Run(() => task1Context = AmbientRequestContext.Current),
            Task.Run(() => task2Context = AmbientRequestContext.Current),
            Task.Run(() => task3Context = AmbientRequestContext.Current)
        );

        // Assert
        Assert.NotNull(task1Context);
        Assert.NotNull(task2Context);
        Assert.NotNull(task3Context);

        // All tasks should see the same shared context
        Assert.Same(task1Context, task2Context);
        Assert.Same(task2Context, task3Context);
        Assert.Equal("shared-correlation", task1Context.CorrelationId);
        Assert.Equal("shared-value", task1Context.GetMetadata<string>("shared-key"));
    }

    [Fact]
    public async Task ExecuteInContextAsync_ProperlyIsolatesContext()
    {
        // Arrange
        AmbientRequestContext.ClearContext();
        var outerContext = new RequestContext
        {
            CorrelationId = "outer-correlation",
            RequestId = "outer-request"
        };
        outerContext.SetMetadata("outer-key", "outer-value");
        AmbientRequestContext.SetContext(outerContext);

        RequestContext? executedContext = null;

        // Act
        await RequestContextExtensions.ExecuteInContextAsync(async ctx =>
        {
            ctx.SetMetadata("executed-key", "executed-value");
            await Task.Delay(10); // Cross await boundary
            executedContext = ctx;
        });

        // Assert
        Assert.NotNull(executedContext);
        Assert.Equal("executed-value", executedContext.GetMetadata<string>("executed-key"));
        Assert.Equal("outer-correlation", AmbientRequestContext.Current.CorrelationId); // Outer context restored
        Assert.Equal("outer-value", AmbientRequestContext.Current.GetMetadata<string>("outer-key"));
    }

    [Fact]
    public void RequestContextScope_ConstructorWithCustomContext_SetsContext()
    {
        // Arrange
        AmbientRequestContext.ClearContext();
        var customContext = new RequestContext
        {
            CorrelationId = "custom-correlation",
            RequestId = "custom-request"
        };

        // Act
        using (var scope = new RequestContextScope(customContext))
        {
            // Assert
            Assert.Same(customContext, AmbientRequestContext.Current);
            Assert.Equal("custom-correlation", AmbientRequestContext.Current.CorrelationId);
        }
    }

    [Fact]
    public void RequestContextScope_Dispose_RestoresPreviousContext()
    {
        // Arrange
        AmbientRequestContext.ClearContext();
        var outerContext = new RequestContext
        {
            CorrelationId = "outer-correlation",
            RequestId = "outer-request"
        };
        AmbientRequestContext.SetContext(outerContext);

        RequestContext? innerContext = null;

        // Act
        RequestContextScope? scope = null;
        try
        {
            scope = new RequestContextScope();
            innerContext = AmbientRequestContext.Current;
            Assert.NotSame(outerContext, innerContext);
        }
        finally
        {
            scope?.Dispose();
        }

        // Assert
        Assert.Same(outerContext, AmbientRequestContext.Current);
    }
}
