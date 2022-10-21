using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetDeployNotify.Utilities
{
    /// <summary>
    /// Provides validation extension methods for enum values using <see cref="EnumExtensions"/>.
    /// </summary>
    public static class EnumExtensionsValidation
    {
        /// <summary>
        /// Validates the specified enum value.
        /// </summary>
        /// <typeparam name="T">The enum type to validate.</typeparam>
        /// <param name="value">The enum value to validate.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate<T>(this T value) where T : Enum
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Validate GetDescription
            try
            {
                var description = value.GetDescription();
                if (string.IsNullOrWhiteSpace(description))
                {
                    problems.Add("GetDescription() returned null or whitespace");
                }
            }
            catch (Exception ex)
            {
                problems.Add($"GetDescription() threw: {ex.Message}");
            }

            // Validate HasFlag - no validation needed as it returns bool
            try
            {
                var hasFlagResult = value.HasFlag(default(T));
            }
            catch (Exception ex)
            {
                problems.Add($"HasFlag() threw: {ex.Message}");
            }

            // Validate ToHumanReadable
            try
            {
                var humanReadable = value.ToHumanReadable();
                if (string.IsNullOrWhiteSpace(humanReadable))
                {
                    problems.Add("ToHumanReadable() returned null or whitespace");
                }
            }
            catch (Exception ex)
            {
                problems.Add($"ToHumanReadable() threw: {ex.Message}");
            }

            // Validate IsIn
            try
            {
                var isInResult = value.IsIn(default(T));
            }
            catch (Exception ex)
            {
                problems.Add($"IsIn() threw: {ex.Message}");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified enum value is valid.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="value">The enum value to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid<T>(this T value) where T : Enum
        {
            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures that the specified enum value is valid.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="value">The enum value to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid, containing a list of problems.</exception>
        public static void EnsureValid<T>(this T value) where T : Enum
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = Validate(value);
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"Enum value {typeof(T).Name}.{value} is not valid. Problems: {string.Join("; ", problems)}");
            }
        }
    }
}