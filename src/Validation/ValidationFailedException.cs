#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace DotNetDeployNotify.Validation;

/// <summary>
/// Thrown when an argument fails model validation. Derives from <see cref="ArgumentException"/>
/// so callers that handle invalid arguments keep working, and exposes every problem that was found.
/// </summary>
public sealed class ValidationFailedException : ArgumentException
{
    /// <summary>
    /// Initializes a new instance with a summary message, the offending parameter and the problems found.
    /// </summary>
    /// <param name="message">The summary message, including the list of problems</param>
    /// <param name="paramName">The name of the parameter that failed validation</param>
    /// <param name="problems">The human-readable problems found during validation</param>
    /// <exception cref="ArgumentNullException"><paramref name="problems"/> is <see langword="null"/></exception>
    public ValidationFailedException(string message, string paramName, IReadOnlyList<string> problems)
        : base(message, paramName)
    {
        ArgumentNullException.ThrowIfNull(problems);
        Problems = problems;
    }

    /// <summary>Gets the human-readable problems found during validation</summary>
    public IReadOnlyList<string> Problems { get; }
}
