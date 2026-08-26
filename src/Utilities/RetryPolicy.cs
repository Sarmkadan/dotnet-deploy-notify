#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Utilities;

/// <summary>
/// Retry policy configuration
/// </summary>
public class RetryPolicy
{
    public int MaxAttempts { get; set; } = 3;
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromMilliseconds(100);
    public double BackoffMultiplier { get; set; } = 2.0;
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(30);
    public Func<Exception, bool>? ShouldRetry { get; set; }

    public override string ToString()
    {
        return $"RetryPolicy {{ MaxAttempts = {MaxAttempts}, InitialDelay = {InitialDelay}, BackoffMultiplier = {BackoffMultiplier}, MaxDelay = {MaxDelay}, ShouldRetry = {(ShouldRetry != null ? "assigned" : "null")} }}";
    }
}

/// <summary>
/// Executes operations with automatic retry logic
/// </summary>
public class RetryHelper
{
    private readonly ILogger<RetryHelper> _logger;

    public RetryHelper(ILogger<RetryHelper> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Executes an operation with retry logic
    /// </summary>
    public async Task<T> ExecuteAsync<T>(
        Func<Task<T>> operation,
        RetryPolicy? policy = null)
    {
        policy ??= new RetryPolicy();

        for (int attempt = 1; attempt <= policy.MaxAttempts; attempt++)
        {
            try
            {
                _logger.LogDebug("Executing operation (attempt {Attempt}/{MaxAttempts})",
                    attempt, policy.MaxAttempts);

                return await operation();
            }
            catch (Exception ex)
            {
                if (attempt == policy.MaxAttempts ||
                    (policy.ShouldRetry is not null && !policy.ShouldRetry(ex)))
                {
                    _logger.LogError(ex, "Operation failed after {Attempt} attempts", attempt);
                    throw;
                }

                var delay = CalculateDelay(attempt, policy);
                _logger.LogWarning("Operation failed (attempt {Attempt}), retrying in {Delay}ms",
                    attempt, delay.TotalMilliseconds);

                await Task.Delay(delay);
            }
        }

        throw new InvalidOperationException("Retry logic failed unexpectedly");
    }

    /// <summary>
    /// Executes a synchronous operation with retry logic
    /// </summary>
    public T Execute<T>(
        Func<T> operation,
        RetryPolicy? policy = null)
    {
        policy ??= new RetryPolicy();

        for (int attempt = 1; attempt <= policy.MaxAttempts; attempt++)
        {
            try
            {
                return operation();
            }
            catch (Exception ex)
            {
                if (attempt == policy.MaxAttempts ||
                    (policy.ShouldRetry is not null && !policy.ShouldRetry(ex)))
                {
                    throw;
                }

                var delay = CalculateDelay(attempt, policy);
                Thread.Sleep(delay);
            }
        }

        throw new InvalidOperationException("Retry logic failed unexpectedly");
    }

    private TimeSpan CalculateDelay(int attemptNumber, RetryPolicy policy)
    {
        var exponentialDelay = policy.InitialDelay.Multiply(Math.Pow(policy.BackoffMultiplier, attemptNumber - 1));
        var cappedDelay = TimeSpan.FromMilliseconds(Math.Min(exponentialDelay.TotalMilliseconds, policy.MaxDelay.TotalMilliseconds));
        return cappedDelay;
    }
}

/// <summary>
/// Exponential backoff strategy
/// </summary>
public class ExponentialBackoff
{
    private readonly int _maxRetries;
    private readonly TimeSpan _initialDelay;
    private readonly double _multiplier;

    public ExponentialBackoff(int maxRetries = 3, TimeSpan? initialDelay = null, double multiplier = 2.0)
    {
        _maxRetries = maxRetries;
        _initialDelay = initialDelay ?? TimeSpan.FromMilliseconds(100);
        _multiplier = multiplier;
    }

    /// <summary>
    /// Gets the delay for the given attempt number
    /// </summary>
    public TimeSpan GetDelay(int attemptNumber)
    {
        if (attemptNumber < 1)
            return TimeSpan.Zero;

        return _initialDelay.Multiply(Math.Pow(_multiplier, attemptNumber - 1));
    }

    /// <summary>
    /// Adds jitter to the delay to prevent thundering herd
    /// </summary>
    public TimeSpan GetDelayWithJitter(int attemptNumber)
    {
        var baseDelay = GetDelay(attemptNumber);
        var jitter = Random.Shared.Next(0, (int)baseDelay.TotalMilliseconds / 2);
        return baseDelay.Add(TimeSpan.FromMilliseconds(jitter));
    }
}

/// <summary>
/// Circuit breaker with exponential backoff
/// </summary>
public class CircuitBreakerWithBackoff
{
    public enum CircuitState { Closed, Open, HalfOpen }

    private readonly object _sync = new();
    private CircuitState _state = CircuitState.Closed;
    private int _failureCount;
    private DateTime _lastFailureTime = DateTime.UtcNow;
    private readonly int _failureThreshold;
    private readonly TimeSpan _openTimeout;
    private readonly ILogger _logger;

    /// <summary>
    /// Gets the current state of the circuit breaker
    /// </summary>
    public CircuitState State
    {
        get
        {
            lock (_sync)
            {
                return _state;
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CircuitBreakerWithBackoff"/> class
    /// </summary>
    /// <param name="failureThreshold">Number of consecutive failures that opens the circuit</param>
    /// <param name="openTimeout">Time the circuit stays open before allowing a trial request</param>
    /// <param name="logger">Optional logger for state transitions</param>
    public CircuitBreakerWithBackoff(
        int failureThreshold = 5,
        TimeSpan? openTimeout = null,
        ILogger? logger = null)
    {
        _failureThreshold = failureThreshold;
        _openTimeout = openTimeout ?? TimeSpan.FromMinutes(1);
        _logger = logger ?? new NullLogger();
    }

    /// <summary>
    /// Executes the given operation through the circuit breaker, recording success or failure
    /// and transitioning the breaker state under a lock so concurrent callers never race
    /// </summary>
    /// <param name="operation">The asynchronous operation to guard</param>
    /// <returns>The result produced by <paramref name="operation"/></returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="operation"/> is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when the circuit is open and the timeout has not yet elapsed</exception>
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var halfOpened = false;
        lock (_sync)
        {
            if (_state == CircuitState.Open)
            {
                if (DateTime.UtcNow - _lastFailureTime > _openTimeout)
                {
                    _state = CircuitState.HalfOpen;
                    halfOpened = true;
                }
                else
                {
                    throw new InvalidOperationException("Circuit breaker is open");
                }
            }
        }

        if (halfOpened)
        {
            _logger.LogInformation("Circuit breaker state changed to HalfOpen");
        }

        try
        {
            var result = await operation();
            OnSuccess();
            return result;
        }
        catch (Exception)
        {
            OnFailure();
            throw;
        }
    }

    private void OnSuccess()
    {
        var recovered = false;
        lock (_sync)
        {
            _failureCount = 0;
            if (_state != CircuitState.Closed)
            {
                _state = CircuitState.Closed;
                recovered = true;
            }
        }

        if (recovered)
        {
            _logger.LogInformation("Circuit breaker state changed to Closed");
        }
    }

    private void OnFailure()
    {
        var opened = false;
        int failures;
        lock (_sync)
        {
            _failureCount++;
            failures = _failureCount;
            _lastFailureTime = DateTime.UtcNow;

            if (_failureCount >= _failureThreshold && _state != CircuitState.Open)
            {
                _state = CircuitState.Open;
                opened = true;
            }
        }

        if (opened)
        {
            _logger.LogWarning("Circuit breaker state changed to Open (failures: {Count})", failures);
        }
    }

    internal class NullLogger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => new NoOpDisposable();
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }

        private class NoOpDisposable : IDisposable
        {
            public void Dispose() { }
        }
    }
}
