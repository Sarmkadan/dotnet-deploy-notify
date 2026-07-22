#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Integration;

/// <summary>
/// Factory for creating configured HttpClient instances with retry policies
/// </summary>
public interface IHttpClientFactory
{
    HttpClient CreateClient(string name = "default");
    HttpClient CreateClientWithRetry(int maxRetries = 3, TimeSpan? timeout = null);
}

/// <summary>
/// Default HttpClient factory implementation
/// </summary>
public class DefaultHttpClientFactory : IHttpClientFactory
{
    private readonly ILogger<DefaultHttpClientFactory> _logger;
    private readonly Dictionary<string, HttpClient> _clients = new();

    public DefaultHttpClientFactory(ILogger<DefaultHttpClientFactory> logger)
    {
        _logger = logger;
    }

    public HttpClient CreateClient(string name = "default")
    {
        if (_clients.TryGetValue(name, out var client))
            return client;

        var newClient = new HttpClient();
        newClient.DefaultRequestHeaders.Add("User-Agent", "DotNetDeployNotify/1.0");
        newClient.Timeout = TimeSpan.FromSeconds(30);

        _clients[name] = newClient;
        _logger.LogDebug("Created HttpClient: {Name}", name);

        return newClient;
    }

    public HttpClient CreateClientWithRetry(int maxRetries = 3, TimeSpan? timeout = null)
    {
        var client = CreateClient($"retry-{maxRetries}");
        client.Timeout = timeout ?? TimeSpan.FromSeconds(30);
        return client;
    }
}

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
    private readonly CircuitBreaker _circuitBreaker;

    public RetryableHttpClient(
        HttpClient client,
        ILogger<RetryableHttpClient> logger,
        int maxRetries = 3,
        TimeSpan? retryDelay = null,
        CircuitBreaker? circuitBreaker = null)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(logger);

        _client = client;
        _logger = logger;
        _maxRetries = maxRetries;
        _retryDelay = retryDelay ?? TimeSpan.FromMilliseconds(500);
        _circuitBreaker = circuitBreaker ?? new CircuitBreaker(logger: logger);
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

        for (int attempt = 1; attempt <= _maxRetries; attempt++)
        {
            // Check circuit breaker before attempting
            if (!_circuitBreaker.CanExecute())
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
                    _circuitBreaker.RecordSuccess();
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
                    _circuitBreaker.RecordFailure();
                    return new HttpResponse<string>
                    {
                        IsSuccessful = false,
                        StatusCode = (int)response.StatusCode,
                        ErrorMessage = responseContent,
                        ElapsedTime = DateTime.UtcNow - startTime
                    };
                }

                _circuitBreaker.RecordFailure();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "POST request exception (attempt {Attempt}): {Url}", attempt, url);
                _circuitBreaker.RecordFailure();

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

            // Wait before retrying (exponential backoff)
            if (attempt < _maxRetries)
            {
                var delay = _retryDelay.Multiply(Math.Pow(2, attempt - 1));
                await Task.Delay(delay);
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
    /// Determines if an HTTP status code should trigger a retry
    /// </summary>
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
    public enum State { Closed, Open, HalfOpen }

    public State CurrentState { get; private set; } = State.Closed;
    private int _failureCount;
    private DateTime _lastFailureTime;
    private readonly int _failureThreshold;
    private readonly TimeSpan _timeout;
    private readonly ILogger _logger;

    public CircuitBreaker(int failureThreshold = 5, TimeSpan? timeout = null, ILogger? logger = null)
    {
        _failureThreshold = failureThreshold;
        _timeout = timeout ?? TimeSpan.FromMinutes(1);
        _logger = logger ?? new NullLogger();
    }

    public void RecordSuccess()
    {
        _failureCount = 0;
        if (CurrentState != State.Closed)
        {
            CurrentState = State.Closed;
            _logger.LogInformation("Circuit breaker closed (recovered)");
        }
    }

    public void RecordFailure()
    {
        _failureCount++;
        _lastFailureTime = DateTime.UtcNow;

        if (_failureCount >= _failureThreshold)
        {
            CurrentState = State.Open;
            _logger.LogWarning("Circuit breaker opened (failures: {Count})", _failureCount);
        }
    }

    public bool IsOpen()
    {
        if (CurrentState == State.Open && DateTime.UtcNow - _lastFailureTime > _timeout)
        {
            CurrentState = State.HalfOpen;
            _logger.LogInformation("Circuit breaker half-open (attempting recovery)");
            return false;
        }

        return CurrentState == State.Open;
    }

    /// <summary>
    /// Checks if the circuit breaker allows execution and transitions from HalfOpen to Open if the attempt fails
    /// </summary>
    /// <returns>True if execution is allowed, false if circuit is open</returns>
    public bool CanExecute()
    {
        if (IsOpen())
        {
            return false;
        }

        if (CurrentState == State.HalfOpen)
        {
            return true; // Allow one attempt in half-open state
        }

        return true;
    }

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