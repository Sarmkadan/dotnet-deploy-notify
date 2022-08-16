using System;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Core;

var validRequest = new RollbackRequest
{
    Id = Guid.NewGuid().ToString(),
    ProjectName = "TestProject",
    TargetVersion = "1.0.0",
    CurrentVersion = "2.0.0",
    TargetEnvironment = Environment.Development,
    RequestedBy = "test-user",
    Reason = "Testing validation",
    Channels = new() { NotificationChannel.Telegram },
    Priority = NotificationPriority.High,
    CreatedAt = DateTime.UtcNow,
    Metadata = new() { { "key", "value" } }
};

Console.WriteLine("Testing valid request:");
var validationErrors = validRequest.Validate();
Console.WriteLine($"IsValid: {validRequest.IsValid()}");
Console.WriteLine($"Validation errors count: {validationErrors.Count}");
if (validationErrors.Count > 0)
{
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"  - {error}");
    }
}

Console.WriteLine("\nTesting invalid request:");
var invalidRequest = new RollbackRequest
{
    Id = "",
    ProjectName = "",
    TargetVersion = "",
    CurrentVersion = "",
    TargetEnvironment = (Environment)999, // Invalid enum value
    RequestedBy = "",
    Reason = "",
    Channels = new(), // Empty channels
    Priority = (NotificationPriority)999, // Invalid enum value
    CreatedAt = default,
    Metadata = null
};

try
{
    invalidRequest.EnsureValid();
    Console.WriteLine("ERROR: Should have thrown exception!");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Correctly threw exception: {ex.Message}");
}

var invalidErrors = invalidRequest.Validate();
Console.WriteLine($"Validation errors count: {invalidErrors.Count}");
foreach (var error in invalidErrors)
{
    Console.WriteLine($"  - {error}");
}