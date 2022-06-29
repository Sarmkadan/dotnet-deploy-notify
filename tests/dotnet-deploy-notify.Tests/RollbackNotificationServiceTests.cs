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

public class RollbackNotificationServiceTests
{
    private readonly RollbackNotificationService _service;
    private readonly INotificationService _notificationService;

    public RollbackNotificationServiceTests()
    {
        _notificationService = Substitute.For<INotificationService>();
        _notificationService
            .CreateNotificationAsync(Arg.Any<DeploymentNotification>())
            .Returns(Task.FromResult("test-id"));
        _notificationService
            .SendNotificationAsync(Arg.Any<string>(), Arg.Any<List<NotificationChannel>?>())
            .Returns(Task.FromResult(new List<NotificationResult>
            {
                new() { Status = DeliveryStatus.Delivered }
            }));

        var logger = Substitute.For<ILogger<RollbackNotificationService>>();
        _service = new RollbackNotificationService(_notificationService, logger);
    }

    // ─── FormatRollbackMessage ──────────────────────────────────────────────

    [Fact]
    public void FormatRollbackMessage_Slack_ContainsMarkdown()
    {
        var request = CreateRequest();

        var msg = _service.FormatRollbackMessage(request, RollbackStatus.InProgress, NotificationChannel.Slack);

        msg.Should().Contain("*Rollback initiated*");
        msg.Should().Contain("MyApp");
        msg.Should().Contain("v1.5.0");
        msg.Should().Contain("v1.4.0");
    }

    [Fact]
    public void FormatRollbackMessage_Discord_ContainsBoldMarkdown()
    {
        var request = CreateRequest();

        var msg = _service.FormatRollbackMessage(request, RollbackStatus.Completed, NotificationChannel.Discord);

        msg.Should().Contain("**Rollback completed successfully**");
        msg.Should().Contain("MyApp");
    }

    [Fact]
    public void FormatRollbackMessage_Telegram_ContainsHtmlTags()
    {
        var request = CreateRequest();

        var msg = _service.FormatRollbackMessage(request, RollbackStatus.Failed, NotificationChannel.Telegram);

        msg.Should().Contain("<b>Rollback failed</b>");
        msg.Should().Contain("<code>");
    }

    [Fact]
    public void FormatRollbackMessage_Generic_ContainsProjectInfo()
    {
        var request = CreateRequest();

        var msg = _service.FormatRollbackMessage(request, RollbackStatus.Cancelled, NotificationChannel.Webhook);

        msg.Should().Contain("MyApp");
        msg.Should().Contain("cancelled");
        msg.Should().Contain("v1.5.0");
    }

    [Fact]
    public void FormatRollbackMessage_WithReason_IncludesReason()
    {
        var request = CreateRequest();
        request.Reason = "Critical production bug";

        var msg = _service.FormatRollbackMessage(request, RollbackStatus.InProgress, NotificationChannel.Slack);

        msg.Should().Contain("Critical production bug");
    }

    [Fact]
    public void FormatRollbackMessage_WithAdditionalDetails_IncludesDetails()
    {
        var request = CreateRequest();
        var msg = _service.FormatRollbackMessage(
            request, RollbackStatus.Failed, NotificationChannel.Slack, "Database migration failed");

        msg.Should().Contain("Database migration failed");
    }

    [Theory]
    [InlineData(RollbackStatus.InProgress, "🔄")]
    [InlineData(RollbackStatus.Completed,  "✅")]
    [InlineData(RollbackStatus.Failed,     "❌")]
    [InlineData(RollbackStatus.Cancelled,  "🚫")]
    public void FormatRollbackMessage_UsesCorrectEmoji(RollbackStatus status, string expectedEmoji)
    {
        var request = CreateRequest();
        var msg = _service.FormatRollbackMessage(request, status, NotificationChannel.Slack);
        msg.Should().Contain(expectedEmoji);
    }

    // ─── NotifyRollbackInitiatedAsync ───────────────────────────────────────

    [Fact]
    public async Task NotifyRollbackInitiatedAsync_CallsNotificationService()
    {
        var request = CreateRequest();

        await _service.NotifyRollbackInitiatedAsync(request);

        await _notificationService.Received(1).CreateNotificationAsync(Arg.Any<DeploymentNotification>());
        await _notificationService.Received(1).SendNotificationAsync(Arg.Any<string>(), Arg.Any<List<NotificationChannel>?>());
    }

    [Fact]
    public async Task NotifyRollbackInitiatedAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        Func<Task> act = () => _service.NotifyRollbackInitiatedAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task NotifyRollbackInitiatedAsync_ReturnsDeliveryResults()
    {
        var request = CreateRequest();
        var results = await _service.NotifyRollbackInitiatedAsync(request);
        results.Should().NotBeNull();
        results.Should().HaveCount(1);
    }

    // ─── NotifyRollbackCompletedAsync ───────────────────────────────────────

    [Fact]
    public async Task NotifyRollbackCompletedAsync_SendsCompletionNotification()
    {
        var request = CreateRequest();
        var result = new RollbackResult
        {
            ProjectName = "MyApp",
            RolledBackFromVersion = "1.5.0",
            RolledBackToVersion = "1.4.0",
            Status = RollbackStatus.Completed
        };

        var results = await _service.NotifyRollbackCompletedAsync(request, result);
        results.Should().NotBeNull();

        await _notificationService.Received(1).CreateNotificationAsync(
            Arg.Is<DeploymentNotification>(n => n.Status == BuildStatus.DeploymentSuccess));
    }

    // ─── NotifyRollbackFailedAsync ──────────────────────────────────────────

    [Fact]
    public async Task NotifyRollbackFailedAsync_SendsFailureNotification()
    {
        var request = CreateRequest();

        var results = await _service.NotifyRollbackFailedAsync(request, "Deployment script error");

        results.Should().NotBeNull();
        await _notificationService.Received(1).CreateNotificationAsync(
            Arg.Is<DeploymentNotification>(n => n.Status == BuildStatus.DeploymentFailed));
    }

    [Fact]
    public async Task NotifyRollbackFailedAsync_SetsCriticalPriority()
    {
        var request = CreateRequest();
        DeploymentNotification? captured = null;

        await _notificationService.CreateNotificationAsync(Arg.Do<DeploymentNotification>(n => captured = n));

        await _service.NotifyRollbackFailedAsync(request, "timeout");

        captured.Should().NotBeNull();
        captured!.Priority.Should().Be(NotificationPriority.Critical);
    }

    // ─── GetRollbackNotificationHistoryAsync ────────────────────────────────

    [Fact]
    public async Task GetRollbackNotificationHistoryAsync_RecordsAfterDispatch()
    {
        var request = CreateRequest();
        await _service.NotifyRollbackInitiatedAsync(request);

        var history = await _service.GetRollbackNotificationHistoryAsync("MyApp");
        history.Should().HaveCount(1);
        history[0].ProjectName.Should().Be("MyApp");
        history[0].TriggerStatus.Should().Be(RollbackStatus.InProgress);
    }

    [Fact]
    public async Task GetRollbackNotificationHistoryAsync_RespectsLimit()
    {
        for (var i = 0; i < 10; i++)
            await _service.NotifyRollbackInitiatedAsync(CreateRequest());

        var history = await _service.GetRollbackNotificationHistoryAsync("MyApp", 3);
        history.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetRollbackNotificationHistoryAsync_FiltersByProject()
    {
        var requestA = CreateRequest("ProjectA");
        var requestB = CreateRequest("ProjectB");

        await _service.NotifyRollbackInitiatedAsync(requestA);
        await _service.NotifyRollbackInitiatedAsync(requestB);

        var historyA = await _service.GetRollbackNotificationHistoryAsync("ProjectA");
        historyA.Should().ContainSingle(r => r.ProjectName == "ProjectA");
    }

    // ─── Helpers ────────────────────────────────────────────────────────────

    private static RollbackRequest CreateRequest(string projectName = "MyApp")
    {
        return new RollbackRequest
        {
            ProjectName = projectName,
            CurrentVersion = "1.5.0",
            TargetVersion = "1.4.0",
            TargetEnvironment = Environment.Production,
            RequestedBy = "ops-team",
            Channels = new List<NotificationChannel> { NotificationChannel.Slack }
        };
    }
}
