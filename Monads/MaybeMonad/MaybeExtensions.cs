using System.Runtime.CompilerServices;
using MWR.Monads.Messages;
using MWR.Monads.ResultMonad;

namespace MWR.Monads.MaybeMonad;

public static class MaybeExtensions
{
    extension<T>(Maybe<T> maybe)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<T> ToResult() => maybe.HasValue()
            ? Results.Success(maybe.GetValue())
            : Results.Failure<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<T> ToResult(Error error) => maybe.HasValue()
            ? Results.Success(maybe.GetValue())
            : Results.Failure<T>(error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Maybe<TOut> Map<TOut>(Func<T, TOut> func) => maybe.HasValue()
            ? Maybe.Some(func(maybe.GetValue()))
            : Maybe.None<TOut>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Maybe<TOut>> MapAsync<TOut>(Func<T, Task<TOut>> func)
        {
            if (maybe.HasNoValue()) return Task.FromResult(Maybe.None<TOut>());

            var task = func(maybe.GetValue());

            return task.IsCompletedSuccessfully
                ? Task.FromResult(Maybe.Some(task.Result))
                : Await(task);

            static async Task<Maybe<TOut>> Await(Task<TOut> task)
                => Maybe.Some(await task.ConfigureAwait(false));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Maybe<TOut>> MapAsync<TOut>(
            Func<T, CancellationToken, Task<TOut>> func,
            CancellationToken ct = default)
        {
            if (maybe.HasNoValue())
                return Task.FromResult(Maybe.None<TOut>());

            var task = func(maybe.GetValue(), ct);

            return task.IsCompletedSuccessfully
                ? Task.FromResult(Maybe.Some(task.Result))
                : Await(task);

            static async Task<Maybe<TOut>> Await(Task<TOut> task)
                => Maybe.Some(await task.ConfigureAwait(false));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Maybe<TOut> Bind<TOut>(Func<T, Maybe<TOut>> func) => maybe.HasValue()
            ? func(maybe.GetValue())
            : Maybe.None<TOut>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Maybe<TOut>> BindAsync<TOut>(Func<T, Task<Maybe<TOut>>> func) =>
            maybe.HasValue()
                ? func(maybe.GetValue())
                : Task.FromResult(Maybe.None<TOut>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Maybe<TOut>> BindAsync<TOut>(
            Func<T, CancellationToken, Task<Maybe<TOut>>> func,
            CancellationToken ct = default)
            => maybe.HasValue()
                ? func(maybe.GetValue(), ct)
                : Task.FromResult(Maybe.None<TOut>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Maybe<T> Filter(Func<T, bool> predicate) =>
            maybe.HasValue() && predicate(maybe.GetValue()) ? maybe : Maybe.None<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Maybe<T> Tap(Action<T> action)
        {
            if (maybe.HasValue()) action(maybe.GetValue());
            return maybe;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Maybe<T>> TapAsync(Func<T, Task> action)
        {
            if (maybe.HasNoValue())
                return Task.FromResult(maybe);

            var task = action(maybe.GetValue());

            return task.IsCompletedSuccessfully
                ? Task.FromResult(maybe)
                : Await(task, maybe);

            static async Task<Maybe<T>> Await(Task task, Maybe<T> maybe)
            {
                await task.ConfigureAwait(false);
                return maybe;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Maybe<T>> TapAsync(Func<T, CancellationToken, Task> action, CancellationToken ct = default)
        {
            if (maybe.HasNoValue())
                return Task.FromResult(maybe);

            var task = action(maybe.GetValue(), ct);

            return task.IsCompletedSuccessfully
                ? Task.FromResult(maybe)
                : Await(task, maybe);

            static async Task<Maybe<T>> Await(Task task, Maybe<T> maybe)
            {
                await task.ConfigureAwait(false);
                return maybe;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TOut Match<TOut>(Func<T, TOut> some, Func<TOut> none) =>
            maybe.HasValue() ? some(maybe.GetValue()) : none();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<TOut> MatchAsync<TOut>(
            Func<T, CancellationToken, Task<TOut>> some,
            Func<CancellationToken, Task<TOut>> none,
            CancellationToken ct = default) =>
            maybe.HasValue() ? some(maybe.GetValue(), ct) : none(ct);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Maybe<T> Or(Maybe<T> fallback) =>
            maybe.HasValue() ? maybe : fallback;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Maybe<T> Or(Func<Maybe<T>> fallback) =>
            maybe.HasValue() ? maybe : fallback();
    }
}
