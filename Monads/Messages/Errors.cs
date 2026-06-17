namespace MWR.Monads.Messages;

public sealed class BadRequestError(string code, string message, string? target = null)
    : Error(code, message, target);

public sealed class NotFoundError(string code, string message, string? target = null)
    : Error(code, message, target);

public sealed class ConflictError(string code, string message, string? target = null)
    : Error(code, message, target);

public sealed class UnauthorizedError(string code, string message, string? target = null)
    : Error(code, message, target);

public sealed class ForbiddenError(string code, string message, string? target = null)
    : Error(code, message, target);

public sealed class ServerError(string code, string message, string? target = null)
    : Error(code, message, target);