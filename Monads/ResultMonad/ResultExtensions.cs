using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using MWR.Monads.MaybeMonad;
using MWR.Monads.Messages;

namespace MWR.Monads.ResultMonad;

public static class ResultExtensions
{
    extension<T>(Result<T> result)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<T> WithSuccessInfo(Information info) =>
            result.IsSuccess()
                ? new Result<T>(result.GetValue(), ((Result)result).WithSuccessInfo(info))
                : result;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Maybe<T> ToMaybe() => result.IsSuccess() ? Maybe<T>.Some(result.GetValue()) : Maybe<T>.None;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<TOut> Map<TOut>(Func<T, TOut> func) =>
            new(result.IsSuccess() ? func(result.GetValue()) : default, result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Result<TOut>> MapAsync<TOut>(Func<T, Task<TOut>> func)
        {
            if (result.IsFailure())
                return Task.FromResult(new Result<TOut>(default, result));

            var task = func(result.GetValue());

            return task.IsCompletedSuccessfully
                ? Task.FromResult(new Result<TOut>(task.Result, result))
                : Await(task, result);

            static async Task<Result<TOut>> Await(Task<TOut> task, Result result)
                => new(await task.ConfigureAwait(false), result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Result<TOut>> MapAsync<TOut>(
            Func<T, CancellationToken, Task<TOut>> func,
            CancellationToken ct = default)
        {
            if (result.IsFailure())
                return Task.FromResult(new Result<TOut>(default, result));

            var task = func(result.GetValue(), ct);

            return task.IsCompletedSuccessfully
                ? Task.FromResult(new Result<TOut>(task.Result, result))
                : Await(task, result);

            static async Task<Result<TOut>> Await(Task<TOut> task, Result result)
                => new(await task.ConfigureAwait(false), result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> func) =>
            result.IsSuccess()
                ? func(result.GetValue())
                : Result<TOut>.Failure(result.RawErrors, result.RawWarnings, result.RawInfos);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result Bind(Func<T, Result> func) =>
            result.IsSuccess()
                ? func(result.GetValue())
                : Result.Failure(result.RawErrors, result.RawWarnings, result.RawInfos);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Result<TOut>> BindAsync<TOut>(Func<T, Task<Result<TOut>>> func) =>
            result.IsSuccess()
                ? func(result.GetValue())
                : Task.FromResult(Result<TOut>.Failure(result.RawErrors, result.RawWarnings, result.RawInfos));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Result<TOut>> BindAsync<TOut>(
            Func<T, CancellationToken, Task<Result<TOut>>> func,
            CancellationToken ct = default)
            => result.IsSuccess()
                ? func(result.GetValue(), ct)
                : Task.FromResult(Result<TOut>.Failure(result.RawErrors, result.RawWarnings, result.RawInfos));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Result> BindAsync(Func<T, Task<Result>> func) =>
            result.IsSuccess()
                ? func(result.GetValue())
                : Task.FromResult(Result.Failure(result.RawErrors, result.RawWarnings, result.RawInfos));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Result> BindAsync(
            Func<T, CancellationToken, Task<Result>> func,
            CancellationToken ct = default)
            => result.IsSuccess()
                ? func(result.GetValue(), ct)
                : Task.FromResult(Result.Failure(result.RawErrors, result.RawWarnings, result.RawInfos));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<T> Ensure(Func<T, bool> condition, Error error) =>
            result.IsFailure() || condition(result.GetValue())
                ? result
                : Result<T>.Failure([error], result.RawWarnings, result.RawInfos);

        // Loop + allocation — NOT inlined: would bloat call sites.
        public Result<T> Ensure(params (Func<T, bool>, Error)[] conditions)
        {
            if (result.IsFailure())
                return result;

            var value = result.GetValue();

            Error[]? errors = null;
            var count = 0;

            foreach (var (condition, error) in conditions)
            {
                if (condition(value)) continue;

                errors ??= new Error[conditions.Length];
                errors[count++] = error;
            }

            return count == 0
                ? result
                : Result<T>.Failure(errors![..count], result.RawWarnings, result.RawInfos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<T> EnsureNot(Func<T, bool> condition, Error error) =>
            result.IsFailure() || !condition(result.GetValue())
                ? result
                : Result<T>.Failure([error], result.RawWarnings, result.RawInfos);

        // Loop + allocation — NOT inlined.
        public Result<T> EnsureNotAny(params (Func<T, bool>, Error)[] conditions)
        {
            if (result.IsFailure())
                return result;

            var value = result.GetValue();

            Error[]? errors = null;
            var count = 0;

            foreach (var (condition, error) in conditions)
            {
                if (!condition(value)) continue;

                errors ??= new Error[conditions.Length];
                errors[count++] = error;
            }

            return count == 0
                ? result
                : Result<T>.Failure(errors![..count], result.RawWarnings, result.RawInfos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<T> Tap(Action<T> action)
        {
            if (result.IsSuccess()) action(result.GetValue());
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Result<T>> TapAsync(Func<T, Task> action)
        {
            if (result.IsFailure())
                return Task.FromResult(result);

            var task = action(result.GetValue());

            return task.IsCompletedSuccessfully
                ? Task.FromResult(result)
                : Await(task, result);

            static async Task<Result<T>> Await(Task task, Result<T> result)
            {
                await task.ConfigureAwait(false);
                return result;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Result<T>> TapAsync(Func<T, CancellationToken, Task> action, CancellationToken ct = default)
        {
            if (result.IsFailure())
                return Task.FromResult(result);

            var task = action(result.GetValue(), ct);

            return task.IsCompletedSuccessfully
                ? Task.FromResult(result)
                : Await(task, result);

            static async Task<Result<T>> Await(Task task, Result<T> result)
            {
                await task.ConfigureAwait(false);
                return result;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<T> TapError(Action<Error[]> action)
        {
            if (result.IsFailure()) action(result.GetErrors());
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<Error[], TOut> onFailure) =>
            result.IsSuccess()
                ? onSuccess(result.GetValue())
                : onFailure(result.GetErrors());
    }

    extension(Result result)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result WithSuccessInfo(Information info) =>
            result.IsSuccess()
                ? result with { RawInfos = [.. result.GetInfos(), info] }
                : result;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<T> Map<T>(Func<T> func) => new(result.IsSuccess() ? func() : default, result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Result<T>> MapAsync<T>(Func<Task<T>> func)
        {
            if (result.IsFailure())
                return Task.FromResult(new Result<T>(default, result));

            var task = func();

            return task.IsCompletedSuccessfully
                ? Task.FromResult(new Result<T>(task.Result, result))
                : Await(task, result);

            static async Task<Result<T>> Await(Task<T> task, Result result)
                => new(await task.ConfigureAwait(false), result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Result<T>> MapAsync<T>(Func<CancellationToken, Task<T>> func, CancellationToken ct = default)
        {
            if (result.IsFailure())
                return Task.FromResult(new Result<T>(default, result));

            var task = func(ct);

            return task.IsCompletedSuccessfully
                ? Task.FromResult(new Result<T>(task.Result, result))
                : Await(task, result);

            static async Task<Result<T>> Await(Task<T> task, Result result)
                => new(await task.ConfigureAwait(false), result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result Bind(Func<Result> func) => result.IsSuccess() ? func() : result;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Result> BindAsync(Func<Task<Result>> func) => result.IsSuccess() ? func() : Task.FromResult(result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result Tap(Action action)
        {
            if (result.IsSuccess()) action();
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Result> TapAsync(Func<Task> action)
        {
            if (result.IsFailure())
                return Task.FromResult(result);

            var task = action();

            return task.IsCompletedSuccessfully
                ? Task.FromResult(result)
                : Await(task, result);

            static async Task<Result> Await(Task task, Result result)
            {
                await task.ConfigureAwait(false);
                return result;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Result> TapAsync(Func<CancellationToken, Task> action, CancellationToken ct = default)
        {
            if (result.IsFailure())
                return Task.FromResult(result);

            var task = action(ct);

            return task.IsCompletedSuccessfully
                ? Task.FromResult(result)
                : Await(task, result);

            static async Task<Result> Await(Task task, Result result)
            {
                await task.ConfigureAwait(false);
                return result;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result TapError(Action<Error[]> action)
        {
            if (result.IsFailure()) action(result.GetErrors());
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TOut Match<TOut>(Func<TOut> onSuccess, Func<Error[], TOut> onFailure) =>
            result.IsSuccess()
                ? onSuccess()
                : onFailure(result.GetErrors());
    }
}
