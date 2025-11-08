namespace NotificaPix.Core.Abstractions.Security;

public interface IPasswordHasher
{
    string Hash(string value);
    bool Verify(string hash, string value);
}
