# NotificationProcessorTests

Unit test suite for the `NotificationProcessor` class, verifying batch processing, retry logic, priority handling, and metrics aggregation functionality. Tests cover success/failure scenarios, performance measurement, and edge cases for notification delivery workflows.

## API

### `NotificationProcessorTests`
Test class containing all verification cases for notification processing behavior.

### `ProcessBatchAsync_WithSuccessfulDeliveries_ReturnsSuccessResult`
Verifies that when all notifications in a batch are delivered successfully, the result contains correct success metrics and zero failures.

Parameters:
- None

Return value:
- `Task`: Completes when assertions pass

Throws:
- `Xunit.AssertException` if success count or metrics are incorrect

### `ProcessBatchAsync_WithMixedResults_CountsCorrectly`
Ensures that a batch with a mix of successful and failed deliveries reports accurate success, failure, and retry counts.

Parameters:
- None

Return value:
- `Task`: Completes when assertions pass

Throws:
- `Xunit.AssertException` if aggregated counts do not match expected values

### `ProcessBatchAsync_WithEmptyResults_ReturnsZeroMetrics`
Confirms that processing an empty batch returns zero for all metrics and does not throw.

Parameters:
- None

Return value:
- `Task`: Completes when assertions pass

Throws:
- `Xunit.AssertException` if any metric is non-zero

### `ProcessBatchAsync_MeasuresDuration`
Validates that the processing duration is recorded and falls within a reasonable range for a batch of notifications.

Parameters:
- None

Return value:
- `Task`: Completes when assertions pass

Throws:
- `Xunit.AssertException` if duration is missing, negative, or exceeds timeout threshold

### `ProcessBatchAsync_WhenExceptionThrown_CatchesAndReturnsError`
Ensures that exceptions during batch processing are caught and returned as error results without crashing the test.

Parameters:
- None

Return value:
- `Task`: Completes when error result is validated

Throws:
- `Xunit.AssertException` if exception is not properly caught or result does not indicate failure

### `ProcessBatchAsync_CalculatesSuccessRate`
Checks that the success rate is computed correctly as the ratio of successful deliveries to total processed.

Parameters:
- None

Return value:
- `Task`: Completes when success rate assertion passes

Throws:
- `Xunit.AssertException` if success rate is not within expected tolerance

### `ProcessFailedAsync_WithFailedResults_RetriesNotifications`
Confirms that failed notifications are retried and tracked in the retry queue.

Parameters:
- None

Return value:
- `Task`: Completes when retry logic is verified

Throws:
- `Xunit.AssertException` if retry count or status is incorrect

### `ProcessFailedAsync_RespectMaxRetries_SkipsExceededRetries`
Validates that notifications exceeding the maximum retry count are skipped and not reprocessed.

Parameters:
- None

Return value:
- `Task`: Completes when skipped notifications are confirmed

Throws:
- `Xunit.AssertException` if skipped notifications are still processed or retried

### `ProcessFailedAsync_WithNoFailedResults_ReturnsZeroMetrics`
Ensures that when no failures exist, the retry processor returns zero metrics and does not throw.

Parameters:
- None

Return value:
- `Task`: Completes when zero metrics are confirmed

Throws:
- `Xunit.AssertException` if any metric is non-zero

### `ProcessFailedAsync_WhenExceptionOccurs_ContinuesProcessing`
Verifies that an exception during retry processing does not halt the processing of remaining failed notifications.

Parameters:
- None

Return value:
- `Task`: Completes when remaining notifications are processed

Throws:
- `Xunit.AssertException` if processing stops prematurely

### `ProcessByPriorityAsync_ProcessesCriticalFirst`
Checks that notifications are processed in priority order, with critical-level notifications handled before others.

Parameters:
- None

Return value:
- `Task`: Completes when order is verified

Throws:
- `Xunit.AssertException` if critical notifications are not processed first

### `ProcessByPriorityAsync_AggregatesResultsAcrossPriorities`
Ensures that results from all priority levels are correctly aggregated into a single metrics object.

Parameters:
- None

Return value:
- `Task`: Completes when aggregation is validated

Throws:
- `Xunit.AssertException` if aggregated totals are incorrect

### `ProcessByPriorityAsync_WhenExceptionThrown_ReturnsError`
Validates that exceptions during priority-based processing are caught and returned as error results.

Parameters:
- None

Return value:
- `Task`: Completes when error result is validated

Throws:
- `Xunit.AssertException` if exception is not caught or result does not indicate failure

### `GetStatisticsAsync_AggregatesMetricsCorrectly`
Confirms that statistics across multiple batches are correctly aggregated over time.

Parameters:
- None

Return value:
- `Task`: Completes when aggregation is verified

Throws:
- `Xunit.AssertException` if aggregated values are incorrect

### `GetStatisticsAsync_CalculatesAverageDeliveryTime`
Ensures that average delivery time is computed correctly from recorded delivery timestamps.

Parameters:
- None

Return value:
- `Task`: Completes when average is validated

Throws:
- `Xunit.AssertException` if average is missing or outside expected range

### `GetStatisticsAsync_WithEmptyResults_ReturnsZeroMetrics`
Verifies that requesting statistics with no data returns zero for all metrics.

Parameters:
- None

Return value:
- `Task`: Completes when zero metrics are confirmed

Throws:
- `Xunit.AssertException` if any metric is non-zero

### `GetStatisticsAsync_WhenExceptionOccurs_ReturnsEmptyStats`
Checks that exceptions during statistics retrieval return an empty statistics object rather than throwing.

Parameters:
- None

Return value:
- `Task`: Completes when empty stats are confirmed

Throws:
- `Xunit.AssertException` if empty result is not returned

### `ProcessingResult_SuccessRate_WithZeroProcessed_ReturnsZero`
Unit test for the `ProcessingResult.SuccessRate` property, confirming it returns zero when no notifications were processed.

Parameters:
- None

Return value:
- `void`

Throws:
- `Xunit.AssertException` if success rate is not zero

### `ProcessingResult_SuccessRate_CalculatesCorrectly`
Unit test for the `ProcessingResult.SuccessRate` property, verifying correct calculation based on success and failure counts.

Parameters:
- None

Return value:
- `void`

Throws:
- `Xunit.AssertException` if success rate is incorrect

## Usage
