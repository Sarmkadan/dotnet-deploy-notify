#nullable enable
using DotNetDeployNotify.Core;
using Environment = DotNetDeployNotify.Core.Environment;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class DeploymentHistoryServiceTests
{
    private readonly DeploymentHistoryService _service;

    public DeploymentHistoryServiceTests()
    {
        var logger = Substitute.For<ILogger<DeploymentHistoryService>>();
        _service = new DeploymentHistoryService(logger);
    }

    // ─── RecordDeploymentAsync ──────────────────────────────────────────────

    [Fact]
    public async Task RecordDeploymentAsync_WithValidEntry_StoresEntry()
    {
        var entry = CreateEntry("MyApp", "1.0.0", BuildStatus.Success);

        await _service.RecordDeploymentAsync(entry);

        var history = await _service.GetProjectHistoryAsync("MyApp");
        history.Should().ContainSingle(e => e.Id == entry.Id);
    }

    [Fact]
    public async Task RecordDeploymentAsync_WithNullEntry_ThrowsArgumentNullException()
    {
        Func<Task> act = () => _service.RecordDeploymentAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task RecordDeploymentAsync_WithEmptyProjectName_ThrowsArgumentException()
    {
        var entry = CreateEntry(string.Empty, "1.0.0", BuildStatus.Success);
        Func<Task> act = () => _service.RecordDeploymentAsync(entry);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ─── RecordFromNotificationAsync ───────────────────────────────────────

    [Fact]
    public async Task RecordFromNotificationAsync_WithValidNotification_CreatesEntry()
    {
        var notification = new DeploymentNotification
        {
            ProjectName = "ApiService",
            Version = "2.0.0",
            Status = BuildStatus.DeploymentSuccess,
            BranchName = "main",
            CommitHash = "abc1234",
            CommitAuthor = "dev",
            Channels = [NotificationChannel.Slack]
        };

        await _service.RecordFromNotificationAsync(notification);

        var history = await _service.GetProjectHistoryAsync("ApiService");
        history.Should().ContainSingle();
        history[0].Version.Should().Be("2.0.0");
        history[0].FinalStatus.Should().Be(BuildStatus.DeploymentSuccess);
    }

    // ─── GetProjectHistoryAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetProjectHistoryAsync_ReturnsNewestFirst()
    {
        var older = CreateEntry("Svc", "1.0.0", BuildStatus.Success, DateTime.UtcNow.AddHours(-2));
        var newer = CreateEntry("Svc", "2.0.0", BuildStatus.Success, DateTime.UtcNow.AddHours(-1));

        await _service.RecordDeploymentAsync(older);
        await _service.RecordDeploymentAsync(newer);

        var history = await _service.GetProjectHistoryAsync("Svc");
        history[0].Version.Should().Be("2.0.0");
        history[1].Version.Should().Be("1.0.0");
    }

    [Fact]
    public async Task GetProjectHistoryAsync_RespectsLimit()
    {
        for (var i = 0; i < 10; i++)
            await _service.RecordDeploymentAsync(CreateEntry("App", $"1.{i}", BuildStatus.Success));

        var history = await _service.GetProjectHistoryAsync("App", 3);
        history.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetProjectHistoryAsync_IsCaseInsensitive()
    {
        await _service.RecordDeploymentAsync(CreateEntry("MyApp", "1.0.0", BuildStatus.Success));

        var history = await _service.GetProjectHistoryAsync("MYAPP");
        history.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetProjectHistoryAsync_ReturnsEmptyForUnknownProject()
    {
        var history = await _service.GetProjectHistoryAsync("NonExistent");
        history.Should().BeEmpty();
    }

    // ─── GetRecentDeploymentsAsync ──────────────────────────────────────────

    [Fact]
    public async Task GetRecentDeploymentsAsync_ReturnsAcrossProjects()
    {
        await _service.RecordDeploymentAsync(CreateEntry("Svc1", "1.0", BuildStatus.Success));
        await _service.RecordDeploymentAsync(CreateEntry("Svc2", "1.0", BuildStatus.Failed));

        var recent = await _service.GetRecentDeploymentsAsync(10);
        recent.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRecentDeploymentsAsync_RespectsLimit()
    {
        for (var i = 0; i < 30; i++)
            await _service.RecordDeploymentAsync(CreateEntry($"P{i}", "1.0", BuildStatus.Success));

        var recent = await _service.GetRecentDeploymentsAsync(5);
        recent.Should().HaveCount(5);
    }

    // ─── GetStatisticsAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetStatisticsAsync_CalculatesSuccessRate()
    {
        await _service.RecordDeploymentAsync(CreateEntry("App", "1.0", BuildStatus.Success));
        await _service.RecordDeploymentAsync(CreateEntry("App", "1.1", BuildStatus.Success));
        await _service.RecordDeploymentAsync(CreateEntry("App", "1.2", BuildStatus.Failed));

        var stats = await _service.GetStatisticsAsync("App");

        stats.TotalDeployments.Should().Be(3);
        stats.SuccessfulDeployments.Should().Be(2);
        stats.FailedDeployments.Should().Be(1);
        stats.SuccessRate.Should().BeApproximately(66.67, 0.1);
    }

    [Fact]
    public async Task GetStatisticsAsync_CountsRollbacks()
    {
        var rollback = CreateEntry("App", "1.0", BuildStatus.DeploymentSuccess);
        rollback.IsRollback = true;
        await _service.RecordDeploymentAsync(rollback);
        await _service.RecordDeploymentAsync(CreateEntry("App", "2.0", BuildStatus.Success));

        var stats = await _service.GetStatisticsAsync("App");
        stats.RollbackCount.Should().Be(1);
    }

    [Fact]
    public async Task GetStatisticsAsync_CalculatesAverageDuration()
    {
        var e1 = CreateEntry("App", "1.0", BuildStatus.Success);
        e1.DurationSeconds = 60;
        var e2 = CreateEntry("App", "1.1", BuildStatus.Success);
        e2.DurationSeconds = 120;

        await _service.RecordDeploymentAsync(e1);
        await _service.RecordDeploymentAsync(e2);

        var stats = await _service.GetStatisticsAsync("App");
        stats.AverageDurationSeconds.Should().BeApproximately(90, 0.1);
    }

    [Fact]
    public async Task GetStatisticsAsync_ReturnsZeroRateWhenNoDeployments()
    {
        var stats = await _service.GetStatisticsAsync("EmptyProject");
        stats.SuccessRate.Should().Be(0);
        stats.TotalDeployments.Should().Be(0);
    }

    // ─── GetByEnvironmentAsync ──────────────────────────────────────────────

    [Fact]
    public async Task GetByEnvironmentAsync_FiltersByEnvironment()
    {
        var prod = CreateEntry("App", "1.0", BuildStatus.Success);
        prod.TargetEnvironment = Environment.Production;
        var stag = CreateEntry("App", "0.9", BuildStatus.Success);
        stag.TargetEnvironment = Environment.Staging;

        await _service.RecordDeploymentAsync(prod);
        await _service.RecordDeploymentAsync(stag);

        var results = await _service.GetByEnvironmentAsync(Environment.Production);
        results.Should().ContainSingle();
        results[0].TargetEnvironment.Should().Be(Environment.Production);
    }

    // ─── GetLastSuccessfulDeploymentAsync ───────────────────────────────────

    [Fact]
    public async Task GetLastSuccessfulDeploymentAsync_ReturnsLatestSuccess()
    {
        var e1 = CreateEntry("App", "1.0", BuildStatus.DeploymentSuccess, DateTime.UtcNow.AddHours(-3));
        e1.TargetEnvironment = Environment.Production;
        var e2 = CreateEntry("App", "2.0", BuildStatus.DeploymentSuccess, DateTime.UtcNow.AddHours(-1));
        e2.TargetEnvironment = Environment.Production;

        await _service.RecordDeploymentAsync(e1);
        await _service.RecordDeploymentAsync(e2);

        var last = await _service.GetLastSuccessfulDeploymentAsync("App", Environment.Production);
        last.Should().NotBeNull();
        last!.Version.Should().Be("2.0");
    }

    [Fact]
    public async Task GetLastSuccessfulDeploymentAsync_ReturnsNullWhenNoneFound()
    {
        var last = await _service.GetLastSuccessfulDeploymentAsync("Ghost", Environment.Production);
        last.Should().BeNull();
    }

    // ─── GetRollbackEntriesAsync ────────────────────────────────────────────

    [Fact]
    public async Task GetRollbackEntriesAsync_ReturnsOnlyRollbacks()
    {
        var normal   = CreateEntry("App", "2.0", BuildStatus.Success);
        var rollback = CreateEntry("App", "1.9", BuildStatus.DeploymentSuccess);
        rollback.IsRollback = true;

        await _service.RecordDeploymentAsync(normal);
        await _service.RecordDeploymentAsync(rollback);

        var rollbacks = await _service.GetRollbackEntriesAsync("App");
        rollbacks.Should().ContainSingle();
        rollbacks[0].IsRollback.Should().BeTrue();
    }

    // ─── DeploymentHistoryEntry.IsSuccessful ────────────────────────────────

    [Theory]
    [InlineData(BuildStatus.Success,           true)]
    [InlineData(BuildStatus.DeploymentSuccess, true)]
    [InlineData(BuildStatus.Failed,            false)]
    [InlineData(BuildStatus.DeploymentFailed,  false)]
    [InlineData(BuildStatus.Cancelled,         false)]
    public void IsSuccessful_ReflectsStatus(BuildStatus status, bool expected)
    {
        var entry = CreateEntry("App", "1.0", status);
        entry.IsSuccessful.Should().Be(expected);
    }

    // ─── FromNotification ───────────────────────────────────────────────────

    [Fact]
    public void FromNotification_MapsAllFields()
    {
        var notification = new DeploymentNotification
        {
            ProjectName = "Proj",
            Version = "3.0.0",
            Status = BuildStatus.Success,
            TargetEnvironment = Environment.Production,
            BranchName = "release",
            CommitHash = "deadbeef",
            CommitAuthor = "alice",
            DurationSeconds = 90,
            Channels = [NotificationChannel.Slack]
        };

        var entry = DeploymentHistoryEntry.FromNotification(notification);

        entry.ProjectName.Should().Be("Proj");
        entry.Version.Should().Be("3.0.0");
        entry.FinalStatus.Should().Be(BuildStatus.Success);
        entry.TargetEnvironment.Should().Be(Environment.Production);
        entry.BranchName.Should().Be("release");
        entry.CommitHash.Should().Be("deadbeef");
        entry.CommitAuthor.Should().Be("alice");
        entry.DurationSeconds.Should().Be(90);
        entry.IsRollback.Should().BeFalse();
    }

    // ─── Helpers ────────────────────────────────────────────────────────────

    private static DeploymentHistoryEntry CreateEntry(
        string projectName,
        string version,
        BuildStatus status,
        DateTime? deployedAt = null)
    {
        return new DeploymentHistoryEntry
        {
            ProjectName = projectName,
            Version = version,
            FinalStatus = status,
            TargetEnvironment = Environment.Production,
            BranchName = "main",
            CommitHash = "abc1234",
            CommitAuthor = "tester",
            DeployedAt = deployedAt ?? DateTime.UtcNow
        };
    }
}
