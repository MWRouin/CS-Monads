using System.Runtime.CompilerServices;
using MWR.Monads.Messages;
using MWR.Monads.ResultMonad;

namespace MWR.Monads.MaybeMonad;

// NOTE on inlining: `async` methods compile to state machines and cannot be
// meaningfully inlined; we only annotate the non-async fast-path wrappers.
public static class TaskMaybeExtensions
{
    extension<T>(Task<Maybe<T>> maybeTask)
    {
        public async Task<Result<T>> ToResult() =>
            (await maybeTask).ToResult();

        public async Task<Result<T>> ToResult(Error error) =>
            (await maybeTask).ToResult(error);

        // Non-async fast-path wrapper — inlining the dispatch is worthwhile.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Maybe<TOut>> Map<TOut>(Func<T, TOut> func)
        {
            return maybeTask.IsCompletedSuccessfully
                ? Task.FromResult(maybeTask.Result.Map(func))
                : Await(maybeTask, func);

            static async Task<Maybe<TOut>> Await(Task<Maybe<T>> task, Func<T, TOut> func)
                => (await task.ConfigureAwait(false)).Map(func);
        }

        // Non-async fast-path wrapper — inlining the dispatch is worthwhile.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Maybe<TOut>> MapAsync<TOut>(Func<T, Task<TOut>> func)
        {
            return maybeTask.IsCompletedSuccessfully
                ? maybeTask.Result.MapAsync(func)
                : Await(maybeTask, func);

            static async Task<Maybe<TOut>> Await(Task<Maybe<T>> task, Func<T, Task<TOut>> func)
                => await (await task.ConfigureAwait(false)).MapAsync(func).ConfigureAwait(false);
        }

        // Non-async fast-path wrapper — inlining the dispatch is worthwhile.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Maybe<TOut>> MapAsync<TOut>(
            Func<T, CancellationToken, Task<TOut>> func,
            CancellationToken ct = default)
        {
            return maybeTask.IsCompletedSuccessfully
                ? maybeTask.Result.MapAsync(func, ct)
                : Await(maybeTask, func, ct);

            static async Task<Maybe<TOut>> Await(
                Task<Maybe<T>> task,
                Func<T, CancellationToken, Task<TOut>> func,
                CancellationToken ct)
                => await (await task.ConfigureAwait(false)).MapAsync(func, ct).ConfigureAwait(false);
        }

        public async Task<Maybe<TOut>> Bind<TOut>(Func<T, Maybe<TOut>> func) =>
            (await maybeTask).Bind(func);

        public async Task<Maybe<TOut>> BindAsync<TOut>(Func<T, Task<Maybe<TOut>>> func) =>
            await (await maybeTask).BindAsync(func).ConfigureAwait(false);

        public async Task<Maybe<TOut>> BindAsync<TOut>(
            Func<T, CancellationToken, Task<Maybe<TOut>>> func,
            CancellationToken ct = default) =>
            await (await maybeTask).BindAsync(func, ct).ConfigureAwait(false);

        public async Task<Maybe<T>> Filter(Func<T, bool> predicate) =>
            (await maybeTask).Filter(predicate);

        public async Task<Maybe<T>> Tap(Action<T> action) =>
            (await maybeTask).Tap(action);

        // Non-async fast-path wrapper — inlining the dispatch is worthwhile.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Maybe<T>> TapAsync(Func<T, Task> action)
        {
            return maybeTask.IsCompletedSuccessfully
                ? maybeTask.Result.TapAsync(action)
                : Await(maybeTask, action);

            static async Task<Maybe<T>> Await(Task<Maybe<T>> task, Func<T, Task> action)
                => await (await task.ConfigureAwait(false)).TapAsync(action).ConfigureAwait(false);
        }

        // Non-async fast-path wrapper — inlining the dispatch is worthwhile.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Maybe<T>> TapAsync(Func<T, CancellationToken, Task> action, CancellationToken ct = default)
        {
            return maybeTask.IsCompletedSuccessfully
                ? maybeTask.Result.TapAsync(action, ct)
                : Await(maybeTask, action, ct);

            static async Task<Maybe<T>> Await(Task<Maybe<T>> task, Func<T, CancellationToken, Task> action, CancellationToken ct)
                => await (await task.ConfigureAwait(false)).TapAsync(action, ct).ConfigureAwait(false);
        }

        public async Task<TOut> Match<TOut>(Func<T, TOut> some, Func<TOut> none) =>
            (await maybeTask).Match(some, none);

        public async Task<TOut> MatchAsync<TOut>(
            Func<T, CancellationToken, Task<TOut>> some,
            Func<CancellationToken, Task<TOut>> none,
            CancellationToken ct = default) =>
            await (await maybeTask).MatchAsync(some, none, ct).ConfigureAwait(false);

        public async Task<Maybe<T>> Or(Maybe<T> fallback) =>
            (await maybeTask).Or(fallback);

        public async Task<Maybe<T>> Or(Func<Maybe<T>> fallback) =>
            (await maybeTask).Or(fallback);
    }
}
