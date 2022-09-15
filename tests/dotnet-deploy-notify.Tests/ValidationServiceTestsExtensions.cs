using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetDeployNotify.Tests
{
    /// <summary>
    /// Provides extension methods for the <see cref="ValidationServiceTests"/> class.
    /// </summary>
    public static class ValidationServiceTestsExtensions
    {
        /// <summary>
        /// Retrieves a list of test method names that validate notification configurations.
        /// </summary>
        /// <param name="tests">The instance of <see cref="ValidationServiceTests"/>.</param>
        /// <returns>A list of test method names.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
        public static IReadOnlyList<string> GetNotificationValidationTestMethods(this ValidationServiceTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            return new[]
            {
                nameof(ValidationServiceTests.ValidateNotification_WithValidNotification_ReturnsSuccess),
                nameof(ValidationServiceTests.ValidateNotification_WithNullNotification_ReturnsFailure),
                nameof(ValidationServiceTests.ValidateNotification_WithMissingProjectName_ReturnsFailure),
                nameof(ValidationServiceTests.ValidateNotification_WithMissingVersion_ReturnsFailure),
                nameof(ValidationServiceTests.ValidateNotification_WithMissingBranchName_ReturnsFailure),
                nameof(ValidationServiceTests.ValidateNotification_WithMissingMessage_ReturnsFailure),
                nameof(ValidationServiceTests.ValidateNotification_WithNoChannels_ReturnsFailure),
                nameof(ValidationServiceTests.ValidateNotification_WithNegativeDeliveryAttempts_ReturnsFailure),
                nameof(ValidationServiceTests.ValidateNotification_WithNegativeDuration_ReturnsFailure),
                nameof(ValidationServiceTests.ValidateNotification_WithPositiveDuration_ReturnsSuccess),
                nameof(ValidationServiceTests.ValidateNotification_WithMultipleErrors_ReturnsAllErrors)
            };
        }

        /// <summary>
        /// Retrieves a list of test method names that validate channel configurations.
        /// </summary>
        /// <param name="tests">The instance of <see cref="ValidationServiceTests"/>.</param>
        /// <returns>A list of test method names.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
        public static IReadOnlyList<string> GetChannelConfigurationValidationTestMethods(this ValidationServiceTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            return new[]
            {
                nameof(ValidationServiceTests.ValidateChannelConfiguration_WithValidConfig_ReturnsSuccess),
                nameof(ValidationServiceTests.ValidateChannelConfiguration_WithNullConfig_ReturnsFailure),
                nameof(ValidationServiceTests.ValidateChannelConfiguration_WithMissingDisplayName_ReturnsFailure),
                nameof(ValidationServiceTests.ValidateChannelConfiguration_WithInvalidWebhookUrl_ReturnsFailure),
                nameof(ValidationServiceTests.ValidateChannelConfiguration_WithMissingTargetId_ReturnsFailure),
                nameof(ValidationServiceTests.ValidateChannelConfiguration_WithZeroTimeout_ReturnsFailure),
                nameof(ValidationServiceTests.ValidateChannelConfiguration_WithNegativeMaxRetries_ReturnsFailure),
                nameof(ValidationServiceTests.ValidateChannelConfiguration_WithNullCustomHeaders_ReturnsFailure)
            };
        }

        /// <summary>
        /// Retrieves a list of all test method names.
        /// </summary>
        /// <param name="tests">The instance of <see cref="ValidationServiceTests"/>.</param>
        /// <returns>A list of test method names.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
        public static IReadOnlyList<string> GetAllTestMethods(this ValidationServiceTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            return tests.GetNotificationValidationTestMethods().Concat(tests.GetChannelConfigurationValidationTestMethods()).ToList();
        }
    }
}
