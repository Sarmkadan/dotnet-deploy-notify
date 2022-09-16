using System;
using System.Collections.Generic;

namespace DotNetDeployNotify.Tests
{
    /// <summary>
    /// Provides extension methods for the <see cref="IntegrationTests"/> class to facilitate test categorization and management.
    /// </summary>
    public static class IntegrationTestsExtensions
    {
        /// <summary>
        /// Retrieves a list of test method names related to the <see cref="DotNetDeployNotify.Services.NotificationService"/>.
        /// </summary>
        /// <param name="tests">The instance of <see cref="IntegrationTests"/>.</param>
        /// <returns>A list of notification service integration test method names.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
        public static IReadOnlyList<string> GetNotificationServiceIntegrationTestMethods(this IntegrationTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            return new[]
            {
                nameof(IntegrationTests.NotificationService_CreateAndSendNotification_EndToEndWorkflow),
                nameof(IntegrationTests.NotificationService_SendToMultipleChannels_DeliverToAllConfiguredChannels),
                nameof(IntegrationTests.NotificationService_WithValidationFailure_ThrowsException),
                nameof(IntegrationTests.NotificationService_RetryFailedDeliveries_UpdatesResultsAndIncrementAttempts)
            };
        }

        /// <summary>
        /// Retrieves a list of test method names related to overall deployment workflows and notification processing.
        /// </summary>
        /// <param name="tests">The instance of <see cref="IntegrationTests"/>.</param>
        /// <returns>A list of deployment workflow integration test method names.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
        public static IReadOnlyList<string> GetDeploymentIntegrationTestMethods(this IntegrationTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            return new[]
            {
                nameof(IntegrationTests.MainUseCase_SendDeploymentNotificationToMultipleChannels_CompleteFlow),
                nameof(IntegrationTests.MultipleNotifications_ProcessConcurrently_AllDeliveredSuccessfully),
                nameof(IntegrationTests.NotificationWithChannelFiltering_SkipsNotConfiguredChannels)
            };
        }

        /// <summary>
        /// Retrieves a list of test method names related to <see cref="DotNetDeployNotify.Infrastructure.WebhookDispatcher"/> integration.
        /// </summary>
        /// <param name="tests">The instance of <see cref="IntegrationTests"/>.</param>
        /// <returns>A list of webhook integration test method names.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
        public static IReadOnlyList<string> GetWebhookIntegrationTestMethods(this IntegrationTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            return new[]
            {
                nameof(IntegrationTests.WebhookDispatcher_WithValidPayload_SendsSuccessfully)
            };
        }
    }
}
