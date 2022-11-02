#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Provides validation helpers for <see cref="NotificationProcessorTests"/> to ensure test methods
/// and class structure follow expected patterns and constraints.
/// </summary>
public static class NotificationProcessorTestsValidation
{
    /// <summary>
    /// Validates that a <see cref="NotificationProcessorTests"/> instance follows expected patterns.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this NotificationProcessorTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate that all test methods exist and follow expected patterns
        ValidateTestMethods(value, problems);

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="NotificationProcessorTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this NotificationProcessorTests value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="NotificationProcessorTests"/> instance is valid,
    /// throwing an <see cref="ArgumentException"/> with detailed validation messages if not.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid.</exception>
    public static void EnsureValid(this NotificationProcessorTests value)
    {
        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"NotificationProcessorTests instance is not valid. Problems:\n{string.Join("\n", problems.Select((p, i) => $"  {i + 1}. {p}"))}");
        }
    }

    private static void ValidateTestMethods(NotificationProcessorTests value, List<string> problems)
    {
        var testMethods = new Dictionary<string, string>
        {
            [nameof(NotificationProcessorTests.ProcessBatchAsync_WithSuccessfulDeliveries_ReturnsSuccessResult)] =
                "ProcessBatchAsync with successful deliveries should return correct success metrics",
            [nameof(NotificationProcessorTests.ProcessBatchAsync_WithMixedResults_CountsCorrectly)] =
                "ProcessBatchAsync with mixed results should count correctly",
            [nameof(NotificationProcessorTests.ProcessBatchAsync_WithEmptyResults_ReturnsZeroMetrics)] =
                "ProcessBatchAsync with empty results should return zero metrics",
            [nameof(NotificationProcessorTests.ProcessBatchAsync_MeasuresDuration)] =
                "ProcessBatchAsync should measure duration",
            [nameof(NotificationProcessorTests.ProcessBatchAsync_WhenExceptionThrown_CatchesAndReturnsError)] =
                "ProcessBatchAsync when exception thrown should catch and return error",
            [nameof(NotificationProcessorTests.ProcessBatchAsync_CalculatesSuccessRate)] =
                "ProcessBatchAsync should calculate success rate",
            [nameof(NotificationProcessorTests.ProcessFailedAsync_WithFailedResults_RetriesNotifications)] =
                "ProcessFailedAsync with failed results should retry notifications",
            [nameof(NotificationProcessorTests.ProcessFailedAsync_RespectMaxRetries_SkipsExceededRetries)] =
                "ProcessFailedAsync should respect max retries and skip exceeded retries",
            [nameof(NotificationProcessorTests.ProcessFailedAsync_WithNoFailedResults_ReturnsZeroMetrics)] =
                "ProcessFailedAsync with no failed results should return zero metrics",
            [nameof(NotificationProcessorTests.ProcessFailedAsync_WhenExceptionOccurs_ContinuesProcessing)] =
                "ProcessFailedAsync when exception occurs should continue processing",
            [nameof(NotificationProcessorTests.ProcessByPriorityAsync_ProcessesCriticalFirst)] =
                "ProcessByPriorityAsync should process critical first",
            [nameof(NotificationProcessorTests.ProcessByPriorityAsync_AggregatesResultsAcrossPriorities)] =
                "ProcessByPriorityAsync should aggregate results across priorities",
            [nameof(NotificationProcessorTests.ProcessByPriorityAsync_WhenExceptionThrown_ReturnsError)] =
                "ProcessByPriorityAsync when exception thrown should return error",
            [nameof(NotificationProcessorTests.GetStatisticsAsync_AggregatesMetricsCorrectly)] =
                "GetStatisticsAsync should aggregate metrics correctly",
            [nameof(NotificationProcessorTests.GetStatisticsAsync_CalculatesAverageDeliveryTime)] =
                "GetStatisticsAsync should calculate average delivery time",
            [nameof(NotificationProcessorTests.GetStatisticsAsync_WithEmptyResults_ReturnsZeroMetrics)] =
                "GetStatisticsAsync with empty results should return zero metrics",
            [nameof(NotificationProcessorTests.GetStatisticsAsync_WhenExceptionOccurs_ReturnsEmptyStats)] =
                "GetStatisticsAsync when exception occurs should return empty stats",
            [nameof(NotificationProcessorTests.ProcessingResult_SuccessRate_WithZeroProcessed_ReturnsZero)] =
                "ProcessingResult success rate with zero processed should return zero",
            [nameof(NotificationProcessorTests.ProcessingResult_SuccessRate_CalculatesCorrectly)] =
                "ProcessingResult success rate should calculate correctly"
        };

        foreach (var (methodName, description) in testMethods)
        {
            try
            {
                var method = typeof(NotificationProcessorTests).GetMethod(methodName);
                if (method is null)
                {
                    problems.Add($"Test method '{methodName}' not found: {description}");
                }
                else if (!method.IsPublic)
                {
                    problems.Add($"Test method '{methodName}' is not public: {description}");
                }
                else if (method.ReturnType != typeof(void) && method.ReturnType != typeof(Task))
                {
                    problems.Add($"Test method '{methodName}' has unexpected return type '{method.ReturnType.Name}': {description}");
                }
            }
            catch (Exception ex)
            {
                problems.Add($"Error checking test method '{methodName}': {ex.Message}");
            }
        }
    }
}