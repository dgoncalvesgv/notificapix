using NotificaPix.Core.Domain.Enums;

namespace NotificaPix.Core.Domain.Entities;

public class Invite : EntityBase
{
    public Guid OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    public string Email { get; set; } = string.Empty;
    public MembershipRole Role { get; set; } = MembershipRole.OrgMember;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
}
