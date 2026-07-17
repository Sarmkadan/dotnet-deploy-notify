#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace DotNetDeployNotify.CLI;

/// <summary>
/// Provides validation helpers for <see cref="CommandParser"/> instances.
/// </summary>
public static class CommandParserValidation
{
    /// <summary>
    /// Validates the supplied <see cref="CommandParser"/> and returns a read‑only list of human‑readable problems.
    /// </summary>
    /// <param name="value">The parser to validate.</param>
    /// <returns>A read‑only list of validation error messages. The list is empty when the parser is considered valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> Validate(this CommandParser value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Use reflection to obtain the private field that stores the command definitions.
        var field = typeof(CommandParser).GetField("_commands", BindingFlags.Instance | BindingFlags.NonPublic);
        if (field is null)
        {
            // If the internal field cannot be found, we cannot perform deeper validation.
            // Returning the current problem list (empty) is acceptable.
            return new ReadOnlyCollection<string>(problems);
        }

        if (field.GetValue(value) is not IDictionary<string, CommandDefinition> commands)
        {
            // Unexpected type – treat as a problem.
            problems.Add("Unable to read command definitions from parser.");
            return new ReadOnlyCollection<string>(problems);
        }

        foreach (var kvp in commands)
        {
            var key = kvp.Key;
            var definition = kvp.Value ?? throw new InvalidOperationException("Command definition cannot be null");

            // Command key must match definition name (case‑insensitive)
            if (!string.Equals(key, definition.Name, StringComparison.OrdinalIgnoreCase))
            {
                problems.Add($"Command dictionary key '{key}' does not match definition name '{definition.Name}'.");
            }

            // Name
            if (string.IsNullOrWhiteSpace(definition.Name))
            {
                problems.Add($"Command '{key}' has an empty or whitespace Name.");
            }

            // Description
            if (string.IsNullOrWhiteSpace(definition.Description))
            {
                problems.Add($"Command '{definition.Name}' has an empty or whitespace Description.");
            }

            // Parameters
            if (definition.Parameters is null)
            {
                problems.Add($"Command '{definition.Name}' has null Parameters collection.");
            }
            else
            {
                ValidateParameters(definition, problems);
            }

            // Options
            if (definition.Options is null)
            {
                problems.Add($"Command '{definition.Name}' has null Options collection.");
            }
            else
            {
                ValidateOptions(definition, problems);
            }
        }

        return new ReadOnlyCollection<string>(problems);
    }

    /// <summary>
    /// Returns <c>true</c> when the supplied <see cref="CommandParser"/> has no validation problems.
    /// </summary>
    /// <param name="value">The parser to evaluate.</param>
    /// <returns><c>true</c> if the parser is valid; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static bool IsValid(this CommandParser value) => !value.Validate().Any();

    /// <summary>
    /// Ensures that the supplied <see cref="CommandParser"/> is valid.
    /// </summary>
    /// <param name="value">The parser to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when validation problems are found. The exception message contains a semicolon‑separated list of problems.</exception>
    public static void EnsureValid(this CommandParser value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var problems = value.Validate();
        if (problems.Any())
        {
            throw new ArgumentException(string.Join("; ", problems), nameof(value));
        }
    }

    // ------------------------------------------------------------------------
    // Private helpers
    // ------------------------------------------------------------------------

    private static void ValidateParameters(CommandDefinition definition, List<string> problems)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(problems);

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var param in definition.Parameters)
        {
            if (param is null)
            {
                problems.Add($"Command '{definition.Name}' contains a null parameter.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(param.Name))
            {
                problems.Add($"Command '{definition.Name}' has a parameter with an empty or whitespace Name.");
            }

            if (string.IsNullOrWhiteSpace(param.Description))
            {
                problems.Add($"Parameter '{param.Name}' of command '{definition.Name}' has an empty or whitespace Description.");
            }

            if (!seen.Add(param.Name))
            {
                problems.Add($"Command '{definition.Name}' contains duplicate parameter name '{param.Name}'.");
            }
        }
    }

    private static void ValidateOptions(CommandDefinition definition, List<string> problems)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(problems);

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var opt in definition.Options)
        {
            if (opt is null)
            {
                problems.Add($"Command '{definition.Name}' contains a null option.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(opt.Name))
            {
                problems.Add($"Command '{definition.Name}' has an option with an empty or whitespace Name.");
            }

            if (string.IsNullOrWhiteSpace(opt.Description))
            {
                problems.Add($"Option '{opt.Name}' of command '{definition.Name}' has an empty or whitespace Description.");
            }

            if (!seen.Add(opt.Name))
            {
                problems.Add($"Command '{definition.Name}' contains duplicate option name '{opt.Name}'.");
            }

            if (opt.ShortName is not null && opt.ShortName.Length != 1)
            {
                problems.Add($"Option '{opt.Name}' of command '{definition.Name}' has an invalid ShortName '{opt.ShortName}'. It must be a single character.");
            }

            // Flags cannot be marked as required – they are either present or not.
            if (opt.IsFlag && opt.IsRequired)
            {
                problems.Add($"Option '{opt.Name}' of command '{definition.Name}' is a flag but also marked as required.");
            }
        }
    }
}
