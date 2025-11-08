namespace NotificaPix.Core.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message, "not_found")
    {
    }
}
