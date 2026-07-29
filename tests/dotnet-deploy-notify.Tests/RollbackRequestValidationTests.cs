// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Tests for the RollbackRequestValidation class.
/// </summary>
public class RollbackRequestValidationTests
{
    private static RollbackRequest CreateValidRequest() => new()
    {
        Id = Guid.NewGuid().ToString(),
        ProjectName = "TestProject",
        TargetVersion = "1.0.1",
        CurrentVersion = "1.0.0",
        TargetEnvironment = DotNetDeployNotify.Core.Environment.Production,
        RequestedBy = "Admin",
        Reason = "Fixing bug",
        Channels = new List<NotificationChannel> { NotificationChannel.Slack },
        Priority = NotificationPriority.High,
        CreatedAt = DateTime.UtcNow,
        Metadata = new Dictionary<string, object> { { "Key", "Value" } }
    };

    [Fact]
    public void Validate_ValidRequest_ReturnsEmptyList()
    {
        var request = CreateValidRequest();
        var errors = request.Validate();
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_InvalidRequest_ReturnsErrors()
    {
        var request = CreateValidRequest();
        request.ProjectName = ""; // Invalid
        request.Id = "invalid-guid"; // Invalid

        var errors = request.Validate();
        errors.Should().NotBeEmpty();
        errors.Should().Contain(e => e.Contains("ProjectName"));
        errors.Should().Contain(e => e.Contains("Id"));
    }

    [Fact]
    public void IsValid_ValidRequest_ReturnsTrue()
    {
        var request = CreateValidRequest();
        request.IsValid().Should().BeTrue();
    }

    [Fact]
    public void EnsureValid_ValidRequest_DoesNotThrow()
    {
        var request = CreateValidRequest();
        var act = () => request.EnsureValid();
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureValid_InvalidRequest_ThrowsArgumentException()
    {
        var request = CreateValidRequest();
        request.ProjectName = null!; // Invalid

        var act = () => request.EnsureValid();
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Validate_NullRequest_ThrowsArgumentNullException()
    {
        RollbackRequest? request = null;
        var act = () => request.Validate();
        act.Should().Throw<ArgumentNullException>();
    }
}
