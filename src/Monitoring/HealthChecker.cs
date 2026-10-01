#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetDeployNotify.Monitoring;

/// <summary>
/// Performs health checks against deployment endpoints to verify service availability.
/// </summary>
public sealed class HealthChecker : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HealthChecker> _logger;
    private readonly HealthCheckerOptions _options;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="HealthChecker"/> class.
    /// </summary>
    /// <param name="httpClient">HTTP client for health probes.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="options">Health checker configuration.</param>
    public HealthChecker(
        HttpClient httpClient,
        ILogger<HealthChecker> logger,
        IOptions<HealthCheckerOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
    }

    /// <summary>
    /// Checks the health of a single endpoint.
    /// </summary>
    /// <param name="endpointUrl">URL to probe.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Health check result.</returns>
    public async Task<HealthCheckResult> CheckEndpointAsync(string endpointUrl, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpointUrl);
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var response = await _httpClient.GetAsync(endpointUrl, cancellationToken);
            sw.Stop();

            var result = new HealthCheckResult
            {
                Endpoint = endpointUrl,
                IsHealthy = response.IsSuccessStatusCode,
                StatusCode = (int)response.StatusCode,
                ResponseTimeMs = sw.ElapsedMilliseconds,
                CheckedAt = DateTimeOffset.UtcNow
            };

            _logger.LogDebug("Health check {Endpoint}: {StatusCode} in {ResponseTime}ms",
                endpointUrl, result.StatusCode, result.ResponseTimeMs);
            return result;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            sw.Stop();
            _logger.LogWarning(ex, "Health check failed for {Endpoint}", endpointUrl);
            return new HealthCheckResult
            {
                Endpoint = endpointUrl,
                IsHealthy = false,
                ErrorMessage = ex.Message,
                ResponseTimeMs = sw.ElapsedMilliseconds,
                CheckedAt = DateTimeOffset.UtcNow
            };
        }
    }

    /// <summary>
    /// Runs health checks against all configured endpoints.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of health check results.</returns>
    public async Task<IReadOnlyList<HealthCheckResult>> CheckAllEndpointsAsync(CancellationToken cancellationToken = default)
    {
        var tasks = _options.Endpoints.Select(ep => CheckEndpointAsync(ep, cancellationToken));
        var results = await Task.WhenAll(tasks);
        return results.ToList().AsReadOnly();
    }

    /// <summary>
    /// Polls an endpoint until it becomes healthy or the retry limit is reached.
    /// </summary>
    /// <param name="endpointUrl">URL to probe.</param>
    /// <param name="maxRetries">Maximum number of retries.</param>
    /// <param name="delayBetweenRetries">Delay between retries.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the endpoint became healthy within retries.</returns>
    public async Task<bool> WaitForHealthyAsync(
        string endpointUrl,
        int maxRetries = 10,
        TimeSpan? delayBetweenRetries = null,
        CancellationToken cancellationToken = default)
    {
        var delay = delayBetweenRetries ?? TimeSpan.FromSeconds(3);

        for (int i = 0; i < maxRetries; i++)
        {
            var result = await CheckEndpointAsync(endpointUrl, cancellationToken);
            if (result.IsHealthy)
            {
                _logger.LogInformation("Endpoint {Endpoint} healthy after {Attempts} attempt(s)", endpointUrl, i + 1);
                return true;
            }

            if (i < maxRetries - 1)
                await Task.Delay(delay, cancellationToken);
        }

        _logger.LogError("Endpoint {Endpoint} unhealthy after {MaxRetries} retries", endpointUrl, maxRetries);
        return false;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
/// Result of a single health check probe.
/// </summary>
public sealed class HealthCheckResult
{
    /// <summary>The probed endpoint URL.</summary>
    public required string Endpoint { get; init; }

    /// <summary>Whether the endpoint is considered healthy.</summary>
    public bool IsHealthy { get; init; }

    /// <summary>HTTP status code, if a response was received.</summary>
    public int? StatusCode { get; init; }

    /// <summary>Response time in milliseconds.</summary>
    public long ResponseTimeMs { get; init; }

    /// <summary>Error message if the check failed.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>When the check was performed.</summary>
    public DateTimeOffset CheckedAt { get; init; }
}

/// <summary>
/// Configuration for <see cref="HealthChecker"/>.
/// </summary>
public sealed class HealthCheckerOptions
{
    /// <summary>List of endpoint URLs to check.</summary>
    public List<string> Endpoints { get; set; } = [];

    /// <summary>HTTP request timeout in seconds.</summary>
    public int TimeoutSeconds { get; set; } = 10;
}
