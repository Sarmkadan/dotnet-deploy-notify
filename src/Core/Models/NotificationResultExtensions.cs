using System;

namespace DotNetDeployNotify.Core.Models
{
    /// <summary>
    /// Provides extension methods for <see cref="NotificationResult"/> to enhance readability and analysis.
    /// </summary>
    public static class NotificationResultExtensions
    {
        /// <summary>
        /// Determines whether the notification result represents a failed delivery attempt.
        /// </summary>
        /// <param name="notificationResult">The notification result to evaluate.</param>
        /// <returns><see langword="true"/> if the result is marked as failed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="notificationResult"/> is <see langword="null"/>.</exception>
        public static bool IsFailed(this NotificationResult notificationResult)
        {
            ArgumentNullException.ThrowIfNull(notificationResult);
            return notificationResult.Status == DeliveryStatus.Failed;
        }

        /// <summary>
        /// Determines whether the notification result is eligible for retry based on retry scheduling.
        /// </summary>
        /// <param name="notificationResult">The notification result to evaluate.</param>
        /// <returns><see langword="true"/> if the result can be retried; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="notificationResult"/> is <see langword="null"/>.</exception>
        public static bool IsRetryable(this NotificationResult notificationResult)
        {
            ArgumentNullException.ThrowIfNull(notificationResult);
            return notificationResult.NextRetryAt.HasValue && DateTime.UtcNow < notificationResult.NextRetryAt.Value;
        }

        /// <summary>
        /// Generates a concise summary string containing key metadata about the notification result.
        /// </summary>
        /// <param name="notificationResult">The notification result to summarize.</param>
        /// <returns>A formatted summary string containing notification ID, status, attempt count, and duration.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="notificationResult"/> is <see langword="null"/>.</exception>
        public static string ToSummary(this NotificationResult notificationResult)
        {
            ArgumentNullException.ThrowIfNull(notificationResult);

            return notificationResult.Status switch
            {
                DeliveryStatus.Delivered => $"Notification: {notificationResult.Id}\nStatus: {notificationResult.Status}\nAttempt: {notificationResult.AttemptNumber}\nDuration: {notificationResult.DurationMs}ms\nAttempted: {notificationResult.AttemptedAt:O}",
                DeliveryStatus.Failed => $"Notification: {notificationResult.Id}\nStatus: {notificationResult.Status}\nAttempt: {notificationResult.AttemptNumber}\nError: {notificationResult.ErrorMessage}\nDuration: {notificationResult.DurationMs}ms\nAttempted: {notificationResult.AttemptedAt:O}",
                DeliveryStatus.Timeout => $"Notification: {notificationResult.Id}\nStatus: {notificationResult.Status}\nAttempt: {notificationResult.AttemptNumber}\nDuration: {notificationResult.DurationMs}ms\nAttempted: {notificationResult.AttemptedAt:O}",
                DeliveryStatus.Retried => $"Notification: {notificationResult.Id}\nStatus: {notificationResult.Status}\nAttempt: {notificationResult.AttemptNumber}\nNextRetry: {notificationResult.NextRetryAt:u}\nDuration: {notificationResult.DurationMs}ms\nAttempted: {notificationResult.AttemptedAt:O}",
                DeliveryStatus.Skipped => $"Notification: {notificationResult.Id}\nStatus: {notificationResult.Status}\nAttempt: {notificationResult.AttemptNumber}\nReason: {notificationResult.ResponseBody}\nDuration: {notificationResult.DurationMs}ms\nAttempted: {notificationResult.AttemptedAt:O}",
                _ => $"Notification: {notificationResult.Id}\nStatus: {notificationResult.Status}\nAttempt: {notificationResult.AttemptNumber}\nDuration: {notificationResult.DurationMs}ms\nAttempted: {notificationResult.AttemptedAt:O}"
            };
        }
    }
}