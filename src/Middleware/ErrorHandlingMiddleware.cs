#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Middleware;

/// <summary>
/// Custom exceptions for domain-specific error handling
/// </summary>
public class NotificationException : Exception
{
    public string ErrorCode { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();

    public NotificationException(string message, string errorCode = "NOTIFICATION_ERROR")
        : base(message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        ArgumentException.ThrowIfNullOrEmpty(errorCode);
        ErrorCode = errorCode;
    }

    public NotificationException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        ArgumentException.ThrowIfNullOrEmpty(errorCode);
        ArgumentNullException.ThrowIfNull(innerException);
        ErrorCode = errorCode;
    }
}

public class WebhookException : NotificationException
{
    public string WebhookUrl { get; set; } = string.Empty;

    public WebhookException(string message, string webhookUrl)
        : base(message, "WEBHOOK_ERROR")
    {
        if (webhookUrl == null) throw new ArgumentNullException(nameof(webhookUrl));
        WebhookUrl = webhookUrl;
    }
}

public class ConfigurationException : NotificationException
{
    public ConfigurationException(string message)
        : base(message, "CONFIG_ERROR")
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
    }
}

public class ValidationException : NotificationException
{
    public List<string> ValidationErrors { get; set; } = new();

    public ValidationException(string message, List<string> errors)
        : base(message, "VALIDATION_ERROR")
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        ValidationErrors = errors;
    }
}

/// <summary>
/// Global error handler middleware
/// </summary>
public class ErrorHandlingInterceptor
{
    private readonly ILogger<ErrorHandlingInterceptor> _logger;

    public ErrorHandlingInterceptor(ILogger<ErrorHandlingInterceptor> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles exceptions and converts them to appropriate error responses
    /// </summary>
    public ErrorResponse HandleException(Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception: {ExceptionType}", exception.GetType().Name);

        return exception switch
        {
            ValidationException ve => new ErrorResponse
            {
                Code = ve.ErrorCode,
                Message = ve.Message,
                Details = new { errors = ve.ValidationErrors }
            },
            WebhookException we => new ErrorResponse
            {
                Code = we.ErrorCode,
                Message = we.Message,
                Details = new { webhookUrl = we.WebhookUrl }
            },
            NotificationException ne => new ErrorResponse
            {
                Code = ne.ErrorCode,
                Message = ne.Message,
                Details = ne.Details
            },
            _ => new ErrorResponse
            {
                Code = "INTERNAL_ERROR",
                Message = "An unexpected error occurred",
                Details = null
            }
        };
    }
}

/// <summary>
/// Standard error response format
/// </summary>
public class ErrorResponse
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public object? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public override string ToString() => $"[{Code}] {Message}";
}

/// <summary>
/// Rate limiting interceptor to prevent abuse
/// </summary>
public class RateLimitingInterceptor
{
    private readonly Dictionary<string, RateLimit> _limits = new();
    private readonly ILogger<RateLimitingInterceptor> _logger;

    public RateLimitingInterceptor(ILogger<RateLimitingInterceptor> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Checks if a request exceeds rate limit for the given key
    /// </summary>
    public bool IsAllowed(string clientId, int requestsPerMinute = 60)
    {
        lock (_limits)
        {
            if (!_limits.TryGetValue(clientId, out var limit))
            {
                limit = new RateLimit();
                _limits[clientId] = limit;
            }

            // Reset if window expired
            if (DateTime.UtcNow - limit.WindowStart > TimeSpan.FromMinutes(1))
            {
                limit.RequestCount = 0;
                limit.WindowStart = DateTime.UtcNow;
            }

            // Check limit
            if (limit.RequestCount >= requestsPerMinute)
            {
                _logger.LogWarning("Rate limit exceeded for client: {ClientId}", clientId);
                return false;
            }

            limit.RequestCount++;
            return true;
        }
    }

    /// <summary>
    /// Gets the remaining requests for a client in the current window
    /// </summary>
    public int GetRemainingRequests(string clientId, int limit = 60)
    {
        lock (_limits)
        {
            if (!_limits.TryGetValue(clientId, out var rl))
                return limit;

            return Math.Max(0, limit - rl.RequestCount);
        }
    }

    private class RateLimit
    {
        public int RequestCount { get; set; }
        public DateTime WindowStart { get; set; } = DateTime.UtcNow;
    }
}

/// <summary>
/// Logging interceptor to track all requests and responses
/// </summary>
public class LoggingInterceptor
{
    private readonly ILogger<LoggingInterceptor> _logger;

    public LoggingInterceptor(ILogger<LoggingInterceptor> logger)
    {
        _logger = logger;
    }

    public void LogRequest(string method, string path, Dictionary<string, string>? parameters = null)
    {
        var paramStr = parameters?.Any() == true
            ? " " + string.Join(", ", parameters.Select(p => $"{p.Key}={p.Value}"))
            : "";

        _logger.LogInformation("→ {Method} {Path}{Params}", method, path, paramStr);
    }

    public void LogResponse(string method, string path, int duration)
    {
        _logger.LogInformation("← {Method} {Path} completed in {Duration}ms", method, path, duration);
    }

    public void LogError(string method, string path, string error)
    {
        _logger.LogError("✗ {Method} {Path} failed: {Error}", method, path, error);
    }
}

/// <summary>
/// Request/Response timing and metrics interceptor
/// </summary>
public class PerformanceInterceptor
{
    private readonly ILogger<PerformanceInterceptor> _logger;
    private readonly int _warningThresholdMs;

    public PerformanceInterceptor(ILogger<PerformanceInterceptor> logger, int warningThresholdMs = 1000)
    {
        _logger = logger;
        _warningThresholdMs = warningThresholdMs;
    }

    public PerformanceTimer StartTimer(string operationName)
    {
        return new PerformanceTimer(operationName, _logger, _warningThresholdMs);
    }
}

/// <summary>
/// Timer for measuring operation performance
/// </summary>
public class PerformanceTimer : IDisposable
{
    private readonly string _operationName;
    private readonly ILogger _logger;
    private readonly int _warningThreshold;
    private readonly DateTime _startTime;

    public PerformanceTimer(string operationName, ILogger logger, int warningThreshold)
    {
        _operationName = operationName;
        _logger = logger;
        _warningThreshold = warningThreshold;
        _startTime = DateTime.UtcNow;
    }

    public void Dispose()
    {
        var duration = (int)(DateTime.UtcNow - _startTime).TotalMilliseconds;

        if (duration > _warningThreshold)
        {
            _logger.LogWarning("Operation took longer than expected: {Operation} ({Duration}ms)",
                _operationName, duration);
        }
        else
        {
            _logger.LogDebug("Operation completed: {Operation} ({Duration}ms)", _operationName, duration);
        }
    }
}
