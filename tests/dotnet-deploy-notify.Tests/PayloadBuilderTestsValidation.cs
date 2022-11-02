using System;
using System.Collections.Generic;

namespace DotNetDeployNotify.Tests
{
    /// <summary>
    /// Provides validation helpers for <see cref="PayloadBuilderTests"/> instances.
    /// </summary>
    public static class PayloadBuilderTestsValidation
    {
        /// <summary>
        /// Validates the specified <see cref="PayloadBuilderTests"/> instance.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this PayloadBuilderTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="PayloadBuilderTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this PayloadBuilderTests value)
        {
            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="PayloadBuilderTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing a list of problems.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static void EnsureValid(this PayloadBuilderTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = Validate(value);
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"The {nameof(PayloadBuilderTests)} instance is invalid. Problems:\n{string.Join("\n", problems)}");
            }
        }
    }
}
