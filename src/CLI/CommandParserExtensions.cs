#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace DotNetDeployNotify.CLI;

/// <summary>
/// Provides extension methods for <see cref="CommandParser"/> to simplify common parsing scenarios
/// </summary>
public static class CommandParserExtensions
{
    /// <summary>
    /// Attempts to parse command-line arguments and returns a parsed command.
    /// If parsing fails, returns a parsed command with the error message.
    /// </summary>
    /// <param name="parser">The command parser instance</param>
    /// <param name="args">Command-line arguments</param>
    /// <returns>A parsed command with success status and error message if applicable</returns>
    /// <exception cref="ArgumentNullException">Thrown when parser or args is null</exception>
    public static ParsedCommand TryParse(this CommandParser parser, string[] args)
    {
        ArgumentNullException.ThrowIfNull(parser);
        ArgumentNullException.ThrowIfNull(args);

        return parser.Parse(args);
    }

    /// <summary>
    /// Gets all parameter names defined for the current command.
    /// </summary>
    /// <param name="parser">The command parser instance</param>
    /// <param name="parsedCommand">The parsed command to inspect</param>
    /// <returns>Read-only collection of parameter names, or empty if none</returns>
    /// <exception cref="ArgumentNullException">Thrown when parser or parsedCommand is null</exception>
    public static IReadOnlyList<string> GetParameterNames(this CommandParser parser, ParsedCommand parsedCommand)
    {
        ArgumentNullException.ThrowIfNull(parser);
        ArgumentNullException.ThrowIfNull(parsedCommand);

        return parsedCommand.Parameters.Keys.ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets all option names defined for the current command.
    /// </summary>
    /// <param name="parser">The command parser instance</param>
    /// <param name="parsedCommand">The parsed command to inspect</param>
    /// <returns>Read-only collection of option names, or empty if none</returns>
    /// <exception cref="ArgumentNullException">Thrown when parser or parsedCommand is null</exception>
    public static IReadOnlyList<string> GetOptionNames(this CommandParser parser, ParsedCommand parsedCommand)
    {
        ArgumentNullException.ThrowIfNull(parser);
        ArgumentNullException.ThrowIfNull(parsedCommand);

        return parsedCommand.Options.Keys.ToList().AsReadOnly();
    }

    /// <summary>
    /// Safely gets a parameter value as an integer. Returns null if the parameter doesn't exist or parsing fails.
    /// </summary>
    /// <param name="parser">The command parser instance</param>
    /// <param name="parsedCommand">The parsed command to inspect</param>
    /// <param name="parameterName">Name of the parameter to retrieve</param>
    /// <returns>The parsed integer value, or null if not found or invalid</returns>
    /// <exception cref="ArgumentNullException">Thrown when parser or parsedCommand is null</exception>
    public static int? GetParameterAsInt(this CommandParser parser, ParsedCommand parsedCommand, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(parser);
        ArgumentNullException.ThrowIfNull(parsedCommand);
        ArgumentException.ThrowIfNullOrEmpty(parameterName);

        var value = parsedCommand.GetParameter(parameterName);
        if (value is null)
            return null;

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Safely gets an option value as a boolean. Returns false if the option doesn't exist.
    /// </summary>
    /// <param name="parser">The command parser instance</param>
    /// <param name="parsedCommand">The parsed command to inspect</param>
    /// <param name="optionName">Name of the option to retrieve</param>
    /// <returns>True if option is present and set to "true", otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown when parser or parsedCommand is null</exception>
    public static bool GetOptionAsBoolean(this CommandParser parser, ParsedCommand parsedCommand, string optionName)
    {
        ArgumentNullException.ThrowIfNull(parser);
        ArgumentNullException.ThrowIfNull(parsedCommand);
        ArgumentException.ThrowIfNullOrEmpty(optionName);

        var value = parsedCommand.GetOption(optionName);
        return value is "true";
    }

    /// <summary>
    /// Safely gets an option value as an integer. Returns null if the option doesn't exist or parsing fails.
    /// </summary>
    /// <param name="parser">The command parser instance</param>
    /// <param name="parsedCommand">The parsed command to inspect</param>
    /// <param name="optionName">Name of the option to retrieve</param>
    /// <returns>The parsed integer value, or null if not found or invalid</returns>
    /// <exception cref="ArgumentNullException">Thrown when parser or parsedCommand is null</exception>
    public static int? GetOptionAsInt(this CommandParser parser, ParsedCommand parsedCommand, string optionName)
    {
        ArgumentNullException.ThrowIfNull(parser);
        ArgumentNullException.ThrowIfNull(parsedCommand);
        ArgumentException.ThrowIfNullOrEmpty(optionName);

        var value = parsedCommand.GetOption(optionName);
        if (value is null)
            return null;

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Determines whether the parsed command has any parameters defined.
    /// </summary>
    /// <param name="parser">The command parser instance</param>
    /// <param name="parsedCommand">The parsed command to inspect</param>
    /// <returns>True if parameters exist, otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown when parser or parsedCommand is null</exception>
    public static bool HasParameters(this CommandParser parser, ParsedCommand parsedCommand)
    {
        ArgumentNullException.ThrowIfNull(parser);
        ArgumentNullException.ThrowIfNull(parsedCommand);

        return parsedCommand.Parameters.Count > 0;
    }

    /// <summary>
    /// Determines whether the parsed command has any options defined.
    /// </summary>
    /// <param name="parser">The command parser instance</param>
    /// <param name="parsedCommand">The parsed command to inspect</param>
    /// <returns>True if options exist, otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown when parser or parsedCommand is null</exception>
    public static bool HasOptions(this CommandParser parser, ParsedCommand parsedCommand)
    {
        ArgumentNullException.ThrowIfNull(parser);
        ArgumentNullException.ThrowIfNull(parsedCommand);

        return parsedCommand.Options.Count > 0;
    }

    /// <summary>
    /// Gets the number of parameters provided in the parsed command.
    /// </summary>
    /// <param name="parser">The command parser instance</param>
    /// <param name="parsedCommand">The parsed command to inspect</param>
    /// <returns>Count of provided parameters</returns>
    /// <exception cref="ArgumentNullException">Thrown when parser or parsedCommand is null</exception>
    public static int GetParameterCount(this CommandParser parser, ParsedCommand parsedCommand)
    {
        ArgumentNullException.ThrowIfNull(parser);
        ArgumentNullException.ThrowIfNull(parsedCommand);

        return parsedCommand.Parameters.Count;
    }

    /// <summary>
    /// Gets the number of options provided in the parsed command.
    /// </summary>
    /// <param name="parser">The command parser instance</param>
    /// <param name="parsedCommand">The parsed command to inspect</param>
    /// <returns>Count of provided options</returns>
    /// <exception cref="ArgumentNullException">Thrown when parser or parsedCommand is null</exception>
    public static int GetOptionCount(this CommandParser parser, ParsedCommand parsedCommand)
    {
        ArgumentNullException.ThrowIfNull(parser);
        ArgumentNullException.ThrowIfNull(parsedCommand);

        return parsedCommand.Options.Count;
    }
}