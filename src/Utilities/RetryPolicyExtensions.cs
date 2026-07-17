#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;
using Microsoft.Extensions.Logging;

namespace DotNetDeployNotify.Utilities;

/// <summary>
/// Adapter to use NullLogger with specific generic logger types
/// </summary>
/// <typeparam name="TCategoryName">The category name type</typeparam>
internal sealed class NullLoggerAdapter<TCategoryName> : ILogger<TCategoryName>
{
    private readonly NullLogger _logger = NullLogger.Instance;

    public IDisposable? BeginScope<TState>(TState state) => _logger.BeginScope(state);

    public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
        => _logger.Log(logLevel, eventId, state, exception, formatter);
}

/// <summary>
/// Simple null logger implementation for use when no logger is provided
/// </summary>
internal sealed class NullLogger : ILogger
{
    public static readonly NullLogger Instance = new();

    private NullLogger() { }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => false;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter) { }
}

/// <summary>
/// Provides extension methods for <see cref="RetryPolicy"/> to simplify retry operations
/// </summary>
public static class RetryPolicyExtensions
{
    /// <summary>
    /// Creates a new <see cref="RetryPolicy"/> with default values
    /// </summary>
    /// <returns>A new <see cref="RetryPolicy"/> instance</returns>
    public static RetryPolicy WithDefaults()
        => new()
        {
            MaxAttempts = 3,
            InitialDelay = TimeSpan.FromMilliseconds(100),
            BackoffMultiplier = 2.0,
            MaxDelay = TimeSpan.FromSeconds(30),
            ShouldRetry = null
        };

    /// <summary>
    /// Creates a new <see cref="RetryPolicy"/> configured for immediate retries
    /// </summary>
    /// <param name="maxAttempts">Maximum number of retry attempts</param>
    /// <returns>A new <see cref="RetryPolicy"/> instance</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxAttempts is less than 1</exception>
    public static RetryPolicy WithImmediateRetries(int maxAttempts = 3)
    {
        if (maxAttempts < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxAttempts), "Max attempts must be at least 1");
        }

        return new RetryPolicy
        {
            MaxAttempts = maxAttempts,
            InitialDelay = TimeSpan.Zero,
            BackoffMultiplier = 1.0,
            MaxDelay = TimeSpan.Zero,
            ShouldRetry = null
        };
    }

    /// <summary>
    /// Creates a new <see cref="RetryPolicy"/> configured for exponential backoff with jitter
    /// </summary>
    /// <param name="maxAttempts">Maximum number of retry attempts</param>
    /// <param name="initialDelay">Initial delay before first retry</param>
    /// <param name="backoffMultiplier">Multiplier for exponential backoff</param>
    /// <param name="maxDelay">Maximum delay between retries</param>
    /// <returns>A new <see cref="RetryPolicy"/> instance</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when parameters are invalid</exception>
    public static RetryPolicy WithExponentialBackoff(
        int maxAttempts = 5,
        TimeSpan? initialDelay = null,
        double backoffMultiplier = 2.0,
        TimeSpan? maxDelay = null)
    {
        if (maxAttempts < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxAttempts), "Max attempts must be at least 1");
        }

        if (backoffMultiplier <= 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(backoffMultiplier), "Backoff multiplier must be greater than 1.0");
        }

        return new RetryPolicy
        {
            MaxAttempts = maxAttempts,
            InitialDelay = initialDelay ?? TimeSpan.FromMilliseconds(100),
            BackoffMultiplier = backoffMultiplier,
            MaxDelay = maxDelay ?? TimeSpan.FromSeconds(30),
            ShouldRetry = null
        };
    }

    /// <summary>
    /// Creates a new <see cref="RetryPolicy"/> with a custom retry condition
    /// </summary>
    /// <param name="maxAttempts">Maximum number of retry attempts</param>
    /// <param name="initialDelay">Initial delay before first retry</param>
    /// <param name="shouldRetry">Function that determines if an exception should trigger a retry</param>
    /// <returns>A new <see cref="RetryPolicy"/> instance</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxAttempts is less than 1</exception>
    public static RetryPolicy WithCustomRetryCondition(
        int maxAttempts = 3,
        TimeSpan? initialDelay = null,
        Func<Exception, bool>? shouldRetry = null)
    {
        if (maxAttempts < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxAttempts), "Max attempts must be at least 1");
        }

        return new RetryPolicy
        {
            MaxAttempts = maxAttempts,
            InitialDelay = initialDelay ?? TimeSpan.FromMilliseconds(100),
            BackoffMultiplier = 2.0,
            MaxDelay = TimeSpan.FromSeconds(30),
            ShouldRetry = shouldRetry
        };
    }

    /// <summary>
    /// Calculates the delay for a specific retry attempt based on the policy configuration
    /// </summary>
    /// <param name="policy">The retry policy to use</param>
    /// <param name="attemptNumber">The retry attempt number (1-based)</param>
    /// <returns>The calculated delay for the specified attempt</returns>
    /// <exception cref="ArgumentNullException">Thrown when policy is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when attemptNumber is less than 1</exception>
    public static TimeSpan GetDelay(this RetryPolicy policy, int attemptNumber)
    {
        ArgumentNullException.ThrowIfNull(policy);

        if (attemptNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(attemptNumber), "Attempt number must be at least 1");
        }

        var exponentialDelay = policy.InitialDelay.TotalMilliseconds *
            Math.Pow(policy.BackoffMultiplier, attemptNumber - 1);
        var cappedDelay = Math.Min(exponentialDelay, policy.MaxDelay.TotalMilliseconds);
        return TimeSpan.FromMilliseconds(Math.Clamp(cappedDelay, 0, int.MaxValue));
    }

    /// <summary>
    /// Calculates the delay for a specific retry attempt with jitter to prevent thundering herd
    /// </summary>
    /// <param name="policy">The retry policy to use</param>
    /// <param name="attemptNumber">The retry attempt number (1-based)</param>
    /// <returns>The calculated delay with jitter for the specified attempt</returns>
    /// <exception cref="ArgumentNullException">Thrown when policy is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when attemptNumber is less than 1</exception>
    public static TimeSpan GetDelayWithJitter(this RetryPolicy policy, int attemptNumber)
    {
        ArgumentNullException.ThrowIfNull(policy);

        if (attemptNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(attemptNumber), "Attempt number must be at least 1");
        }

        var baseDelay = policy.GetDelay(attemptNumber);
        var jitterRange = (int)Math.Min(baseDelay.TotalMilliseconds * 0.5, int.MaxValue);
        var jitter = Random.Shared.Next(0, Math.Max(1, jitterRange));
        return baseDelay.Add(TimeSpan.FromMilliseconds(jitter));
    }

    /// <summary>
    /// Creates a retry helper instance configured with this policy
    /// </summary>
    /// <param name="logger">Optional logger for retry operations</param>
    /// <returns>A new <see cref="RetryHelper"/> instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when policy is null</exception>
    public static RetryHelper CreateHelper(this RetryPolicy policy, ILogger<RetryHelper>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(policy);
        return new RetryHelper(logger ?? new NullLoggerAdapter<RetryHelper>());
    }

    /// <summary>
    /// Executes an asynchronous operation with retry logic using this policy
    /// </summary>
    /// <typeparam name="T">Return type of the operation</typeparam>
    /// <param name="policy">The retry policy to use</param>
    /// <param name="operation">The operation to execute</param>
    /// <returns>The result of the operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when policy or operation is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when all retry attempts are exhausted</exception>
    public static async Task<T> ExecuteAsync<T>(
        this RetryPolicy policy,
        Func<Task<T>> operation)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(operation);

        var helper = policy.CreateHelper();
        return await helper.ExecuteAsync(operation, policy);
    }

    /// <summary>
    /// Executes a synchronous operation with retry logic using this policy
    /// </summary>
    /// <typeparam name="T">Return type of the operation</typeparam>
    /// <param name="policy">The retry policy to use</param>
    /// <param name="operation">The operation to execute</param>
    /// <returns>The result of the operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when policy or operation is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when all retry attempts are exhausted</exception>
    public static T Execute<T>(
        this RetryPolicy policy,
        Func<T> operation)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(operation);

        var helper = policy.CreateHelper();
        return helper.Execute(operation, policy);
    }

    /// <summary>
    /// Gets all retry policy configuration values as a formatted string
    /// </summary>
    /// <param name="policy">The retry policy to format</param>
    /// <returns>A formatted string representing the policy configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when policy is null</exception>
    public static string FormatConfiguration(this RetryPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        return $"RetryPolicy Configuration:\n" +
               $"MaxAttempts: {policy.MaxAttempts}\n" +
               $"InitialDelay: {policy.InitialDelay.TotalMilliseconds}ms\n" +
               $"BackoffMultiplier: {policy.BackoffMultiplier}\n" +
               $"MaxDelay: {policy.MaxDelay.TotalMilliseconds}ms\n" +
               $"ShouldRetry: {(policy.ShouldRetry is null ? "null (default retry logic)" : "custom condition")}";
    }

    /// <summary>
    /// Determines if a retry should be attempted for the given exception
    /// </summary>
    /// <param name="policy">The retry policy to use</param>
    /// <param name="exception">The exception that occurred</param>
    /// <returns>True if a retry should be attempted; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when policy or exception is null</exception>
    public static bool ShouldRetryFor(this RetryPolicy policy, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(exception);

        return policy.ShouldRetry is null
            ? exception switch
            {
                OperationCanceledException => false,
                OutOfMemoryException => false,
                StackOverflowException => false,
                _ => true
            }
            : policy.ShouldRetry(exception);
    }

    /// <summary>
    /// Gets the maximum number of attempts including the initial attempt
    /// </summary>
    /// <param name="policy">The retry policy to use</param>
    /// <returns>The total number of attempts that will be made</returns>
    /// <exception cref="ArgumentNullException">Thrown when policy is null</exception>
    public static int GetTotalAttempts(this RetryPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        return policy.MaxAttempts;
    }

    /// <summary>
    /// Gets the retry attempts as a sequence of delays
    /// </summary>
    /// <param name="policy">The retry policy to use</param>
    /// <returns>An enumerable of delays for each retry attempt</returns>
    /// <exception cref="ArgumentNullException">Thrown when policy is null</exception>
    public static IEnumerable<TimeSpan> GetRetryDelays(this RetryPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        for (int attempt = 1; attempt <= policy.MaxAttempts; attempt++)
        {
            yield return policy.GetDelay(attempt);
        }
    }

    /// <summary>
    /// Gets the retry attempts with jitter as a sequence of delays
    /// </summary>
    /// <param name="policy">The retry policy to use</param>
    /// <returns>An enumerable of delays with jitter for each retry attempt</returns>
    /// <exception cref="ArgumentNullException">Thrown when policy is null</exception>
    public static IEnumerable<TimeSpan> GetRetryDelaysWithJitter(this RetryPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        for (int attempt = 1; attempt <= policy.MaxAttempts; attempt++)
        {
            yield return policy.GetDelayWithJitter(attempt);
        }
    }

    /// <summary>
    /// Clones this retry policy with modified values
    /// </summary>
    /// <param name="policy">The retry policy to clone</param>
    /// <param name="maxAttempts">New max attempts value, or null to keep current</param>
    /// <param name="initialDelay">New initial delay value, or null to keep current</param>
    /// <param name="backoffMultiplier">New backoff multiplier value, or null to keep current</param>
    /// <param name="maxDelay">New max delay value, or null to keep current</param>
    /// <param name="shouldRetry">New should retry function, or null to keep current</param>
    /// <returns>A new <see cref="RetryPolicy"/> instance with the specified modifications</returns>
    /// <exception cref="ArgumentNullException">Thrown when policy is null</exception>
    public static RetryPolicy With(
        this RetryPolicy policy,
        int? maxAttempts = null,
        TimeSpan? initialDelay = null,
        double? backoffMultiplier = null,
        TimeSpan? maxDelay = null,
        Func<Exception, bool>? shouldRetry = null)
    {
        ArgumentNullException.ThrowIfNull(policy);

        return new RetryPolicy
        {
            MaxAttempts = maxAttempts ?? policy.MaxAttempts,
            InitialDelay = initialDelay ?? policy.InitialDelay,
            BackoffMultiplier = backoffMultiplier ?? policy.BackoffMultiplier,
            MaxDelay = maxDelay ?? policy.MaxDelay,
            ShouldRetry = shouldRetry ?? policy.ShouldRetry
        };
    }

    /// <summary>
    /// Validates that the retry policy configuration is valid
    /// </summary>
    /// <param name="policy">The retry policy to validate</param>
    /// <returns>True if the policy is valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when policy is null</exception>
    public static bool IsValid(this RetryPolicy policy) =>
        policy is not null
        && policy.MaxAttempts >= 1
        && policy.InitialDelay >= TimeSpan.Zero
        && policy.BackoffMultiplier > 1.0
        && policy.MaxDelay >= policy.InitialDelay;

    /// <summary>
    /// Validates the retry policy and throws an exception if invalid
    /// </summary>
    /// <param name="policy">The retry policy to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when policy is null</exception>
    /// <exception cref="ArgumentException">Thrown when policy configuration is invalid</exception>
    public static void Validate(this RetryPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        if (policy.MaxAttempts < 1)
        {
            throw new ArgumentException("MaxAttempts must be at least 1", nameof(policy));
        }

        if (policy.InitialDelay < TimeSpan.Zero)
        {
            throw new ArgumentException("InitialDelay cannot be negative", nameof(policy));
        }

        if (policy.BackoffMultiplier <= 1.0)
        {
            throw new ArgumentException("BackoffMultiplier must be greater than 1.0", nameof(policy));
        }

        if (policy.MaxDelay < policy.InitialDelay)
        {
            throw new ArgumentException("MaxDelay cannot be less than InitialDelay", nameof(policy));
        }
    }
}