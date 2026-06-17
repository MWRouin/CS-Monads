using System.Runtime.CompilerServices;
using MWR.Monads.Messages;

namespace MWR.Monads.ResultMonad;

public static class Results
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> Create<T>(T? value, Error? error = null)
    {
        if (value is not null)
            return Result<T>.Success(value);

        return error is null
            ? Result<T>.Failure()
            : Result<T>.Failure([error]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result Success() => Result.Success();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result Success(params Information[]? infos) =>
        Result.Success(infos);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result Success(Information[]? infos, Warning[]? warnings) =>
        Result.Success(infos, warnings);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> Success<T>(T value) =>
        Result<T>.Success(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> Success<T>(T value, params Information[]? infos) =>
        Result<T>.Success(value, infos);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> Success<T>(T value, Information[]? infos, Warning[]? warnings) =>
        Result<T>.Success(value, infos, warnings);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result Failure() => default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result Failure(params Error[]? errors) =>
        Result.Failure(errors);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result Failure(Error[]? errors, Warning[]? warnings, Information[]? infos) =>
        Result.Failure(errors, warnings, infos);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> Failure<T>() => default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> Failure<T>(params Error[]? errors) =>
        Result<T>.Failure(errors);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> Failure<T>(Error[]? errors, Warning[]? warnings, Information[]? infos) =>
        Result<T>.Failure(errors, warnings, infos);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> Ensure<T>(T? value, Func<T, bool> condition, Error error) =>
        value is null || !condition(value)
            ? Result<T>.Failure([error])
            : Result<T>.Success(value);

    // Loop + allocation — NOT inlined.
    public static Result<T> EnsureAll<T>(T? value, params (Func<T, bool> condition, Error error)[] conditions)
    {
        Error[]? errors = null;
        var count = 0;

        if (value is null)
        {
            foreach (var (_, error) in conditions)
            {
                errors ??= new Error[conditions.Length];
                errors[count++] = error;
            }

            return Result<T>.Failure(errors);
        }

        foreach (var (condition, error) in conditions)
        {
            if (condition(value)) continue;

            errors ??= new Error[conditions.Length];
            errors[count++] = error;
        }

        return count == 0 ? Result<T>.Success(value) : Result<T>.Failure(errors![..count]);
    }

    public static Result<T> EnsureAll<T>(T? value, Error error, params Func<T, bool>[] conditions)
    {
        if (value is null) return Result<T>.Failure([error]);

        foreach (var condition in conditions)
            if (!condition(value))
                return Result<T>.Failure([error]);

        return Result<T>.Success(value);
    }

    public static Result<T> EnsureAny<T>(T? value, Error error, params Func<T, bool>[] conditions)
    {
        if (value is null) return Result<T>.Failure([error]);
        if (conditions.Length == 0) return Result<T>.Success(value);

        foreach (var condition in conditions)
            if (condition(value))
                return Result<T>.Success(value);

        return Result<T>.Failure([error]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> EnsureNot<T>(T? value, Func<T, bool> condition, Error error) =>
        value is null || condition(value)
            ? Result<T>.Failure([error])
            : Result<T>.Success(value);

    // Loop + allocation — NOT inlined.
    public static Result<T> EnsureNotAny<T>(T? value, params (Func<T, bool>, Error)[] conditions)
    {
        Error[]? errors = null;
        var count = 0;

        if (value is null)
        {
            foreach (var (_, error) in conditions)
            {
                errors ??= new Error[conditions.Length];
                errors[count++] = error;
            }

            return Result<T>.Failure(errors);
        }

        foreach (var (condition, error) in conditions)
        {
            if (!condition(value)) continue;

            errors ??= new Error[conditions.Length];
            errors[count++] = error;
        }

        return count == 0 ? Result<T>.Success(value) : Result<T>.Failure(errors![..count]);
    }

    public static Result<T> EnsureNotAny<T>(T? value, Error error, params Func<T, bool>[] conditions)
    {
        if (value is null) return Result<T>.Failure([error]);

        foreach (var condition in conditions)
            if (condition(value))
                return Result<T>.Failure([error]);

        return Result<T>.Success(value);
    }

    public static Result<T> EnsureNotAll<T>(T? value, Error error, params Func<T, bool>[] conditions)
    {
        if (value is null) return Result<T>.Failure([error]);
        if (conditions.Length == 0) return Result<T>.Success(value);

        foreach (var condition in conditions)
            if (!condition(value))
                return Result<T>.Success(value);

        return Result<T>.Failure([error]);
    }

    public static Result Combine(params Result[] results)
    {
        if (results.Length == 0) return Result.Success();

        var isSuccess = true;
        List<Information>? infos = null;
        List<Warning>? warnings = null;
        List<Error>? errors = null;

        foreach (var result in results)
        {
            if (result.RawInfos is { Length: > 0 } ri)
            {
                (infos ??= new List<Information>(ri.Length)).AddRange(ri);
            }
            if (result.RawWarnings is { Length: > 0 } rw)
            {
                (warnings ??= new List<Warning>(rw.Length)).AddRange(rw);
            }
            if (result.IsFailure())
            {
                isSuccess = false;
                if (result.RawErrors is { Length: > 0 } re)
                {
                    (errors ??= new List<Error>(re.Length)).AddRange(re);
                }
            }
        }

        var infoArr = infos?.ToArray();
        var warnArr = warnings?.ToArray();

        return isSuccess
            ? Result.Success(infoArr, warnArr)
            : Result.Failure(errors?.ToArray(), warnArr, infoArr);
    }

    public static Result<T[]> Combine<T>(params Result<T>[] results)
    {
        if (results.Length == 0) return Result<T[]>.Success([]);

        var isSuccess = true;
        List<Information>? infos = null;
        List<Warning>? warnings = null;
        List<Error>? errors = null;

        foreach (var result in results)
        {
            if (result.RawInfos is { Length: > 0 } ri)
            {
                (infos ??= new List<Information>(ri.Length)).AddRange(ri);
            }
            if (result.RawWarnings is { Length: > 0 } rw)
            {
                (warnings ??= new List<Warning>(rw.Length)).AddRange(rw);
            }
            if (result.IsFailure())
            {
                isSuccess = false;
                if (result.RawErrors is { Length: > 0 } re)
                {
                    (errors ??= new List<Error>(re.Length)).AddRange(re);
                }
            }
        }

        var infoArr = infos?.ToArray();
        var warnArr = warnings?.ToArray();

        if (!isSuccess)
            return Result<T[]>.Failure(errors?.ToArray(), warnArr, infoArr);

        var values = new T[results.Length];
        for (var i = 0; i < results.Length; i++)
            values[i] = results[i].GetValue();

        return Result<T[]>.Success(values, infoArr, warnArr);
    }
}