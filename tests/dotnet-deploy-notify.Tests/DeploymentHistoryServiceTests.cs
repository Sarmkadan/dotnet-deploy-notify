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

/// <summary>
/// Contains unit tests for <see cref="DeploymentHistoryService"/> which manages deployment history tracking and statistics.
/// </summary>
public class DeploymentHistoryServiceTests
{
	private readonly DeploymentHistoryService _service;

	/// <summary>
	/// Initializes a new instance of the <see cref="DeploymentHistoryServiceTests"/> class.
	/// </summary>
	public DeploymentHistoryServiceTests()
	{
		var logger = Substitute.For<ILogger<DeploymentHistoryService>>();
		_service = new DeploymentHistoryService(logger);
	}

	// ─── RecordDeploymentAsync ──────────────────────────────────────────────

	/// <summary>
	/// Tests that <see cref="DeploymentHistoryService.RecordDeploymentAsync(DeploymentHistoryEntry)"/> correctly stores a valid deployment entry.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	[Fact]
	public async Task RecordDeploymentAsync_WithValidEntry_StoresEntry()
	{
		var entry = CreateEntry("MyApp", "1.0.0", BuildStatus.Success);

		await _service.RecordDeploymentAsync(entry);

		var history = await _service.GetProjectHistoryAsync("MyApp");
		history.Should().ContainSingle(e => e.Id == entry.Id);
	}

	/// <summary>
	/// Tests that <see cref="DeploymentHistoryService.RecordDeploymentAsync(DeploymentHistoryEntry)"/> throws an <see cref="ArgumentNullException"/> when passed a null entry.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	[Fact]
	public async Task RecordDeploymentAsync_WithNullEntry_ThrowsArgumentNullException()
	{
		Func<Task> act = () => _service.RecordDeploymentAsync(null!);
		await act.Should().ThrowAsync<ArgumentNullException>();
	}

	/// <summary>
	/// Tests that <see cref="DeploymentHistoryService.RecordDeploymentAsync(DeploymentHistoryEntry)"/> throws an <see cref="ArgumentException"/> when passed an entry with an empty project name.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	[Fact]
	public async Task RecordDeploymentAsync_WithEmptyProjectName_ThrowsArgumentException()
	{
		var entry = CreateEntry(string.Empty, "1.0.0", BuildStatus.Success);
		Func<Task> act = () => _service.RecordDeploymentAsync(entry);
		await act.Should().ThrowAsync<ArgumentException>();
	}

	// ─── RecordFromNotificationAsync ───────────────────────────────────────────

	/// <summary>
	/// Tests that <see cref="DeploymentHistoryService.RecordFromNotificationAsync(DeploymentNotification)"/> creates a deployment entry from a valid notification.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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

	/// <summary>
	/// Tests that <see cref="DeploymentHistoryService.GetProjectHistoryAsync(string)"/> returns deployments in newest-first order.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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

	/// <summary>
	/// Tests that <see cref="DeploymentHistoryService.GetProjectHistoryAsync(string, int)"/> respects the specified limit parameter.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	[Fact]
	public async Task GetProjectHistoryAsync_RespectsLimit()
	{
		for (var i = 0; i < 10; i++)
			await _service.RecordDeploymentAsync(CreateEntry("App", $"1.{i}", BuildStatus.Success));

		var history = await _service.GetProjectHistoryAsync("App", 3);
		history.Should().HaveCount(3);
	}

	/// <summary>
	/// Tests that <see cref="DeploymentHistoryService.GetProjectHistoryAsync(string)"/> performs case-insensitive project name matching.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	[Fact]
	public async Task GetProjectHistoryAsync_IsCaseInsensitive()
	{
		await _service.RecordDeploymentAsync(CreateEntry("MyApp", "1.0.0", BuildStatus.Success));

		var history = await _service.GetProjectHistoryAsync("MYAPP");
		history.Should().HaveCount(1);
	}

	/// <summary>
	/// Tests that <see cref="DeploymentHistoryService.GetProjectHistoryAsync(string)"/> returns an empty collection for non-existent projects.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	[Fact]
	public async Task GetProjectHistoryAsync_ReturnsEmptyForUnknownProject()
	{
		var history = await _service.GetProjectHistoryAsync("NonExistent");
		history.Should().BeEmpty();
	}

	// ─── GetRecentDeploymentsAsync ──────────────────────────────────────────

	/// <summary>
	/// Tests that <see cref="DeploymentHistoryService.GetRecentDeploymentsAsync(int)"/> returns deployments across all projects.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	[Fact]
	public async Task GetRecentDeploymentsAsync_ReturnsAcrossProjects()
	{
		await _service.RecordDeploymentAsync(CreateEntry("Svc1", "1.0", BuildStatus.Success));
		await _service.RecordDeploymentAsync(CreateEntry("Svc2", "1.0", BuildStatus.Failed));

		var recent = await _service.GetRecentDeploymentsAsync(10);
		recent.Should().HaveCount(2);
	}

	/// <summary>
	/// Tests that <see cref="DeploymentHistoryService.GetRecentDeploymentsAsync(int)"/> respects the specified limit parameter.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	[Fact]
	public async Task GetRecentDeploymentsAsync_RespectsLimit()
	{
		for (var i = 0; i < 30; i++)
			await _service.RecordDeploymentAsync(CreateEntry($"P{i}", "1.0", BuildStatus.Success));

		var recent = await _service.GetRecentDeploymentsAsync(5);
		recent.Should().HaveCount(5);
	}

	// ─── GetStatisticsAsync ─────────────────────────────────────────────────

	/// <summary>
	/// Tests that <see cref="DeploymentHistoryService.GetStatisticsAsync(string)"/> correctly calculates success rate from deployment statistics.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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

	/// <summary>
	/// Tests that <see cref="DeploymentHistoryService.GetStatisticsAsync(string)"/> correctly counts rollback deployments.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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

	/// <summary>
	/// Tests that <see cref="DeploymentHistoryService.GetStatisticsAsync(string)"/> correctly calculates average deployment duration.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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

	/// <summary>
	/// Tests that <see cref="DeploymentHistoryService.GetStatisticsAsync(string)"/> returns zero values when no deployments exist for the project.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	[Fact]
	public async Task GetStatisticsAsync_ReturnsZeroRateWhenNoDeployments()
	{
		var stats = await _service.GetStatisticsAsync("EmptyProject");
		stats.SuccessRate.Should().Be(0);
		stats.TotalDeployments.Should().Be(0);
	}

	// ─── GetByEnvironmentAsync ──────────────────────────────────────────────

	/// <summary>
	/// Tests that <see cref="DeploymentHistoryService.GetByEnvironmentAsync(Environment)"/> correctly filters deployments by environment.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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

	// ─── GetLastSuccessfulDeploymentAsync ─────────────────────────────────────

	/// <summary>
	/// Tests that <see cref="DeploymentHistoryService.GetLastSuccessfulDeploymentAsync(string, Environment)"/> returns the most recent successful deployment.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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

	/// <summary>
	/// Tests that <see cref="DeploymentHistoryService.GetLastSuccessfulDeploymentAsync(string, Environment)"/> returns null when no successful deployment exists.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	[Fact]
	public async Task GetLastSuccessfulDeploymentAsync_ReturnsNullWhenNoneFound()
	{
		var last = await _service.GetLastSuccessfulDeploymentAsync("Ghost", Environment.Production);
		last.Should().BeNull();
	}

	// ─── GetRollbackEntriesAsync ────────────────────────────────────────────

	/// <summary>
	/// Tests that <see cref="DeploymentHistoryService.GetRollbackEntriesAsync(string)"/> returns only rollback entries.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	[Fact]
	public async Task GetRollbackEntriesAsync_ReturnsOnlyRollbacks()
	{
		var normal = CreateEntry("App", "2.0", BuildStatus.Success);
		var rollback = CreateEntry("App", "1.9", BuildStatus.DeploymentSuccess);
		rollback.IsRollback = true;

		await _service.RecordDeploymentAsync(normal);
		await _service.RecordDeploymentAsync(rollback);

		var rollbacks = await _service.GetRollbackEntriesAsync("App");
		rollbacks.Should().ContainSingle();
		rollbacks[0].IsRollback.Should().BeTrue();
	}

	// ─── DeploymentHistoryEntry.IsSuccessful ────────────────────────────────

	/// <summary>
	/// Tests that <see cref="DeploymentHistoryEntry.IsSuccessful"/> property correctly reflects the deployment status.
	/// </summary>
	/// <param name="status">The <see cref="BuildStatus"/> to test.</param>
	/// <param name="expected">The expected boolean result.</param>
	[Theory]
	[InlineData(BuildStatus.Success, true)]
	[InlineData(BuildStatus.DeploymentSuccess, true)]
	[InlineData(BuildStatus.Failed, false)]
	[InlineData(BuildStatus.DeploymentFailed, false)]
	[InlineData(BuildStatus.Cancelled, false)]
	public void IsSuccessful_ReflectsStatus(BuildStatus status, bool expected)
	{
		var entry = CreateEntry("App", "1.0", status);
		entry.IsSuccessful.Should().Be(expected);
	}

	// ─── FromNotification ───────────────────────────────────────────────────

	/// <summary>
	/// Tests that <see cref="DeploymentHistoryEntry.FromNotification(DeploymentNotification)"/> correctly maps all fields from a notification to a deployment entry.
	/// </summary>
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

	/// <summary>
	/// Creates a test <see cref="DeploymentHistoryEntry"/> with the specified parameters.
	/// </summary>
	/// <param name="projectName">The name of the project.</param>
	/// <param name="version">The version number.</param>
	/// <param name="status">The <see cref="BuildStatus"/> of the deployment.</param>
	/// <param name="deployedAt">Optional deployment timestamp. Uses current UTC time if not specified.</param>
	/// <returns>A new <see cref="DeploymentHistoryEntry"/> instance.</returns>
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