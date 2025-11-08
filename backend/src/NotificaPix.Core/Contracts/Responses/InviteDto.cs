using NotificaPix.Core.Domain.Enums;

namespace NotificaPix.Core.Contracts.Responses;

public record InviteDto(Guid Id, string Email, MembershipRole Role, DateTime ExpiresAt, DateTime? AcceptedAt);
