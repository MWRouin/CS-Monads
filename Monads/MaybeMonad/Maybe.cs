using System.Runtime.CompilerServices;

namespace MWR.Monads.MaybeMonad;

public static class Maybe
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Maybe<T> None<T>() => Maybe<T>.None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Maybe<T> Some<T>(T value) => Maybe<T>.Some(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Maybe<T> From<T>(T? value) => Maybe<T>.From(value);
}
