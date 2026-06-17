using System.Runtime.CompilerServices;
using System.Text;

namespace MWR.Monads.Messages;

public abstract class Message : IEquatable<Message>, IFormattable
{
    protected Message(
        string code,
        string message,
        string? target = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrEmpty(message);

        Code = code;
        Content = message;
        Target = target;
    }

    public string Code { get; }

    public string Content { get; }

    public string? Target { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator string(Message msg) => msg.Content;

    public override bool Equals(object? obj) => obj is Message other && Equals(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Message? other)
        => other is not null && string.Equals(Code, other.Code, StringComparison.Ordinal);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
        => Code.GetHashCode(StringComparison.Ordinal);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Message? left, Message? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Message? left, Message? right)
        => !(left == right);

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (string.IsNullOrEmpty(format) || format.Equals("G", StringComparison.OrdinalIgnoreCase))
            return ToString();

        bool hasC = false, hasM = false, hasT = false, hasF = false;

        foreach (var ch in format)
        {
            switch (char.ToUpperInvariant(ch))
            {
                case 'C': hasC = true; break;
                case 'M': hasM = true; break;
                case 'T': hasT = true; break;
                case 'F': hasF = true; break;
                default:
                    throw new FormatException(
                        $"The '{ch}' format specifier is not supported on {nameof(Message)}.");
            }
        }

        if (hasF || (!hasC && !hasM && !hasT))
            return ToString();

        var sb = new StringBuilder();

        if (hasC) sb.Append("Code: ").Append(Code);

        if (hasT && Target is not null)
        {
            if (sb.Length > 0) sb.Append('\n');
            sb.Append("For Target: ").Append(Target);
        }

        if (hasM)
        {
            if (sb.Length > 0) sb.Append('\n');
            sb.Append("Content: ").Append(Content);
        }

        return sb.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToString(string? format) => ToString(format, null);

    public override string ToString() => Target is null
        ? $"""
           Code: {Code}
           Content: {Content}
           """
        : $"""
           Code: {Code}
           For Target: {Target}
           Content: {Content}
           """;
}
