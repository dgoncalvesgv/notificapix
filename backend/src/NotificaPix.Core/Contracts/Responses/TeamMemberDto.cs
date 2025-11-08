using NotificaPix.Core.Domain.Enums;

namespace NotificaPix.Core.Contracts.Responses;

public record TeamMemberDto(Guid MembershipId, Guid UserId, string Email, MembershipRole Role, DateTime JoinedAt);
