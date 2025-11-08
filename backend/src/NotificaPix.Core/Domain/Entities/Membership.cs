using NotificaPix.Core.Domain.Enums;

namespace NotificaPix.Core.Domain.Entities;

public class Membership : EntityBase
{
    public Guid OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public MembershipRole Role { get; set; } = MembershipRole.OrgMember;
}
