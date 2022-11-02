using System;
using System.Collections.Generic;

namespace DotNetDeployNotify.Tests
{
    /// <summary>
    /// Provides validation helpers for <see cref="StringExtensionsTests"/> instances.
    /// </summary>
    public static class StringExtensionsTestsValidation
    {
        /// <summary>
        /// Validates the specified <see cref="StringExtensionsTests"/> instance.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this StringExtensionsTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            return Array.Empty<string>();
        }

        /// <summary>
        /// Determines whether the specified <see cref="StringExtensionsTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this StringExtensionsTests value)
        {
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="StringExtensionsTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid, containing a list of problems.</exception>
        public static void EnsureValid(this StringExtensionsTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"StringExtensionsTests instance is not valid. Problems:\n{string.Join("\n", problems)}");
            }
        }
    }
}
