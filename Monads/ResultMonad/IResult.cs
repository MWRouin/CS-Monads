using MWR.Monads.Messages;

namespace MWR.Monads.ResultMonad;

public interface IResult
{
    bool IsSuccess();
    bool IsFailure();

    Information[] GetInfos();
    Warning[] GetWarnings();
    Error[] GetErrors();

    static bool operator true(IResult x) => x.IsSuccess();

    static bool operator false(IResult x) => x.IsFailure();
}

public interface IResult<T> : IResult
{
    T GetValue();

    T GetValueOr(T defaultValue);

    T? GetValueOrDefault();
}