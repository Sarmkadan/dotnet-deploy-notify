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
    /// Validates a <see cref="NotificationTests"/> instance by verifying all test scenarios through reflection.
    /// This method checks that the test class has properly initialized all required test fields.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this NotificationTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        static bool ValidateField<T>(NotificationTests test, string fieldName, Func<T, bool> validator, string errorMessage)
        {
            var field = test.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field?.GetValue(test) is not T fieldValue)
            {
                return false;
            }

            return validator?.Invoke(fieldValue) ?? false;
        }

        // Validate private fields that store test results using pattern matching
        if (!ValidateField<DeploymentNotification>(
            value,
            "_notificationBuilder_WithAllRequiredFields_BuildsValidNotification",
            static _ => true,
            "NotificationTests._notificationBuilder_WithAllRequiredFields_BuildsValidNotification must not be null."))
        {
            problems.Add("NotificationTests._notificationBuilder_WithAllRequiredFields_BuildsValidNotification must not be null.");
        }

        if (!ValidateField<DeploymentNotification>(
            value,
            "_notificationBuilder_AsFailure_SetsCriticalPriorityAndFailedStatus",
            static notification => notification.Status == BuildStatus.Failed && notification.Priority == NotificationPriority.Critical,
            "NotificationTests._notificationBuilder_AsFailure_SetsCriticalPriorityAndFailedStatus must be a valid DeploymentNotification with Failed status and Critical priority."))
        {
            problems.Add("NotificationTests._notificationBuilder_AsFailure_SetsCriticalPriorityAndFailedStatus must not be null or have incorrect properties.");
        }

        if (!ValidateField<DeploymentNotification>(
            value,
            "_notificationBuilder_AsDeploymentSuccess_SetsHighPriorityAndCorrectStatus",
            static notification => notification.Status == BuildStatus.DeploymentSuccess && notification.Priority == NotificationPriority.High,
            "NotificationTests._notificationBuilder_AsDeploymentSuccess_SetsHighPriorityAndCorrectStatus must be a valid DeploymentNotification with DeploymentSuccess status and High priority."))
        {
            problems.Add("NotificationTests._notificationBuilder_AsDeploymentSuccess_SetsHighPriorityAndCorrectStatus must not be null or have incorrect properties.");
        }

        if (!ValidateField<InvalidOperationException>(
            value,
            "_notificationBuilder_Build_WithMissingProjectName_ThrowsInvalidOperationException",
            static _ => true,
            "NotificationTests._notificationBuilder_Build_WithMissingProjectName_ThrowsInvalidOperationException must not be null."))
        {
            problems.Add("NotificationTests._notificationBuilder_Build_WithMissingProjectName_ThrowsInvalidOperationException must not be null.");
        }

        if (!ValidateField<InvalidOperationException>(
            value,
            "_notificationBuilder_Build_WithNoChannels_ThrowsInvalidOperationException",
            static _ => true,
            "NotificationTests._notificationBuilder_Build_WithNoChannels_ThrowsInvalidOperationException must not be null."))
        {
            problems.Add("NotificationTests._notificationBuilder_Build_WithNoChannels_ThrowsInvalidOperationException must not be null.");
        }

        if (!ValidateField<ValidationResult>(
            value,
            "_iValidationService_WhenMocked_ReturnsConfiguredErrorsAndIsVerifiable",
            static result => !result.IsValid && result.Errors.Count == 2,
            "NotificationTests._iValidationService_WhenMocked_ReturnsConfiguredErrorsAndIsVerifiable must return an invalid ValidationResult with exactly 2 errors."))
        {
            problems.Add("NotificationTests._iValidationService_WhenMocked_ReturnsConfiguredErrorsAndIsVerifiable must not be null and must be invalid with exactly 2 errors.");
        }

        if (!ValidateField<bool>(
            value,
            "_channelConfiguration_ShouldSendNotification_WhenChannelDisabled_ReturnsFalse",
            static result => result == false,
            "NotificationTests._channelConfiguration_ShouldSendNotification_WhenChannelDisabled_ReturnsFalse must return false."))
        {
            problems.Add("NotificationTests._channelConfiguration_ShouldSendNotification_WhenChannelDisabled_ReturnsFalse should be false.");
        }

        if (!ValidateField<bool>(
            value,
            "_channelConfiguration_ShouldSendNotification_WhenPriorityBelowMinimum_ReturnsFalse",
            static result => result == false,
            "NotificationTests._channelConfiguration_ShouldSendNotification_WhenPriorityBelowMinimum_ReturnsFalse must return false."))
        {
            problems.Add("NotificationTests._channelConfiguration_ShouldSendNotification_WhenPriorityBelowMinimum_ReturnsFalse should be false.");
        }

        if (!ValidateField<NotificationResult>(
            value,
            "_notificationResult_MarkAsSuccessful_SetsDeliveredStatusAndClearsError",
            static result => result.Status == DeliveryStatus.Delivered && result.ErrorMessage is null && result.IsSuccessful,
            "NotificationTests._notificationResult_MarkAsSuccessful_SetsDeliveredStatusAndClearsError must return a successful NotificationResult with Delivered status and no error message."))
        {
            problems.Add("NotificationTests._notificationResult_MarkAsSuccessful_SetsDeliveredStatusAndClearsError must not be null and must have correct properties.");
        }

        if (!ValidateField<NotificationResult>(
            value,
            "_notificationResult_MarkAsFailed_SetsFailedStatusWithErrorDetails",
            static result => result.Status == DeliveryStatus.Failed && !string.IsNullOrWhiteSpace(result.ErrorMessage) && !result.IsSuccessful,
            "NotificationTests._notificationResult_MarkAsFailed_SetsFailedStatusWithErrorDetails must return a failed NotificationResult with error details."))
        {
            problems.Add("NotificationTests._notificationResult_MarkAsFailed_SetsFailedStatusWithErrorDetails must not be null and must have correct properties.");
        }

        if (!ValidateField<bool>(
            value,
            "_deploymentNotification_SetAndGetMetadata_RoundTripsTypedValues",
            static result => result == true,
            "NotificationTests._deploymentNotification_SetAndGetMetadata_RoundTripsTypedValues must return true."))
        {
            problems.Add("NotificationTests._deploymentNotification_SetAndGetMetadata_RoundTripsTypedValues should be true.");
        }

        if (!ValidateField<bool>(
            value,
            "_deploymentNotification_IncrementDeliveryAttempt_IncrementsCounterEachCall",
            static result => result == true,
            "NotificationTests._deploymentNotification_IncrementDeliveryAttempt_IncrementsCounterEachCall must return true."))
        {
            problems.Add("NotificationTests._deploymentNotification_IncrementDeliveryAttempt_IncrementsCounterEachCall should be true.");
        }

        if (!ValidateField<bool>(
            value,
            "_deploymentNotification_MarkAsProcessed_SetsIsProcessedTrue",
            static result => result == true,
            "NotificationTests._deploymentNotification_MarkAsProcessed_SetsIsProcessedTrue must return true."))
        {
            problems.Add("NotificationTests._deploymentNotification_MarkAsProcessed_SetsIsProcessedTrue should be true.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="NotificationTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The test instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static bool IsValid(this NotificationTests value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that a <see cref="NotificationTests"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
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
