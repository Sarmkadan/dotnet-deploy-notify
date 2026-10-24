#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using FluentAssertions;
using Xunit;
using Environment = DotNetDeployNotify.Core.Environment;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Tests for <see cref="ChannelConfigurationExtensions"/> covering copying, environment and status filters,
/// timeout and retry clamping, display helpers, include flags and header access.
/// </summary>
public class ChannelConfigurationExtensionsTests
{
    [Fact]
    public void DeepCopy_CopiesValues_AndDetachesCollections()
    {
        var original = new ChannelConfiguration
        {
            Id = "cfg-1",
            ChannelType = NotificationChannel.Slack,
            WebhookUrl = "https://hooks.example.com/abc",
            DisplayName = "Ops Slack",
            IsEnabled = false,
            IncludeCommitDetails = false,
            MinimumPriority = NotificationPriority.High,
            MaxRetries = 7,
            TimeoutMs = 25000,
            UseSlackBlockKit = true,
            EnableEmojis = false,
            AllowedEnvironments = new List<Environment> { Environment.Production },
            AllowedStatuses = new List<BuildStatus> { BuildStatus.Failed },
            CustomHeaders = new Dictionary<string, string> { ["X-Team"] = "platform" },
            Settings = new Dictionary<string, string> { ["lang"] = "en" }
        };

        var copy = original.DeepCopy();

        copy.Should().NotBeSameAs(original);
        copy.Id.Should().Be("cfg-1");
        copy.ChannelType.Should().Be(NotificationChannel.Slack);
        copy.WebhookUrl.Should().Be("https://hooks.example.com/abc");
        copy.DisplayName.Should().Be("Ops Slack");
        copy.IsEnabled.Should().BeFalse();
        copy.IncludeCommitDetails.Should().BeFalse();
        copy.MinimumPriority.Should().Be(NotificationPriority.High);
        copy.MaxRetries.Should().Be(7);
        copy.TimeoutMs.Should().Be(25000);
        copy.UseSlackBlockKit.Should().BeTrue();
        copy.EnableEmojis.Should().BeFalse();
        copy.AllowedEnvironments.Should().Equal(Environment.Production);
        copy.AllowedStatuses.Should().Equal(BuildStatus.Failed);
        copy.CustomHeaders.Should().BeEquivalentTo(original.CustomHeaders);
        copy.Settings.Should().BeEquivalentTo(original.Settings);

        copy.AllowedEnvironments.Add(Environment.Staging);
        copy.AllowedStatuses.Clear();
        copy.CustomHeaders["X-Team"] = "changed";
        copy.Settings["lang"] = "fr";

        original.AllowedEnvironments.Should().Equal(Environment.Production);
        original.AllowedStatuses.Should().Equal(BuildStatus.Failed);
        original.CustomHeaders["X-Team"].Should().Be("platform");
        original.Settings["lang"].Should().Be("en");
    }

    [Fact]
    public void NullConfiguration_ThrowsArgumentNullException()
    {
        Action copy = () => ((ChannelConfiguration)null!).DeepCopy();
        Action headers = () => ((ChannelConfiguration)null!).GetCustomHeaders();

        copy.Should().Throw<ArgumentNullException>();
        headers.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EnvironmentAndStatusFilters_EmptyListsAllowAll_OtherwiseOnlyListedValues()
    {
        var config = new ChannelConfiguration();

        foreach (var environment in Enum.GetValues<Environment>())
        {
            config.IsEnvironmentAllowed(environment).Should().BeTrue();
        }

        foreach (var status in Enum.GetValues<BuildStatus>())
        {
            config.IsStatusAllowed(status).Should().BeTrue();
        }

        config.AllowedEnvironments.Add(Environment.Production);
        config.AllowedStatuses.Add(BuildStatus.Success);
        config.AllowedStatuses.Add(BuildStatus.DeploymentSuccess);

        config.IsEnvironmentAllowed(Environment.Production).Should().BeTrue();
        config.IsEnvironmentAllowed(Environment.Staging).Should().BeFalse();
        config.IsStatusAllowed(BuildStatus.DeploymentSuccess).Should().BeTrue();
        config.IsStatusAllowed(BuildStatus.Failed).Should().BeFalse();
    }

    [Theory]
    [InlineData(-5, 1000)]
    [InlineData(999, 1000)]
    [InlineData(1000, 1000)]
    [InlineData(30000, 30000)]
    [InlineData(60000, 60000)]
    [InlineData(60001, 60000)]
    [InlineData(int.MaxValue, 60000)]
    public void GetEffectiveTimeout_ClampsToOneToSixtySeconds(int timeoutMs, int expected)
    {
        var config = new ChannelConfiguration { TimeoutMs = timeoutMs };

        config.GetEffectiveTimeout().Should().Be(expected);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, 0)]
    [InlineData(5, 5)]
    [InlineData(10, 10)]
    [InlineData(11, 10)]
    public void GetEffectiveRetryCount_ClampsToZeroThroughTen(int maxRetries, int expected)
    {
        var config = new ChannelConfiguration { MaxRetries = maxRetries };

        config.GetEffectiveRetryCount().Should().Be(expected);
    }

    [Fact]
    public void DisplayHelpers_MapKnownValues_AndFallBackToEnumName()
    {
        var config = new ChannelConfiguration();

        config.MinimumPriority = NotificationPriority.Critical;
        config.GetPriorityThresholdDisplay().Should().Be("Critical");
        config.MinimumPriority = NotificationPriority.Low;
        config.GetPriorityThresholdDisplay().Should().Be("Low");
        config.MinimumPriority = (NotificationPriority)42;
        config.GetPriorityThresholdDisplay().Should().Be("42");

        config.ChannelType = NotificationChannel.Telegram;
        config.GetChannelTypeDisplay().Should().Be("Telegram");
        config.ChannelType = NotificationChannel.Email;
        config.GetChannelTypeDisplay().Should().Be("Email");
        config.ChannelType = NotificationChannel.Teams;
        config.GetChannelTypeDisplay().Should().Be("Teams");
        config.ChannelType = (NotificationChannel)99;
        config.GetChannelTypeDisplay().Should().Be("99");
    }

    [Fact]
    public void ShouldInclude_RequiresBothFlagAndEnabled()
    {
        var config = new ChannelConfiguration
        {
            IsEnabled = true,
            IncludeCommitDetails = true,
            IncludeBuildUrl = true
        };

        config.ShouldIncludeCommitDetails().Should().BeTrue();
        config.ShouldIncludeBuildUrl().Should().BeTrue();

        config.IsEnabled = false;
        config.ShouldIncludeCommitDetails().Should().BeFalse();
        config.ShouldIncludeBuildUrl().Should().BeFalse();

        config.IsEnabled = true;
        config.IncludeCommitDetails = false;
        config.IncludeBuildUrl = false;
        config.ShouldIncludeCommitDetails().Should().BeFalse();
        config.ShouldIncludeBuildUrl().Should().BeFalse();
    }

    [Fact]
    public void GetCustomHeaders_ReturnsReadOnlyView()
    {
        var config = new ChannelConfiguration
        {
            CustomHeaders = new Dictionary<string, string> { ["X-Env"] = "prod" }
        };

        var headers = config.GetCustomHeaders();

        headers["X-Env"].Should().Be("prod");
        var writable = (IDictionary<string, string>)headers;
        Action add = () => writable.Add("X-New", "1");
        add.Should().Throw<NotSupportedException>();
    }
}
