#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Diagnostics.CodeAnalysis;

namespace DotNetDeployNotify.Results;

/// <summary>
/// Provides extension methods for working with <see cref="Result"/> and <see cref="Result{T}"/> types.
/// Includes Try-catch wrappers for converting exceptions to results and fluent composition methods.
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
            var value = await func();
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

    /// <summary>
    /// Combines multiple results into a single result with all errors.
    /// If all results are successful, returns a successful result.
    /// If any result failed, returns a failed result with all errors collected.
    /// </summary>
    /// <param name="results">The results to combine</param>
    /// <returns>A single result containing all errors if any failed, otherwise success</returns>
    /// <exception cref="ArgumentNullException">Thrown when results is null</exception>
    public static Result Combine(this IEnumerable<Result> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var resultList = results.ToList();
        if (resultList.Count == 0)
        {
            return Result.Ok();
        }

        var allErrors = new List<string>();
        foreach (var result in resultList)
        {
            if (!result.IsSuccess)
            {
                allErrors.AddRange(result.Errors);
            }
        }

        return allErrors.Count > 0
            ? Result.Fail(allErrors)
            : Result.Ok();
    }

    /// <summary>
    /// Combines multiple typed results into a single typed result.
    /// If all results are successful, returns a successful result containing all values.
    /// If any result failed, returns a failed result with all errors collected.
    /// </summary>
    /// <typeparam name="T">The type of values in the results</typeparam>
    /// <param name="results">The results to combine</param>
    /// <returns>A single result containing all values if all succeeded, or all errors if any failed</returns>
    /// <exception cref="ArgumentNullException">Thrown when results is null</exception>
    public static Result<IReadOnlyList<T>> Combine<T>(this IEnumerable<Result<T>> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var resultList = results.ToList();
        if (resultList.Count == 0)
        {
            return Result<IReadOnlyList<T>>.Ok(Array.Empty<T>());
        }

        var allErrors = new List<string>();
        var values = new List<T>();

        foreach (var result in resultList)
        {
            if (!result.IsSuccess)
            {
                allErrors.AddRange(result.Errors);
            }
            else if (result.Value is not null)
            {
                values.Add(result.Value);
            }
        }

        return allErrors.Count > 0
            ? Result<IReadOnlyList<T>>.Fail(allErrors)
            : Result<IReadOnlyList<T>>.Ok(values.AsReadOnly());
    }

    /// <summary>
    /// Filters the result based on a predicate.
    /// If the result is successful and the predicate returns true for the value, returns the result.
    /// If the result is successful but the predicate returns false, returns a failed result with the specified error.
    /// If the result is already failed, returns it unchanged.
    /// </summary>
    /// <typeparam name="T">The type of the result value</typeparam>
    /// <param name="result">The result to filter</param>
    /// <param name="predicate">The predicate to apply to the value</param>
    /// <param name="errorMessage">The error message to use if the predicate fails</param>
    /// <returns>The filtered result</returns>
    /// <exception cref="ArgumentNullException">Thrown when result or predicate is null</exception>
    public static Result<T> Where<T>(this Result<T> result, Func<T, bool> predicate, string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentException.ThrowIfNullOrEmpty(errorMessage);

        if (!result.IsSuccess)
        {
            return result;
        }

        return predicate(result.Value!)
            ? result
            : Result<T>.Fail(errorMessage);
    }

    /// <summary>
    /// Filters the result based on a predicate that returns a result.
    /// If the result is successful and the predicate returns a successful result, returns that result.
    /// If the result is successful but the predicate returns a failed result, returns the failed result.
    /// If the result is already failed, returns it unchanged.
    /// </summary>
    /// <typeparam name="T">The type of the result value</typeparam>
    /// <typeparam name="TNew">The type returned by the predicate</typeparam>
    /// <param name="result">The result to filter</param>
    /// <param name="predicate">The predicate that returns a result</param>
    /// <returns>The filtered result</returns>
    /// <exception cref="ArgumentNullException">Thrown when result or predicate is null</exception>
    public static Result<TNew> Where<T, TNew>(this Result<T> result, Func<T, Result<TNew>> predicate)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(predicate);

        return !result.IsSuccess
            ? Result<TNew>.Fail(result.Error ?? "")
            : predicate(result.Value!);
    }

    /// <summary>
    /// Projects the result value using a selector function if the result is successful.
    /// Similar to LINQ Select, but preserves the result state.
    /// </summary>
    /// <typeparam name="T">The type of the source result value</typeparam>
    /// <typeparam name="TResult">The type of the projected result value</typeparam>
    /// <param name="result">The source result</param>
    /// <param name="selector">The selector function</param>
    /// <returns>A new result with the projected value if successful, otherwise the original error</returns>
    /// <exception cref="ArgumentNullException">Thrown when result or selector is null</exception>
    public static Result<TResult> Select<T, TResult>(this Result<T> result, Func<T, TResult> selector)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selector);

        return result.IsSuccess
            ? Result<TResult>.Ok(selector(result.Value!))
            : Result<TResult>.Fail(result.Error ?? "");
    }

    /// <summary>
    /// Projects the result value using a selector function that returns a result.
    /// Similar to SelectMany in LINQ, enabling chaining of result-returning operations.
    /// </summary>
    /// <typeparam name="T">The type of the source result value</typeparam>
    /// <typeparam name="TResult">The type of the result value</typeparam>
    /// <param name="result">The source result</param>
    /// <param name="selector">The selector function that returns a result</param>
    /// <returns>The result from the selector function</returns>
    /// <exception cref="ArgumentNullException">Thrown when result or selector is null</exception>
    public static Result<TResult> SelectMany<T, TResult>(
        this Result<T> result,
        Func<T, Result<TResult>> selector)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selector);

        return !result.IsSuccess
            ? Result<TResult>.Fail(result.Error ?? "")
            : selector(result.Value!);
    }

    /// <summary>
    /// Projects the result value using a selector function that returns a result,
    /// and projects the result value using a result selector function.
    /// Enables LINQ-style query syntax with multiple from clauses.
    /// </summary>
    /// <typeparam name="T">The type of the source result value</typeparam>
    /// <typeparam name="TIntermediate">The type of the intermediate result value</typeparam>
    /// <typeparam name="TResult">The type of the final result value</typeparam>
    /// <param name="result">The source result</param>
    /// <param name="selector">The selector function that returns an intermediate result</param>
    /// <param name="resultSelector">The function that projects the final result</param>
    /// <returns>The result from the resultSelector function</returns>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
    public static Result<TResult> SelectMany<T, TIntermediate, TResult>(
        this Result<T> result,
        Func<T, Result<TIntermediate>> selector,
        Func<T, TIntermediate, TResult> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(resultSelector);

        return !result.IsSuccess
            ? Result<TResult>.Fail(result.Error ?? "")
            : selector(result.Value!).Map(intermediate => resultSelector(result.Value!, intermediate));
    }

    /// <summary>
    /// Executes an action with the result value if successful, and returns the original result.
    /// Similar to <see cref="Result{T}.OnSuccess(Action{T})" /> but returns the original result for fluent chaining.
    /// </summary>
    /// <typeparam name="T">The type of the result value</typeparam>
    /// <param name="result">The result</param>
    /// <param name="action">The action to execute with the value</param>
    /// <returns>The original result for fluent chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when result or action is null</exception>
    public static Result<T> Do<T>(this Result<T> result, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(action);

        if (result.IsSuccess)
        {
            action(result.Value!);
        }

        return result;
    }

    /// <summary>
    /// Executes an action if the result is successful, and returns a new result.
    /// The action can modify the value, and the modified value is returned in a new result.
    /// </summary>
    /// <typeparam name="T">The type of the result value</typeparam>
    /// <param name="result">The result</param>
    /// <param name="action">The action to execute with the value</param>
    /// <returns>A new result with the (potentially modified) value</returns>
    /// <exception cref="ArgumentNullException">Thrown when result or action is null</exception>
    public static Result<T> DoAndReturn<T>(this Result<T> result, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(action);

        if (!result.IsSuccess)
        {
            return Result<T>.Fail(result.Error ?? "");
        }

        var newValue = result.Value;
        action(newValue!);
        return Result<T>.Ok(newValue);
    }

    /// <summary>
    /// Returns a default value if the result is failed.
    /// </summary>
    /// <typeparam name="T">The type of the result value</typeparam>
    /// <param name="result">The result</param>
    /// <param name="defaultValueFactory">The default value factory to call if the result failed</param>
    /// <returns>The result value if successful, otherwise the default value</returns>
    /// <exception cref="ArgumentNullException">Thrown when result or defaultValueFactory is null</exception>
    public static T GetValueOrDefault<T>(this Result<T> result, Func<T> defaultValueFactory)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(defaultValueFactory);

        return result.IsSuccess ? result.Value! : defaultValueFactory();
    }

    /// <summary>
    /// Returns a result from a fallback function if the current result is failed.
    /// </summary>
    /// <typeparam name="T">The type of the result value</typeparam>
    /// <param name="result">The result</param>
    /// <param name="fallback">The fallback function to execute if the result failed</param>
    /// <returns>The original result if successful, otherwise the result from the fallback function</returns>
    /// <exception cref="ArgumentNullException">Thrown when result or fallback is null</exception>
    public static Result<T> OrElse<T>(this Result<T> result, Func<Result<T>> fallback)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(fallback);

        return result.IsSuccess
            ? result
            : fallback();
    }

    /// <summary>
    /// Returns a result from a fallback function if the current result is failed.
    /// The fallback function receives the error message.
    /// </summary>
    /// <typeparam name="T">The type of the result value</typeparam>
    /// <param name="result">The result</param>
    /// <param name="fallback">The fallback function to execute if the result failed, receiving the error</param>
    /// <returns>The original result if successful, otherwise the result from the fallback function</returns>
    /// <exception cref="ArgumentNullException">Thrown when result or fallback is null</exception>
    public static Result<T> OrElse<T>(this Result<T> result, Func<string, Result<T>> fallback)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(fallback);

        return result.IsSuccess
            ? result
            : fallback(result.Error ?? "Unknown error");
    }
}