using NotificaPix.Core.Domain.Enums;

namespace NotificaPix.Core.Contracts.Responses;

public record UserDto(Guid Id, string Email, MembershipRole Role);
