# Contributing to DotNetDeployNotify

Thank you for your interest in contributing to DotNetDeployNotify! This document provides guidelines and instructions for getting involved.

## Code of Conduct

This project adheres to the Contributor Covenant Code of Conduct. By participating, you are expected to uphold this code. Please read [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) for details.

## How to Contribute

### Reporting Issues

- Use [GitHub Issues](https://github.com/sarmkadan/dotnet-deploy-notify/issues) to report bugs or request features
- Check existing issues first to avoid duplicates
- Provide clear, descriptive titles and detailed descriptions
- Include steps to reproduce for bugs
- Specify your .NET version and OS

### Security Vulnerabilities

**Do not open public GitHub issues for security vulnerabilities.** See [SECURITY.md](SECURITY.md) for responsible disclosure procedures.

## Development Setup

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later
- Git
- A text editor or IDE (Visual Studio, Visual Studio Code, Rider, etc.)

### Getting Started

1. **Fork the repository**
   ```bash
   # Visit https://github.com/sarmkadan/dotnet-deploy-notify and click "Fork"
   ```

2. **Clone your fork**
   ```bash
   git clone https://github.com/YOUR_USERNAME/dotnet-deploy-notify.git
   cd dotnet-deploy-notify
   ```

3. **Add upstream remote**
   ```bash
   git remote add upstream https://github.com/sarmkadan/dotnet-deploy-notify.git
   ```

4. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   # or
   git checkout -b fix/your-bug-fix
   ```

5. **Build the project**
   ```bash
   dotnet build
   ```

6. **Run the application**
   ```bash
   dotnet run
   ```

## Making Changes

### Code Style

- Follow existing code conventions in the repository
- Use meaningful variable and method names
- Keep methods focused and reasonably sized
- Format code consistently with the existing style

### XML Documentation

- Add XML documentation comments (`///`) to public classes and methods
- Include `<summary>`, `<param>`, and `<returns>` tags where applicable
- Documentation should clearly explain purpose and usage

Example:
```csharp
/// <summary>
/// Sends a deployment notification to configured channels
/// </summary>
/// <param name="notification">The notification to send</param>
/// <returns>Delivery result for each channel</returns>
public async Task<NotificationResult> SendNotificationAsync(DeploymentNotification notification)
{
    // implementation
}
```

### Author Headers

All source files must retain the author header format:
```csharp
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================
```

Do not modify or remove existing author headers when editing files.

### Testing

- Write tests for new features and bug fixes
- Ensure all existing tests pass
- Run tests locally before submitting a pull request

```bash
dotnet test
```

## Submitting Changes

### Pull Request Process

1. **Sync with upstream**
   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

2. **Commit your changes**
   - Write clear, descriptive commit messages
   - Keep commits logical and atomic
   - Reference issues when applicable (e.g., "Fixes #123")

3. **Push to your fork**
   ```bash
   git push origin feature/your-feature-name
   ```

4. **Create a Pull Request**
   - Use a clear title describing the change
   - Reference related issues
   - Explain what was changed and why
   - Include any breaking changes or migration notes

5. **Respond to feedback**
   - Be open to suggestions and constructive criticism
   - Request re-review after making changes
   - Keep discussions professional and respectful

### Pull Request Guidelines

- Target the `main` branch
- Keep changes focused on a single concern
- Avoid unrelated formatting or refactoring
- Ensure CI checks pass
- Provide context and rationale in the description

## Architecture & Project Structure

The project is organized as follows:

- `src/Core/` - Domain models and contracts
- `src/Services/` - Business logic and orchestration
- `src/Data/` - Data access layer
- `src/Infrastructure/` - Configuration and utilities
- `src/Channels/` - Channel-specific implementations
- `src/Middleware/` - Request processing pipeline
- `src/BackgroundWorkers/` - Async processing

Maintain this structure when adding new features.

## Documentation

- Update `README.md` for user-facing changes
- Update relevant code documentation for technical changes
- Keep examples up-to-date

## Questions?

- Check existing [GitHub Issues](https://github.com/sarmkadan/dotnet-deploy-notify/issues)
- Review the [README.md](README.md) and code documentation
- Feel free to open a discussion issue if you have questions

## License

By contributing to DotNetDeployNotify, you agree that your contributions will be licensed under the MIT License. See [LICENSE](LICENSE) for details.

---

Thank you for contributing to DotNetDeployNotify!
