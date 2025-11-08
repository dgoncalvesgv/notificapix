using NotificaPix.Core.Domain.Entities;
using NotificaPix.Core.Domain.Enums;

namespace NotificaPix.Core.Abstractions.Security;

public interface IJwtTokenService
{
    string Generate(User user, Organization organization, MembershipRole role);
}
