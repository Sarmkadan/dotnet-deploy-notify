#nullable enable
using System.Text.Json;
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Data;
using DotNetDeployNotify.Services;
using DotNetDeployNotify.Tests.Fakes;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Environment = DotNetDeployNotify.Core.Environment;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// End-to-end tests for the rollback flow that wire real repositories, the
/// validation service, the payload builder, and the webhook dispatcher together,
/// exercising rollback discovery through deployment history. Only the network
/// boundary is faked, via <see cref="FakeWebhookTransport"/>.
/// </summary>
public class RollbackFlowIntegrationTests
{
    private static ILogger<T> Log<T>() => Substitute.For<ILogger<T>>();

    private sealed record Harness(
        RollbackService RollbackService,
        NotificationRepository NotificationRepo,
        DeploymentHistoryService History,
        FakeWebhookTransport Transport);

    private static Harness BuildHarness()
    {
        var notificationRepo = new NotificationRepository(Log<NotificationRepository>());
        var resultRepo = new NotificationResultRepository(Log<NotificationResultRepository>());

        var slackConfig = new ChannelConfiguration
        {
            ChannelType = NotificationChannel.Slack,
            DisplayName = "Slack Prod",
            WebhookUrl = "https://hooks.slack.com/services/T/B/X",
            TimeoutMs = 5000,
            IsEnabled = true,
            CustomHeaders = new Dictionary<string, string>()
        };
        var configRepo = new ChannelConfigRepository(
            Log<ChannelConfigRepository>(),
            new[] { slackConfig });

        var transport = new FakeWebhookTransport();
        var dispatcher = new WebhookDispatcher(
            new HttpClient(transport),
            Log<WebhookDispatcher>(),
            new PayloadBuilder(Log<PayloadBuilder>()));

        var notificationService = new NotificationService(
            notificationRepo, configRepo, resultRepo, dispatcher,
            new ValidationService(), Log<NotificationService>());

        var rollbackService = new RollbackService(
            notificationService, notificationRepo, Log<RollbackService>());

        var history = new DeploymentHistoryService(Log<DeploymentHistoryService>());

        return new Harness(rollbackService, notificationRepo, history, transport);
    }

    private static DeploymentNotification PriorDeployment() => new()
    {
        ProjectName = "Checkout.Api",
        Version = "3.1.0",
        Status = BuildStatus.DeploymentSuccess,
        Message = "Original release",
        TargetEnvironment = Environment.Production,
        BranchName = "main",
        CommitHash = "cafebabe0011",
        CommitAuthor = "alice@example.com",
        RepositoryUrl = "https://github.com/org/checkout",
        BuildUrl = "https://ci.example.com/builds/10",
        Channels = new List<NotificationChannel> { NotificationChannel.Slack }
    };

    private static RollbackRequest RollbackTo310() => new()
    {
        ProjectName = "Checkout.Api",
        TargetVersion = "3.1.0",
        CurrentVersion = "3.2.0",
        TargetEnvironment = Environment.Production,
        RequestedBy = "oncall@example.com",
        Reason = "elevated 5xx after 3.2.0",
        Channels = new List<NotificationChannel> { NotificationChannel.Slack },
        Priority = NotificationPriority.High
    };

    [Fact]
    public async Task Rollback_discovers_prior_deployment_and_reuses_its_commit_metadata()
    {
        var h = BuildHarness();
        await h.NotificationRepo.CreateAsync(PriorDeployment());

        var result = await h.RollbackService.InitiateRollbackAsync(RollbackTo310());

        result.IsSuccessful.Should().BeTrue();
        result.RolledBackFromVersion.Should().Be("3.2.0");
        result.RolledBackToVersion.Should().Be("3.1.0");

        // Two notifications go out: rollback-initiated and rollback-completed.
        h.Transport.Requests.Should().HaveCountGreaterThanOrEqualTo(2);

        var firstData = ParseData(h.Transport.Requests[0].Body);
        firstData.GetProperty("Message").GetString().Should().Contain("Rolling back Checkout.Api");
        firstData.GetProperty("Message").GetString().Should().Contain("elevated 5xx");
        // Branch/commit were carried over from the discovered prior deployment.
        firstData.GetProperty("Branch").GetString().Should().Be("main");
        firstData.GetProperty("CommitHash").GetString().Should().Be("cafebab");
    }

    [Fact]
    public async Task Rollback_notification_records_rollback_metadata_for_history()
    {
        var h = BuildHarness();
        await h.NotificationRepo.CreateAsync(PriorDeployment());

        await h.RollbackService.InitiateRollbackAsync(RollbackTo310());

        // Feed the persisted notifications into the deployment-history service and
        // confirm the rollback surfaces through the history query path.
        var stored = await h.NotificationRepo.GetByProjectAsync("Checkout.Api", 50);
        foreach (var n in stored)
            await h.History.RecordFromNotificationAsync(n);

        var rollbacks = await h.History.GetRollbackEntriesAsync("Checkout.Api");
        rollbacks.Should().NotBeEmpty();
        rollbacks.Should().OnlyContain(e => e.IsRollback);
        rollbacks.Should().Contain(e => e.RolledBackFromVersion == "3.2.0");
    }

    [Fact]
    public async Task Rollback_records_result_retrievable_from_rollback_history()
    {
        var h = BuildHarness();
        await h.NotificationRepo.CreateAsync(PriorDeployment());

        var result = await h.RollbackService.InitiateRollbackAsync(RollbackTo310());

        var history = await h.RollbackService.GetRollbackHistoryAsync("Checkout.Api");
        history.Should().ContainSingle();
        history[0].Id.Should().Be(result.Id);
        history[0].Status.Should().Be(RollbackStatus.Completed);
        history[0].NotificationResults.Should().NotBeEmpty();
    }

    private static JsonElement ParseData(string body)
    {
        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("Data").Clone();
    }
}
