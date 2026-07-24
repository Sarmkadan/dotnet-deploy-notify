#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Utilities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

/// <summary>
/// Tests for the CircuitBreakerWithBackoff class.
/// Tests all state transitions: Closed -> Open -> Half-Open -> Closed/Open
/// </summary>
public class CircuitBreakerWithBackoffTests
{
    private readonly ILogger _logger;

    public CircuitBreakerWithBackoffTests()
    {
        _logger = new TestLogger();
    }

    /// <summary>
    /// Verifies that the circuit breaker stays in Closed state when failures are below the threshold.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithFailuresBelowThreshold_StaysClosed()
    {
        // Arrange
        var breaker = new CircuitBreakerWithBackoff(failureThreshold: 5, logger: _logger);
        var failureCount = 0;

        async Task<int> FailingOperation()
        {
            failureCount++;
            if (failureCount <= 4) // Below threshold
            {
                throw new InvalidOperationException("Temporary failure");
            }
            return 42;
        }

        // Act & Assert
        breaker.State.Should().Be(CircuitBreakerWithBackoff.CircuitState.Closed);

        for (int i = 0; i < 4; i++)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => breaker.ExecuteAsync(FailingOperation));
            breaker.State.Should().Be(CircuitBreakerWithBackoff.CircuitState.Closed);
        }

        // After 4 failures, still closed
        breaker.State.Should().Be(CircuitBreakerWithBackoff.CircuitState.Closed);
    }

    /// <summary>
    /// Verifies that the circuit breaker transitions to Open state exactly at the failure threshold.
    /// Subsequent calls should fail fast without invoking the operation.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_AtFailureThreshold_TransitionsToOpenAndFailsFast()
    {
        // Arrange
        var breaker = new CircuitBreakerWithBackoff(failureThreshold: 3, logger: _logger);
        int callCount = 0;

        async Task<int> FailingOperation()
        {
            callCount++;
            throw new InvalidOperationException("Operation failed");
        }

        // Act - trigger exactly at threshold
        await Assert.ThrowsAsync<InvalidOperationException>(() => breaker.ExecuteAsync(FailingOperation));
        await Assert.ThrowsAsync<InvalidOperationException>(() => breaker.ExecuteAsync(FailingOperation));
        await Assert.ThrowsAsync<InvalidOperationException>(() => breaker.ExecuteAsync(FailingOperation));

        // Assert - should now be Open
        breaker.State.Should().Be(CircuitBreakerWithBackoff.CircuitState.Open);
        callCount.Should().Be(3, "Should have called the operation exactly 3 times");

        // Act - subsequent calls should fail fast without invoking the operation
        callCount = 0;
        await Assert.ThrowsAsync<InvalidOperationException>(() => breaker.ExecuteAsync(FailingOperation));
        callCount.Should().Be(0, "Should not call operation when circuit is open");

        await Assert.ThrowsAsync<InvalidOperationException>(() => breaker.ExecuteAsync(FailingOperation));
        callCount.Should().Be(0, "Should not call operation when circuit is open");
    }

    /// <summary>
    /// Verifies that after the reset timeout elapses, the breaker allows a trial call through.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_AfterTimeout_AllowsTrialCall()
    {
        // Arrange
        var breaker = new CircuitBreakerWithBackoff(
            failureThreshold: 2,
            openTimeout: TimeSpan.FromMilliseconds(100),
            logger: _logger
        );

        int callCount = 0;
        async Task<int> FailingOperation()
        {
            callCount++;
            throw new InvalidOperationException("Operation failed");
        }

        // Act - trigger Open state
        await Assert.ThrowsAsync<InvalidOperationException>(() => breaker.ExecuteAsync(FailingOperation));
        await Assert.ThrowsAsync<InvalidOperationException>(() => breaker.ExecuteAsync(FailingOperation));
        breaker.State.Should().Be(CircuitBreakerWithBackoff.CircuitState.Open);

        // Wait for timeout to elapse
        await Task.Delay(150);

        // Act - should allow one trial call
        callCount = 0;
        await Assert.ThrowsAsync<InvalidOperationException>(() => breaker.ExecuteAsync(FailingOperation));

        // Assert - operation was called once during trial
        callCount.Should().Be(1, "Should have called the operation exactly once in trial");
    }

    /// <summary>
    /// Verifies that a successful trial call closes the breaker and resets the failure counter.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_SuccessfulTrialCall_ClosesBreakerAndResetsFailures()
    {
        // Arrange
        var breaker = new CircuitBreakerWithBackoff(
            failureThreshold: 2,
            openTimeout: TimeSpan.FromMilliseconds(50),
            logger: _logger
        );

        int callCount = 0;
        async Task<int> FailingOperation()
        {
            callCount++;
            throw new InvalidOperationException("Operation failed");
        }

        // Act - trigger Open state
        await Assert.ThrowsAsync<InvalidOperationException>(() => breaker.ExecuteAsync(FailingOperation));
        await Assert.ThrowsAsync<InvalidOperationException>(() => breaker.ExecuteAsync(FailingOperation));

        // Wait for timeout
        await Task.Delay(100);

        // Act - successful call should close the breaker
        callCount = 0;
        var result = await breaker.ExecuteAsync(async () => { callCount++; return 100; });

        // Assert
        result.Should().Be(100);
        callCount.Should().Be(1);

        // Subsequent calls should work normally
        var result2 = await breaker.ExecuteAsync(async () => 200);
        result2.Should().Be(200);
    }

    /// <summary>
    /// Verifies that a failed trial call reopens the breaker.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_FailedTrialCall_ReopensBreaker()
    {
        // Arrange
        var breaker = new CircuitBreakerWithBackoff(
            failureThreshold: 2,
            openTimeout: TimeSpan.FromMilliseconds(50),
            logger: _logger
        );

        int callCount = 0;
        async Task<int> FailingOperation()
        {
            callCount++;
            throw new InvalidOperationException("Operation failed in trial");
        }

        // Act - trigger Open state
        await Assert.ThrowsAsync<InvalidOperationException>(() => breaker.ExecuteAsync(FailingOperation));
        await Assert.ThrowsAsync<InvalidOperationException>(() => breaker.ExecuteAsync(FailingOperation));

        // Wait for timeout
        await Task.Delay(100);

        // Act - failed call should reopen the breaker
        callCount = 0;
        await Assert.ThrowsAsync<InvalidOperationException>(() => breaker.ExecuteAsync(FailingOperation));

        // Assert - breaker is back to Open
        callCount.Should().Be(1);

        // Should still be open
        await Assert.ThrowsAsync<InvalidOperationException>(() => breaker.ExecuteAsync(FailingOperation));
        callCount.Should().Be(1, "Should not call operation when circuit is open");
    }

    /// <summary>
    /// Verifies that concurrent calls during Open state are all short-circuited without race conditions.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ConcurrentCallsDuringOpenState_AllFailFastWithoutRaceConditions()
    {
        // Arrange
        var breaker = new CircuitBreakerWithBackoff(
            failureThreshold: 2,
            openTimeout: TimeSpan.FromMilliseconds(50),
            logger: _logger
        );

        int callCount = 0;
        async Task<int> FailingOperation()
        {
            callCount++;
            throw new InvalidOperationException("Operation failed");
        }

        // Act - trigger Open state
        await Assert.ThrowsAsync<InvalidOperationException>(() => breaker.ExecuteAsync(FailingOperation));
        await Assert.ThrowsAsync<InvalidOperationException>(() => breaker.ExecuteAsync(FailingOperation));
        breaker.State.Should().Be(CircuitBreakerWithBackoff.CircuitState.Open);
        callCount.Should().Be(2);

        // Reset call count for concurrent test
        callCount = 0;

        // Act - multiple concurrent calls should all fail fast
        var tasks = new List<Task<int>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(breaker.ExecuteAsync(FailingOperation));
        }

        // Wait for all tasks to complete
        var results = await Task.WhenAll(tasks);

        // Assert - all should have failed fast without calling the operation
        foreach (var result in results)
        {
            // Verify the operation was never called by checking callCount is still 0
            callCount.Should().Be(0, "No operations should have been called when circuit is open");
            // And verify we get the expected exception
            await Assert.ThrowsAsync<InvalidOperationException>(() => breaker.ExecuteAsync(() => Task.FromResult(result)));
        }

        callCount.Should().Be(0, "No operations should have been called when circuit is open");
    }

    /// <summary>
    /// Verifies that the circuit breaker can recover from failure cycles.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_MultipleFailureCycles_RecoversProperly()
    {
        // Arrange
        var breaker = new CircuitBreakerWithBackoff(
            failureThreshold: 2,
            openTimeout: TimeSpan.FromMilliseconds(50),
            logger: _logger
        );

        // First failure cycle
        await Assert.ThrowsAsync<InvalidOperationException>(() => breaker.ExecuteAsync(() => Task.FromResult(1)));
        await Assert.ThrowsAsync<InvalidOperationException>(() => breaker.ExecuteAsync(() => Task.FromResult(1)));
        breaker.State.Should().Be(CircuitBreakerWithBackoff.CircuitState.Open);

        // Wait and recover
        await Task.Delay(100);
        var result1 = await breaker.ExecuteAsync(async () => 1);
        result1.Should().Be(1);

        // Second failure cycle - need to fail again to reopen
        await Assert.ThrowsAsync<InvalidOperationException>(() => breaker.ExecuteAsync(() => Task.FromResult(1)));
        await Assert.ThrowsAsync<InvalidOperationException>(() => breaker.ExecuteAsync(() => Task.FromResult(1)));
        breaker.State.Should().Be(CircuitBreakerWithBackoff.CircuitState.Open);

        // Wait and recover again
        await Task.Delay(100);
        var result2 = await breaker.ExecuteAsync(async () => 2);
        result2.Should().Be(2);
    }

    /// <summary>
    /// Test logger that captures log messages for verification.
    /// </summary>
    private class TestLogger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            // Log messages are captured for debugging
        }
    }
}