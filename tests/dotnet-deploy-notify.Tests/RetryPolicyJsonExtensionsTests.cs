#nullable enable

using System;
using System.Text.Json;
using DotNetDeployNotify.Utilities;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class RetryPolicyJsonExtensionsTests
{
    [Fact]
    public void ToJson_ValidRetryPolicy_ReturnsJsonString()
    {
        var retryPolicy = new RetryPolicy();
        var json = RetryPolicyJsonExtensions.ToJson(retryPolicy);
        json.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ToJson_NullRetryPolicy_ThrowsArgumentNullException()
    {
        RetryPolicy? retryPolicy = null;
        Action act = () => RetryPolicyJsonExtensions.ToJson(retryPolicy!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsRetryPolicy()
    {
        var json = "{\"MaxAttempts\":3,\"Delay\":1000,\"BackoffFactor\":2}";
        var retryPolicy = RetryPolicyJsonExtensions.FromJson(json);
        retryPolicy.Should().NotBeNull();
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsJsonException()
    {
        var json = "{ invalid json }";
        Action act = () => RetryPolicyJsonExtensions.FromJson(json);
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrue()
    {
        var json = "{\"MaxAttempts\":3,\"Delay\":1000,\"BackoffFactor\":2}";
        var result = RetryPolicyJsonExtensions.TryFromJson(json, out var retryPolicy);
        result.Should().BeTrue();
        retryPolicy.Should().NotBeNull();
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalse()
    {
        var json = "{ invalid json }";
        var result = RetryPolicyJsonExtensions.TryFromJson(json, out var retryPolicy);
        result.Should().BeFalse();
        retryPolicy.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_NullJson_ThrowsArgumentNullException()
    {
        string? json = null;
        Action act = () => RetryPolicyJsonExtensions.TryFromJson(json!, out _);
        act.Should().Throw<ArgumentNullException>();
    }
}
