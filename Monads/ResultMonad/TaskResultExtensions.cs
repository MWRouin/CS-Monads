using System.Runtime.CompilerServices;
using MWR.Monads.MaybeMonad;
using MWR.Monads.Messages;

namespace MWR.Monads.ResultMonad;

// NOTE on inlining:
// `async` methods are compiler-rewritten into state machines — applying
// [MethodImpl(AggressiveInlining)] to them has no effect (the JIT inlines
// the state-machine kickoff, not the user-visible signature). We only
// annotate the non-async fast-path wrappers below.
public static class TaskResultExtensions
{
    extension<T>(Task<Result<T>> resultTask)
    {
        public async Task<Result<T>> WithSuccessInfo(Information info) =>
            (await resultTask).WithSuccessInfo(info);

        public async Task<Maybe<T>> ToMaybe() => (await resultTask).ToMaybe();

        public async Task<Result<TOut>> Map<TOut>(Func<T, TOut> func) =>
            (await resultTask).Map(func);

        public async Task<Result<TOut>> MapAsync<TOut>(Func<T, Task<TOut>> func) =>
            await (await resultTask).MapAsync(func);

        public async Task<Result<TOut>> MapAsync<TOut>(
            Func<T, CancellationToken, Task<TOut>> func,
            CancellationToken ct = default) =>
            await (await resultTask).MapAsync(func, ct);

        public async Task<Result<TOut>> Bind<TOut>(Func<T, Result<TOut>> func) =>
            (await resultTask).Bind(func);

        public async Task<Result> Bind(Func<T, Result> func) =>
            (await resultTask).Bind(func);

        public async Task<Result<TOut>> BindAsync<TOut>(Func<T, Task<Result<TOut>>> func) =>
            await (await resultTask).BindAsync(func);

        public async Task<Result<TOut>> BindAsync<TOut>(
            Func<T, CancellationToken, Task<Result<TOut>>> func,
            CancellationToken ct = default) =>
            await (await resultTask).BindAsync(func, ct);

        public async Task<Result> BindAsync(Func<T, Task<Result>> func) =>
            await (await resultTask).BindAsync(func);

        public async Task<Result> BindAsync(
            Func<T, CancellationToken, Task<Result>> func,
            CancellationToken ct = default) =>
            await (await resultTask).BindAsync(func, ct);

        public async Task<Result<T>> Ensure(Func<T, bool> predicate, Error error) =>
            (await resultTask).Ensure(predicate, error);

        public async Task<Result<T>> EnsureNot(Func<T, bool> predicate, Error error) =>
            (await resultTask).EnsureNot(predicate, error);

        public async Task<Result<T>> Tap(Action<T> action) =>
            (await resultTask).Tap(action);

        // Non-async fast-path wrapper — inlining the dispatch is worthwhile.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Result<T>> TapAsync(Func<T, Task> action)
        {
            return resultTask.IsCompletedSuccessfully
                ? resultTask.Result.TapAsync(action)
                : Await(resultTask, action);

            static async Task<Result<T>> Await(Task<Result<T>> resultTask, Func<T, Task> action)
                => await (await resultTask.ConfigureAwait(false)).TapAsync(action).ConfigureAwait(false);
        }

        // Non-async fast-path wrapper — inlining the dispatch is worthwhile.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Result<T>> TapAsync(Func<T, CancellationToken, Task> action, CancellationToken ct = default)
        {
            return resultTask.IsCompletedSuccessfully
                ? resultTask.Result.TapAsync(action, ct)
                : Await(resultTask, action, ct);

            static async Task<Result<T>> Await(Task<Result<T>> resultTask, Func<T, CancellationToken, Task> action, CancellationToken ct)
                => await (await resultTask.ConfigureAwait(false)).TapAsync(action, ct).ConfigureAwait(false);
        }

        public async Task<Result<T>> TapError(Action<Error[]> action) =>
            (await resultTask).TapError(action);

        public async Task<TOut> Match<TOut>(Func<T, TOut> onSuccess, Func<Error[], TOut> onFailure) =>
            (await resultTask).Match(onSuccess, onFailure);
    }

    extension(Task<Result> resultTask)
    {
        public async Task<Result> WithSuccessInfo(Information info) =>
            (await resultTask).WithSuccessInfo(info);

        public async Task<Result<T>> Map<T>(Func<T> func) =>
            (await resultTask).Map(func);

        public async Task<Result<T>> MapAsync<T>(Func<Task<T>> func) =>
            await (await resultTask).MapAsync(func);

        public async Task<Result> Bind(Func<Result> func) =>
            (await resultTask).Bind(func);

        public async Task<Result> BindAsync(Func<Task<Result>> func) =>
            await (await resultTask).BindAsync(func);

        public async Task<Result> Tap(Action action) =>
            (await resultTask).Tap(action);

        // Non-async fast-path wrapper — inlining the dispatch is worthwhile.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Result> TapAsync(Func<Task> action)
        {
            return resultTask.IsCompletedSuccessfully
                ? resultTask.Result.TapAsync(action)
                : Await(resultTask, action);

            static async Task<Result> Await(Task<Result> resultTask, Func<Task> action)
                => await (await resultTask.ConfigureAwait(false)).TapAsync(action).ConfigureAwait(false);
        }

        // Non-async fast-path wrapper — inlining the dispatch is worthwhile.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Result> TapAsync(Func<CancellationToken, Task> action, CancellationToken ct = default)
        {
            return resultTask.IsCompletedSuccessfully
                ? resultTask.Result.TapAsync(action, ct)
                : Await(resultTask, action, ct);

            static async Task<Result> Await(Task<Result> resultTask, Func<CancellationToken, Task> action, CancellationToken ct)
                => await (await resultTask.ConfigureAwait(false)).TapAsync(action, ct).ConfigureAwait(false);
        }

        public async Task<Result> TapError(Action<Error[]> action) =>
            (await resultTask).TapError(action);

        public async Task<TOut> Match<TOut>(Func<TOut> onSuccess, Func<Error[], TOut> onFailure) =>
            (await resultTask).Match(onSuccess, onFailure);
    }
}
