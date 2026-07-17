using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetDeployNotify.Utilities
{
    /// <summary>
    /// Provides validation extension methods for enum values.
    /// </summary>
    public static class EnumExtensionsValidation
    {
        /// <summary>
        /// Validates that the enum value is defined in its underlying enum type.
        /// </summary>
        /// <typeparam name="T">The enum type to validate.</typeparam>
        /// <param name="value">The enum value to validate.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate<T>(this T value) where T : Enum
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();
            var validValues = Enum.GetValues(typeof(T));

            // Check if the value is defined in the enum
            if (!validValues.Cast<object>().Any(v => object.Equals(v, value)))
            {
                problems.Add($"Value {value} is not defined in enum {typeof(T).Name}");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified enum value is valid and defined in its enum type.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="value">The enum value to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid<T>(this T value) where T : Enum
        {
            ArgumentNullException.ThrowIfNull(value);
            return Enum.IsDefined(typeof(T), value);
        }

        /// <summary>
        /// Ensures that the specified enum value is valid and defined in its enum type.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="value">The enum value to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid or not defined in the enum.</exception>
        public static void EnsureValid<T>(this T value) where T : Enum
        {
            ArgumentNullException.ThrowIfNull(value);

            if (!IsValid(value))
            {
                throw new ArgumentException($"Enum value {typeof(T).Name}.{value} is not valid or not defined in the enum.");
            }
        }
    }
}