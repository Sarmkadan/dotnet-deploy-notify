#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;

namespace DotNetDeployNotify.Integration;

/// <summary>
/// Provides independent circuit breakers keyed by channel, so an outage on one
/// webhook endpoint (e.g. Slack) never blocks delivery to the others
/// </summary>
public interface ICircuitBreakerRegistry
{
    /// <summary>
    /// Gets the circuit breaker for the given channel key, creating one on first use
    /// </summary>
    /// <param name="channelKey">Stable identifier of the channel or endpoint (e.g. "slack", webhook host)</param>
    /// <returns>The circuit breaker dedicated to that channel</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="channelKey"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="channelKey"/> is empty</exception>
    CircuitBreaker GetOrAdd(string channelKey);
}

/// <summary>
/// Thread-safe registry of per-channel circuit breakers backed by a concurrent dictionary
/// </summary>
public sealed class CircuitBreakerRegistry : ICircuitBreakerRegistry
{
    private readonly ConcurrentDictionary<string, CircuitBreaker> _breakers = new(StringComparer.OrdinalIgnoreCase);
    private readonly int _failureThreshold;
    private readonly TimeSpan _timeout;
    private readonly ILoggerFactory? _loggerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="CircuitBreakerRegistry"/> class
    /// </summary>
    /// <param name="failureThreshold">Failure threshold applied to each created breaker</param>
    /// <param name="timeout">Open-state timeout applied to each created breaker</param>
    /// <param name="loggerFactory">Optional logger factory used to create per-breaker loggers</param>
    public CircuitBreakerRegistry(int failureThreshold = 5, TimeSpan? timeout = null, ILoggerFactory? loggerFactory = null)
    {
        _failureThreshold = failureThreshold;
        _timeout = timeout ?? TimeSpan.FromMinutes(1);
        _loggerFactory = loggerFactory;
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="channelKey"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="channelKey"/> is empty</exception>
    public CircuitBreaker GetOrAdd(string channelKey)
    {
        ArgumentException.ThrowIfNullOrEmpty(channelKey);

        return _breakers.GetOrAdd(
            channelKey,
            key => new CircuitBreaker(
                _failureThreshold,
                _timeout,
                _loggerFactory?.CreateLogger($"CircuitBreaker:{key}")));
    }
}
