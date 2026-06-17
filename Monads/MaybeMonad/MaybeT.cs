using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MWR.Monads.MaybeMonad;

[DebuggerDisplay("{DebugDisplay,nq}")]
public readonly struct Maybe<T> : IEquatable<Maybe<T>>
{
    private string DebugDisplay => _hasValue ? $"Some({_value})" : "None";

    private readonly T? _value;
    private readonly bool _hasValue;

    public Maybe() // None
    {
        _value = default;
        _hasValue = false;
    }

    private Maybe(T value) // Some(value)
    {
        ArgumentNullException.ThrowIfNull(value);

        _value = value;
        _hasValue = true;
    }

    [MemberNotNullWhen(true, nameof(_value))]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasValue() => _hasValue;

    [MemberNotNullWhen(false, nameof(_value))]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasNoValue() => !_hasValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValue() =>
        HasValue()
            ? _value
            : throw new InvalidOperationException("The value of None cannot be accessed.");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValueOr(T defaultValue) => HasValue() ? _value : defaultValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T? GetValueOrDefault() => _hasValue ? _value : default;

    public static readonly Maybe<T> None = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Maybe<T> Some(T value) => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Maybe<T> From(T? value) => value is null ? new Maybe<T>() : new Maybe<T>(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Maybe<T>(T? value) => From(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator T(Maybe<T> maybe) => maybe.GetValue();

    #region IEquatable

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Maybe<T> other)
    {
        if (!(_hasValue || other._hasValue)) return true;

        return _hasValue == other._hasValue
               && EqualityComparer<T?>.Default.Equals(_value, other._value);
    }

    public override bool Equals(object? obj) => obj is Maybe<T> other && Equals(other);

    public override string ToString() => _hasValue ? $"Some({_value})" : "None";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => HasValue() ? EqualityComparer<T>.Default.GetHashCode(_value) : 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Maybe<T> left, Maybe<T> right) => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Maybe<T> left, Maybe<T> right) => !left.Equals(right);

    #endregion
}
