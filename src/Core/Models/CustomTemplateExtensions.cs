using System;
using System.Collections.Generic;

namespace DotNetDeployNotify.Core.Models
{
    /// <summary>
    /// Provides extension methods for the <see cref="CustomTemplate"/> class.
    /// </summary>
    public static class CustomTemplateExtensions
    {
        /// <summary>
        /// Generates a summary string containing key metadata about the template.
        /// </summary>
        /// <param name="template">The template to summarize.</param>
        /// <returns>Formatted summary string with template metadata.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when template name is empty.</exception>
        public static string GenerateSummary(this CustomTemplate template)
        {
            ArgumentNullException.ThrowIfNull(template);
            ArgumentException.ThrowIfNullOrEmpty(template.Name);
            
            return $"Template '{template.Name}' (ID: {template.Id}, Category: {template.Category}) " +
                   $"Created: {template.CreatedAt:O}, Last Updated: {template.UpdatedAt:O}";
        }

        /// <summary>
        /// Determines if the template is considered outdated based on last update time.
        /// </summary>
        /// <param name="template">The template to check.</param>
        /// <param name="maxAgeInDays">Maximum age in days before considered outdated.</param>
        /// <returns>True if template is outdated, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when maxAgeInDays is less than or equal to 0.</exception>
        public static bool IsOutdated(this CustomTemplate template, int maxAgeInDays)
        {
            ArgumentNullException.ThrowIfNull(template);
            ArgumentOutOfRangeException.ThrowIfLessThan(maxAgeInDays, 1);
            
            return template.UpdatedAt < DateTime.UtcNow.Subtract(TimeSpan.FromDays(maxAgeInDays));
        }

        /// <summary>
        /// Creates a formatted preview of the template content.
        /// </summary>
        /// <param name="template">The template to preview.</param>
        /// <param name="maxLength">Maximum length of the preview text.</param>
        /// <returns>Truncated content with ellipsis if truncated.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when maxLength is less than or equal to 0.</exception>
        public static string GenerateContentPreview(this CustomTemplate template, int maxLength = 100)
        {
            ArgumentNullException.ThrowIfNull(template);
            ArgumentOutOfRangeException.ThrowIfLessThan(maxLength, 1);
            
            if (string.IsNullOrEmpty(template.Content))
                return "[Empty content]";
                
            return template.Content.Length > maxLength 
                ? template.Content.Substring(0, maxLength) + "..." 
                : template.Content;
        }
    }
}
