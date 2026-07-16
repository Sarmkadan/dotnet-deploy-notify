#nullable enable
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Environment = DotNetDeployNotify.Core.Environment;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Tests for <see cref="DryRunRenderer"/>, which previews channel payloads without
/// dispatching them.
/// </summary>
public class DryRunRendererTests
{
    private static DryRunRenderer CreateRenderer() =>
        new(new PayloadBuilder(Substitute.For<ILogger<PayloadBuilder>>()),
            Substitute.For<ILogger<DryRunRenderer>>());

    private static DeploymentNotification Notification(
        NotificationPriority priority = NotificationPriority.High,
        Environment environment = Environment.Production) => new()
    {
        ProjectName = "Billing.Worker",
        Version = "5.0.0",
        Status = BuildStatus.DeploymentSuccess,
        Message = "Deployed",
        TargetEnvironment = environment,
        BranchName = "main",
        CommitHash = "0123456789ab",
        CommitAuthor = "bob@example.com",
        Priority = priority
    };

    private static ChannelConfiguration Config(NotificationChannel channel, string url) => new()
    {
        ChannelType = channel,
        DisplayName = $"{channel} config",
        WebhookUrl = url,
        TimeoutMs = 5000,
        IncludeCommitDetails = true
    };

    [Fact]
    public void Render_slack_produces_json_payload_and_marks_would_send()
    {
        var renderer = CreateRenderer();

        var result = renderer.Render(
            Notification(),
            Config(NotificationChannel.Slack, "https://hooks.slack.com/services/T/B/XYZ"));

        result.Channel.Should().Be(NotificationChannel.Slack);
        result.WouldSend.Should().BeTrue();
        result.SkipReason.Should().BeNull();
        result.RenderedPayload.Should().Contain("slack_format");
        result.RenderedPayload.Should().Contain("Billing.Worker");
    }

    [Fact]
    public void Render_telegram_produces_html_text_not_json()
    {
        var renderer = CreateRenderer();

        var result = renderer.Render(
            Notification(),
            Config(NotificationChannel.Telegram, "https://api.telegram.org/bot123456:secret/sendMessage"));

        result.RenderedPayload.Should().Contain("<b>Billing.Worker</b> v5.0.0");
        result.RenderedPayload.Should().NotContain("\"EventType\""); // not the JSON envelope
    }

    [Fact]
    public void Render_masks_telegram_bot_token_in_target_url()
    {
        var renderer = CreateRenderer();

        var result = renderer.Render(
            Notification(),
            Config(NotificationChannel.Telegram, "https://api.telegram.org/bot123456:secret/sendMessage"));

        result.TargetUrl.Should().NotContain("123456:secret");
        result.TargetUrl.Should().Contain("/bot***/sendMessage");
    }

    [Fact]
    public void Render_flags_skip_when_priority_below_minimum()
    {
        var renderer = CreateRenderer();
        var config = Config(NotificationChannel.Slack, "https://hooks.slack.com/services/T/B/XYZ");
        config.MinimumPriority = NotificationPriority.Critical;

        var result = renderer.Render(Notification(priority: NotificationPriority.Normal), config);

        result.WouldSend.Should().BeFalse();
        result.SkipReason.Should().Contain("priority");
        // Even when it would be skipped, the payload is still rendered for preview.
        result.RenderedPayload.Should().NotBeEmpty();
    }

    [Fact]
    public void Render_flags_skip_when_environment_not_allowed()
    {
        var renderer = CreateRenderer();
        var config = Config(NotificationChannel.Slack, "https://hooks.slack.com/services/T/B/XYZ");
        config.AllowedEnvironments = new List<Environment> { Environment.Staging };

        var result = renderer.Render(Notification(environment: Environment.Production), config);

        result.WouldSend.Should().BeFalse();
        result.SkipReason.Should().Contain("environment");
    }

    [Fact]
    public void RenderAll_returns_one_result_per_config()
    {
        var renderer = CreateRenderer();
        var configs = new[]
        {
            Config(NotificationChannel.Slack, "https://hooks.slack.com/services/T/B/XYZ"),
            Config(NotificationChannel.Telegram, "https://api.telegram.org/bot1/sendMessage"),
            Config(NotificationChannel.Webhook, "https://example.com/hook")
        };

        var results = renderer.RenderAll(Notification(), configs);

        results.Should().HaveCount(3);
        results.Select(r => r.Channel).Should().BeEquivalentTo(new[]
        {
            NotificationChannel.Slack, NotificationChannel.Telegram, NotificationChannel.Webhook
        });
    }

    [Fact]
    public void Render_does_not_perform_any_dispatch()
    {
        // The renderer takes only a payload builder — there is no transport it could
        // call — so constructing it and rendering cannot cause a network send.
        var renderer = CreateRenderer();

        var act = () => renderer.RenderAll(
            Notification(),
            new[] { Config(NotificationChannel.Webhook, "https://example.com/hook") });

        act.Should().NotThrow();
    }
}
