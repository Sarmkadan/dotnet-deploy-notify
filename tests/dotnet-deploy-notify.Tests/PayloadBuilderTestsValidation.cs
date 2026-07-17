using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

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

            ValidateTestFixtureStructure(value, problems);
            ValidateTestMethods(value, problems);

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="PayloadBuilderTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this PayloadBuilderTests value) => Validate(value).Count == 0;

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

        private static void ValidateTestFixtureStructure(PayloadBuilderTests value, List<string> problems)
        {
            if (value is null)
            {
                problems.Add("The PayloadBuilderTests instance is null.");
                return;
            }

            var type = value.GetType();
            var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

            if (fields.Length == 0)
            {
                problems.Add("The PayloadBuilderTests class has no private fields defined.");
            }

            var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            var testMethods = methods.Where(m => m.Name.StartsWith("Test", StringComparison.Ordinal) || m.Name.StartsWith("Build")).ToList();

            if (testMethods.Count < 10)
            {
                problems.Add($"The PayloadBuilderTests class should have at least 10 test methods, but has only {testMethods.Count}.");
            }
        }

        private static void ValidateTestMethods(PayloadBuilderTests value, List<string> problems)
        {
            var type = value.GetType();
            var testMethods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                .Where(m => m.Name.StartsWith("Test", StringComparison.Ordinal) || m.Name.StartsWith("Build"))
                .ToList();

            if (testMethods.Count == 0)
            {
                return;
            }

            var factAttributes = new[] { typeof(FactAttribute), typeof(TheoryAttribute) };
            var validMethods = testMethods.Where(m => factAttributes.Any(attr => m.GetCustomAttribute(attr) != null)).ToList();

            if (validMethods.Count < testMethods.Count)
            {
                problems.Add($"Found {testMethods.Count} methods starting with 'Test' or 'Build', but only {validMethods.Count} have proper test attributes (Fact/Theory).");
            }

            foreach (var method in validMethods)
            {
                var parameters = method.GetParameters();
                if (parameters.Length > 0)
                {
                    problems.Add($"Test method '{method.Name}' should not have parameters.");
                }
            }
        }
    }
}
