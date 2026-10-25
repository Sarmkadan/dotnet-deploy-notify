#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Configuration;
using DotNetDeployNotify.Core;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Unit tests for <see cref="DotnetDeployNotifyOptionsExtensions"/>.
/// </summary>
public class DotnetDeployNotifyOptionsExtensionsTests
{
    private static DotnetDeployNotifyOptions CreateOptions(Action<NotificationConfig>? configure = null)
    {
        var options = new DotnetDeployNotifyOptions();
        configure?.Invoke(options.Notification);
        return options;
    }

    [Fact]
    public void GetWebhookTimeoutMs_GetMaxRetries_AndFlags_ReturnConfiguredBoundaryValues()
    {
        var defaults = CreateOptions();
        defaults.GetWebhookTimeoutMs("prod").Should().Be(10000);
        defaults.GetMaxRetries("prod").Should().Be(3);

        var options = CreateOptions(n =>
        {
            n.WebhookTimeoutMs = 100;
            n.MaxRetries = 0;
            n.AutoProcessNotifications = false;
            n.EnableAuditLogging = false;
        });

        options.GetWebhookTimeoutMs("prod").Should().Be(100);
        options.GetMaxRetries("prod").Should().Be(0);
        options.IsAutoProcessingEnabled("prod").Should().BeFalse();
        options.IsAuditLoggingEnabled("prod").Should().BeFalse();
    }

    [Fact]
    public void GetPriority_ParsesDefaultPriority_CaseInsensitively()
    {
        CreateOptions(n => n.DefaultPriority = "high").GetPriority("prod").Should().Be(NotificationPriority.High);
        CreateOptions(n => n.DefaultPriority = "CRITICAL").GetPriority("prod").Should().Be(NotificationPriority.Critical);
        CreateOptions().GetPriority("prod").Should().Be(NotificationPriority.Normal);
    }

    [Theory]
    [InlineData("Urgent")]
    [InlineData("")]
    [InlineData("   ")]
    public void GetPriority_FallsBackToNormal_WhenValueIsUnknownOrBlank(string value)
    {
        CreateOptions(n => n.DefaultPriority = value).GetPriority("prod").Should().Be(NotificationPriority.Normal);
        CreateOptions(n => n.DefaultPriority = null!).GetPriority("prod").Should().Be(NotificationPriority.Normal);
    }

    [Fact]
    public void GetDisplayName_UsesConfiguredName_OrFormatsEnvironmentAndChannelType()
    {
        var options = CreateOptions(n =>
        {
            n.EnvironmentChannels["prod"] = new EnvironmentChannelConfig { DisplayName = "Production Alerts" };
            n.EnvironmentChannels["staging"] = new EnvironmentChannelConfig { ChannelType = "Teams", DisplayName = "   " };
        });

        options.GetDisplayName("prod").Should().Be("Production Alerts");
        options.GetDisplayName("staging").Should().Be("staging-Teams");
        options.GetDisplayName("dev").Should().Be("dev-Slack");
    }

    [Fact]
    public void GetLogLevel_AndGetStoragePath_ReturnConfiguredValues_OrDefaults()
    {
        var defaults = CreateOptions();
        defaults.GetLogLevel("prod").Should().Be("Information");
        defaults.GetStoragePath("prod").Should().BeNull();

        CreateOptions(n => n.LogLevel = null!).GetLogLevel("prod").Should().Be("Information");

        var configured = CreateOptions(n =>
        {
            n.LogLevel = "Debug";
            n.StoragePath = "/var/lib/dnd";
        });
        configured.GetLogLevel("prod").Should().Be("Debug");
        configured.GetStoragePath("prod").Should().Be("/var/lib/dnd");
    }

    [Fact]
    public void IsCanaryEnabled_AndIsCanaryAutoRollbackEnabled_ReflectCanaryOptions()
    {
        var options = new DotnetDeployNotifyOptions();
        options.IsCanaryEnabled().Should().BeTrue();
        options.IsCanaryAutoRollbackEnabled().Should().BeTrue();

        options.Canary.Enabled = false;
        options.Canary.AutoRollbackOnFailure = false;
        options.IsCanaryEnabled().Should().BeFalse();
        options.IsCanaryAutoRollbackEnabled().Should().BeFalse();
    }

    [Fact]
    public void AllMethods_ThrowArgumentNullException_WhenOptionsIsNull()
    {
        var calls = new Action<DotnetDeployNotifyOptions>[]
        {
            o => o.GetWebhookTimeoutMs("prod"),
            o => o.GetMaxRetries("prod"),
            o => o.IsAutoProcessingEnabled("prod"),
            o => o.GetPriority("prod"),
            o => o.IsAuditLoggingEnabled("prod"),
            o => o.GetDisplayName("prod"),
            o => o.GetStoragePath("prod"),
            o => o.GetLogLevel("prod"),
            o => o.IsCanaryEnabled(),
            o => o.IsCanaryAutoRollbackEnabled()
        };

        foreach (var call in calls)
        {
            Assert.Throws<ArgumentNullException>(() => call(null!));
        }
    }

    [Fact]
    public void EnvironmentMethods_ThrowArgumentException_WhenEnvironmentNameIsBlank()
    {
        var calls = new Action<DotnetDeployNotifyOptions, string>[]
        {
            (o, env) => o.GetWebhookTimeoutMs(env),
            (o, env) => o.GetMaxRetries(env),
            (o, env) => o.IsAutoProcessingEnabled(env),
            (o, env) => o.GetPriority(env),
            (o, env) => o.IsAuditLoggingEnabled(env),
            (o, env) => o.GetDisplayName(env),
            (o, env) => o.GetStoragePath(env),
            (o, env) => o.GetLogLevel(env)
        };
        var options = new DotnetDeployNotifyOptions();

        foreach (var blank in new[] { "", "   " })
        {
            foreach (var call in calls)
            {
                Assert.Throws<ArgumentException>(() => call(options, blank));
            }
        }
    }
}
