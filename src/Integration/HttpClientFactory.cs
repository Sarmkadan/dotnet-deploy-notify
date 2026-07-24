#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using DotNetDeployNotify.Core.Models;

namespace DotNetDeployNotify.Integration;

/// <summary>
/// HTTP response wrapper with status and content
/// </summary>
public class HttpResponse<T>
{
    public bool IsSuccessful { get; set; }
    public int StatusCode { get; set; }
    public T? Content { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan ElapsedTime { get; set; }

    public override string ToString()
    {
        return $"HTTP {StatusCode}: {(IsSuccessful ? "Success" : ErrorMessage)}";
    }
}

/// <summary>
/// Exception thrown when the circuit breaker is open and prevents execution
/// </summary>
public class CircuitOpenException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CircuitOpenException"/> class
    /// </summary>
    public CircuitOpenException()
        : base("Circuit breaker is open and prevents execution")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CircuitOpenException"/> class with a custom message
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public CircuitOpenException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CircuitOpenException"/> class with a custom message and inner exception
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public CircuitOpenException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// HTTP client wrapper with built-in retry logic and error handling
/// </summary>
public class RetryableHttpClient
{
    private readonly HttpClient _client;
    private readonly ILogger<RetryableHttpClient> _logger;
    private readonly int _maxRetries;
    private readonly TimeSpan _retryDelay;
    private readonly ICircuitBreakerRegistry _circuitBreakers;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetryableHttpClient"/> class
    /// </summary>
    /// <param name="client">The underlying HTTP client</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="maxRetries">Maximum number of attempts per request</param>
    /// <param name="retryDelay">Base delay between retries (doubled per attempt)</param>
    /// <param name="circuitBreakers">Registry of per-endpoint circuit breakers; a default registry is created when omitted</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="client"/> or <paramref name="logger"/> is null</exception>
    public RetryableHttpClient(
        HttpClient client,
        ILogger<RetryableHttpClient> logger,
        int maxRetries = 3,
        TimeSpan? retryDelay = null,
        ICircuitBreakerRegistry? circuitBreakers = null)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(logger);

        _client = client;
        _logger = logger;
        _maxRetries = maxRetries;
        _retryDelay = retryDelay ?? TimeSpan.FromMilliseconds(500);
        _circuitBreakers = circuitBreakers ?? new CircuitBreakerRegistry();
    }

    /// <summary>
    /// Sends a POST request with automatic retry on failure
    /// </summary>
    /// <param name="url">The URL to send the POST request to</param>
    /// <param name="content">The HTTP content to send</param>
    /// <returns>HTTP response with status and content</returns>
    /// <exception cref="CircuitOpenException">Thrown when the circuit breaker is open and prevents execution</exception>
    public async Task<HttpResponse<string>> PostWithRetryAsync(string url, HttpContent content)
    {
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(content);

        var startTime = DateTime.UtcNow;
        var circuitBreaker = _circuitBreakers.GetOrAdd(GetEndpointKey(url));

        for (int attempt = 1; attempt <= _maxRetries; attempt++)
        {
            // Check the endpoint's circuit breaker before attempting
            if (!circuitBreaker.CanExecute())
            {
                _logger.LogWarning("Circuit breaker is open, failing fast for request: {Url}", url);
                throw new CircuitOpenException("Circuit breaker is open and prevents execution");
            }

            try
            {
                _logger.LogDebug("POST request attempt {Attempt}/{MaxRetries}: {Url}", attempt, _maxRetries, url);

                var response = await _client.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("POST succeeded: {Url} ({StatusCode})", url, response.StatusCode);
                    circuitBreaker.RecordSuccess();
                    return new HttpResponse<string>
                    {
                        IsSuccessful = true,
                        StatusCode = (int)response.StatusCode,
                        Content = responseContent,
                        ElapsedTime = DateTime.UtcNow - startTime
                    };
                }

                _logger.LogWarning("POST failed (attempt {Attempt}): {Url} returned {StatusCode}", attempt, url, response.StatusCode);

                if (!IsRetryable((int)response.StatusCode) || attempt == _maxRetries)
                {
                    circuitBreaker.RecordFailure();
                    return new HttpResponse<string>
                    {
                        IsSuccessful = false,
                        StatusCode = (int)response.StatusCode,
                        ErrorMessage = responseContent,
                        ElapsedTime = DateTime.UtcNow - startTime
                    };
                }

                // Handle 429 Too Many Requests with Retry-After header
                if ((int)response.StatusCode == 429)
                {
                    var retryAfterDelay = await GetRetryAfterDelayAsync(response, responseContent);
                    if (retryAfterDelay.HasValue)
                    {
                        // Cap the delay at 30 seconds to prevent excessive waiting
                        var cappedDelay = TimeSpan.FromMilliseconds(Math.Min(retryAfterDelay.Value.TotalMilliseconds, TimeSpan.FromSeconds(30).TotalMilliseconds));
                        _logger.LogInformation("Rate limited with Retry-After header/body, waiting {Delay}ms before retry", cappedDelay.TotalMilliseconds);
                        await Task.Delay(cappedDelay);
                        // Don't count 429 against circuit breaker failure count
                        circuitBreaker.RecordSuccess(); // Reset failure count since we're respecting the rate limit
                        continue;
                    }
                }

                // Wait before retrying (exponential backoff for non-429 errors)
                if (attempt < _maxRetries)
                {
                    var delay = _retryDelay.Multiply(Math.Pow(2, attempt - 1));
                    await Task.Delay(delay);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "POST request exception (attempt {Attempt}): {Url}", attempt, url);
                circuitBreaker.RecordFailure();

                if (attempt == _maxRetries)
                {
                    return new HttpResponse<string>
                    {
                        IsSuccessful = false,
                        StatusCode = 0,
                        ErrorMessage = ex.Message,
                        ElapsedTime = DateTime.UtcNow - startTime
                    };
                }
            }
        }

        _logger.LogError("Failed after {MaxRetries} attempts: {Url}", _maxRetries, url);
        return new HttpResponse<string>
        {
            IsSuccessful = false,
            StatusCode = 0,
            ErrorMessage = $"Failed after {_maxRetries} attempts",
            ElapsedTime = DateTime.UtcNow - startTime
        };
    }

    /// <summary>
    /// Extracts the Retry-After delay from the response, either from the Retry-After header or Discord's retry_after field
    /// </summary>
    /// <param name="response">The HTTP response</param>
    /// <param name="responseContent">The response content as string</param>
    /// <returns>The delay to wait before retrying, or null if no valid Retry-After information is found</returns>
    private async Task<TimeSpan?> GetRetryAfterDelayAsync(HttpResponseMessage response, string responseContent)
    {
        // First try the Retry-After header (standard HTTP header)
        if (response.Headers.TryGetValues("Retry-After", out var retryAfterValues))
        {
            var retryAfterHeader = retryAfterValues.FirstOrDefault();
            if (retryAfterHeader != null)
            {
                // Retry-After can be either a delay in seconds or an HTTP-date
                if (long.TryParse(retryAfterHeader, out var retryAfterSeconds))
                {
                    var delay = TimeSpan.FromSeconds(retryAfterSeconds);
                    _logger.LogDebug("Retry-After header specifies {Delay} seconds delay", retryAfterSeconds);
                    return delay;
                }
                else if (DateTime.TryParse(retryAfterHeader, out var retryAfterDate))
                {
                    var delay = retryAfterDate - DateTime.UtcNow;
                    if (delay > TimeSpan.Zero)
                    {
                        _logger.LogDebug("Retry-After header specifies HTTP-date delay: {Delay}", delay);
                        return delay;
                    }
                }
            }
        }

        // Try Discord-specific retry_after field in JSON body
        try
        {
            var jsonDoc = JsonDocument.Parse(responseContent);
            if (jsonDoc.RootElement.TryGetProperty("retry_after", out var retryAfterElement))
            {
                if (retryAfterElement.ValueKind == JsonValueKind.Number && retryAfterElement.TryGetInt64(out var retryAfterMs))
                {
                    var delay = TimeSpan.FromMilliseconds(retryAfterMs);
                    _logger.LogDebug("Discord retry_after field specifies {Delay}ms delay", retryAfterMs);
                    return delay;
                }
            }
        }
        catch (JsonException)
        {
            // Not JSON, ignore
        }

        return null;
    }

    /// <summary>
    /// Determines if an HTTP status code should trigger a retry
    /// </summary>
    /// <summary>
    /// Derives a stable circuit breaker key from a request URL (scheme + host + port),
    /// so each webhook endpoint gets its own independent breaker
    /// </summary>
    private static string GetEndpointKey(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri)
            ? $"{uri.Scheme}://{uri.Authority}"
            : url;

    private bool IsRetryable(int statusCode)
    {
        // Retry on server errors and specific client errors
        return statusCode >= 500 || statusCode == 408 || statusCode == 429;
    }
}

/// <summary>
/// HTTP request builder for fluent API construction
/// </summary>
public class HttpRequestBuilder
{
    private readonly HttpRequestMessage _request;
    private readonly Dictionary<string, string> _headers = new();

    public HttpRequestBuilder(HttpMethod method, string url)
    {
        _request = new HttpRequestMessage(method, url);
    }

    public HttpRequestBuilder AddHeader(string name, string value)
    {
        _headers[name] = value;
        return this;
    }

    public HttpRequestBuilder AddJsonContent(string json)
    {
        _request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        return this;
    }

    public HttpRequestBuilder SetTimeout(TimeSpan timeout)
    {
        _request.Options.Set(new HttpRequestOptionsKey<TimeSpan>("RequestTimeout"), timeout);
        return this;
    }

    public HttpRequestMessage Build()
    {
        foreach (var header in _headers)
        {
            _request.Headers.Add(header.Key, header.Value);
        }

        return _request;
    }

    public static HttpRequestBuilder Post(string url) => new(HttpMethod.Post, url);
    public static HttpRequestBuilder Get(string url) => new(HttpMethod.Get, url);
    public static HttpRequestBuilder Put(string url) => new(HttpMethod.Put, url);
    public static HttpRequestBuilder Delete(string url) => new(HttpMethod.Delete, url);
}

/// <summary>
/// Circuit breaker pattern implementation for HTTP requests
/// </summary>
public class CircuitBreaker
{
    /// <summary>
    /// Represents the possible states of the circuit breaker
    /// </summary>
    public enum State
    {
        /// <summary>Requests flow normally; failures are being counted</summary>
        Closed,

        /// <summary>Requests are rejected until the timeout elapses</summary>
        Open,

        /// <summary>A single trial request is allowed to probe recovery</summary>
        HalfOpen
    }

    private readonly object _sync = new();
    private State _state = State.Closed;
    private int _failureCount;
    private DateTime _lastFailureTime;
    private readonly int _failureThreshold;
    private readonly TimeSpan _timeout;
    private readonly ILogger _logger;

    /// <summary>
    /// Gets the current state of the circuit breaker
    /// </summary>
    public State CurrentState
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
    /// Initializes a new instance of the <see cref="CircuitBreaker"/> class
    /// </summary>
    /// <param name="failureThreshold">Number of consecutive failures that opens the circuit</param>
    /// <param name="timeout">Time the circuit stays open before allowing a trial request</param>
    /// <param name="logger">Optional logger for state transitions</param>
    public CircuitBreaker(int failureThreshold = 5, TimeSpan? timeout = null, ILogger? logger = null)
    {
        _failureThreshold = failureThreshold;
        _timeout = timeout ?? TimeSpan.FromMinutes(1);
        _logger = logger ?? new NullLogger();
    }

    /// <summary>
    /// Records a successful call, resetting the failure count and closing the circuit
    /// </summary>
    public void RecordSuccess()
    {
        var recovered = false;
        lock (_sync)
        {
            _failureCount = 0;
            if (_state != State.Closed)
            {
                _state = State.Closed;
                recovered = true;
            }
        }

        if (recovered)
        {
            _logger.LogInformation("Circuit breaker closed (recovered)");
        }
    }

    /// <summary>
    /// Records a failed call, opening the circuit once the failure threshold is reached
    /// </summary>
    public void RecordFailure()
    {
        var opened = false;
        int failures;
        lock (_sync)
        {
            _failureCount++;
            failures = _failureCount;
            _lastFailureTime = DateTime.UtcNow;

            if (_failureCount >= _failureThreshold && _state != State.Open)
            {
                _state = State.Open;
                opened = true;
            }
        }

        if (opened)
        {
            _logger.LogWarning("Circuit breaker opened (failures: {Count})", failures);
        }
    }

    /// <summary>
    /// Determines whether the circuit is currently open, transitioning to half-open once the timeout has elapsed
    /// </summary>
    /// <returns>True if the circuit is open and calls must be rejected, false otherwise</returns>
    public bool IsOpen()
    {
        var halfOpened = false;
        bool isOpen;
        lock (_sync)
        {
            if (_state == State.Open && DateTime.UtcNow - _lastFailureTime > _timeout)
            {
                _state = State.HalfOpen;
                halfOpened = true;
            }

            isOpen = _state == State.Open;
        }

        if (halfOpened)
        {
            _logger.LogInformation("Circuit breaker half-open (attempting recovery)");
        }

        return isOpen;
    }

    /// <summary>
    /// Checks if the circuit breaker allows execution
    /// </summary>
    /// <returns>True if execution is allowed, false if circuit is open</returns>
    public bool CanExecute() => !IsOpen();

    /// <summary>
    /// Null logger for when no logger is provided
    /// </summary>
    private class NullLogger : ILogger
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

/// <summary>
/// Extension methods for converting HttpResponse to NotificationResult for unified error handling
/// </summary>
public static class HttpResponseExtensions
{
    /// <summary>
    /// Converts an HttpResponse to a NotificationResult with consistent error classification
    /// </summary>
    /// <param name="httpResponse">The HTTP response to convert</param>
    /// <param name="notificationId">The notification ID for tracking</param>
    /// <param name="channel">The notification channel</param>
    /// <param name="configurationId">The configuration ID</param>
    /// <returns>A NotificationResult with properly classified status</returns>
    /// <exception cref="ArgumentNullException">Thrown when httpResponse is null</exception>
    public static NotificationResult ToNotificationResult(
        this HttpResponse<string> httpResponse,
        string notificationId,
        NotificationChannel channel,
        string configurationId)
    {
        ArgumentNullException.ThrowIfNull(httpResponse);
        ArgumentException.ThrowIfNullOrEmpty(notificationId);
        ArgumentException.ThrowIfNullOrEmpty(configurationId);

        var result = new NotificationResult
        {
            NotificationId = notificationId,
            Channel = channel,
            ConfigurationId = configurationId,
            HttpStatusCode = httpResponse.StatusCode,
            ResponseBody = httpResponse.Content,
            AttemptedAt = DateTime.UtcNow,
            DurationMs = (long)httpResponse.ElapsedTime.TotalMilliseconds
        };

        // Classify the status based on HTTP status code
        var statusClassification = ClassifyHttpStatusCode(httpResponse.StatusCode);

        switch (statusClassification)
        {
            case HttpStatusCodeClassification.Success:
                result.MarkAsSuccessful(httpResponse.StatusCode, httpResponse.Content ?? string.Empty);
                break;

            case HttpStatusCodeClassification.RetryableFailure:
                result.MarkAsFailed(
                    $"HTTP {httpResponse.StatusCode}: {httpResponse.ErrorMessage ?? httpResponse.Content ?? "Unknown error"}",
                    null,
                    httpResponse.StatusCode);
                result.MarkForRetry(GetRetryDelay(httpResponse.StatusCode));
                break;

            case HttpStatusCodeClassification.PermanentFailure:
            default:
                result.MarkAsFailed(
                    $"HTTP {httpResponse.StatusCode}: {httpResponse.ErrorMessage ?? httpResponse.Content ?? "Unknown error"}",
                    null,
                    httpResponse.StatusCode);
                break;
        }

        return result;
    }

    /// <summary>
    /// Classifies HTTP status codes into success, retryable failure, or permanent failure categories
    /// </summary>
    /// <param name="statusCode">The HTTP status code to classify</param>
    /// <returns>The classification of the status code</returns>
    private static HttpStatusCodeClassification ClassifyHttpStatusCode(int statusCode)
    {
        // 2xx: Success - always successful
        if (statusCode >= 200 && statusCode < 300)
        {
            return HttpStatusCodeClassification.Success;
        }

        // 4xx: Client errors - typically permanent failures, except specific retryable ones
        if (statusCode >= 400 && statusCode < 500)
        {
            // Retryable client errors (rate limiting, timeouts)
            if (statusCode == 408 || statusCode == 429)
            {
                return HttpStatusCodeClassification.RetryableFailure;
            }

            // All other 4xx errors are permanent failures
            return HttpStatusCodeClassification.PermanentFailure;
        }

        // 5xx: Server errors - always retryable
        if (statusCode >= 500 && statusCode < 600)
        {
            return HttpStatusCodeClassification.RetryableFailure;
        }

        // Unknown status codes default to permanent failure
        return HttpStatusCodeClassification.PermanentFailure;
    }

    /// <summary>
    /// Determines the retry delay based on HTTP status code
    /// </summary>
    /// <param name="statusCode">The HTTP status code</param>
    /// <returns>DateTime for next retry attempt</returns>
    private static DateTime GetRetryDelay(int statusCode)
    {
        // For 429 Too Many Requests, use exponential backoff based on Retry-After header
        // For other retryable errors, use a base delay
        var baseDelay = TimeSpan.FromSeconds(5);

        // Exponential backoff for server errors
        if (statusCode >= 500)
        {
            baseDelay = TimeSpan.FromSeconds(Math.Min(30, Math.Pow(2, 3))); // Cap at 30s
        }

        return DateTime.UtcNow.Add(baseDelay);
    }

    /// <summary>
    /// Classification of HTTP status codes for retry logic
    /// </summary>
    private enum HttpStatusCodeClassification
    {
        /// <summary>Request was successful</summary>
        Success,

        /// <summary>Failure that may succeed on retry</summary>
        RetryableFailure,

        /// <summary>Failure that won't succeed on retry</summary>
        PermanentFailure
    }
}
