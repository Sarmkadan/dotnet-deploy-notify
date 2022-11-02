#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Services;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Provides validation helpers for <see cref="NotificationTests"/> instances.
/// Validates the test class structure and its test scenarios.
/// </summary>
public static class NotificationTestsValidation
{
    /// <summary>
    /// Validates a NotificationTests instance and returns a list of validation problems.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this NotificationTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate private fields that store test results
        var builderField = value.GetType().GetField("_notificationBuilder_WithAllRequiredFields_BuildsValidNotification",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (builderField?.GetValue(value) is not DeploymentNotification)
        {
            problems.Add("NotificationTests._notificationBuilder_WithAllRequiredFields_BuildsValidNotification must not be null.");
        }

        var failureField = value.GetType().GetField("_notificationBuilder_AsFailure_SetsCriticalPriorityAndFailedStatus",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (failureField?.GetValue(value) is not DeploymentNotification failureNotification)
        {
            problems.Add("NotificationTests._notificationBuilder_AsFailure_SetsCriticalPriorityAndFailedStatus must not be null.");
        }
        else
        {
            // Validate the failure notification has correct properties
            if (failureNotification.Status != BuildStatus.Failed)
            {
                problems.Add("NotificationTests._notificationBuilder_AsFailure_SetsCriticalPriorityAndFailedStatus.Status should be BuildStatus.Failed");
            }
            if (failureNotification.Priority != NotificationPriority.Critical)
            {
                problems.Add("NotificationTests._notificationBuilder_AsFailure_SetsCriticalPriorityAndFailedStatus.Priority should be NotificationPriority.Critical");
            }
        }

        var successField = value.GetType().GetField("_notificationBuilder_AsDeploymentSuccess_SetsHighPriorityAndCorrectStatus",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (successField?.GetValue(value) is not DeploymentNotification successNotification)
        {
            problems.Add("NotificationTests._notificationBuilder_AsDeploymentSuccess_SetsHighPriorityAndCorrectStatus must not be null.");
        }
        else
        {
            // Validate the success notification has correct properties
            if (successNotification.Status != BuildStatus.DeploymentSuccess)
            {
                problems.Add("NotificationTests._notificationBuilder_AsDeploymentSuccess_SetsHighPriorityAndCorrectStatus.Status should be BuildStatus.DeploymentSuccess");
            }
            if (successNotification.Priority != NotificationPriority.High)
            {
                problems.Add("NotificationTests._notificationBuilder_AsDeploymentSuccess_SetsHighPriorityAndCorrectStatus.Priority should be NotificationPriority.High");
            }
        }

        var missingProjectField = value.GetType().GetField("_notificationBuilder_Build_WithMissingProjectName_ThrowsInvalidOperationException",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (missingProjectField?.GetValue(value) is not InvalidOperationException)
        {
            problems.Add("NotificationTests._notificationBuilder_Build_WithMissingProjectName_ThrowsInvalidOperationException must not be null.");
        }

        var noChannelsField = value.GetType().GetField("_notificationBuilder_Build_WithNoChannels_ThrowsInvalidOperationException",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (noChannelsField?.GetValue(value) is not InvalidOperationException)
        {
            problems.Add("NotificationTests._notificationBuilder_Build_WithNoChannels_ThrowsInvalidOperationException must not be null.");
        }

        var mockField = value.GetType().GetField("_iValidationService_WhenMocked_ReturnsConfiguredErrorsAndIsVerifiable",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (mockField?.GetValue(value) is not ValidationResult mockResult)
        {
            problems.Add("NotificationTests._iValidationService_WhenMocked_ReturnsConfiguredErrorsAndIsVerifiable must not be null.");
        }
        else
        {
            // Validate the mock validation result
            if (mockResult.IsValid)
            {
                problems.Add("NotificationTests._iValidationService_WhenMocked_ReturnsConfiguredErrorsAndIsVerifiable.IsValid should be false");
            }
            if (mockResult.Errors.Count != 2)
            {
                problems.Add($"NotificationTests._iValidationService_WhenMocked_ReturnsConfiguredErrorsAndIsVerifiable.Errors.Count should be 2, was {mockResult.Errors.Count}");
            }
        }

        var disabledChannelField = value.GetType().GetField("_channelConfiguration_ShouldSendNotification_WhenChannelDisabled_ReturnsFalse",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (disabledChannelField?.GetValue(value) is not bool disabledChannelResult)
        {
            problems.Add("NotificationTests._channelConfiguration_ShouldSendNotification_WhenChannelDisabled_ReturnsFalse must not be null.");
        }
        else if (disabledChannelResult != false)
        {
            problems.Add("NotificationTests._channelConfiguration_ShouldSendNotification_WhenChannelDisabled_ReturnsFalse should be false");
        }

        var priorityChannelField = value.GetType().GetField("_channelConfiguration_ShouldSendNotification_WhenPriorityBelowMinimum_ReturnsFalse",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (priorityChannelField?.GetValue(value) is not bool priorityChannelResult)
        {
            problems.Add("NotificationTests._channelConfiguration_ShouldSendNotification_WhenPriorityBelowMinimum_ReturnsFalse must not be null.");
        }
        else if (priorityChannelResult != false)
        {
            problems.Add("NotificationTests._channelConfiguration_ShouldSendNotification_WhenPriorityBelowMinimum_ReturnsFalse should be false");
        }

        var successResultField = value.GetType().GetField("_notificationResult_MarkAsSuccessful_SetsDeliveredStatusAndClearsError",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (successResultField?.GetValue(value) is not NotificationResult successResult)
        {
            problems.Add("NotificationTests._notificationResult_MarkAsSuccessful_SetsDeliveredStatusAndClearsError must not be null.");
        }
        else
        {
            // Validate the success result
            if (successResult.Status != DeliveryStatus.Delivered)
            {
                problems.Add("NotificationTests._notificationResult_MarkAsSuccessful_SetsDeliveredStatusAndClearsError.Status should be DeliveryStatus.Delivered");
            }
            if (successResult.ErrorMessage != null)
            {
                problems.Add("NotificationTests._notificationResult_MarkAsSuccessful_SetsDeliveredStatusAndClearsError.ErrorMessage should be null");
            }
            if (successResult.IsSuccessful != true)
            {
                problems.Add("NotificationTests._notificationResult_MarkAsSuccessful_SetsDeliveredStatusAndClearsError.IsSuccessful should be true");
            }
        }

        var failedResultField = value.GetType().GetField("_notificationResult_MarkAsFailed_SetsFailedStatusWithErrorDetails",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (failedResultField?.GetValue(value) is not NotificationResult failedResult)
        {
            problems.Add("NotificationTests._notificationResult_MarkAsFailed_SetsFailedStatusWithErrorDetails must not be null.");
        }
        else
        {
            // Validate the failed result
            if (failedResult.Status != DeliveryStatus.Failed)
            {
                problems.Add("NotificationTests._notificationResult_MarkAsFailed_SetsFailedStatusWithErrorDetails.Status should be DeliveryStatus.Failed");
            }
            if (string.IsNullOrWhiteSpace(failedResult.ErrorMessage))
            {
                problems.Add("NotificationTests._notificationResult_MarkAsFailed_SetsFailedStatusWithErrorDetails.ErrorMessage should not be null or empty");
            }
            if (failedResult.IsSuccessful)
            {
                problems.Add("NotificationTests._notificationResult_MarkAsFailed_SetsFailedStatusWithErrorDetails.IsSuccessful should be false");
            }
        }

        var metadataField = value.GetType().GetField("_deploymentNotification_SetAndGetMetadata_RoundTripsTypedValues",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (metadataField?.GetValue(value) is not bool metadataResult)
        {
            problems.Add("NotificationTests._deploymentNotification_SetAndGetMetadata_RoundTripsTypedValues must not be null.");
        }
        else if (metadataResult != true)
        {
            problems.Add("NotificationTests._deploymentNotification_SetAndGetMetadata_RoundTripsTypedValues should be true");
        }

        var incrementField = value.GetType().GetField("_deploymentNotification_IncrementDeliveryAttempt_IncrementsCounterEachCall",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (incrementField?.GetValue(value) is not bool incrementResult)
        {
            problems.Add("NotificationTests._deploymentNotification_IncrementDeliveryAttempt_IncrementsCounterEachCall must not be null.");
        }
        else if (incrementResult != true)
        {
            problems.Add("NotificationTests._deploymentNotification_IncrementDeliveryAttempt_IncrementsCounterEachCall should be true");
        }

        var processedField = value.GetType().GetField("_deploymentNotification_MarkAsProcessed_SetsIsProcessedTrue",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (processedField?.GetValue(value) is not bool processedResult)
        {
            problems.Add("NotificationTests._deploymentNotification_MarkAsProcessed_SetsIsProcessedTrue must not be null.");
        }
        else if (processedResult != true)
        {
            problems.Add("NotificationTests._deploymentNotification_MarkAsProcessed_SetsIsProcessedTrue should be true");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a NotificationTests instance is valid.
    /// </summary>
    /// <param name="value">The test instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this NotificationTests value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that a NotificationTests instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the test instance is invalid.</exception>
    public static void EnsureValid(this NotificationTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"NotificationTests is invalid:{System.Environment.NewLine} - {string.Join($"{System.Environment.NewLine} - ", problems)}");
        }
    }
}