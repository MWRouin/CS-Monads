using MWR.Monads.MaybeMonad;
using MWR.Monads.Messages;

namespace MWR.Monads.Tests;

static class Fixtures
{
    public static readonly Error AnyError = new BadRequestError("Test.Error", "test error");
    public static readonly Error AnotherError = new ServerError("Test.Error2", "second error");
    public static readonly Information AnyInfo = new Information("Test.Info", "test info");
    public static readonly Warning AnyWarning = new Warning("Test.Warning", "test warning");
}

static class Async
{
    public static async Task<T> Slow<T>(T value) { await Task.Yield(); return value; }
    public static async Task<Maybe<T>> SlowNone<T>() { await Task.Yield(); return Maybe<T>.None; }
}
