namespace MWR.Monads.Messages;

public class Information : Message
{
    public Information(
        string code,
        string message,
        string? target = null)
        : base("Info." + code, message, target)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
    }
}