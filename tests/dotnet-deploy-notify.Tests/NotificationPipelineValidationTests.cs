using System;
using System.Collections.Generic;
using DotNetDeployNotify.Middleware;
using DotNetDeployNotify.Core.Models;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests
{
    public class NotificationPipelineValidationTests
    {
        private static PipelineResult CreateValidResult()
        {
            return new PipelineResult
            {
                Notification = new DeploymentNotification(),
                ProcessedNotification = new DeploymentNotification(),
                Errors = new List<string>(),
                Success = true
            };
        }

        [Fact]
        public void Validate_ReturnsEmpty_WhenResultIsValid()
        {
            // Arrange
            var result = CreateValidResult();

            // Act
            var errors = result.Validate();

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_ReturnsErrors_ForNullPropertiesAndInvalidSuccess()
        {
            // Arrange
            var result = new PipelineResult
            {
                Notification = null,
                ProcessedNotification = null,
                Errors = null,
                Success = true
            };

            // Act
            var errors = result.Validate();

            // Assert
            errors.Should().Contain("Notification is required.");
            errors.Should().Contain("ProcessedNotification is required.");
            errors.Should().Contain("Errors collection is required.");
        }

        [Fact]
        public void IsValid_ReturnsTrue_ForValidResult()
        {
            // Arrange
            var result = CreateValidResult();

            // Act
            var isValid = result.IsValid();

            // Assert
            isValid.Should().BeTrue();
        }

        [Fact]
        public void IsValid_ReturnsFalse_WhenSuccessFlagInconsistentWithErrors()
        {
            // Arrange
            var result = new PipelineResult
            {
                Notification = new DeploymentNotification(),
                ProcessedNotification = new DeploymentNotification(),
                Errors = new List<string> { "Some error" },
                Success = true // inconsistent: errors exist but Success is true
            };

            // Act
            var isValid = result.IsValid();

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public void EnsureValid_DoesNotThrow_WhenResultIsValid()
        {
            // Arrange
            var result = CreateValidResult();

            // Act / Assert
            Action act = () => result.EnsureValid();
            act.Should().NotThrow();
        }

        [Fact]
        public void EnsureValid_ThrowsArgumentException_WithAllValidationMessages_WhenInvalid()
        {
            // Arrange
            var result = new PipelineResult
            {
                Notification = null,
                ProcessedNotification = null,
                Errors = new List<string> { "Error1", "Error2" },
                Success = true // also inconsistent with Errors.Count > 0
            };

            // Act
            Action act = () => result.EnsureValid();

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*PipelineResult validation failed*")
                .Where(ex => ex.Message.Contains("Notification is required.") &&
                             ex.Message.Contains("ProcessedNotification is required.") &&
                             ex.Message.Contains("Pipeline cannot be marked as successful when errors exist."));
        }
    }
}
