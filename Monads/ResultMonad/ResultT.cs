using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using MWR.Monads.Messages;

namespace MWR.Monads.ResultMonad;

[DebuggerDisplay("{DebugDisplay,nq}")]
public readonly struct Result<T> : IResult<T>
{
    private string DebugDisplay => IsSuccess() ? $"Success({_value})" : "Failure";

    private readonly Result _innerResult;
    private readonly T? _value;

    internal Result(
        T? value,
        Result result)
    {
        if (result.IsSuccess() && value is null)
            throw new ArgumentNullException(nameof(value), "Null value for a successful.");

        _value = value;
        _innerResult = result;
    }
  
    [MemberNotNullWhen(true, nameof(_value))]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSuccess() => _innerResult.IsSuccess();

    [MemberNotNullWhen(false, nameof(_value))]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsFailure() => _innerResult.IsFailure();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValue() => IsSuccess()
        ? _value
        : throw new InvalidOperationException("Accessing a value of a failed result.");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValueOr(T defaultValue) => IsSuccess()
        ? _value
        : defaultValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T? GetValueOrDefault() => IsSuccess() ? _value : default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Information[] GetInfos() => _innerResult.GetInfos();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Warning[] GetWarnings() => _innerResult.GetWarnings();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Error[] GetErrors() => _innerResult.GetErrors();

    internal Information[]? RawInfos
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _innerResult.RawInfos;
    }

    internal Warning[]? RawWarnings
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _innerResult.RawWarnings;
    }

    internal Error[]? RawErrors
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _innerResult.RawErrors;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Result<T> Success(
        T value,
        Information[]? infos = null,
        Warning[]? warnings = null) =>
        new(
            value,
            Result.Success(infos, warnings));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Result<T> Failure(
        Error[]? errors = null,
        Warning[]? warnings = null,
        Information[]? infos = null) =>
        new(
            default,
            Result.Failure(errors, warnings, infos));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator true(Result<T> result) => result.IsSuccess();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator false(Result<T> result) => result.IsFailure();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result<T>(T value) => Success(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator T(Result<T> result) => result.IsSuccess()
        ? result._value
        : throw new InvalidCastException("Unboxing a failed result.");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result(Result<T> result) => result._innerResult;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result<T>(Error error) => Failure([error]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result<T>(Error[] errors) => Failure(errors);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result<T>(List<Error> errors) => Failure(errors.ToArray());

    public override string ToString() => IsSuccess() ? $"Success({_value})" : "Failure";
}
