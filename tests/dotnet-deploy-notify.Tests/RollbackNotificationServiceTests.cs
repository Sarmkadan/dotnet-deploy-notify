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
/// Contains unit tests for the <see cref="RollbackNotificationService"/> class.
/// Tests various rollback notification scenarios including message formatting,
/// notification dispatch, and history tracking.
/// </summary>
public class RollbackNotificationServiceTests
{
    private readonly RollbackNotificationService _service;
    private readonly INotificationService _notificationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RollbackNotificationServiceTests"/> class.
    /// Sets up mock dependencies for testing rollback notification functionality.
    /// </summary>
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

    /// <summary>
    /// Tests that the rollback message for Slack channel contains proper markdown formatting.
    /// Verifies that asterisk-based markdown is used for emphasis in the message.
    /// </summary>
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

    /// <summary>
    /// Tests that the rollback message for Discord channel contains bold markdown formatting.
    /// Verifies that double-asterisk-based markdown is used for emphasis in the message.
    /// </summary>
    [Fact]
    public void FormatRollbackMessage_Discord_ContainsBoldMarkdown()
    {
        var request = CreateRequest();

        var msg = _service.FormatRollbackMessage(request, RollbackStatus.Completed, NotificationChannel.Discord);

        msg.Should().Contain("**Rollback completed successfully**");
        msg.Should().Contain("MyApp");
    }

    /// <summary>
    /// Tests that the rollback message for Telegram channel contains HTML tags.
    /// Verifies that HTML bold tags are used for emphasis in the message.
    /// </summary>
    [Fact]
    public void FormatRollbackMessage_Telegram_ContainsHtmlTags()
    {
        var request = CreateRequest();

        var msg = _service.FormatRollbackMessage(request, RollbackStatus.Failed, NotificationChannel.Telegram);

        msg.Should().Contain("<b>Rollback failed</b>");
        msg.Should().Contain("<code>");
    }

    /// <summary>
    /// Tests that the generic rollback message contains project information.
    /// Verifies that the message includes the project name, status, and version information.
    /// </summary>
    [Fact]
    public void FormatRollbackMessage_Generic_ContainsProjectInfo()
    {
        var request = CreateRequest();

        var msg = _service.FormatRollbackMessage(request, RollbackStatus.Cancelled, NotificationChannel.Webhook);

        msg.Should().Contain("MyApp");
        msg.Should().Contain("cancelled");
        msg.Should().Contain("v1.5.0");
    }

    /// <summary>
    /// Tests that the rollback message includes the reason when provided.
    /// Verifies that custom reasons are properly included in the notification message.
    /// </summary>
    [Fact]
    public void FormatRollbackMessage_WithReason_IncludesReason()
    {
        var request = CreateRequest();
        request.Reason = "Critical production bug";

        var msg = _service.FormatRollbackMessage(request, RollbackStatus.InProgress, NotificationChannel.Slack);

        msg.Should().Contain("Critical production bug");
    }

    /// <summary>
    /// Tests that the rollback message includes additional details when provided.
    /// Verifies that custom details are properly included in the notification message.
    /// </summary>
    [Fact]
    public void FormatRollbackMessage_WithAdditionalDetails_IncludesDetails()
    {
        var request = CreateRequest();
        var msg = _service.FormatRollbackMessage(
            request, RollbackStatus.Failed, NotificationChannel.Slack, "Database migration failed");

        msg.Should().Contain("Database migration failed");
    }

    /// <summary>
    /// Tests that the rollback message uses the correct emoji for each status.
    /// Verifies that appropriate status indicators (🔄, ✅, ❌, 🚫) are used in the message.
    /// </summary>
    /// <param name="status">The rollback status to test.</param>
    /// <param name="expectedEmoji">The expected emoji character for the given status.</param>
    [Theory]
    [InlineData(RollbackStatus.InProgress, "🔄")]
    [InlineData(RollbackStatus.Completed, "✅")]
    [InlineData(RollbackStatus.Failed, "❌")]
    [InlineData(RollbackStatus.Cancelled, "🚫")]
    public void FormatRollbackMessage_UsesCorrectEmoji(RollbackStatus status, string expectedEmoji)
    {
        var request = CreateRequest();
        var msg = _service.FormatRollbackMessage(request, status, NotificationChannel.Slack);
        msg.Should().Contain(expectedEmoji);
    }

    // ─── NotifyRollbackInitiatedAsync ───────────────────────────────────────

    /// <summary>
    /// Tests that NotifyRollbackInitiatedAsync calls the notification service methods.
    /// Verifies that both CreateNotificationAsync and SendNotificationAsync are invoked.
    /// </summary>
    [Fact]
    public async Task NotifyRollbackInitiatedAsync_CallsNotificationService()
    {
        var request = CreateRequest();

        await _service.NotifyRollbackInitiatedAsync(request);

        await _notificationService.Received(1).CreateNotificationAsync(Arg.Any<DeploymentNotification>());
        await _notificationService.Received(1).SendNotificationAsync(Arg.Any<string>(), Arg.Any<List<NotificationChannel>?>());
    }

    /// <summary>
    /// Tests that NotifyRollbackInitiatedAsync throws ArgumentNullException when null request is provided.
    /// Verifies proper null checking and exception handling.
    /// </summary>
    [Fact]
    public async Task NotifyRollbackInitiatedAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        Func<Task> act = () => _service.NotifyRollbackInitiatedAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that NotifyRollbackInitiatedAsync returns delivery results.
    /// Verifies that the method returns a non-null collection of notification results.
    /// </summary>
    [Fact]
    public async Task NotifyRollbackInitiatedAsync_ReturnsDeliveryResults()
    {
        var request = CreateRequest();
        var results = await _service.NotifyRollbackInitiatedAsync(request);

        results.Should().NotBeNull();
        results.Should().HaveCount(1);
    }

    // ─── NotifyRollbackCompletedAsync ───────────────────────────────────────

    /// <summary>
    /// Tests that NotifyRollbackCompletedAsync sends a completion notification.
    /// Verifies that CreateNotificationAsync is called with DeploymentSuccess status.
    /// </summary>
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

    /// <summary>
    /// Tests that NotifyRollbackFailedAsync sends a failure notification.
    /// Verifies that CreateNotificationAsync is called with DeploymentFailed status.
    /// </summary>
    [Fact]
    public async Task NotifyRollbackFailedAsync_SendsFailureNotification()
    {
        var request = CreateRequest();

        var results = await _service.NotifyRollbackFailedAsync(request, "Deployment script error");

        results.Should().NotBeNull();
        await _notificationService.Received(1).CreateNotificationAsync(
            Arg.Is<DeploymentNotification>(n => n.Status == BuildStatus.DeploymentFailed));
    }

    /// <summary>
    /// Tests that NotifyRollbackFailedAsync sets critical priority for failure notifications.
    /// Verifies that failed rollback notifications are marked as critical priority.
    /// </summary>
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

    /// <summary>
    /// Tests that GetRollbackNotificationHistoryAsync records notifications after dispatch.
    /// Verifies that initiated rollback notifications are properly tracked in history.
    /// </summary>
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

    /// <summary>
    /// Tests that GetRollbackNotificationHistoryAsync respects the limit parameter.
    /// Verifies that the history collection size is limited to the specified maximum.
    /// </summary>
    [Fact]
    public async Task GetRollbackNotificationHistoryAsync_RespectsLimit()
    {
        for (var i = 0; i < 10; i++)
            await _service.NotifyRollbackInitiatedAsync(CreateRequest());

        var history = await _service.GetRollbackNotificationHistoryAsync("MyApp", 3);
        history.Should().HaveCount(3);
    }

    /// <summary>
    /// Tests that GetRollbackNotificationHistoryAsync filters by project name.
    /// Verifies that only notifications for the specified project are returned.
    /// </summary>
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

    /// <summary>
    /// Creates a test rollback request with default values.
    /// </summary>
    /// <param name="projectName">The name of the project to create the request for.</param>
    /// <returns>A new <see cref="RollbackRequest"/> instance with test data.</returns>
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
