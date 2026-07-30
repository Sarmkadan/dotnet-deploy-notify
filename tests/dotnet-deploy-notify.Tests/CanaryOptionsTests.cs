using System;
using DotNetDeployNotify.Configuration;
using DotNetDeployNotify.Core;
using Xunit;

namespace dotnet_deploy_notify.Tests
{
    public class CanaryOptionsTests
    {
        [Fact]
        public void DefaultValues_ShouldMatchExpected()
        {
            // Arrange
            var options = new CanaryOptions();

            // Assert default scalar properties
            Assert.True(options.Enabled);
            Assert.True(options.AutoRollbackOnFailure);
            Assert.False(options.AutoAdvanceOnSuccess);
            Assert.Equal(5, options.LinearStepCount);
            Assert.Equal(TimeSpan.FromMinutes(10), options.StepSoakDuration);
            Assert.Equal(TimeSpan.FromHours(4), options.MaxDeploymentDuration);
            Assert.Equal(NotificationPriority.High, options.AlertPriority);

            // Assert default thresholds
            var thresholds = options.Thresholds;
            Assert.NotNull(thresholds);
            Assert.Equal(1.0, thresholds.MaxErrorRatePercent);
            Assert.Equal(1_000, thresholds.MaxP95LatencyMs);
            Assert.Equal(2_000, thresholds.MaxP99LatencyMs);
            Assert.Equal(2.0, thresholds.ErrorRateMultiplier);
            Assert.Equal(20.0, thresholds.LatencyDegradationPercent);
        }

        [Fact]
        public void CanModifyProperties_ShouldPersist()
        {
            // Arrange
            var options = new CanaryOptions
            {
                Enabled = false,
                AutoRollbackOnFailure = false,
                AutoAdvanceOnSuccess = true,
                LinearStepCount = 10,
                StepSoakDuration = TimeSpan.FromMinutes(30),
                MaxDeploymentDuration = TimeSpan.FromHours(8),
                AlertPriority = NotificationPriority.Low
            };

            // Assert that the set values are retained
            Assert.False(options.Enabled);
            Assert.False(options.AutoRollbackOnFailure);
            Assert.True(options.AutoAdvanceOnSuccess);
            Assert.Equal(10, options.LinearStepCount);
            Assert.Equal(TimeSpan.FromMinutes(30), options.StepSoakDuration);
            Assert.Equal(TimeSpan.FromHours(8), options.MaxDeploymentDuration);
            Assert.Equal(NotificationPriority.Low, options.AlertPriority);
        }

        [Fact]
        public void Thresholds_DefaultValues_ShouldMatchExpected()
        {
            // Arrange
            var thresholds = new CanaryThresholds();

            // Assert defaults
            Assert.Equal(1.0, thresholds.MaxErrorRatePercent);
            Assert.Equal(1_000, thresholds.MaxP95LatencyMs);
            Assert.Equal(2_000, thresholds.MaxP99LatencyMs);
            Assert.Equal(2.0, thresholds.ErrorRateMultiplier);
            Assert.Equal(20.0, thresholds.LatencyDegradationPercent);
        }

        [Fact]
        public void CanModifyThresholds_ShouldPersist()
        {
            // Arrange
            var thresholds = new CanaryThresholds
            {
                MaxErrorRatePercent = 5.5,
                MaxP95LatencyMs = 1500,
                MaxP99LatencyMs = 2500,
                ErrorRateMultiplier = 3.0,
                LatencyDegradationPercent = 30.0
            };

            // Assert that the set values are retained
            Assert.Equal(5.5, thresholds.MaxErrorRatePercent);
            Assert.Equal(1500, thresholds.MaxP95LatencyMs);
            Assert.Equal(2500, thresholds.MaxP99LatencyMs);
            Assert.Equal(3.0, thresholds.ErrorRateMultiplier);
            Assert.Equal(30.0, thresholds.LatencyDegradationPercent);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(20)]
        public void LinearStepCount_BoundaryValues_ShouldPersist(int stepCount)
        {
            // Arrange
            var options = new CanaryOptions { LinearStepCount = stepCount };

            // Assert
            Assert.Equal(stepCount, options.LinearStepCount);
        }
    }
}
