#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics.CodeAnalysis;

namespace DotNetDeployNotify.Utilities;

/// <summary>
/// Provides validation helpers for <see cref="RetryPolicy"/> instances
/// </summary>
public static class RetryPolicyValidation
{
    /// <summary>
    /// Validates a <see cref="RetryPolicy"/> instance for common issues
    /// </summary>
    /// <param name="policy">The retry policy to validate</param>
    /// <returns>A list of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentNullException"><paramref name="policy"/> is <see langword="null"/></exception>
    public static IReadOnlyList<string> ValidateRetryPolicy([NotNull] this RetryPolicy? policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        var problems = new List<string>();

        // Validate MaxAttempts
        if (policy.MaxAttempts < 1)
        {
            problems.Add("MaxAttempts must be at least 1.");
        }

        // Validate InitialDelay
        if (policy.InitialDelay <= TimeSpan.Zero)
        {
            problems.Add("InitialDelay must be greater than zero.");
        }

        // Validate BackoffMultiplier
        if (policy.BackoffMultiplier <= 0)
        {
            problems.Add("BackoffMultiplier must be greater than zero.");
        }

        // Validate MaxDelay
        if (policy.MaxDelay <= TimeSpan.Zero)
        {
            problems.Add("MaxDelay must be greater than zero.");
        }

        // Validate that MaxDelay is greater than or equal to InitialDelay
        if (policy.MaxDelay < policy.InitialDelay)
        {
            problems.Add("MaxDelay must be greater than or equal to InitialDelay.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a <see cref="RetryPolicy"/> instance is valid
    /// </summary>
    /// <param name="policy">The retry policy to check</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/></returns>
    public static bool IsRetryPolicyValid([NotNullWhen(true)] this RetryPolicy? policy)
    {
        if (policy is null)
        {
            return false;
        }

        return policy.ValidateRetryPolicy().Count == 0;
    }

    /// <summary>
    /// Ensures a <see cref="RetryPolicy"/> instance is valid, throwing if not
    /// </summary>
    /// <param name="policy">The retry policy to validate</param>
    /// <exception cref="ArgumentNullException"><paramref name="policy"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException">Thrown if validation fails with a list of problems</exception>
    public static void EnsureRetryPolicyIsValid(this RetryPolicy? policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        var problems = policy.ValidateRetryPolicy();

        if (problems.Count > 0)
        {
            var problemList = string.Join("\n", problems.Select((p, i) => $"  {i + 1}. {p}"));
            throw new ArgumentException(
                $"RetryPolicy is invalid. Problems:\n{problemList}",
                nameof(policy));
        }
    }
}
