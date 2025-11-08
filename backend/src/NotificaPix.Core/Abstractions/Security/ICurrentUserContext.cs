using NotificaPix.Core.Domain.Enums;

namespace NotificaPix.Core.Abstractions.Security;

public interface ICurrentUserContext
{
    Guid UserId { get; }
    Guid OrganizationId { get; }
    MembershipRole Role { get; }
    bool IsInRole(MembershipRole role);
}
