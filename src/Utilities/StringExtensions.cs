#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Utilities;

/// <summary>
/// Extension methods for string manipulation and formatting
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Truncates string to specified length and adds ellipsis
    /// </summary>
    /// <param name="input">The string to truncate.</param>
    /// <param name="maxLength">Maximum length of the result string.</param>
    /// <param name="suffix">Suffix to append when truncating (default: "...").</param>
    /// <returns>Truncated string with suffix, or original string if shorter than maxLength.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxLength is negative.</exception>
    public static string Truncate(this string input, int maxLength, string suffix = "...")
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);
        ArgumentNullException.ThrowIfNull(suffix);

        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return input.Length <= maxLength
            ? input
            : input[..(maxLength - suffix.Length)] + suffix;
    }

    /// <summary>
    /// Converts string to slug format (lowercase, hyphens)
    /// </summary>
    /// <param name="input">The string to convert to slug format.</param>
    /// <returns>String in slug format with lowercase letters and hyphens.</returns>
    /// <exception cref="ArgumentNullException">Thrown when input is null.</exception>
    public static string ToSlug(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return System.Text.RegularExpressions.Regex.Replace(input.ToLowerInvariant(), @"[^\w\s-]", "")
            .Replace(" ", "-")
            .Replace("--", "-")
            .Trim('-');
    }

    /// <summary>
    /// Converts string to PascalCase
    /// </summary>
    /// <param name="input">The string to convert to PascalCase.</param>
    /// <returns>String in PascalCase format.</returns>
    /// <exception cref="ArgumentNullException">Thrown when input is null.</exception>
    public static string ToPascalCase(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var words = input.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(words.Select(w =>
        {
            if (string.IsNullOrEmpty(w))
                return string.Empty;
            return char.ToUpper(w[0]) + w[1..].ToLowerInvariant();
        }));
    }

    /// <summary>
    /// Converts string to camelCase
    /// </summary>
    /// <param name="input">The string to convert to camelCase.</param>
    /// <returns>String in camelCase format.</returns>
    /// <exception cref="ArgumentNullException">Thrown when input is null.</exception>
    public static string ToCamelCase(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var pascalCase = input.ToPascalCase();
        return string.IsNullOrWhiteSpace(pascalCase)
            ? string.Empty
            : char.ToLowerInvariant(pascalCase[0]) + pascalCase[1..];
    }

    /// <summary>
    /// Masks sensitive parts of the string (e.g., API keys, tokens)
    /// </summary>
    /// <param name="input">The string to mask.</param>
    /// <param name="visibleChars">Number of characters to leave visible at the start.</param>
    /// <returns>Masked string with visible prefix and asterisks.</returns>
    /// <exception cref="ArgumentNullException">Thrown when input is null.</exception>
    public static string MaskSensitive(this string input, int visibleChars = 4)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(visibleChars);

        if (input.Length <= visibleChars)
            return "****";

        var visiblePart = input[..visibleChars];
        var maskedLength = input.Length - visibleChars;
        return $"{visiblePart}{new string('*', maskedLength)}";
    }

    /// <summary>
    /// Checks if string contains any of the specified substrings
    /// </summary>
    /// <param name="input">The string to search in.</param>
    /// <param name="substrings">Substrings to search for.</param>
    /// <returns>True if any substring is found; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when input or substrings is null.</exception>
    public static bool ContainsAny(this string input, params string[] substrings)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(substrings);

        if (string.IsNullOrWhiteSpace(input))
            return false;

        return substrings.Any(s =>
        {
            ArgumentNullException.ThrowIfNull(s);
            return input.Contains(s, StringComparison.OrdinalIgnoreCase);
        });
    }

    /// <summary>
    /// Normalizes line endings to Unix format (LF)
    /// </summary>
    /// <param name="input">The string to normalize.</param>
    /// <returns>String with normalized line endings.</returns>
    /// <exception cref="ArgumentNullException">Thrown when input is null.</exception>
    public static string NormalizeLineEndings(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return input.Replace("\r\n", "\n").Replace("\r", "\n");
    }

    /// <summary>
    /// Counts occurrences of a substring
    /// </summary>
    /// <param name="input">The string to search in.</param>
    /// <param name="substring">The substring to count.</param>
    /// <returns>Number of occurrences found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when input or substring is null.</exception>
    public static int CountOccurrences(this string input, string substring)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(substring);

        if (string.IsNullOrWhiteSpace(substring))
            return 0;

        int count = 0;
        int startIndex = 0;

        while ((startIndex = input.IndexOf(substring, startIndex, StringComparison.OrdinalIgnoreCase)) != -1)
        {
            count++;
            startIndex += substring.Length;
        }

        return count;
    }

    /// <summary>
    /// Removes duplicate consecutive characters
    /// </summary>
    /// <param name="input">The string to process.</param>
    /// <returns>String with consecutive duplicate characters removed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when input is null.</exception>
    public static string RemoveDuplicateCharacters(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var result = new StringBuilder();
        char lastChar = '\0';

        foreach (char c in input)
        {
            if (c != lastChar)
            {
                result.Append(c);
                lastChar = c;
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Extracts first N words from a string
    /// </summary>
    /// <param name="input">The string to extract words from.</param>
    /// <param name="wordCount">Number of words to extract.</param>
    /// <returns>String containing the first wordCount words.</returns>
    /// <exception cref="ArgumentNullException">Thrown when input is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when wordCount is negative.</exception>
    public static string TakeWords(this string input, int wordCount)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentOutOfRangeException.ThrowIfNegative(wordCount);

        if (string.IsNullOrWhiteSpace(input) || wordCount == 0)
            return string.Empty;

        var words = input.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", words.Take(wordCount));
    }

    /// <summary>
    /// Wraps text to specified line length
    /// </summary>
    /// <param name="input">The text to wrap.</param>
    /// <param name="lineLength">Maximum line length.</param>
    /// <returns>Text with line breaks inserted at appropriate word boundaries.</returns>
    /// <exception cref="ArgumentNullException">Thrown when input is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when lineLength is not positive.</exception>
    public static string WrapText(this string input, int lineLength = 80)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(lineLength);

        if (string.IsNullOrWhiteSpace(input))
            return input;

        var lines = new List<string>();
        var words = input.Split(' ');
        var currentLine = new StringBuilder();

        foreach (var word in words)
        {
            if (currentLine.Length + word.Length + 1 > lineLength)
            {
                if (currentLine.Length > 0)
                    lines.Add(currentLine.ToString().TrimEnd());
                currentLine.Clear();
            }

            if (currentLine.Length > 0)
                currentLine.Append(' ');

            currentLine.Append(word);
        }

        if (currentLine.Length > 0)
            lines.Add(currentLine.ToString().TrimEnd());

        return string.Join(global::System.Environment.NewLine, lines);
    }

    /// <summary>
    /// Converts string representation to boolean safely
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <param name="defaultValue">Default value to return if conversion fails.</param>
    /// <returns>Boolean value based on string content, or defaultValue if conversion fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when input is null.</exception>
    public static bool ToBooleanSafe(this string input, bool defaultValue = false)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (string.IsNullOrWhiteSpace(input))
            return defaultValue;

        return input switch
        {
            "1" or "true" or "yes" or "on" => true,
            "0" or "false" or "no" or "off" => false,
            _ => defaultValue
        };
    }
}