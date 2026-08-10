#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.RegularExpressions;

namespace DotNetDeployNotify.Validation;

/// <summary>
/// Base class for validation rules
/// </summary>
public abstract class ValidationRule<T>
{
    public abstract bool Validate(T value);
    public abstract string GetErrorMessage();
}

/// <summary>
/// Validates that a string is not empty
/// </summary>
public class NotEmptyRule : ValidationRule<string>
{
    private readonly string _fieldName;

    public NotEmptyRule(string fieldName)
    {
        ArgumentException.ThrowIfNullOrEmpty(fieldName);
        _fieldName = fieldName;
    }

    public override bool Validate(string value) => !string.IsNullOrWhiteSpace(value);
    public override string GetErrorMessage() => $"{_fieldName} cannot be empty";
}

/// <summary>
/// Validates string length
/// </summary>
public class LengthRule : ValidationRule<string>
{
    private readonly int _minLength;
    private readonly int _maxLength;
    private readonly string _fieldName;

    public LengthRule(string fieldName, int minLength = 0, int maxLength = int.MaxValue)
    {
        ArgumentException.ThrowIfNullOrEmpty(fieldName);
        _fieldName = fieldName;
        _minLength = minLength;
        _maxLength = maxLength;
    }

    public override bool Validate(string value)
    {
        if (string.IsNullOrEmpty(value))
            return _minLength == 0;

        return value.Length >= _minLength && value.Length <= _maxLength;
    }

    public override string GetErrorMessage()
    {
        if (_maxLength == int.MaxValue)
            return $"{_fieldName} must be at least {_minLength} characters";
        return $"{_fieldName} must be between {_minLength} and {_maxLength} characters";
    }
}

/// <summary>
/// Validates URL format
/// </summary>
public class UrlRule : ValidationRule<string>
{
    private readonly string _fieldName;

    public UrlRule(string fieldName)
    {
        ArgumentException.ThrowIfNullOrEmpty(fieldName);
        _fieldName = fieldName;
    }

    public override bool Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    public override string GetErrorMessage() => $"{_fieldName} must be a valid URL";
}

/// <summary>
/// Validates email format
/// </summary>
public class EmailRule : ValidationRule<string>
{
    private readonly string _fieldName;
    private static readonly Regex EmailRegex = new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$");

    public EmailRule(string fieldName)
    {
        ArgumentException.ThrowIfNullOrEmpty(fieldName);
        _fieldName = fieldName;
    }

    public override bool Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return EmailRegex.IsMatch(value);
    }

    public override string GetErrorMessage() => $"{_fieldName} must be a valid email address";
}

/// <summary>
/// Validates regex pattern
/// </summary>
public class PatternRule : ValidationRule<string>
{
    private readonly string _fieldName;
    private readonly string _pattern;
    private readonly Regex _regex;

    public PatternRule(string fieldName, string pattern)
    {
        ArgumentException.ThrowIfNullOrEmpty(fieldName);
        ArgumentException.ThrowIfNullOrEmpty(pattern);
        _fieldName = fieldName;
        _pattern = pattern;
        _regex = new Regex(pattern);
    }

    public override bool Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return _regex.IsMatch(value);
    }

    public override string GetErrorMessage() => $"{_fieldName} does not match the required pattern";
}

/// <summary>
/// Validates numeric range
/// </summary>
public class RangeRule : ValidationRule<int>
{
    private readonly int _min;
    private readonly int _max;
    private readonly string _fieldName;

    public RangeRule(string fieldName, int min, int max)
    {
        ArgumentException.ThrowIfNullOrEmpty(fieldName);
        _fieldName = fieldName;
        _min = min;
        _max = max;
    }

    public override bool Validate(int value) => value >= _min && value <= _max;
    public override string GetErrorMessage() => $"{_fieldName} must be between {_min} and {_max}";
}

/// <summary>
/// Composite validator for validating multiple rules
/// </summary>
public class CompositeValidator<T>
{
    private readonly List<ValidationRule<T>> _rules = new();
    private readonly List<string> _errors = new();

    public CompositeValidator<T> AddRule(ValidationRule<T> rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        _rules.Add(rule);
        return this;
    }

    public CompositeValidator<T> AddRules(params ValidationRule<T>[] rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        _rules.AddRange(rules);
        return this;
    }

    public bool Validate(T value)
    {
        _errors.Clear();

        foreach (var rule in _rules)
        {
            if (!rule.Validate(value))
            {
                _errors.Add(rule.GetErrorMessage());
            }
        }

        return _errors.Count == 0;
    }

    public List<string> GetErrors() => _errors;
    public string GetErrorsAsString() => string.Join("; ", _errors);
}

/// <summary>
/// Validator for URL format and accessibility
/// </summary>
public class UrlValidator
{
    /// <summary>
    /// Validates URL format
    /// </summary>
    public static bool IsValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// Extracts domain from URL
    /// </summary>
    public static string? ExtractDomain(string url)
    {
        if (!IsValidUrl(url))
            return null;

        var uri = new Uri(url);
        return uri.Host;
    }

    /// <summary>
    /// Checks if URL contains query parameters
    /// </summary>
    public static bool HasQueryParameters(string url)
    {
        if (!IsValidUrl(url))
            return false;

        var uri = new Uri(url);
        return !string.IsNullOrEmpty(uri.Query);
    }
}
