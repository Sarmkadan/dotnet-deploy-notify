using System;
using System.Collections.Generic;
using DotNetDeployNotify.Core.Models;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class WebhookPayloadValidationTests
{
    private static WebhookPayload CreateValidPayload()
    {
        return new WebhookPayload
        {
            EventId = Guid.NewGuid().ToString(),
            EventType = "deployment",
            Timestamp = DateTime.UtcNow,
            Source = "ci",
            SchemaVersion = "1.0.0",
            Data = new WebhookData
            {
                ProjectName = "TestProject",
                Version = "1.0.0",
                Status = "success",
                Environment = "production",
                Branch = "main",
                CommitHash = "abcdef1",
                CommitAuthor = "John Doe",
                RepositoryUrl = "https://github.com/example/repo.git",
                BuildUrl = "https://ci.example.com/build/123",
                DurationSeconds = 120
            }
        };
    }

    [Fact]
    public void Validate_HappyPath_ReturnsEmptyList()
    {
        var payload = CreateValidPayload();

        var errors = payload.Validate();

        Assert.Empty(errors);
        Assert.True(payload.IsValid());
        var ex = Record.Exception(() => payload.EnsureValid());
        Assert.Null(ex);
    }

    [Fact]
    public void Validate_NullPayload_ThrowsArgumentNullException()
    {
        WebhookPayload? payload = null;

        Assert.Throws<ArgumentNullException>(() => payload!.Validate());
        Assert.Throws<ArgumentNullException>(() => payload!.IsValid());
        Assert.Throws<ArgumentNullException>(() => payload!.EnsureValid());
    }

    [Fact]
    public void Validate_InvalidEventId_ReturnsError()
    {
        var payload = CreateValidPayload();
        payload.EventId = "not-a-guid";

        var errors = payload.Validate();

        Assert.Contains("EventId must be a valid GUID format", errors);
        Assert.False(payload.IsValid());
        var ex = Assert.Throws<ArgumentException>(() => payload.EnsureValid());
        Assert.Contains("EventId must be a valid GUID format", ex.Message);
    }

    [Fact]
    public void Validate_TimestampFuture_ReturnsError()
    {
        var payload = CreateValidPayload();
        payload.Timestamp = DateTime.UtcNow.AddMinutes(10); // beyond 5 minute window

        var errors = payload.Validate();

        Assert.Contains("Timestamp cannot be in the future", errors);
        Assert.False(payload.IsValid());
        Assert.Throws<ArgumentException>(() => payload.EnsureValid());
    }

    [Fact]
    public void Validate_DataNull_ReturnsError()
    {
        var payload = CreateValidPayload();
        payload.Data = null!;

        var errors = payload.Validate();

        Assert.Contains("Data must not be null", errors);
        Assert.False(payload.IsValid());
        Assert.Throws<ArgumentException>(() => payload.EnsureValid());
    }

    [Fact]
    public void Validate_DataInvalid_ReturnsError()
    {
        var payload = CreateValidPayload();
        payload.Data!.ProjectName = ""; // invalid

        var errors = payload.Validate();

        Assert.Contains("ProjectName must not be null or whitespace", errors);
        Assert.False(payload.IsValid());
        Assert.Throws<ArgumentException>(() => payload.EnsureValid());
    }

    [Fact]
    public void Validate_BoundaryTimestamp_Valid()
    {
        var payload = CreateValidPayload();
        // Exactly 5 minutes ahead is considered invalid
        payload.Timestamp = DateTime.UtcNow.AddMinutes(5);
        var errorsFuture = payload.Validate();
        Assert.Contains("Timestamp cannot be in the future", errorsFuture);

        // Exactly 1 year ago is valid
        payload.Timestamp = DateTime.UtcNow.AddYears(-1);
        var errorsPast = payload.Validate();
        Assert.DoesNotContain("Timestamp cannot be more than one year in the past", errorsPast);
        Assert.Empty(errorsPast);
    }

    [Fact]
    public void Validate_SchemaVersionInvalid_ReturnsError()
    {
        var payload = CreateValidPayload();
        payload.SchemaVersion = "invalid";

        var errors = payload.Validate();

        Assert.Contains("SchemaVersion must be a valid semantic version", errors);
        Assert.False(payload.IsValid());
        Assert.Throws<ArgumentException>(() => payload.EnsureValid());
    }
}
