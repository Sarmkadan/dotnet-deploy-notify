#nullable enable
using System.Net;
using System.Text.Json;
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Services;
using DotNetDeployNotify.Tests.Fakes;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Environment = DotNetDeployNotify.Core.Environment;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Verifies per-channel payload formatting by driving the real
/// <see cref="WebhookDispatcher"/> and <see cref="PayloadBuilder"/> through a
/// <see cref="FakeWebhookTransport"/> and asserting on the captured wire payload.
/// </summary>
public class ChannelPayloadTests
{
    private static WebhookDispatcher CreateDispatcher(FakeWebhookTransport transport)
    {
        var httpClient = new HttpClient(transport);
        var payloadBuilder = new PayloadBuilder(Substitute.For<ILogger<PayloadBuilder>>());
        return new WebhookDispatcher(httpClient, Substitute.For<ILogger<WebhookDispatcher>>(), payloadBuilder);
    }

    private static DeploymentNotification SampleNotification(
        BuildStatus status = BuildStatus.DeploymentSuccess) => new()
    {
        ProjectName = "Checkout.Api",
        Version = "3.2.1",
        Status = status,
        Message = "Deployment finished",
        TargetEnvironment = Environment.Production,
        BranchName = "main",
        CommitHash = "abcdef1234567890",
        CommitAuthor = "alice@example.com",
        RepositoryUrl = "https://github.com/org/checkout",
        BuildUrl = "https://ci.example.com/builds/42",
        DurationSeconds = 97,
        Priority = NotificationPriority.High
    };

    private static ChannelConfiguration Config(NotificationChannel channel, string url) => new()
    {
        ChannelType = channel,
        DisplayName = $"{channel} config",
        WebhookUrl = url,
        TimeoutMs = 5000,
        IncludeCommitDetails = true,
        IncludeBuildUrl = true,
        CustomHeaders = new Dictionary<string, string>()
    };

    private static JsonElement ParseData(string body)
    {
        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("Data").Clone();
    }

    [Fact]
    public async Task Slack_payload_contains_attachment_block_with_status_color()
    {
        var transport = new FakeWebhookTransport();
        var dispatcher = CreateDispatcher(transport);

        var result = await dispatcher.SendToWebhookAsync(
            Config(NotificationChannel.Slack, "https://hooks.slack.com/services/T/B/X"),
            SampleNotification());

        result.IsSuccessful.Should().BeTrue();
        transport.Requests.Should().ContainSingle();

        var data = ParseData(transport.LastRequest!.Body);
        var slack = data.GetProperty("CustomProperties").GetProperty("slack_format");
        var attachment = slack.GetProperty("attachments")[0];

        attachment.GetProperty("color").GetString().Should().Be("#00ff00"); // DeploymentSuccess
        attachment.GetProperty("title").GetString().Should().Contain("Checkout.Api v3.2.1");
    }

    [Fact]
    public async Task Slack_block_kit_payload_uses_blocks_when_enabled()
    {
        var transport = new FakeWebhookTransport();
        var dispatcher = CreateDispatcher(transport);
        var config = Config(NotificationChannel.Slack, "https://hooks.slack.com/services/T/B/X");
        config.UseSlackBlockKit = true;

        await dispatcher.SendToWebhookAsync(config, SampleNotification());

        var data = ParseData(transport.LastRequest!.Body);
        var slack = data.GetProperty("CustomProperties").GetProperty("slack_format");
        var firstBlock = slack.GetProperty("blocks")[0];

        firstBlock.GetProperty("type").GetString().Should().Be("header");
    }

    [Fact]
    public async Task Telegram_payload_carries_html_formatted_text()
    {
        var transport = new FakeWebhookTransport();
        var dispatcher = CreateDispatcher(transport);

        await dispatcher.SendToWebhookAsync(
            Config(NotificationChannel.Telegram, "https://api.telegram.org/bot123/sendMessage"),
            SampleNotification());

        var data = ParseData(transport.LastRequest!.Body);
        // JsonDocument decodes escaped characters, so raw HTML tags come back intact.
        var text = data.GetProperty("CustomProperties").GetProperty("telegram_text").GetString();

        text.Should().Contain("<b>Checkout.Api</b> v3.2.1");
        text.Should().Contain("<b>Status:</b>");
        text.Should().Contain("<code>abcdef1</code>"); // shortened commit hash
    }

    [Fact]
    public async Task Generic_webhook_payload_maps_notification_fields()
    {
        var transport = new FakeWebhookTransport();
        var dispatcher = CreateDispatcher(transport);

        await dispatcher.SendToWebhookAsync(
            Config(NotificationChannel.Webhook, "https://example.com/hooks/deploy"),
            SampleNotification());

        transport.LastRequest!.Url.Should().Be("https://example.com/hooks/deploy");
        transport.LastRequest.Headers.Should().ContainKey("Content-Type");
        transport.LastRequest.Headers["Content-Type"].Should().Contain("application/json");

        var data = ParseData(transport.LastRequest.Body);
        data.GetProperty("ProjectName").GetString().Should().Be("Checkout.Api");
        data.GetProperty("Version").GetString().Should().Be("3.2.1");
        data.GetProperty("Status").GetString().Should().Be("DeploymentSuccess");
        data.GetProperty("Environment").GetString().Should().Be("Production");
        // Generic webhook must not carry channel-specific decorations.
        data.GetProperty("CustomProperties").TryGetProperty("slack_format", out _).Should().BeFalse();
    }

    [Fact]
    public async Task Custom_headers_are_forwarded_to_transport()
    {
        var transport = new FakeWebhookTransport();
        var dispatcher = CreateDispatcher(transport);
        var config = Config(NotificationChannel.Webhook, "https://example.com/hooks/deploy");
        config.CustomHeaders["X-Deploy-Token"] = "s3cr3t";

        await dispatcher.SendToWebhookAsync(config, SampleNotification());

        transport.LastRequest!.Headers.Should().ContainKey("X-Deploy-Token");
        transport.LastRequest.Headers["X-Deploy-Token"].Should().Be("s3cr3t");
    }

    [Fact]
    public async Task Failed_status_maps_to_red_slack_color()
    {
        var transport = new FakeWebhookTransport();
        var dispatcher = CreateDispatcher(transport);

        await dispatcher.SendToWebhookAsync(
            Config(NotificationChannel.Slack, "https://hooks.slack.com/services/T/B/X"),
            SampleNotification(BuildStatus.DeploymentFailed));

        var data = ParseData(transport.LastRequest!.Body);
        var color = data.GetProperty("CustomProperties")
            .GetProperty("slack_format")
            .GetProperty("attachments")[0]
            .GetProperty("color").GetString();

        color.Should().Be("#ff0000");
    }

    [Fact]
    public async Task Non_success_http_response_is_reported_as_failure()
    {
        var transport = new FakeWebhookTransport(HttpStatusCode.InternalServerError, "boom");
        var dispatcher = CreateDispatcher(transport);

        var result = await dispatcher.SendToWebhookAsync(
            Config(NotificationChannel.Slack, "https://hooks.slack.com/services/T/B/X"),
            SampleNotification());

        result.IsSuccessful.Should().BeFalse();
        result.HttpStatusCode.Should().Be(500);
    }
}
