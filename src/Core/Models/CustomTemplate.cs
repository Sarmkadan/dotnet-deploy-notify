#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Core.Models;

/// <summary>
/// A user-defined named notification template stored in the engine registry
/// </summary>
public sealed class CustomTemplate
{
    /// <summary>Unique identifier for this template</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Unique name used to retrieve the template</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Human-readable description of this template's purpose</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Template content with variable placeholders and optional conditional blocks</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>UTC timestamp when the template was registered</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp of the last content update</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Optional category tag for organising templates</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Marks the template as active or soft-deleted</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Records that the template content was modified
    /// </summary>
    public void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
