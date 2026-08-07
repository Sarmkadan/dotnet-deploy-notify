#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using DotNetDeployNotify.Core.Models;
using Microsoft.Extensions.Logging;

using System.Globalization;

namespace DotNetDeployNotify.Services;

/// <summary>
/// Template engine that supports user-defined named templates, conditional blocks,
/// value filters, and custom variables in addition to the standard notification fields
/// </summary>
public interface ICustomTemplateEngine
{
    /// <summary>Registers or updates a named template in the engine registry</summary>
    void RegisterTemplate(CustomTemplate template);

    /// <summary>Retrieves a named template from the registry</summary>
    CustomTemplate? GetTemplate(string name);

    /// <summary>Returns all active registered templates</summary>
    IReadOnlyList<CustomTemplate> ListTemplates();

    /// <summary>Removes a named template from the registry</summary>
    bool DeleteTemplate(string name);

    /// <summary>Renders a named template using notification data and optional custom variables</summary>
    string Render(string templateName, DeploymentNotification notification, Dictionary<string, string>? customVariables = null);

    /// <summary>Renders an inline template string (not from registry)</summary>
    string RenderInline(string templateContent, DeploymentNotification notification, Dictionary<string, string>? customVariables = null);

    /// <summary>Validates a template string and returns any errors found</summary>
    (bool IsValid, List<string> Errors) ValidateTemplate(string templateContent);

    /// <summary>Loads all built-in presets into the registry</summary>
    void LoadPresets();
}

/// <summary>
/// Implementation of <see cref="ICustomTemplateEngine"/> with support for
/// <c>{{Variable}}</c> substitution, <c>{{Variable | filter}}</c> value transforms,
/// and <c>{{#if Variable == "value"}}…{{/if}}</c> conditional blocks
/// </summary>
public sealed class CustomTemplateEngine : ICustomTemplateEngine
{
    private readonly ConcurrentDictionary<string, CustomTemplate> _registry
        = new(StringComparer.OrdinalIgnoreCase);

    private readonly ILogger<CustomTemplateEngine> _logger;

    private static readonly Regex VariablePattern   = new(@"\{\{(\w+)(?:\s*\|\s*(\w+))?\}\}", RegexOptions.Compiled);
    private static readonly Regex ConditionalPattern = new(@"\{\{#if\s+(\w+)\s*==\s*""([^""]*)""\s*\}\}(.*?)\{\{/if\}\}", RegexOptions.Compiled | RegexOptions.Singleline);

    private readonly Dictionary<string, Func<DeploymentNotification, string>> _builtins =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "ProjectName",     n => n.ProjectName },
            { "Version",         n => n.Version },
            { "Status",          n => n.Status.ToString() },
            { "Message",         n => n.Message },
            { "Environment",     n => n.TargetEnvironment.ToString() },
            { "Branch",          n => n.BranchName },
            { "CommitHash",      n => n.CommitHash },
            { "CommitHashShort", n => n.CommitHash[..Math.Min(7, n.CommitHash.Length)] },
            { "CommitAuthor",    n => n.CommitAuthor },
            { "RepositoryUrl",   n => n.RepositoryUrl },
            { "BuildUrl",        n => n.BuildUrl },
            { "Duration",        n => n.DurationSeconds?.ToString() ?? "N/A" },
            { "Priority",        n => n.Priority.ToString() },
            { "CreatedAt",       n => n.CreatedAt.ToString("O") },
            { "CreatedAtLocal",  n => n.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) }
        };

    private static readonly IReadOnlyDictionary<string, Func<string, string>> Filters
        = new Dictionary<string, Func<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            { "upper",    v => v.ToUpperInvariant() },
            { "lower",    v => v.ToLowerInvariant() },
            { "trim",     v => v.Trim() },
            { "truncate", v => v.Length > 50 ? v[..47] + "…" : v }
        };

    /// <summary>Initialises the engine with its logger dependency</summary>
    public CustomTemplateEngine(ILogger<CustomTemplateEngine> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Registers or replaces a named template in the engine registry
    /// </summary>
    public void RegisterTemplate(CustomTemplate template)
    {
        ArgumentNullException.ThrowIfNull(template);
        if (string.IsNullOrWhiteSpace(template.Name))
            throw new ArgumentException("Template name must not be empty", nameof(template));

        _logger.LogInformation("RegisterTemplate called for template {TemplateName}", template.Name);

        template.Touch();
        _registry[template.Name] = template;
        _logger.LogDebug("Template registered: {Name}", template.Name);
        _logger.LogInformation("RegisterTemplate completed for template {TemplateName}", template.Name);
    }

    /// <summary>
    /// Retrieves a template by name, or null if not found
    /// </summary>
    public CustomTemplate? GetTemplate(string name)
    {
        _logger.LogInformation("GetTemplate called for template {TemplateName}", name);

        _registry.TryGetValue(name, out var template);

        _logger.LogInformation("GetTemplate for template {TemplateName} found: {TemplateFound}", name, template is not null);
        return template;
    }

    /// <summary>
    /// Returns all active (non-deleted) templates in the registry
    /// </summary>
    public IReadOnlyList<CustomTemplate> ListTemplates()
    {
        _logger.LogInformation("ListTemplates called");

        var templates = _registry.Values.Where(t => t.IsActive).OrderBy(t => t.Name).ToList();

        _logger.LogInformation("ListTemplates returning {TemplateCount} active templates", templates.Count);
        return templates;
    }

    /// <summary>
    /// Soft-deletes a named template from the registry
    /// </summary>
    public bool DeleteTemplate(string name)
    {
        _logger.LogInformation("DeleteTemplate called for template {TemplateName}", name);

        if (_registry.TryGetValue(name, out var template))
        {
            template.IsActive = false;
            _logger.LogDebug("Template deleted: {Name}", name);
            _logger.LogInformation("DeleteTemplate soft-deleted template {TemplateName}", name);
            return true;
        }

        _logger.LogWarning("DeleteTemplate could not find template {TemplateName}; nothing was deleted", name);
        return false;
    }

    /// <summary>
    /// Renders a named template using notification data and optional custom variables
    /// </summary>
    public string Render(string templateName, DeploymentNotification notification, Dictionary<string, string>? customVariables = null)
    {
        _logger.LogInformation("Render called for template {TemplateName}", templateName);

        if (!_registry.TryGetValue(templateName, out var template) || !template.IsActive)
        {
            _logger.LogWarning("Template not found: {Name}", templateName);
            throw new KeyNotFoundException($"Template '{templateName}' not found in registry");
        }

        var rendered = RenderInline(template.Content, notification, customVariables);

        _logger.LogInformation("Render completed for template {TemplateName}", templateName);
        return rendered;
    }

    /// <summary>
    /// Renders an inline template string against notification data
    /// </summary>
    public string RenderInline(string templateContent, DeploymentNotification notification, Dictionary<string, string>? customVariables = null)
    {
        _logger.LogInformation("RenderInline called with template length {TemplateLength}", templateContent?.Length ?? 0);

        if (string.IsNullOrWhiteSpace(templateContent))
        {
            _logger.LogWarning("RenderInline received empty template content; returning empty string");
            return string.Empty;
        }

        // Resolve conditionals first so inner variables are substituted after
        var result = ResolveConditionals(templateContent, notification, customVariables);
        result = ResolveVariables(result, notification, customVariables);

        _logger.LogInformation("RenderInline completed with rendered length {RenderedLength}", result.Length);
        return result;
    }

    /// <summary>
    /// Validates template syntax and variable references
    /// </summary>
    public (bool IsValid, List<string> Errors) ValidateTemplate(string templateContent)
    {
        _logger.LogInformation("ValidateTemplate called with template length {TemplateLength}", templateContent?.Length ?? 0);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(templateContent))
            return (true, errors);

        // Check for unbalanced conditional blocks
        var openCount  = Regex.Matches(templateContent, @"\{\{#if\s").Count;
        var closeCount = Regex.Matches(templateContent, @"\{\{/if\}\}").Count;
        if (openCount != closeCount)
            errors.Add($"Unbalanced conditional blocks: {openCount} opening vs {closeCount} closing");

        // Check variable references outside conditionals
        var variableMatches = VariablePattern.Matches(templateContent);
        foreach (Match match in variableMatches)
        {
            var varName    = match.Groups[1].Value;
            var filterName = match.Groups[2].Success ? match.Groups[2].Value : null;

            if (!_builtins.ContainsKey(varName))
                errors.Add($"Unknown variable: {varName}");

            if (filterName != null && !Filters.ContainsKey(filterName))
                errors.Add($"Unknown filter '{filterName}' on variable '{varName}'");
        }

        if (errors.Count > 0)
        {
            _logger.LogWarning("ValidateTemplate found {ErrorCount} validation error(s): {ValidationErrors}", errors.Count, string.Join("; ", errors));
        }

        _logger.LogInformation("ValidateTemplate completed with result {IsValid}", errors.Count == 0);
        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// Loads built-in preset templates into the engine registry
    /// </summary>
    public void LoadPresets()
    {
        _logger.LogInformation("LoadPresets called");

        var presets = new[]
        {
            new CustomTemplate
            {
                Name        = "SuccessAlert",
                Description = "Brief success notification",
                Category    = "Preset",
                Content     = "✅ {{ProjectName}} v{{Version}} deployed successfully to {{Environment}}\n" +
                              "Branch: {{Branch}} | Author: {{CommitAuthor}}"
            },
            new CustomTemplate
            {
                Name        = "FailureAlert",
                Description = "Urgent failure notification",
                Category    = "Preset",
                Content     = "❌ {{ProjectName}} v{{Version}} deployment FAILED on {{Environment}}\n" +
                              "{{#if Message == \"\"}}No details provided{{/if}}\n" +
                              "{{Message}}\nBuild: {{BuildUrl}}"
            },
            new CustomTemplate
            {
                Name        = "DetailedDeployment",
                Description = "Full deployment context with commit info",
                Category    = "Preset",
                Content     = "[{{Status | upper}}] {{ProjectName}} v{{Version}}\n" +
                              "Environment: {{Environment}}\n" +
                              "Branch: {{Branch}}\n" +
                              "Commit: {{CommitHashShort}} by {{CommitAuthor}}\n" +
                              "Duration: {{Duration}}s\n" +
                              "{{Message}}"
            },
            new CustomTemplate
            {
                Name        = "SlackRich",
                Description = "Slack-formatted deployment with markdown",
                Category    = "Preset",
                Content     = "*[{{Status}}] {{ProjectName}}* v`{{Version}}`\n" +
                              "Environment: `{{Environment}}`  Branch: `{{Branch}}`\n" +
                              "Commit: `{{CommitHashShort}}` — {{CommitAuthor}}\n" +
                              "{{Message}}"
            }
        };

        foreach (var preset in presets)
            RegisterTemplate(preset);

        _logger.LogInformation("Loaded {Count} built-in template presets", presets.Length);
    }

    // ─── Private helpers ───────────────────────────────────────────────────────

    private string ResolveConditionals(
        string content,
        DeploymentNotification notification,
        Dictionary<string, string>? customVariables)
    {
        return ConditionalPattern.Replace(content, match =>
        {
            var varName       = match.Groups[1].Value;
            var expectedValue = match.Groups[2].Value;
            var innerContent  = match.Groups[3].Value;

            var actualValue = ResolveValue(varName, notification, customVariables);
            return string.Equals(actualValue, expectedValue, StringComparison.OrdinalIgnoreCase)
                ? innerContent
                : string.Empty;
        });
    }

    private string ResolveVariables(
        string content,
        DeploymentNotification notification,
        Dictionary<string, string>? customVariables)
    {
        return VariablePattern.Replace(content, match =>
        {
            var varName    = match.Groups[1].Value;
            var filterName = match.Groups[2].Success ? match.Groups[2].Value : null;
            var value      = ResolveValue(varName, notification, customVariables);

            if (filterName != null && Filters.TryGetValue(filterName, out var filter))
                value = filter(value);

            return value;
        });
    }

    private string ResolveValue(
        string varName,
        DeploymentNotification notification,
        Dictionary<string, string>? customVariables)
    {
        if (customVariables != null && customVariables.TryGetValue(varName, out var custom))
            return custom;

        if (_builtins.TryGetValue(varName, out var getter))
        {
            try { return getter(notification) ?? string.Empty; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving variable {Variable}", varName);
                return $"{{ERROR:{varName}}}";
            }
        }

        _logger.LogWarning("Variable {Variable} is not a built-in variable and no custom value was provided; leaving placeholder unchanged", varName);
        return $"{{{{{varName}}}}}";
    }
}
