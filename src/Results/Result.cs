#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Results;

/// <summary>
/// Represents the result of an operation without a value
/// </summary>
public class Result
{
    public bool IsSuccess { get; protected set; }
    public string? Error { get; protected set; }
    public List<string> Errors { get; protected set; } = new();

    public static Result Ok() => new() { IsSuccess = true };
    public static Result Fail(string error) => new() { IsSuccess = false, Error = error, Errors = new List<string> { error } };
    public static Result Fail(List<string> errors) => new() { IsSuccess = false, Errors = errors, Error = string.Join("; ", errors) };

    public override string ToString() => IsSuccess ? "Success" : $"Failed: {Error}";
}

/// <summary>
/// Represents the result of an operation with a value
/// </summary>
public class Result<T> : Result
{
    public T? Value { get; private set; }

    public static Result<T> Ok(T value) => new() { IsSuccess = true, Value = value };
    public new static Result<T> Fail(string error) => new() { IsSuccess = false, Error = error };
    public new static Result<T> Fail(List<string> errors) => new() { IsSuccess = false, Errors = errors, Error = string.Join("; ", errors) };

    /// <summary>
    /// Maps the result to another type
    /// </summary>
    public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        return IsSuccess ? Result<TNew>.Ok(mapper(Value!)) : Result<TNew>.Fail(Error ?? "");
    }

    /// <summary>
    /// Binds the result with another operation
    /// </summary>
    public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> binder)
    {
        return IsSuccess ? binder(Value!) : Result<TNew>.Fail(Error ?? "");
    }

    /// <summary>
    /// Executes an action if the result is success
    /// </summary>
    public Result<T> OnSuccess(Action<T> action)
    {
        if (IsSuccess)
            action(Value!);
        return this;
    }

    /// <summary>
    /// Executes an action if the result is failure
    /// </summary>
    public Result<T> OnFailure(Action<string> action)
    {
        if (!IsSuccess)
            action(Error ?? "");
        return this;
    }

    /// <summary>
    /// Gets the value or a default value
    /// </summary>
    public T GetValueOrDefault(T defaultValue) => IsSuccess ? Value! : defaultValue;

    /// <summary>
    /// Gets the value or throws an exception
    /// </summary>
    public T GetValueOrThrow() => IsSuccess ? Value! : throw new InvalidOperationException(Error);
}

/// <summary>
/// Result builder for fluent construction
/// </summary>
public class ResultBuilder
{
    private bool _isSuccess = true;
    private List<string> _errors = new();

    public ResultBuilder Success()
    {
        _isSuccess = true;
        _errors.Clear();
        return this;
    }

    public ResultBuilder Error(string error)
    {
        _isSuccess = false;
        _errors.Add(error);
        return this;
    }

    public ResultBuilder AddError(string error)
    {
        _errors.Add(error);
        _isSuccess = false;
        return this;
    }

    public Result Build()
    {
        return _isSuccess ? Result.Ok() : Result.Fail(_errors);
    }
}

/// <summary>
/// Result builder with value
/// </summary>
public class ResultBuilder<T>
{
    private bool _isSuccess = true;
    private List<string> _errors = new();
    private T? _value;

    public ResultBuilder<T> Success(T value)
    {
        _isSuccess = true;
        _value = value;
        _errors.Clear();
        return this;
    }

    public ResultBuilder<T> Failure(string error)
    {
        _isSuccess = false;
        _errors.Add(error);
        return this;
    }

    public Result<T> Build()
    {
        return _isSuccess ? Result<T>.Ok(_value!) : Result<T>.Fail(_errors);
    }
}

/// <summary>
/// Try-catch wrapper for converting exceptions to results
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Tries to execute a function and returns a result
    /// </summary>
    public static Result<T> Try<T>(Func<T> func)
    {
        try
        {
            return Result<T>.Ok(func());
        }
        catch (Exception ex)
        {
            return Result<T>.Fail(ex.Message);
        }
    }

    /// <summary>
    /// Tries to execute an async function and returns a result
    /// </summary>
    public static async Task<Result<T>> TryAsync<T>(Func<Task<T>> func)
    {
        try
        {
            var value = await func().ConfigureAwait(false);
            return Result<T>.Ok(value);
        }
        catch (Exception ex)
        {
            return Result<T>.Fail(ex.Message);
        }
    }

    /// <summary>
    /// Tries to execute an action and returns a result
    /// </summary>
    public static Result Try(Action action)
    {
        try
        {
            action();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    /// <summary>
    /// Tries to execute an async action and returns a result
    /// </summary>
    public static async Task<Result> TryAsync(Func<Task> action)
    {
        try
        {
            await action().ConfigureAwait(false);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }
}
