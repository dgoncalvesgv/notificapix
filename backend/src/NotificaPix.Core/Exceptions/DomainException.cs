namespace NotificaPix.Core.Exceptions;

public class DomainException : Exception
{
    public string? ErrorCode { get; }

    public DomainException(string message, string? errorCode = null) : base(message) => ErrorCode = errorCode;
}
