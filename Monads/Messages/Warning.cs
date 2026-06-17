namespace MWR.Monads.Messages;

public class Warning : Message
{
    public Warning(
        string code,
        string message,
        string? target = null)
        : base("Warn." + code, message, target)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
    }
}