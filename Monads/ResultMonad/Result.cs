using System.Diagnostics;
using System.Runtime.CompilerServices;
using MWR.Monads.Messages;

namespace MWR.Monads.ResultMonad;

[DebuggerDisplay("{DebugDisplay,nq}")]
public readonly struct Result : IResult
{
    private string DebugDisplay => _isSuccess ? "Success" : "Failure";

    private Result(
        bool isSuccess,
        Information[]? infos,
        Warning[]? warnings,
        Error[]? errors)
    {
        if (isSuccess && (errors?.Any() ?? false))
            throw new InvalidOperationException("A successful result with an error.");
        _isSuccess = isSuccess;
        RawInfos = infos;
        RawWarnings = warnings;
        RawErrors = errors;
    }

    private readonly bool _isSuccess;

    internal Information[]? RawInfos { get; init; }

    internal Warning[]? RawWarnings { get; init; }

    internal Error[]? RawErrors { get; init; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSuccess() => _isSuccess;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsFailure() => !_isSuccess;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Information[] GetInfos() => RawInfos ?? [];
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Warning[] GetWarnings() => RawWarnings ?? [];
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Error[] GetErrors() => RawErrors ?? [];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Result Success(
        Information[]? infos = null,
        Warning[]? warnings = null) =>
        new(true,
            infos,
            warnings,
            null);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Result Failure(
        Error[]? errors = null,
        Warning[]? warnings = null,
        Information[]? infos = null) =>
        new(false,
            infos,
            warnings,
            errors);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator true(Result result) => result.IsSuccess();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator false(Result result) => result.IsFailure();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result(Information infos) => Success([infos]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result(Information[] infos) => Success(infos);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result(List<Information> infos) => Success(infos.ToArray());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result(Warning warning) => Success(null, [warning]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result(Warning[] warnings) => Success(null, warnings);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result(List<Warning> warnings) => Success(null, warnings.ToArray());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result(Error error) => Failure([error]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result(Error[] errors) => Failure(errors);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result(List<Error> errors) => Failure(errors.ToArray());

    public override string ToString() => _isSuccess ? "Success" : "Failure";
}