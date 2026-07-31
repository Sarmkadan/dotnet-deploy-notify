#nullable enable

using System;
using System.Threading.Tasks;
using DotNetDeployNotify.Utilities;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class RetryPolicyTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsResult_OnFirstAttempt()
    {
        var helper = new RetryHelper(NullLogger<RetryHelper>.Instance);
        var result = await helper.ExecuteAsync(() => Task.FromResult(42));

        result.Should().Be(42);
    }

    [Fact]
    public async Task ExecuteAsync_RetriesAndSucceeds_WhenShouldRetryIsTrue()
    {
        var attempts = 0;
        var policy = new RetryPolicy
        {
            MaxAttempts = 3,
            ShouldRetry = ex => ex is InvalidOperationException
        };

        var helper = new RetryHelper(NullLogger<RetryHelper>.Instance);
        var result = await helper.ExecuteAsync<int>(async () =>
        {
            attempts++;
            if (attempts < 2)
                throw new InvalidOperationException("transient");
            return 99;
        }, policy);

        result.Should().Be(99);
        attempts.Should().Be(2);
    }

    [Fact]
    public async Task ExecuteAsync_StopsRetryAndThrows_WhenShouldRetryIsFalse()
    {
        var attempts = 0;
        var policy = new RetryPolicy
        {
            MaxAttempts = 5,
            ShouldRetry = ex => false // never retry
        };

        var helper = new RetryHelper(NullLogger<RetryHelper>.Instance);
        Func<Task> act = async () => await helper.ExecuteAsync<int>(async () =>
        {
            attempts++;
            throw new InvalidOperationException("non‑retryable");
        }, policy);

        await act.Should().ThrowAsync<InvalidOperationException>();
        attempts.Should().Be(1); // only first attempt
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsAfterMaxAttempts_WhenAllAttemptsFail()
    {
        var attempts = 0;
        var policy = new RetryPolicy { MaxAttempts = 3 };
        var helper = new RetryHelper(NullLogger<RetryHelper>.Instance);

        Func<Task> act = async () => await helper.ExecuteAsync<int>(async () =>
        {
            attempts++;
            throw new InvalidOperationException("always fail");
        }, policy);

        await act.Should().ThrowAsync<InvalidOperationException>();
        attempts.Should().Be(3);
    }

    [Fact]
    public void Execute_RetriesSyncAndSucceeds()
    {
        var attempts = 0;
        var policy = new RetryPolicy { MaxAttempts = 4 };
        var helper = new RetryHelper(NullLogger<RetryHelper>.Instance);

        var result = helper.Execute(() =>
        {
            attempts++;
            if (attempts < 3)
                throw new InvalidOperationException("transient");
            return "ok";
        }, policy);

        result.Should().Be("ok");
        attempts.Should().Be(3);
    }

    [Fact]
    public void ExponentialBackoff_GetDelay_ReturnsExpectedValues()
    {
        var backoff = new ExponentialBackoff(maxRetries: 5, initialDelay: TimeSpan.FromMilliseconds(100), multiplier: 2.0);

        backoff.GetDelay(0).Should().Be(TimeSpan.Zero);
        backoff.GetDelay(1).Should().Be(TimeSpan.FromMilliseconds(100));
        backoff.GetDelay(2).Should().Be(TimeSpan.FromMilliseconds(200));
        backoff.GetDelay(3).Should().Be(TimeSpan.FromMilliseconds(400));
        backoff.GetDelay(4).Should().Be(TimeSpan.FromMilliseconds(800));
    }

    [Fact]
    public void RetryPolicy_DefaultValues_AreSetCorrectly()
    {
        var policy = new RetryPolicy();

        policy.MaxAttempts.Should().Be(3);
        policy.InitialDelay.Should().Be(TimeSpan.FromMilliseconds(100));
        policy.BackoffMultiplier.Should().Be(2.0);
        policy.MaxDelay.Should().Be(TimeSpan.FromSeconds(30));
        policy.ShouldRetry.Should().BeNull();
    }
}
