#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core.Models;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

using System.Globalization;

namespace DotNetDeployNotify.Services;

/// <summary>
/// Interface for rendering notification message templates
/// </summary>
public interface ITemplateService
{
    /// <summary>Renders a template string with notification variables</summary>
    string RenderTemplate(string template, DeploymentNotification notification);

    /// <summary>Gets available template variables</summary>
    List<string> GetAvailableVariables();

    /// <summary>Validates a template string</summary>
    (bool IsValid, List<string> Errors) ValidateTemplate(string template);

    /// <summary>Gets preset templates</summary>
    Dictionary<string, string> GetPresetTemplates();

    /// <summary>Renders HTML-safe version of message</summary>
    string RenderHtmlSafe(string template, DeploymentNotification notification);
}

/// <summary>
/// Implementation of template service
/// </summary>
public class TemplateService : ITemplateService
{
    private readonly ILogger<TemplateService> _logger;

    private readonly Dictionary<string, Func<DeploymentNotification, string>> _variables =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "ProjectName", n => n.ProjectName },
            { "Version", n => n.Version },
            { "Status", n => n.Status.ToString() },
            { "Message", n => n.Message },
            { "Environment", n => n.TargetEnvironment.ToString() },
            { "Branch", n => n.BranchName },
            { "CommitHash", n => n.CommitHash },
            { "CommitHashShort", n => n.CommitHash[..Math.Min(7, n.CommitHash.Length)] },
            { "CommitAuthor", n => n.CommitAuthor },
            { "RepositoryUrl", n => n.RepositoryUrl },
            { "BuildUrl", n => n.BuildUrl },
            { "Duration", n => n.DurationSeconds?.ToString() ?? "N/A" },
            { "Priority", n => n.Priority.ToString() },
            { "CreatedAt", n => n.CreatedAt.ToString("O") },
            { "CreatedAtLocal", n => n.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) }
        };

    /// <summary>Initializes the template service</summary>
    public TemplateService(ILogger<TemplateService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Renders a template string by replacing variables with notification data
    /// </summary>
    public string RenderTemplate(string? template, DeploymentNotification? notification)
    {
        if (string.IsNullOrWhiteSpace(template))
            return string.Empty;

        var result = template;
        var pattern = @"\{\{(\w+)\}\}";

        result = Regex.Replace(result, pattern, match =>
        {
            var variableName = match.Groups[1].Value;
            if (_variables.TryGetValue(variableName, out var getter))
            {
                try
                {
                    return getter(notification) ?? string.Empty;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error rendering variable {Variable}", variableName);
                    return $"{{ERROR: {variableName}}}";
                }
            }

            return match.Value;
        });

        return result;
    }

    /// <summary>
    /// Gets list of available template variables
    /// </summary>
    public List<string> GetAvailableVariables()
    {
        return _variables.Keys.ToList();
    }

    /// <summary>
    /// Validates a template for correct syntax
    /// </summary>
    public (bool IsValid, List<string> Errors) ValidateTemplate(string template)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(template))
        {
            return (true, errors);
        }

        // Check for mismatched braces
        var openBraces = template.Count(c => c == '{');
        var closeBraces = template.Count(c => c == '}');
        if (openBraces != closeBraces)
        {
            errors.Add("Mismatched curly braces");
        }

        // Check for valid variable names
        var pattern = @"\{\{(\w+)\}\}";
        var matches = Regex.Matches(template, pattern);
        foreach (Match match in matches)
        {
            var variableName = match.Groups[1].Value;
            if (!_variables.ContainsKey(variableName))
            {
                errors.Add($"Unknown variable: {variableName}");
            }
        }

        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// Gets preset message templates
    /// </summary>
    public Dictionary<string, string> GetPresetTemplates()
    {
        return new Dictionary<string, string>
        {
            {
                "Simple",
                "[{{Status}}] {{ProjectName}} v{{Version}} - {{Environment}}"
            },
            {
                "Detailed",
                "[{{Status}}] {{ProjectName}} v{{Version}}\n" +
                "Environment: {{Environment}}\n" +
                "Branch: {{Branch}}\n" +
                "Message: {{Message}}"
            },
            {
                "WithCommit",
                "[{{Status}}] {{ProjectName}} v{{Version}}\n" +
                "Branch: {{Branch}}\n" +
                "Commit: {{CommitHashShort}} by {{CommitAuthor}}\n" +
                "Message: {{Message}}"
            },
            {
                "SlackFormatted",
                ":{{Status}}: *{{ProjectName}}* v{{Version}}\n" +
                "Environment: `{{Environment}}`\n" +
                "Branch: `{{Branch}}`\n" +
                "{{Message}}"
            },
            {
                "Comprehensive",
                "*Deployment Notification*\n" +
                "Project: {{ProjectName}}\n" +
                "Version: {{Version}}\n" +
                "Status: {{Status}}\n" +
                "Environment: {{Environment}}\n" +
                "Branch: {{Branch}}\n" +
                "Commit: {{CommitHashShort}}\n" +
                "Author: {{CommitAuthor}}\n" +
                "Duration: {{Duration}} seconds\n" +
                "Message: {{Message}}\n" +
                "Build: {{BuildUrl}}"
            },
            {
                "SuccessNotification",
                "✅ {{ProjectName}} v{{Version}} deployed successfully to {{Environment}}\n" +
                "Branch: {{Branch}} | Commit: {{CommitHashShort}} by {{CommitAuthor}}"
            },
            {
                "FailedNotification",
                "❌ {{ProjectName}} v{{Version}} deployment FAILED on {{Environment}}\n" +
                "Branch: {{Branch}}\n" +
                "{{Message}}\n" +
                "Build: {{BuildUrl}}"
            }
        };
    }

    /// <summary>
    /// Renders an HTML-safe version of the template
    /// </summary>
    public string RenderHtmlSafe(string template, DeploymentNotification notification)
    {
        var rendered = RenderTemplate(template, notification);
        return System.Net.WebUtility.HtmlEncode(rendered);
    }
}
