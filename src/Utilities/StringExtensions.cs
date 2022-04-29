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
    public static string Truncate(this string input, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return input.Length <= maxLength ? input : input.Substring(0, maxLength - suffix.Length) + suffix;
    }

    /// <summary>
    /// Converts string to slug format (lowercase, hyphens)
    /// </summary>
    public static string ToSlug(this string input)
    {
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
    public static string ToPascalCase(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var words = input.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(words.Select(w => char.ToUpper(w[0]) + w.Substring(1).ToLower()));
    }

    /// <summary>
    /// Converts string to camelCase
    /// </summary>
    public static string ToCamelCase(this string input)
    {
        var pascalCase = input.ToPascalCase();
        return string.IsNullOrWhiteSpace(pascalCase) ? string.Empty :
            char.ToLower(pascalCase[0]) + pascalCase.Substring(1);
    }

    /// <summary>
    /// Masks sensitive parts of the string (e.g., API keys, tokens)
    /// </summary>
    public static string MaskSensitive(this string input, int visibleChars = 4)
    {
        if (string.IsNullOrWhiteSpace(input) || input.Length <= visibleChars)
            return "****";

        var visiblePart = input.Substring(0, visibleChars);
        var maskedLength = input.Length - visibleChars;
        return $"{visiblePart}{'*'.ToString().PadRight(maskedLength, '*')}";
    }

    /// <summary>
    /// Checks if string contains any of the specified substrings
    /// </summary>
    public static bool ContainsAny(this string input, params string[] substrings)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        return substrings.Any(s => input.Contains(s, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Normalizes line endings to Unix format (LF)
    /// </summary>
    public static string NormalizeLineEndings(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return input.Replace("\r\n", "\n").Replace("\r", "\n");
    }

    /// <summary>
    /// Counts occurrences of a substring
    /// </summary>
    public static int CountOccurrences(this string input, string substring)
    {
        if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(substring))
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
    public static string RemoveDuplicateCharacters(this string input)
    {
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
    public static string TakeWords(this string input, int wordCount)
    {
        if (string.IsNullOrWhiteSpace(input) || wordCount <= 0)
            return string.Empty;

        var words = input.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", words.Take(wordCount));
    }

    /// <summary>
    /// Wraps text to specified line length
    /// </summary>
    public static string WrapText(this string input, int lineLength = 80)
    {
        if (string.IsNullOrWhiteSpace(input) || lineLength <= 0)
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

        return string.Join(System.Environment.NewLine, lines);
    }

    /// <summary>
    /// Converts string representation to boolean safely
    /// </summary>
    public static bool ToBooleanSafe(this string input, bool defaultValue = false)
    {
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
