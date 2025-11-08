namespace NotificaPix.Core.Contracts.Responses;

public record AuthResponse(string Token, UserDto User, OrganizationDto Organization);
