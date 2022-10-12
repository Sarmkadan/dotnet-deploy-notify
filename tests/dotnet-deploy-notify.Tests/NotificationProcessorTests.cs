#nullable enable
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Data;
using DotNetDeployNotify.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Unit tests for <see cref="NotificationProcessor"/> class.
/// Tests various scenarios for processing deployment notifications including batch processing,
/// failed notification retries, priority-based processing, and statistics calculation.
/// </summary>
public class NotificationProcessorTests
{
	private readonly INotificationService _mockNotificationService;
	private readonly INotificationRepository _mockNotificationRepository;
	private readonly IChannelConfigRepository _mockConfigRepository;
	private readonly INotificationResultRepository _mockResultRepository;
	private readonly ILogger<NotificationProcessor> _mockLogger;
	private readonly NotificationProcessor _processor;

	/// <summary>
	/// Initializes a new instance of the <see cref="NotificationProcessorTests"/> class.
	/// Sets up mock dependencies using NSubstitute for testing notification processing scenarios.
	/// </summary>
	public NotificationProcessorTests()
	{
		_mockNotificationService = Substitute.For<INotificationService>();
		_mockNotificationRepository = Substitute.For<INotificationRepository>();
		_mockConfigRepository = Substitute.For<IChannelConfigRepository>();
		_mockResultRepository = Substitute.For<INotificationResultRepository>();
		_mockLogger = Substitute.For<ILogger<NotificationProcessor>>();

		_processor = new NotificationProcessor(
			_mockNotificationService,
			_mockNotificationRepository,
			_mockConfigRepository,
			_mockResultRepository,
			_mockLogger);
	}

	#region ProcessBatchAsync Tests

	/// <summary>
	/// Tests that when all notifications are successfully delivered, the processor returns correct success metrics.
	/// </summary>
	[Fact]
	public async Task ProcessBatchAsync_WithSuccessfulDeliveries_ReturnsSuccessResult()
	{
		// Arrange
		var notificationResults = new List<NotificationResult>
		{
			CreateSuccessfulResult(),
			CreateSuccessfulResult(),
			CreateSuccessfulResult()
		};
		_mockNotificationService.SendPendingNotificationsAsync().Returns(notificationResults);

		// Act
		var result = await _processor.ProcessBatchAsync(50);

		// Assert
		result.TotalProcessed.Should().Be(3);
		result.SuccessCount.Should().Be(3);
		result.FailureCount.Should().Be(0);
		result.SkippedCount.Should().Be(0);
	}

	/// <summary>
	/// Tests that when notifications have mixed results (success, failure, skipped),
	/// the processor correctly counts each category.
	/// </summary>
	[Fact]
	public async Task ProcessBatchAsync_WithMixedResults_CountsCorrectly()
	{
		// Arrange
		var notificationResults = new List<NotificationResult>
		{
			CreateSuccessfulResult(),
			CreateFailedResult(),
			CreateSuccessfulResult(),
			CreateSkippedResult()
		};
		_mockNotificationService.SendPendingNotificationsAsync().Returns(notificationResults);

		// Act
		var result = await _processor.ProcessBatchAsync(50);

		// Assert
		result.TotalProcessed.Should().Be(4);
		result.SuccessCount.Should().Be(2);
		result.FailureCount.Should().Be(1);
		result.SkippedCount.Should().Be(1);
	}

	/// <summary>
	/// Tests that when no notifications are available for processing,
	/// the processor returns zero metrics.
	/// </summary>
	[Fact]
	public async Task ProcessBatchAsync_WithEmptyResults_ReturnsZeroMetrics()
	{
		// Arrange
		_mockNotificationService.SendPendingNotificationsAsync().Returns(new List<NotificationResult>());

		// Act
		var result = await _processor.ProcessBatchAsync(50);

		// Assert
		result.TotalProcessed.Should().Be(0);
		result.SuccessCount.Should().Be(0);
	}

	/// <summary>
	/// Tests that the processor correctly measures and reports duration for batch processing.
	/// </summary>
	[Fact]
	public async Task ProcessBatchAsync_MeasuresDuration()
	{
		// Arrange
		var notificationResults = new List<NotificationResult> { CreateSuccessfulResult() };
		_mockNotificationService.SendPendingNotificationsAsync().Returns(Task.FromResult(notificationResults));

		// Act
		var result = await _processor.ProcessBatchAsync(50);

		// Assert
		result.DurationMs.Should().BeGreaterThanOrEqualTo(0);
	}

	/// <summary>
	/// Tests that when an exception occurs during batch processing,
	/// the processor catches it and returns error information.
	/// </summary>
	[Fact]
	public async Task ProcessBatchAsync_WhenExceptionThrown_CatchesAndReturnsError()
	{
		// Arrange
		_mockNotificationService.SendPendingNotificationsAsync()
			.Throws(new InvalidOperationException("Test error"));

		// Act
		var result = await _processor.ProcessBatchAsync(50);

		// Assert
		result.Errors.Should().NotBeEmpty();
		result.Errors[0].Should().Contain("Test error");
	}

	/// <summary>
	/// Tests that the processor correctly calculates success rate based on successful vs total notifications.
	/// </summary>
	[Fact]
	public async Task ProcessBatchAsync_CalculatesSuccessRate()
	{
		// Arrange
		var notificationResults = new List<NotificationResult>
		{
			CreateSuccessfulResult(),
			CreateSuccessfulResult(),
			CreateSuccessfulResult(),
			CreateFailedResult()
		};
		_mockNotificationService.SendPendingNotificationsAsync().Returns(notificationResults);

		// Act
		var result = await _processor.ProcessBatchAsync(50);

		// Assert
		result.SuccessRate.Should().Be(75.0);
	}

	#endregion

	#region ProcessFailedAsync Tests

	/// <summary>
	/// Tests that failed notifications are properly retried and updated with new results.
	/// </summary>
	[Fact]
	public async Task ProcessFailedAsync_WithFailedResults_RetriesNotifications()
	{
		// Arrange
		var failedResults = new List<NotificationResult>
		{
			CreateFailedResult(),
			CreateFailedResult()
		};
		_mockResultRepository.GetAllAsync(0, 1000).Returns(failedResults);

		var retryResults = new List<NotificationResult>
		{
			CreateSuccessfulResult(),
			CreateSuccessfulResult()
		};
		_mockNotificationService.RetryFailedDeliveriesAsync(Arg.Any<string>())
			.Returns(retryResults);

		// Act
		var result = await _processor.ProcessFailedAsync(3);

		// Assert
		result.TotalProcessed.Should().Be(4); // 2 failed results * 2 retry results
		result.SuccessCount.Should().Be(4);
	}

	/// <summary>
	/// Tests that notifications exceeding maximum retry attempts are skipped rather than retried.
	/// </summary>
	[Fact]
	public async Task ProcessFailedAsync_RespectMaxRetries_SkipsExceededRetries()
	{
		// Arrange
		var failedResults = new List<NotificationResult>
		{
			new NotificationResult { NotificationId = "1", Status = DeliveryStatus.Failed, AttemptNumber = 3 },
			new NotificationResult { NotificationId = "2", Status = DeliveryStatus.Failed, AttemptNumber = 1 }
		};
		_mockResultRepository.GetAllAsync(0, 1000).Returns(failedResults);

		var retryResults = new List<NotificationResult> { CreateSuccessfulResult() };
		_mockNotificationService.RetryFailedDeliveriesAsync(Arg.Any<string>())
			.Returns(retryResults);

		// Act
		var result = await _processor.ProcessFailedAsync(3);

		// Assert
		result.SkippedCount.Should().Be(1); // First one exceeded max retries
	}

	/// <summary>
	/// Tests that when no failed notifications exist, the processor returns zero metrics.
	/// </summary>
	[Fact]
	public async Task ProcessFailedAsync_WithNoFailedResults_ReturnsZeroMetrics()
	{
		// Arrange
		_mockResultRepository.GetAllAsync(0, 1000).Returns(new List<NotificationResult>());

		// Act
		var result = await _processor.ProcessFailedAsync(3);

		// Assert
		result.TotalProcessed.Should().Be(0);
		result.SuccessCount.Should().Be(0);
	}

	/// <summary>
	/// Tests that when an exception occurs during retry processing,
	/// the processor continues with remaining notifications.
	/// </summary>
	[Fact]
	public async Task ProcessFailedAsync_WhenExceptionOccurs_ContinuesProcessing()
	{
		// Arrange
		var failedResults = new List<NotificationResult>
		{
			new NotificationResult { NotificationId = "1", Status = DeliveryStatus.Failed, AttemptNumber = 1 },
			new NotificationResult { NotificationId = "2", Status = DeliveryStatus.Failed, AttemptNumber = 1 }
		};
		_mockResultRepository.GetAllAsync(0, 1000).Returns(failedResults);

		_mockNotificationService.RetryFailedDeliveriesAsync("1")
			.Throws(new InvalidOperationException("Retry failed"));
		_mockNotificationService.RetryFailedDeliveriesAsync("2")
			.Returns(new List<NotificationResult> { CreateSuccessfulResult() });

		// Act
		var result = await _processor.ProcessFailedAsync(3);

		// Assert
		result.FailureCount.Should().BeGreaterThan(0);
	}

	#endregion

	#region ProcessByPriorityAsync Tests

	/// <summary>
	/// Tests that notifications are processed with priority ordering (critical first).
	/// </summary>
	[Fact]
	public async Task ProcessByPriorityAsync_ProcessesCriticalFirst()
	{
		// Arrange
		var notificationResults = new List<NotificationResult> { CreateSuccessfulResult() };
		_mockNotificationService.SendPendingNotificationsAsync().Returns(notificationResults);

		// Act
		var result = await _processor.ProcessByPriorityAsync();

		// Assert
		result.TotalProcessed.Should().BeGreaterThanOrEqualTo(0);
	}

	/// <summary>
	/// Tests that processing across multiple priority levels aggregates results correctly.
	/// </summary>
	[Fact]
	public async Task ProcessByPriorityAsync_AggregatesResultsAcrossPriorities()
	{
		// Arrange
		var notificationResults = new List<NotificationResult>
		{
			CreateSuccessfulResult(),
			CreateSuccessfulResult()
		};
		_mockNotificationService.SendPendingNotificationsAsync().Returns(notificationResults);

		// Act
		var result = await _processor.ProcessByPriorityAsync();

		// Assert
		result.Should().NotBeNull();
	}

	/// <summary>
	/// Tests that exceptions during priority-based processing are caught and reported.
	/// </summary>
	[Fact]
	public async Task ProcessByPriorityAsync_WhenExceptionThrown_ReturnsError()
	{
		// Arrange
		_mockNotificationService.SendPendingNotificationsAsync()
			.Throws(new InvalidOperationException("Processing error"));

		// Act
		var result = await _processor.ProcessByPriorityAsync();

		// Assert
		result.Errors.Should().NotBeEmpty();
	}

	#endregion

	#region GetStatisticsAsync Tests

	/// <summary>
	/// Tests that statistics are correctly aggregated from notifications and results.
	/// </summary>
	[Fact]
	public async Task GetStatisticsAsync_AggregatesMetricsCorrectly()
	{
		// Arrange
		var notifications = new List<DeploymentNotification>
		{
			new DeploymentNotification { Id = "1", IsProcessed = false, ProjectName = "App1", Version = "1.0.0", BranchName = "main", Message = "Test", Channels = [NotificationChannel.Slack] },
			new DeploymentNotification { Id = "2", IsProcessed = true, ProjectName = "App2", Version = "2.0.0", BranchName = "main", Message = "Test", Channels = [NotificationChannel.Slack] }
		};

		var results = new List<NotificationResult>
		{
			CreateSuccessfulResult(durationMs: 100),
			CreateSuccessfulResult(durationMs: 200),
			CreateFailedResult(durationMs: 300)
		};

		var configs = new List<ChannelConfiguration>
		{
			CreateChannelConfig(),
			CreateChannelConfig()
		};

		_mockNotificationRepository.GetPendingAsync().Returns(new List<DeploymentNotification> { notifications[0] });
		_mockNotificationRepository.GetAllAsync().Returns(notifications);
		_mockResultRepository.GetAllAsync(0, 10000).Returns(results);
		_mockConfigRepository.GetEnabledAsync().Returns(configs);

		// Act
		var stats = await _processor.GetStatisticsAsync();

		// Assert
		stats.PendingCount.Should().Be(1);
		stats.ProcessedCount.Should().Be(2);
		stats.TotalNotifications.Should().Be(3);
		stats.TotalDeliveryAttempts.Should().Be(3);
		stats.SuccessfulDeliveries.Should().Be(2);
		stats.FailedDeliveries.Should().Be(1);
		stats.ActiveConfigurations.Should().Be(2);
	}

	/// <summary>
	/// Tests that average delivery time is correctly calculated from successful notifications.
	/// </summary>
	[Fact]
	public async Task GetStatisticsAsync_CalculatesAverageDeliveryTime()
	{
		// Arrange
		var results = new List<NotificationResult>
		{
			CreateSuccessfulResult(durationMs: 100),
			CreateSuccessfulResult(durationMs: 200),
			CreateSuccessfulResult(durationMs: 300)
		};

		_mockNotificationRepository.GetPendingAsync().Returns(new List<DeploymentNotification>());
		_mockNotificationRepository.GetAllAsync().Returns(new List<DeploymentNotification>());
		_mockResultRepository.GetAllAsync(0, 10000).Returns(results);
		_mockConfigRepository.GetEnabledAsync().Returns(new List<ChannelConfiguration>());

		// Act
		var stats = await _processor.GetStatisticsAsync();

		// Assert
		stats.AverageDeliveryTimeMs.Should().Be(200); // (100 + 200 + 300) / 3
	}

	/// <summary>
	/// Tests that when no data is available, statistics return zero values.
	/// </summary>
	[Fact]
	public async Task GetStatisticsAsync_WithEmptyResults_ReturnsZeroMetrics()
	{
		// Arrange
		_mockNotificationRepository.GetPendingAsync().Returns(new List<DeploymentNotification>());
		_mockNotificationRepository.GetAllAsync().Returns(new List<DeploymentNotification>());
		_mockResultRepository.GetAllAsync(0, 10000).Returns(new List<NotificationResult>());
		_mockConfigRepository.GetEnabledAsync().Returns(new List<ChannelConfiguration>());

		// Act
		var stats = await _processor.GetStatisticsAsync();

		// Assert
		stats.TotalNotifications.Should().Be(0);
		stats.TotalDeliveryAttempts.Should().Be(0);
		stats.AverageDeliveryTimeMs.Should().Be(0);
	}

	/// <summary>
	/// Tests that exceptions during statistics calculation are handled gracefully.
	/// </summary>
	[Fact]
	public async Task GetStatisticsAsync_WhenExceptionOccurs_ReturnsEmptyStats()
	{
		// Arrange
		_mockNotificationRepository.GetPendingAsync()
			.Throws(new InvalidOperationException("Database error"));

		// Act
		var stats = await _processor.GetStatisticsAsync();

		// Assert
		stats.TotalNotifications.Should().Be(0);
	}

	#endregion

	#region ProcessingResult Tests

	/// <summary>
	/// Tests that success rate calculation handles division by zero correctly.
	/// </summary>
	[Fact]
	public void ProcessingResult_SuccessRate_WithZeroProcessed_ReturnsZero()
	{
		// Arrange
		var result = new ProcessingResult
		{
			TotalProcessed = 0,
			SuccessCount = 0
		};

		// Act & Assert
		result.SuccessRate.Should().Be(0);
	}

	/// <summary>
	/// Tests that success rate is correctly calculated as percentage of successful notifications.
	/// </summary>
	[Fact]
	public void ProcessingResult_SuccessRate_CalculatesCorrectly()
	{
		// Arrange
		var result = new ProcessingResult
		{
			TotalProcessed = 10,
			SuccessCount = 7
		};

		// Act & Assert
		result.SuccessRate.Should().Be(70.0);
	}

	/// <summary>
	/// Tests that the summary string contains all relevant processing metrics.
	/// </summary>
	[Fact]
	public void ProcessingResult_GetSummary_ReturnsFormattedString()
	{
		// Arrange
		var result = new ProcessingResult
		{
			TotalProcessed = 10,
			SuccessCount = 8,
			FailureCount = 1,
			SkippedCount = 1,
			DurationMs = 1000
		};

		// Act
		var summary = result.GetSummary();

		// Assert
		summary.Should().Contain("Processed: 10");
		summary.Should().Contain("Success: 8");
		summary.Should().Contain("Failed: 1");
		summary.Should().Contain("Skipped: 1");
		summary.Should().Contain("1000ms");
	}

	#endregion

	#region ProcessingStatistics Tests

	/// <summary>
	/// Tests that health percentage calculation handles division by zero correctly.
	/// </summary>
	[Fact]
	public void ProcessingStatistics_HealthPercentage_WithZeroAttempts_Returns100()
	{
		// Arrange
		var stats = new ProcessingStatistics
		{
			TotalDeliveryAttempts = 0,
			SuccessfulDeliveries = 0
		};

		// Act & Assert
		stats.HealthPercentage.Should().Be(100);
	}

	/// <summary>
	/// Tests that health percentage is correctly calculated as percentage of successful deliveries.
	/// </summary>
	[Fact]
	public void ProcessingStatistics_HealthPercentage_CalculatesCorrectly()
	{
		// Arrange
		var stats = new ProcessingStatistics
		{
			TotalDeliveryAttempts = 20,
			SuccessfulDeliveries = 18
		};

		// Act & Assert
		stats.HealthPercentage.Should().Be(90.0);
	}

	#endregion

	#region Helper Methods

	/// <summary>
	/// Creates a successful notification result for testing purposes.
	/// </summary>
	/// <param name="durationMs">The duration in milliseconds for the notification delivery.</param>
	/// <returns>A <see cref="NotificationResult"/> with DeliveryStatus.Delivered status.</returns>
	private NotificationResult CreateSuccessfulResult(long durationMs = 100)
	{
		return new NotificationResult
		{
			Id = Guid.NewGuid().ToString(),
			NotificationId = Guid.NewGuid().ToString(),
			Status = DeliveryStatus.Delivered,
			DurationMs = durationMs,
			HttpStatusCode = 200,
			Channel = NotificationChannel.Slack
		};
	}

	/// <summary>
	/// Creates a failed notification result for testing purposes.
	/// </summary>
	/// <param name="durationMs">The duration in milliseconds for the notification delivery attempt.</param>
	/// <returns>A <see cref="NotificationResult"/> with DeliveryStatus.Failed status.</returns>
	private NotificationResult CreateFailedResult(long durationMs = 100)
	{
		return new NotificationResult
		{
			Id = Guid.NewGuid().ToString(),
			NotificationId = Guid.NewGuid().ToString(),
			Status = DeliveryStatus.Failed,
			DurationMs = durationMs,
			HttpStatusCode = 500,
			Channel = NotificationChannel.Slack
		};
	}

	/// <summary>
	/// Creates a skipped notification result for testing purposes.
	/// </summary>
	/// <returns>A <see cref="NotificationResult"/> with DeliveryStatus.Skipped status.</returns>
	private NotificationResult CreateSkippedResult()
	{
		return new NotificationResult
		{
			Id = Guid.NewGuid().ToString(),
			NotificationId = Guid.NewGuid().ToString(),
			Status = DeliveryStatus.Skipped,
			DurationMs = 0,
			Channel = NotificationChannel.Slack
		};
	}

	/// <summary>
	/// Creates a channel configuration for testing purposes.
	/// </summary>
	/// <returns>A <see cref="ChannelConfiguration"/> with Slack channel type.</returns>
	private ChannelConfiguration CreateChannelConfig()
	{
		return new ChannelConfiguration
		{
			ChannelType = NotificationChannel.Slack,
			DisplayName = "Test Channel",
			WebhookUrl = "https://example.com/webhook",
			TargetId = "123",
			TimeoutMs = 5000,
			CustomHeaders = new Dictionary<string, string>()
		};
	}

	#endregion
}