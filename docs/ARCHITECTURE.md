# Architecture

## Overview

`dotnet-deploy-notify` is a console application (single executable project, `dotnet-deploy-notify.csproj`, net10.0) that sends deployment notifications to webhook-based channels (Slack, Discord, Telegram, Teams-style generic webhooks). It is not an ASP.NET service: `Program.Main` builds an `IConfiguration`, wires a plain `ServiceCollection`, and runs a demo flow (creates sample channel configurations, sends sample notifications, then blocks on `Task.Delay(Timeout.Infinite)`).

The composition root that the entry point actually uses is `DotNetDeployNotify.Infrastructure.DependencyInjection.AddNotificationServices(IConfiguration)`. A second, larger set of registration helpers exists in `src/DependencyInjection/ServiceCollectionExtensions.cs` (CLI, caching, event bus, middleware pipeline, background workers, integration) plus a fluent `ServiceConfigurationBuilder` — but `Program.cs` does not call them; they are opt-in for library-style consumers.

## Module breakdown (src/)

- **Core** — enums (`NotificationChannel`, `BuildStatus`, `Environment`), domain models (`DeploymentNotification`, `ChannelConfiguration`, `NotificationResult`, `WebhookPayload`, ...), exceptions. No dependencies on other modules.
- **Configuration** — `DotnetDeployNotifyOptions` (section `DotnetDeployNotify`) with nested `NotificationConfig` (retries, timeouts, per-environment channel map) and `CanaryOptions` (section `CanaryDeployment`). Validated via DataAnnotations + `ValidateOnStart()`.
- **Services** — the working core:
  - `NotificationService` (`INotificationService`) — orchestrates create/send; depends on the three repositories, `IWebhookDispatcher`, `IValidationService`.
  - `WebhookDispatcher` (`IWebhookDispatcher`) — HTTP delivery via an injected `HttpClient` (registered as a typed client so headers configured in DI are applied), uses `IPayloadBuilder` to build channel-specific payloads.
  - `PayloadBuilder`, `ValidationService`, `DryRunRenderer`, `TemplateService`/`CustomTemplateEngine`, `DeploymentHistoryService`, `RollbackNotificationService`/`RollbackService`, `BatchNotificationService`/`BatchProcessor`, `HealthCheckService`, `MetricsService`, `AuditService`.
- **Data** — `Repositories.cs`: three in-memory repositories (`NotificationRepository`, `ChannelConfigRepository`, `NotificationResultRepository`), each a `List<T>` guarded by a `lock`. Registered as singletons — this is the only reason state survives across scopes; there is no persistent storage despite `NotificationConfig.StorageType`/`StoragePath` existing in options.
- **Channels** — strategy pattern: `IChannelStrategy`, `BaseChannelStrategy`, concrete `SlackChannelStrategy`/`DiscordChannelStrategy`/`TelegramChannelStrategy`, a `ChannelStrategyResolver` and `ChannelAdapter`. Strategies are constructed manually (see examples); they are not registered in either composition root.
- **Canary** — `CanaryDeploymentEngine`, `TrafficSplitter`, driven by `CanaryOptions` (linear/step rollout, auto-rollback flags). Standalone; not wired into `Program.cs`.
- **BackgroundWorkers** — `NotificationWorker.cs` hosts `NotificationProcessingWorker`, `HealthCheckWorker`, `ScheduledTaskWorker` (`IHostedService`s). Only started if a consumer uses `AddBackgroundWorkers()` inside a Generic Host; the console demo never starts them.
- **Events** — `EventBus.cs`: `IEventBus`/`InMemoryEventBus`, `NotificationCreatedEvent`, `ChannelDeliveryFailedEvent` and handlers. Handlers must be attached explicitly via `RegisterEventHandlers(...)`.
- **Middleware** — `NotificationPipeline` with pluggable processors (Validation/Enrichment/Filter/Sanitization) and interceptors (error handling, rate limiting, logging, performance). Composed via `ConfigureNotificationPipeline(...)`.
- **Caching, Formatters/Formatting, Serialization, Export, Search, Monitoring, Results, Utilities, Validation** — supporting libraries (memory cache, formatter factory, JSON helpers, `Result<T>`, guard/string/date extensions, `RetryPolicy`).

Many files follow a generated `*Extensions.cs` / `*JsonExtensions.cs` / `*Validation.cs` triple per type; these are auxiliary helpers, not separate subsystems.

## Data flow (the path Program.cs exercises)

1. Configuration is read from `appsettings.json` + environment variables and bound to `DotnetDeployNotifyOptions`.
2. `AddNotificationServices` builds initial `ChannelConfiguration`s from `Notification.EnvironmentChannels` (environment name and `ChannelType` parsed leniently; unknown channel type falls back to Slack) and seeds the singleton `ChannelConfigRepository` with them.
3. `INotificationService.CreateNotificationAsync` validates a `DeploymentNotification` and stores it in the in-memory `NotificationRepository`.
4. `SendNotificationAsync` / `SendPendingNotificationsAsync` resolves matching channel configs, calls `IWebhookDispatcher.SendToWebhookAsync` per channel; `PayloadBuilder` shapes the JSON per channel type; results are persisted to `NotificationResultRepository`.

## Key design decisions and trade-offs

- **In-memory repositories behind interfaces** — keeps the demo dependency-free; the interfaces (`INotificationRepository` etc.) are the intended seam for a persistent implementation. Trade-off: all state is lost on restart, and `StorageType`/`StoragePath` options are currently decorative.
- **Two composition roots** — `Infrastructure.DependencyInjection` is the minimal "make notifications work" set used by the app; `DependencyInjection.ServiceCollectionExtensions` exposes the optional subsystems à la carte. Trade-off: easy to assume the event bus/pipeline/workers are active when they are not.
- **Typed HttpClient for the dispatcher** — `AddHttpClient<IWebhookDispatcher, WebhookDispatcher>()` ensures the configured `HttpClient` (User-Agent, `X-Client-Name`) is the one injected. A parallel plain `AddScoped` registration would resolve the default unnamed client and silently lose those headers (this was a real bug, fixed).
- **Strategy pattern for channels alongside `PayloadBuilder`** — two overlapping mechanisms exist for channel-specific behavior; the dispatcher path uses `PayloadBuilder`, the strategies are for consumers wiring `ChannelStrategyResolver` themselves.

## Extension points

- Implement `INotificationRepository` / `IChannelConfigRepository` / `INotificationResultRepository` for real storage; swap the singleton registrations.
- Implement `IChannelStrategy` and register with `ChannelStrategyResolver` for a new channel type; or extend `PayloadBuilder` for the dispatcher path.
- Add `INotificationProcessor` steps to `NotificationPipeline` via `Use(...)`.
- Subscribe additional `IEventHandler<T>` to `InMemoryEventBus`.
- `ServiceConfigurationBuilder.WithX()` methods for composing optional subsystems.

## Known limitations

- No persistence: everything lives in process memory.
- `Program.cs` is a demo harness, not a production entry point; background workers, event bus, and the middleware pipeline are not active in it.
- `ValidateOnStart()` on options only fires eagerly under a Generic Host; with the plain `ServiceCollection` in `Program.cs`, validation happens on first resolution of the options.
- Channel/environment parsing in `BuildChannelConfigsFromSettings` silently falls back (unknown channel → Slack, unknown environment → no allowed environments) rather than failing fast.
