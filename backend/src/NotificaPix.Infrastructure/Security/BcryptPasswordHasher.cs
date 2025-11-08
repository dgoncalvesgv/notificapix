using BCrypt.Net;
using NotificaPix.Core.Abstractions.Security;

namespace NotificaPix.Infrastructure.Security;

public class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string value) => BCrypt.Net.BCrypt.EnhancedHashPassword(value);

    public bool Verify(string hash, string value) => BCrypt.Net.BCrypt.EnhancedVerify(value, hash);
}
