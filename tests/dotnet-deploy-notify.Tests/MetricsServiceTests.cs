#nullable enable
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class MetricsServiceTests
{
    private readonly MetricsService _metricsService;
    private readonly ILogger<MetricsService> _mockLogger;

    public MetricsServiceTests()
    {
        _mockLogger = Substitute.For<ILogger<MetricsService>>();
        _metricsService = new MetricsService(_mockLogger);
    }

    #region RecordNotificationCreated Tests

    [Fact]
    public void RecordNotificationCreated_IncrementsNotificationCount()
    {
        // Act
        _metricsService.RecordNotificationCreated();
        _metricsService.RecordNotificationCreated();
        var metrics = _metricsService.GetMetricsAsync().Result;

        // Assert
        metrics.NotificationsCreated.Should().Be(2);
    }

    [Fact]
    public void RecordNotificationCreated_WithMultipleCalls_CountsAccurately()
    {
        // Act
        for (int i = 0; i < 10; i++)
        {
            _metricsService.RecordNotificationCreated();
        }
        var metrics = _metricsService.GetMetricsAsync().Result;

        // Assert
        metrics.NotificationsCreated.Should().Be(10);
    }

    #endregion

    #region RecordDeliveryAttempt Tests

    [Fact]
    public void RecordDeliveryAttempt_WithSuccessfulDelivery_IncrementsSuccessCount()
    {
        // Act
        _metricsService.RecordDeliveryAttempt(NotificationChannel.Slack, success: true, durationMs: 100);
        _metricsService.RecordDeliveryAttempt(NotificationChannel.Slack, success: true, durationMs: 150);
        var metrics = _metricsService.GetMetricsAsync().Result;

        // Assert
        metrics.SuccessfulDeliveries.Should().Be(2);
        metrics.DeliveryAttempts.Should().Be(2);
    }

    [Fact]
    public void RecordDeliveryAttempt_WithFailedDelivery_IncrementsFailureCount()
    {
        // Act
        _metricsService.RecordDeliveryAttempt(NotificationChannel.Slack, success: false, durationMs: 100);
        _metricsService.RecordDeliveryAttempt(NotificationChannel.Slack, success: false, durationMs: 150);
        var metrics = _metricsService.GetMetricsAsync().Result;

        // Assert
        metrics.FailedDeliveries.Should().Be(2);
        metrics.DeliveryAttempts.Should().Be(2);
    }

    [Fact]
    public void RecordDeliveryAttempt_WithMixedResults_CountsBothSuccessAndFailure()
    {
        // Act
        _metricsService.RecordDeliveryAttempt(NotificationChannel.Slack, success: true, durationMs: 100);
        _metricsService.RecordDeliveryAttempt(NotificationChannel.Slack, success: false, durationMs: 200);
        _metricsService.RecordDeliveryAttempt(NotificationChannel.Slack, success: true, durationMs: 150);
        var metrics = _metricsService.GetMetricsAsync().Result;

        // Assert
        metrics.SuccessfulDeliveries.Should().Be(2);
        metrics.FailedDeliveries.Should().Be(1);
        metrics.DeliveryAttempts.Should().Be(3);
    }

    [Fact]
    public void RecordDeliveryAttempt_TracksDeliveryDuration()
    {
        // Act
        _metricsService.RecordDeliveryAttempt(NotificationChannel.Slack, success: true, durationMs: 100);
        _metricsService.RecordDeliveryAttempt(NotificationChannel.Slack, success: true, durationMs: 200);
        _metricsService.RecordDeliveryAttempt(NotificationChannel.Slack, success: true, durationMs: 300);
        var metrics = _metricsService.GetMetricsAsync().Result;

        // Assert
        metrics.AverageDeliveryTimeMs.Should().Be(200); // (100 + 200 + 300) / 3
        metrics.MinDeliveryTimeMs.Should().Be(100);
        metrics.MaxDeliveryTimeMs.Should().Be(300);
    }

    [Fact]
    public void RecordDeliveryAttempt_WithDifferentChannels_TracksPerChannel()
    {
        // Act
        _metricsService.RecordDeliveryAttempt(NotificationChannel.Slack, success: true, durationMs: 100);
        _metricsService.RecordDeliveryAttempt(NotificationChannel.Discord, success: true, durationMs: 150);
        _metricsService.RecordDeliveryAttempt(NotificationChannel.Telegram, success: false, durationMs: 200);
        var metrics = _metricsService.GetMetricsAsync().Result;

        // Assert
        metrics.DeliveryAttempts.Should().Be(3);
        metrics.ChannelMetrics.Should().ContainKey(NotificationChannel.Slack);
        metrics.ChannelMetrics.Should().ContainKey(NotificationChannel.Discord);
        metrics.ChannelMetrics.Should().ContainKey(NotificationChannel.Telegram);
    }

    #endregion

    #region RecordValidationFailure Tests

    [Fact]
    public void RecordValidationFailure_IncrementsValidationFailureCount()
    {
        // Act
        _metricsService.RecordValidationFailure();
        _metricsService.RecordValidationFailure();
        var metrics = _metricsService.GetMetricsAsync().Result;

        // Assert
        metrics.ValidationFailures.Should().Be(2);
    }

    [Fact]
    public void RecordValidationFailure_WithMultipleCalls_CountsAccurately()
    {
        // Act
        for (int i = 0; i < 5; i++)
        {
            _metricsService.RecordValidationFailure();
        }
        var metrics = _metricsService.GetMetricsAsync().Result;

        // Assert
        metrics.ValidationFailures.Should().Be(5);
    }

    #endregion

    #region RecordConfigurationChange Tests

    [Fact]
    public void RecordConfigurationChange_IncrementsConfigurationChangeCount()
    {
        // Act
        _metricsService.RecordConfigurationChange();
        _metricsService.RecordConfigurationChange();
        var metrics = _metricsService.GetMetricsAsync().Result;

        // Assert
        metrics.ConfigurationChanges.Should().Be(2);
    }

    #endregion

    #region GetMetricsAsync Tests

    [Fact]
    public async Task GetMetricsAsync_WithNoActivity_ReturnsZeroMetrics()
    {
        // Act
        var metrics = await _metricsService.GetMetricsAsync();

        // Assert
        metrics.NotificationsCreated.Should().Be(0);
        metrics.DeliveryAttempts.Should().Be(0);
        metrics.SuccessfulDeliveries.Should().Be(0);
        metrics.FailedDeliveries.Should().Be(0);
    }

    [Fact]
    public async Task GetMetricsAsync_ReturnsCurrentSnapshot()
    {
        // Arrange
        _metricsService.RecordNotificationCreated();
        _metricsService.RecordDeliveryAttempt(NotificationChannel.Slack, success: true, durationMs: 100);

        // Act
        var metrics = await _metricsService.GetMetricsAsync();

        // Assert
        metrics.NotificationsCreated.Should().Be(1);
        metrics.DeliveryAttempts.Should().Be(1);
        metrics.SuccessfulDeliveries.Should().Be(1);
    }

    [Fact]
    public async Task GetMetricsAsync_HasTimestamp()
    {
        // Act
        var metrics = await _metricsService.GetMetricsAsync();

        // Assert
        metrics.Timestamp.Should().BeOnOrAfter(DateTime.UtcNow.AddSeconds(-1));
    }

    #endregion

    #region GetMetricsByPeriodAsync Tests

    [Fact]
    public async Task GetMetricsByPeriodAsync_WithPeriodIncludingActivity_ReturnsActivity()
    {
        // Arrange
        var now = DateTime.UtcNow;
        _metricsService.RecordNotificationCreated();
        _metricsService.RecordDeliveryAttempt(NotificationChannel.Slack, success: true, durationMs: 100);

        // Act
        var metrics = await _metricsService.GetMetricsByPeriodAsync(now.AddSeconds(-5), now.AddSeconds(5));

        // Assert
        metrics.NotificationsCreated.Should().Be(1);
        metrics.DeliveryAttempts.Should().Be(1);
    }

    [Fact]
    public async Task GetMetricsByPeriodAsync_WithPeriodExcludingActivity_ReturnsZero()
    {
        // Arrange
        var now = DateTime.UtcNow;
        _metricsService.RecordNotificationCreated();

        // Act
        var metrics = await _metricsService.GetMetricsByPeriodAsync(now.AddSeconds(-20), now.AddSeconds(-10));

        // Assert
        metrics.NotificationsCreated.Should().Be(0);
    }

    #endregion

    #region GetChannelMetricsAsync Tests

    [Fact]
    public async Task GetChannelMetricsAsync_WithSlackActivity_ReturnsSlackMetrics()
    {
        // Arrange
        _metricsService.RecordDeliveryAttempt(NotificationChannel.Slack, success: true, durationMs: 100);
        _metricsService.RecordDeliveryAttempt(NotificationChannel.Slack, success: true, durationMs: 200);

        // Act
        var metrics = await _metricsService.GetChannelMetricsAsync(NotificationChannel.Slack);

        // Assert
        metrics.Channel.Should().Be(NotificationChannel.Slack);
        metrics.DeliveryAttempts.Should().Be(2);
        metrics.SuccessfulDeliveries.Should().Be(2);
    }

    [Fact]
    public async Task GetChannelMetricsAsync_WithNoActivity_ReturnsEmptyMetrics()
    {
        // Act
        var metrics = await _metricsService.GetChannelMetricsAsync(NotificationChannel.Discord);

        // Assert
        metrics.Channel.Should().Be(NotificationChannel.Discord);
        metrics.DeliveryAttempts.Should().Be(0);
    }

    [Fact]
    public async Task GetChannelMetricsAsync_WithMixedSuccessAndFailure_CalculatesSuccessRate()
    {
        // Arrange
        _metricsService.RecordDeliveryAttempt(NotificationChannel.Telegram, success: true, durationMs: 100);
        _metricsService.RecordDeliveryAttempt(NotificationChannel.Telegram, success: true, durationMs: 100);
        _metricsService.RecordDeliveryAttempt(NotificationChannel.Telegram, success: false, durationMs: 100);
        _metricsService.RecordDeliveryAttempt(NotificationChannel.Telegram, success: false, durationMs: 100);

        // Act
        var metrics = await _metricsService.GetChannelMetricsAsync(NotificationChannel.Telegram);

        // Assert
        metrics.DeliveryAttempts.Should().Be(4);
        metrics.SuccessfulDeliveries.Should().Be(2);
        metrics.FailedDeliveries.Should().Be(2);
        metrics.GetSuccessRate().Should().Be(50.0);
    }

    #endregion

    #region MetricsSnapshot Tests

    [Fact]
    public void MetricsSnapshot_GetSuccessRate_WithZeroAttempts_ReturnsZero()
    {
        // Arrange
        var snapshot = new MetricsSnapshot
        {
            DeliveryAttempts = 0,
            SuccessfulDeliveries = 0
        };

        // Act
        var rate = snapshot.GetSuccessRate();

        // Assert
        rate.Should().Be(0);
    }

    [Fact]
    public void MetricsSnapshot_GetSuccessRate_CalculatesCorrectly()
    {
        // Arrange
        var snapshot = new MetricsSnapshot
        {
            DeliveryAttempts = 10,
            SuccessfulDeliveries = 8
        };

        // Act
        var rate = snapshot.GetSuccessRate();

        // Assert
        rate.Should().Be(80.0);
    }

    [Fact]
    public void MetricsSnapshot_GetFailureRate_CalculatesCorrectly()
    {
        // Arrange
        var snapshot = new MetricsSnapshot
        {
            DeliveryAttempts = 10,
            FailedDeliveries = 2
        };

        // Act
        var rate = snapshot.GetFailureRate();

        // Assert
        rate.Should().Be(20.0);
    }

    #endregion

    #region ChannelMetrics Tests

    [Fact]
    public void ChannelMetrics_GetSuccessRate_CalculatesCorrectly()
    {
        // Arrange
        var metrics = new ChannelMetrics
        {
            DeliveryAttempts = 5,
            SuccessfulDeliveries = 4
        };

        // Act
        var rate = metrics.GetSuccessRate();

        // Assert
        rate.Should().Be(80.0);
    }

    [Fact]
    public void ChannelMetrics_GetSummary_ReturnsFormattedString()
    {
        // Arrange
        var metrics = new ChannelMetrics
        {
            Channel = NotificationChannel.Slack,
            DeliveryAttempts = 10,
            SuccessfulDeliveries = 9,
            FailedDeliveries = 1,
            AverageDeliveryTimeMs = 150,
            LastDeliveryAt = DateTime.UtcNow
        };

        // Act
        var summary = metrics.GetSummary();

        // Assert
        summary.Should().Contain("Slack");
        summary.Should().Contain("9/10");
        summary.Should().Contain("150ms");
    }

    #endregion

    #region Concurrency Tests

    [Fact]
    public async Task RecordDeliveryAttempt_WithConcurrentCalls_HandlesThreadSafety()
    {
        // Act
        var tasks = new List<Task>();
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                _metricsService.RecordDeliveryAttempt(NotificationChannel.Slack, success: true, durationMs: 100);
            }));
        }
        await Task.WhenAll(tasks);

        var metrics = await _metricsService.GetMetricsAsync();

        // Assert
        metrics.DeliveryAttempts.Should().Be(100);
    }

    [Fact]
    public async Task GetMetricsAsync_WhileRecordingMetrics_ReturnsConsistentData()
    {
        // Act
        var recordingTasks = new List<Task>();
        for (int i = 0; i < 50; i++)
        {
            recordingTasks.Add(Task.Run(() =>
            {
                _metricsService.RecordNotificationCreated();
                _metricsService.RecordDeliveryAttempt(NotificationChannel.Slack, success: true, durationMs: 100);
            }));
        }

        var metricsTask = _metricsService.GetMetricsAsync();
        await Task.WhenAll(recordingTasks.Append(metricsTask));

        // Assert
        metricsTask.Result.NotificationsCreated.Should().Be(50);
    }

    #endregion
}
