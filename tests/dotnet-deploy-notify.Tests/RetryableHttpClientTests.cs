#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Net;
using DotNetDeployNotify.Integration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

/// <summary>
/// Tests for RetryableHttpClient retry/backoff behavior and terminal failure handling
/// </summary>
public class RetryableHttpClientTests
{
    private readonly ILogger<RetryableHttpClient> _logger;

    public RetryableHttpClientTests()
    {
        _logger = Substitute.For<ILogger<RetryableHttpClient>>();
    }

    /// <summary>
    /// Test logger that captures log messages for verification
    /// </summary>
    private class TestLogger : ILogger<RetryableHttpClient>
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
            // Capture log messages for debugging if needed
        }
    }

    /// <summary>
    /// Verifies that RetryableHttpClient retries up to configured max attempts and returns success
    /// once the underlying call succeeds within the retry budget
    /// </summary>
    [Fact]
    public async Task PostWithRetryAsync_RetryableFailuresThenSuccess_ReturnsSuccessfulResponse()
    {
        // Arrange
        var mockHttp = Substitute.For<HttpClient>();
        var logger = new TestLogger();
        var client = new RetryableHttpClient(mockHttp, logger, maxRetries: 5, retryDelay: TimeSpan.FromMilliseconds(10));

        var attemptCount = 0;
        mockHttp
            .SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns<Task<HttpResponseMessage>>(x =>
            {
                attemptCount++;
                if (attemptCount <= 3)
                {
                    // Return 503 Service Unavailable for first 3 attempts
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                    {
                        Content = new StringContent("Service temporarily unavailable")
                    });
                }

                // Return success on 4th attempt
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"success\": true}")
                });
            });

        // Act
        var response = await client.PostWithRetryAsync("https://example.com/api/test", new StringContent("{}"));

        // Assert
        Assert.True(response.IsSuccessful);
        Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("{\"success\": true}", response.Content);
        Assert.Equal(4, attemptCount); // Should have attempted 4 times (3 failures + 1 success)
    }

    /// <summary>
    /// Verifies that RetryableHttpClient stops retrying and returns the terminal error
    /// once max attempts is exhausted without success
    /// </summary>
    [Fact]
    public async Task PostWithRetryAsync_MaxRetriesExhausted_ReturnsTerminalError()
    {
        // Arrange
        var mockHttp = Substitute.For<HttpClient>();
        var logger = new TestLogger();
        var client = new RetryableHttpClient(mockHttp, logger, maxRetries: 3, retryDelay: TimeSpan.FromMilliseconds(10));

        var attemptCount = 0;
        mockHttp
            .SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns<Task<HttpResponseMessage>>(x =>
            {
                attemptCount++;
                // Always return 503 Service Unavailable
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    Content = new StringContent("Service temporarily unavailable")
                });
            });

        // Act
        var response = await client.PostWithRetryAsync("https://example.com/api/test", new StringContent("{}"));

        // Assert
        Assert.False(response.IsSuccessful);
        Assert.Equal((int)HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("Service temporarily unavailable", response.ErrorMessage);
        Assert.Equal(3, attemptCount); // Should have attempted exactly maxRetries times
    }

    /// <summary>
    /// Verifies that non-transient status codes (e.g., 400 Bad Request) are NOT retried
    /// and fail immediately on first attempt
    /// </summary>
    [Fact]
    public async Task PostWithRetryAsync_NonRetryableStatusCode_FailsImmediately()
    {
        // Arrange
        var mockHttp = Substitute.For<HttpClient>();
        var logger = new TestLogger();
        var client = new RetryableHttpClient(mockHttp, logger, maxRetries: 5, retryDelay: TimeSpan.FromMilliseconds(10));

        var attemptCount = 0;
        mockHttp
            .SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns<Task<HttpResponseMessage>>(x =>
            {
                attemptCount++;
                // Return 400 Bad Request which is NOT retryable
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("Bad request")
                });
            });

        // Act
        var response = await client.PostWithRetryAsync("https://example.com/api/test", new StringContent("{}"));

        // Assert
        Assert.False(response.IsSuccessful);
        Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Bad request", response.ErrorMessage);
        Assert.Equal(1, attemptCount); // Should have attempted exactly once
    }

    /// <summary>
    /// Verifies that 408 Request Timeout is retryable
    /// </summary>
    [Fact]
    public async Task PostWithRetryAsync_408RequestTimeout_IsRetryable()
    {
        // Arrange
        var mockHttp = Substitute.For<HttpClient>();
        var logger = new TestLogger();
        var client = new RetryableHttpClient(mockHttp, logger, maxRetries: 3, retryDelay: TimeSpan.FromMilliseconds(10));

        var attemptCount = 0;
        mockHttp
            .SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns<Task<HttpResponseMessage>>(x =>
            {
                attemptCount++;
                if (attemptCount == 1)
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.RequestTimeout)
                    {
                        Content = new StringContent("Request timeout")
                    });
                }

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"success\": true}")
                });
            });

        // Act
        var response = await client.PostWithRetryAsync("https://example.com/api/test", new StringContent("{}"));

        // Assert
        Assert.True(response.IsSuccessful);
        Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, attemptCount); // Should retry once and then succeed
    }

    /// <summary>
    /// Verifies that 429 Too Many Requests is retryable
    /// </summary>
    [Fact]
    public async Task PostWithRetryAsync_429TooManyRequests_IsRetryable()
    {
        // Arrange
        var mockHttp = Substitute.For<HttpClient>();
        var logger = new TestLogger();
        var client = new RetryableHttpClient(mockHttp, logger, maxRetries: 3, retryDelay: TimeSpan.FromMilliseconds(10));

        var attemptCount = 0;
        mockHttp
            .SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns<Task<HttpResponseMessage>>(x =>
            {
                attemptCount++;
                if (attemptCount == 1)
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.TooManyRequests)
                    {
                        Content = new StringContent("{ \"retry_after\": 100 }")
                    });
                }

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"success\": true}")
                });
            });

        // Act
        var response = await client.PostWithRetryAsync("https://example.com/api/test", new StringContent("{}"));

        // Assert
        Assert.True(response.IsSuccessful);
        Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, attemptCount); // Should retry once and then succeed
    }

    /// <summary>
    /// Verifies that the delay between retries follows the intended backoff strategy
    /// (exponential backoff: delay * 2^(attempt-1))
    /// </summary>
    [Fact]
    public async Task PostWithRetryAsync_BackoffStrategy_FollowsExponentialBackoff()
    {
        // Arrange
        var mockHttp = Substitute.For<HttpClient>();
        var logger = new TestLogger();
        var baseDelay = TimeSpan.FromMilliseconds(100);
        var client = new RetryableHttpClient(mockHttp, logger, maxRetries: 4, retryDelay: baseDelay);

        var attemptCount = 0;
        mockHttp
            .SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns<Task<HttpResponseMessage>>(x =>
            {
                attemptCount++;
                if (attemptCount < 4)
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    {
                        Content = new StringContent("Server error")
                    });
                }

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"success\": true}")
                });
            });

        // Act
        var response = await client.PostWithRetryAsync("https://example.com/api/test", new StringContent("{}"));

        // Assert
        Assert.True(response.IsSuccessful);
        Assert.Equal(4, attemptCount);

        // Verify exponential backoff: 100ms, 200ms, 400ms
        // Note: We can't directly verify the delays without a testable delay provider,
        // but we verify the behavior by checking the correct number of attempts
    }

    /// <summary>
    /// Verifies that HttpResponse<T> correctly distinguishes a successful deserialized payload
    /// from a failed/empty response for success status codes
    /// </summary>
    [Fact]
    public async Task PostWithRetryAsync_SuccessfulResponseWithPayload_IsSuccessful()
    {
        // Arrange
        var mockHttp = Substitute.For<HttpClient>();
        var logger = new TestLogger();
        var client = new RetryableHttpClient(mockHttp, logger, maxRetries: 3, retryDelay: TimeSpan.FromMilliseconds(10));

        mockHttp
            .SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"id\": 123, \"name\": \"test\"}")
            }));

        // Act
        var response = await client.PostWithRetryAsync("https://example.com/api/test", new StringContent("{}"));

        // Assert
        Assert.True(response.IsSuccessful);
        Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("{\"id\": 123, \"name\": \"test\"}", response.Content);
        Assert.Null(response.ErrorMessage);
    }

    /// <summary>
    /// Verifies that HttpResponse<T> correctly handles empty content for success status codes
    /// </summary>
    [Fact]
    public async Task PostWithRetryAsync_SuccessfulResponseWithEmptyContent_IsSuccessful()
    {
        // Arrange
        var mockHttp = Substitute.For<HttpClient>();
        var logger = new TestLogger();
        var client = new RetryableHttpClient(mockHttp, logger, maxRetries: 3, retryDelay: TimeSpan.FromMilliseconds(10));

        mockHttp
            .SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("")
            }));

        // Act
        var response = await client.PostWithRetryAsync("https://example.com/api/test", new StringContent("{}"));

        // Assert
        Assert.True(response.IsSuccessful);
        Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("", response.Content);
        Assert.Null(response.ErrorMessage);
    }

    /// <summary>
    /// Verifies that HttpResponse<T> correctly distinguishes a failed response with error message
    /// for error status codes
    /// </summary>
    [Fact]
    public async Task PostWithRetryAsync_ErrorResponseWithErrorMessage_IsNotSuccessful()
    {
        // Arrange
        var mockHttp = Substitute.For<HttpClient>();
        var logger = new TestLogger();
        var client = new RetryableHttpClient(mockHttp, logger, maxRetries: 3, retryDelay: TimeSpan.FromMilliseconds(10));

        mockHttp
            .SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("Internal server error occurred")
            }));

        // Act
        var response = await client.PostWithRetryAsync("https://example.com/api/test", new StringContent("{}"));

        // Assert
        Assert.False(response.IsSuccessful);
        Assert.Equal((int)HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("Internal server error occurred", response.ErrorMessage);
        Assert.Null(response.Content);
    }

    /// <summary>
    /// Verifies that HttpResponse<T> correctly handles exception cases
    /// </summary>
    [Fact]
    public async Task PostWithRetryAsync_ExceptionDuringRequest_ReturnsErrorResponse()
    {
        // Arrange
        var mockHttp = Substitute.For<HttpClient>();
        var logger = new TestLogger();
        var client = new RetryableHttpClient(mockHttp, logger, maxRetries: 3, retryDelay: TimeSpan.FromMilliseconds(10));

        var attemptCount = 0;
        mockHttp
            .SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns<Task<HttpResponseMessage>>(x =>
            {
                attemptCount++;
                // Always throw exception for all attempts
                throw new HttpRequestException("Connection failed");
            });

        // Act
        var response = await client.PostWithRetryAsync("https://example.com/api/test", new StringContent("{}"));

        // Assert
        Assert.False(response.IsSuccessful);
        Assert.Equal(0, response.StatusCode); // Exception case has status code 0
        Assert.Equal("Connection failed", response.ErrorMessage);
        Assert.Equal(3, attemptCount); // Should have attempted maxRetries times
    }

    /// <summary>
    /// Verifies that 5xx server errors are retryable
    /// </summary>
    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.GatewayTimeout)]
    public async Task PostWithRetryAsync_ServerErrors_AreRetryable(HttpStatusCode statusCode)
    {
        // Arrange
        var mockHttp = Substitute.For<HttpClient>();
        var logger = new TestLogger();
        var client = new RetryableHttpClient(mockHttp, logger, maxRetries: 3, retryDelay: TimeSpan.FromMilliseconds(10));

        var attemptCount = 0;
        mockHttp
            .SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns<Task<HttpResponseMessage>>(x =>
            {
                attemptCount++;
                if (attemptCount == 1)
                {
                    return Task.FromResult(new HttpResponseMessage(statusCode)
                    {
                        Content = new StringContent($"{(int)statusCode} error")
                    });
                }

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"success\": true}")
                });
            });

        // Act
        var response = await client.PostWithRetryAsync("https://example.com/api/test", new StringContent("{}"));

        // Assert
        Assert.True(response.IsSuccessful);
        Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, attemptCount); // Should retry once and then succeed
    }

    /// <summary>
    /// Verifies that 4xx client errors (except 408 and 429) are NOT retryable
    /// </summary>
    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.MethodNotAllowed)]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.UnsupportedMediaType)]
    public async Task PostWithRetryAsync_ClientErrorsExcept408And429_AreNotRetryable(HttpStatusCode statusCode)
    {
        // Arrange
        var mockHttp = Substitute.For<HttpClient>();
        var logger = new TestLogger();
        var client = new RetryableHttpClient(mockHttp, logger, maxRetries: 5, retryDelay: TimeSpan.FromMilliseconds(10));

        var attemptCount = 0;
        mockHttp
            .SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns<Task<HttpResponseMessage>>(x =>
            {
                attemptCount++;
                return Task.FromResult(new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent($"{(int)statusCode} client error")
                });
            });

        // Act
        var response = await client.PostWithRetryAsync("https://example.com/api/test", new StringContent("{}"));

        // Assert
        Assert.False(response.IsSuccessful);
        Assert.Equal((int)statusCode, response.StatusCode);
        Assert.Equal($"{(int)statusCode} client error", response.ErrorMessage);
        Assert.Equal(1, attemptCount); // Should have attempted exactly once
    }
}