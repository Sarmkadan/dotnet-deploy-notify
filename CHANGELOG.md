# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-07-28

### Added
- Stable public API with full XML documentation
- NuGet package publishing workflow
- `PackageId` and full NuGet metadata in project file
- CodeQL security scanning workflow
- Dependabot configuration for NuGet and GitHub Actions

### Changed
- Promoted all interfaces to public surface
- Hardened retry backoff ceilings to prevent runaway retries
- Finalized `appsettings.json` schema with validation

### Fixed
- Edge case where `BatchProcessor` could double-deliver on transient HTTP 429

## [0.9.0] - 2025-07-07

### Added
- `RollbackService` and `RollbackRequest` model for deployment rollback signalling
- `RollbackServiceExtensions` for clean DI registration
- `NotificationSearchEngine` with full-text and status filtering
- `ExportService` for exporting notification history to JSON/CSV

### Changed
- `MetricsCollector` now tracks P95 and P99 latency per channel
- Increased default `WebhookTimeoutMs` from 5000 to 10000

## [0.8.0] - 2025-06-09

### Added
- `AuditService` with configurable retention policy (`RetentionDays`)
- `EventBus` for internal pub/sub between pipeline stages
- `RequestLogger` middleware captures full request/response pairs
- Background `NotificationWorker` for autonomous notification processing

### Changed
- `NotificationPipeline` refactored to use `EventBus` internally
- `ErrorHandlingMiddleware` now emits structured log entries on each failure

## [0.7.0] - 2025-05-19

### Added
- `BatchNotificationService` for grouping and scheduling batch deliveries
- `BatchNotification` and batch statistics models
- Partial success tracking: batches report per-item outcomes
- `CacheService` with in-memory TTL caching for channel configurations

### Fixed
- `WebhookDispatcher` was not propagating `CancellationToken` through retry loops

## [0.6.0] - 2025-04-28

### Added
- `TemplateService` with variable substitution (`{{ProjectName}}`, `{{Version}}`, `{{Status}}`, etc.)
- Preset templates for success, failure, and rollback scenarios
- `NotificationFormatter` for per-channel message shaping

### Changed
- `PayloadBuilder` split into channel-specific formatters (Slack Block Kit, Discord Embeds, Telegram HTML)
- `ChannelConfiguration` extended with `MessageTemplate` and `FormatOptions` properties

## [0.5.0] - 2025-04-07

### Added
- `HealthCheckService` reporting system-wide and per-channel health
- `MetricsService` for delivery analytics collection
- `ValidationService` with URL format, email format, and required-field checks
- `ConfigurationValidator` with human-readable suggestions on misconfiguration

### Changed
- `NotificationService` now calls `ValidationService` before dispatch
- Health endpoint reports connectivity status for each configured channel

## [0.4.0] - 2025-03-17

### Added
- `CommandParser` and `CommandHandler` for CLI-driven notification dispatch
- `RequestContext` for correlating requests across pipeline stages
- `Result<T>` monad in `Results/Result.cs` for explicit error propagation
- Guard extensions (`GuardExtensions`) for argument validation

### Changed
- `Program.cs` wired up full CLI argument parsing
- `DependencyInjection.cs` centralised all service registrations

## [0.3.0] - 2025-02-24

### Added
- `NotificationProcessor` for priority-based batch processing of pending notifications
- Dead-letter handling: failed notifications are marked and retried on next cycle
- `Repositories.cs` in-memory repository layer for notifications and results
- `CollectionExtensions`, `DateTimeExtensions`, `EnumExtensions`, `MathExtensions` utility helpers

### Fixed
- Retry counter was not persisted across processor cycles, causing infinite retries

## [0.2.0] - 2025-02-03

### Added
- Discord webhook support with Embed payload formatting
- Slack Block Kit payload builder
- Telegram HTML message formatter
- `ChannelStrategy` for per-channel dispatch routing
- `RetryPolicy` with configurable exponential backoff and jitter
- `StringExtensions` helpers (`TruncateWithEllipsis`, `ToKebabCase`, etc.)

### Changed
- `WebhookDispatcher` refactored to accept `IHttpClientFactory`
- Channel selection moved out of `NotificationService` into `ChannelStrategy`

## [0.1.0] - 2025-01-15

### Added
- Initial project scaffold targeting .NET 10
- `DeploymentNotification` core model with `ProjectName`, `Version`, `Status`, `Environment`, `BranchName`, and `CommitHash` fields
- `BuildStatus` and `NotificationChannel` enums
- `NotificationService` with single-channel HTTP webhook dispatch
- `WebhookDispatcher` using `HttpClient` with timeout configuration
- `WebhookPayload` and `WebhookPayloadBuilder` for generic webhook bodies
- `ChannelConfiguration` model with URL, auth token, and retry settings
- `NotificationResult` tracking delivery status and HTTP response details
- `NotificationException` for typed error handling
- `appsettings.json` with baseline configuration
- MIT license and initial README

[1.0.0]: https://github.com/sarmkadan/dotnet-deploy-notify/compare/v0.9.0...v1.0.0
[0.9.0]: https://github.com/sarmkadan/dotnet-deploy-notify/compare/v0.8.0...v0.9.0
[0.8.0]: https://github.com/sarmkadan/dotnet-deploy-notify/compare/v0.7.0...v0.8.0
[0.7.0]: https://github.com/sarmkadan/dotnet-deploy-notify/compare/v0.6.0...v0.7.0
[0.6.0]: https://github.com/sarmkadan/dotnet-deploy-notify/compare/v0.5.0...v0.6.0
[0.5.0]: https://github.com/sarmkadan/dotnet-deploy-notify/compare/v0.4.0...v0.5.0
[0.4.0]: https://github.com/sarmkadan/dotnet-deploy-notify/compare/v0.3.0...v0.4.0
[0.3.0]: https://github.com/sarmkadan/dotnet-deploy-notify/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/sarmkadan/dotnet-deploy-notify/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/sarmkadan/dotnet-deploy-notify/releases/tag/v0.1.0
