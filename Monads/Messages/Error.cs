namespace MWR.Monads.Messages;

public class Error : Message
{
    public Error(
        string code,
        string message,
        string? target = null)
        : base("Error." + code, message, target)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
    }
}