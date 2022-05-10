// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;

namespace DotNetDeployNotify.CLI;

/// <summary>
/// Parses command-line arguments with support for subcommands, options, and flags
/// </summary>
public class CommandParser
{
    private readonly Dictionary<string, CommandDefinition> _commands = new();
    private readonly ILogger<CommandParser> _logger;

    public CommandParser(ILogger<CommandParser> logger)
    {
        _logger = logger;
        RegisterBuiltInCommands();
    }

    /// <summary>
    /// Registers a new command definition
    /// </summary>
    public void RegisterCommand(CommandDefinition definition)
    {
        if (_commands.ContainsKey(definition.Name))
            throw new InvalidOperationException($"Command '{definition.Name}' is already registered");

        _commands[definition.Name] = definition;
        _logger.LogDebug("Registered command: {CommandName}", definition.Name);
    }

    /// <summary>
    /// Parses command-line arguments into a structured command
    /// </summary>
    public ParsedCommand Parse(string[] args)
    {
        if (args == null || args.Length == 0)
            return CreateHelpCommand();

        var commandName = args[0].ToLowerInvariant();

        // Handle help flag
        if (commandName is "-h" or "--help" or "help")
            return CreateHelpCommand();

        // Handle version flag
        if (commandName is "-v" or "--version" or "version")
            return CreateVersionCommand();

        if (!_commands.TryGetValue(commandName, out var definition))
            return new ParsedCommand { Success = false, Error = $"Unknown command: {commandName}" };

        return ParseCommandArguments(definition, args.Skip(1).ToArray());
    }

    /// <summary>
    /// Parses arguments for a specific command definition
    /// </summary>
    private ParsedCommand ParseCommandArguments(CommandDefinition definition, string[] args)
    {
        var command = new ParsedCommand { CommandName = definition.Name };
        var remainingArgs = new List<string>(args);
        var paramIndex = 0;

        // Process all arguments
        for (int i = 0; i < remainingArgs.Count; i++)
        {
            var arg = remainingArgs[i];

            if (arg.StartsWith("--"))
            {
                // Long option
                ProcessLongOption(arg, remainingArgs, ref i, definition, command);
            }
            else if (arg.StartsWith("-") && arg != "-")
            {
                // Short option
                ProcessShortOption(arg, remainingArgs, ref i, definition, command);
            }
            else
            {
                // Positional argument
                if (paramIndex < definition.Parameters.Count)
                {
                    command.Parameters[definition.Parameters[paramIndex].Name] = arg;
                    paramIndex++;
                }
            }
        }

        // Validate required parameters
        foreach (var param in definition.Parameters.Where(p => p.IsRequired))
        {
            if (!command.Parameters.ContainsKey(param.Name))
            {
                command.Success = false;
                command.Error = $"Missing required parameter: {param.Name}";
                return command;
            }
        }

        command.Success = true;
        return command;
    }

    /// <summary>
    /// Processes long-form options (--option-name value)
    /// </summary>
    private void ProcessLongOption(string arg, List<string> args, ref int index,
        CommandDefinition definition, ParsedCommand command)
    {
        var parts = arg.Substring(2).Split('=', 2);
        var optionName = parts[0];
        var optionValue = parts.Length > 1 ? parts[1] : null;

        var option = definition.Options.FirstOrDefault(o => o.Name == optionName);
        if (option == null)
        {
            command.Success = false;
            command.Error = $"Unknown option: --{optionName}";
            return;
        }

        if (option.IsFlag)
        {
            command.Options[optionName] = "true";
        }
        else
        {
            if (optionValue == null && index + 1 < args.Count)
            {
                index++;
                optionValue = args[index];
            }

            if (optionValue == null)
            {
                command.Success = false;
                command.Error = $"Option --{optionName} requires a value";
                return;
            }

            command.Options[optionName] = optionValue;
        }
    }

    /// <summary>
    /// Processes short-form options (-o value)
    /// </summary>
    private void ProcessShortOption(string arg, List<string> args, ref int index,
        CommandDefinition definition, ParsedCommand command)
    {
        var optionChar = arg.Substring(1, 1);
        var option = definition.Options.FirstOrDefault(o => o.ShortName == optionChar);

        if (option == null)
        {
            command.Success = false;
            command.Error = $"Unknown option: -{optionChar}";
            return;
        }

        if (option.IsFlag)
        {
            command.Options[option.Name] = "true";
        }
        else
        {
            var optionValue = arg.Length > 2 ? arg.Substring(2) : null;
            if (optionValue == null && index + 1 < args.Count)
            {
                index++;
                optionValue = args[index];
            }

            if (optionValue == null)
            {
                command.Success = false;
                command.Error = $"Option -{optionChar} requires a value";
                return;
            }

            command.Options[option.Name] = optionValue;
        }
    }

    /// <summary>
    /// Generates help text for all commands
    /// </summary>
    public string GetHelpText()
    {
        var sb = new StringBuilder();
        sb.AppendLine("dotnet-deploy-notify - Deployment Notification Pipeline");
        sb.AppendLine();
        sb.AppendLine("USAGE:");
        sb.AppendLine("  dotnet-deploy-notify [COMMAND] [OPTIONS] [PARAMETERS]");
        sb.AppendLine();
        sb.AppendLine("COMMANDS:");

        foreach (var cmd in _commands.OrderBy(c => c.Key))
        {
            sb.AppendLine($"  {cmd.Key,-20} {cmd.Value.Description}");
        }

        sb.AppendLine();
        sb.AppendLine("FLAGS:");
        sb.AppendLine("  -h, --help        Show this help message");
        sb.AppendLine("  -v, --version     Show version information");

        return sb.ToString();
    }

    /// <summary>
    /// Generates help text for a specific command
    /// </summary>
    public string GetCommandHelpText(string commandName)
    {
        if (!_commands.TryGetValue(commandName, out var definition))
            return $"Command '{commandName}' not found";

        var sb = new StringBuilder();
        sb.AppendLine($"Command: {definition.Name}");
        sb.AppendLine($"Description: {definition.Description}");
        sb.AppendLine();
        sb.AppendLine("USAGE:");
        sb.Append($"  {definition.Name}");

        if (definition.Parameters.Any())
        {
            foreach (var param in definition.Parameters)
            {
                sb.Append($" {(param.IsRequired ? "" : "[")}{param.Name}{(param.IsRequired ? "" : "]")}");
            }
        }

        if (definition.Options.Any())
            sb.Append(" [OPTIONS]");

        sb.AppendLine();

        if (definition.Parameters.Any())
        {
            sb.AppendLine("PARAMETERS:");
            foreach (var param in definition.Parameters)
            {
                sb.AppendLine($"  {param.Name,-20} {param.Description}");
            }
            sb.AppendLine();
        }

        if (definition.Options.Any())
        {
            sb.AppendLine("OPTIONS:");
            foreach (var opt in definition.Options)
            {
                var shortOpt = opt.ShortName != null ? $"-{opt.ShortName}, " : "";
                sb.AppendLine($"  {shortOpt}--{opt.Name,-15} {opt.Description}");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Registers built-in commands for the application
    /// </summary>
    private void RegisterBuiltInCommands()
    {
        RegisterCommand(new CommandDefinition
        {
            Name = "send",
            Description = "Send a deployment notification",
            Parameters = new List<ParameterDefinition>
            {
                new() { Name = "project", Description = "Project name", IsRequired = true },
                new() { Name = "version", Description = "Version number", IsRequired = true }
            },
            Options = new List<OptionDefinition>
            {
                new() { Name = "status", ShortName = "s", Description = "Build status", IsRequired = true },
                new() { Name = "environment", ShortName = "e", Description = "Target environment" },
                new() { Name = "branch", Description = "Branch name" },
                new() { Name = "channels", ShortName = "c", Description = "Comma-separated channels" },
                new() { Name = "message", ShortName = "m", Description = "Custom message" }
            }
        });

        RegisterCommand(new CommandDefinition
        {
            Name = "list",
            Description = "List configurations or notifications",
            Parameters = new List<ParameterDefinition>
            {
                new() { Name = "type", Description = "Type to list (configs, notifications)", IsRequired = true }
            },
            Options = new List<OptionDefinition>
            {
                new() { Name = "filter", ShortName = "f", Description = "Filter criteria" },
                new() { Name = "limit", ShortName = "l", Description = "Result limit" }
            }
        });

        RegisterCommand(new CommandDefinition
        {
            Name = "config",
            Description = "Manage channel configurations",
            Parameters = new List<ParameterDefinition>
            {
                new() { Name = "action", Description = "Action (add, remove, list)", IsRequired = true }
            },
            Options = new List<OptionDefinition>
            {
                new() { Name = "type", ShortName = "t", Description = "Channel type" },
                new() { Name = "webhook", Description = "Webhook URL" }
            }
        });

        RegisterCommand(new CommandDefinition
        {
            Name = "health",
            Description = "Check system health and configuration",
            Options = new List<OptionDefinition>
            {
                new() { Name = "detailed", ShortName = "d", Description = "Show detailed info", IsFlag = true }
            }
        });
    }

    private ParsedCommand CreateHelpCommand()
    {
        return new ParsedCommand
        {
            CommandName = "help",
            Success = true,
            Output = GetHelpText()
        };
    }

    private ParsedCommand CreateVersionCommand()
    {
        return new ParsedCommand
        {
            CommandName = "version",
            Success = true,
            Output = $"dotnet-deploy-notify v1.0.0"
        };
    }
}

/// <summary>
/// Represents a parsed command with extracted parameters and options
/// </summary>
public class ParsedCommand
{
    public string CommandName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? Output { get; set; }
    public Dictionary<string, string> Parameters { get; set; } = new();
    public Dictionary<string, string> Options { get; set; } = new();

    public string? GetParameter(string name) => Parameters.TryGetValue(name, out var value) ? value : null;
    public string? GetOption(string name) => Options.TryGetValue(name, out var value) ? value : null;
    public bool HasOption(string name) => Options.ContainsKey(name);
}

/// <summary>
/// Defines the structure of a CLI command
/// </summary>
public class CommandDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<ParameterDefinition> Parameters { get; set; } = new();
    public List<OptionDefinition> Options { get; set; } = new();
}

/// <summary>
/// Defines a command parameter
/// </summary>
public class ParameterDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
}

/// <summary>
/// Defines a command option or flag
/// </summary>
public class OptionDefinition
{
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsFlag { get; set; }
    public bool IsRequired { get; set; }
}
