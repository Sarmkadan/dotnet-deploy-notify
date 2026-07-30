using System;
using System.Collections.Generic;
using DotNetDeployNotify.Configuration;
using Xunit;

namespace dotnet_deploy_notify.Tests
{
    public class DotnetDeployNotifyOptionsValidationTests
    {
        private static DotnetDeployNotifyOptions CreateValidOptions()
        {
            // Notification configuration – all values inside the allowed ranges.
            var notification = new NotificationConfig
            {
                MaxRetries = 3,
                WebhookTimeoutMs = 5000,
                RetryDelayMs = 2000,
                ProcessingIntervalSeconds = 30,
                StorageType = "Sql",
                DefaultPriority = "Normal",
                RetentionDays = 30,
                StoragePath = "/var/notify",
                LogLevel = "Info",
                EnvironmentChannels = new Dictionary<string, EnvironmentChannelConfig>
                {
                    {
                        "prod",
                        new EnvironmentChannelConfig
                        {
                            WebhookUrl = "https://example.com/webhook",
                            DisplayName = "Production",
                            TargetId = "prod-1",
                            ChannelType = "Slack"
                        }
                    }
                }
            };

            // Canary thresholds – all values within the allowed limits.
            var thresholds = new CanaryThresholds
            {
                MaxErrorRatePercent = 5,
                MaxP95LatencyMs = 200,
                MaxP99LatencyMs = 500,
                ErrorRateMultiplier = 1.5,
                LatencyDegradationPercent = 10
            };

            var canary = new CanaryOptions
            {
                Thresholds = thresholds
            };

            return new DotnetDeployNotifyOptions
            {
                Notification = notification,
                Canary = canary
            };
        }

        [Fact]
        public void Validate_WithValidOptions_ReturnsEmptyList()
        {
            var options = CreateValidOptions();

            var problems = options.Validate();

            Assert.Empty(problems);
        }

        [Fact]
        public void Validate_NullOptions_ThrowsArgumentNullException()
        {
            DotnetDeployNotifyOptions? options = null;

            Assert.Throws<ArgumentNullException>(() => options!.Validate());
        }

        [Fact]
        public void IsValid_WithValidOptions_ReturnsTrue()
        {
            var options = CreateValidOptions();

            bool result = options.IsValid();

            Assert.True(result);
        }

        [Fact]
        public void IsValid_WithInvalidOptions_ReturnsFalse()
        {
            var options = CreateValidOptions();
            options.Notification.MaxRetries = -1; // invalid

            bool result = options.IsValid();

            Assert.False(result);
        }

        [Fact]
        public void EnsureValid_WithValidOptions_DoesNotThrow()
        {
            var options = CreateValidOptions();

            var exception = Record.Exception(() => options.EnsureValid());

            Assert.Null(exception);
        }

        [Fact]
        public void EnsureValid_WithInvalidOptions_ThrowsArgumentException()
        {
            var options = CreateValidOptions();
            options.Notification.StorageType = ""; // invalid

            var ex = Assert.Throws<ArgumentException>(() => options.EnsureValid());

            Assert.Contains("Notification.StorageType is required", ex.Message);
        }

        [Fact]
        public void NotificationConfig_Validate_NullEnvironmentChannels_ProducesProblem()
        {
            var notification = new NotificationConfig
            {
                MaxRetries = 1,
                WebhookTimeoutMs = 1000,
                RetryDelayMs = 1000,
                ProcessingIntervalSeconds = 10,
                StorageType = "Sql",
                DefaultPriority = "Normal",
                RetentionDays = 10,
                EnvironmentChannels = null // deliberately null
            };

            var problems = notification.Validate();

            Assert.Contains("Notification.EnvironmentChannels is required and cannot be null.", problems);
        }

        [Fact]
        public void EnvironmentChannelConfig_Validate_EmptyWebhookUrl_ProducesProblem()
        {
            var channel = new EnvironmentChannelConfig
            {
                WebhookUrl = "",
                DisplayName = "Test",
                TargetId = "t1",
                ChannelType = "Slack"
            };

            var problems = channel.Validate();

            Assert.Contains("EnvironmentChannelConfig.WebhookUrl is required and cannot be empty.", problems);
        }

        [Fact]
        public void CanaryThresholds_Validate_NegativeValues_ProducesProblems()
        {
            var thresholds = new CanaryThresholds
            {
                MaxErrorRatePercent = -5,
                MaxP95LatencyMs = -1,
                MaxP99LatencyMs = -1,
                ErrorRateMultiplier = -0.1,
                LatencyDegradationPercent = -2
            };

            var problems = thresholds.Validate();

            Assert.Contains("Canary.Thresholds.MaxErrorRatePercent must be between 0 and 100", problems);
            Assert.Contains("Canary.Thresholds.MaxP95LatencyMs must be non-negative", problems);
            Assert.Contains("Canary.Thresholds.MaxP99LatencyMs must be non-negative", problems);
            Assert.Contains("Canary.Thresholds.ErrorRateMultiplier must be non-negative", problems);
            Assert.Contains("Canary.Thresholds.LatencyDegradationPercent must be non-negative", problems);
        }
    }
}
