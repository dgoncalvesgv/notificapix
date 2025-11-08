namespace NotificaPix.Core.Contracts.Requests;

public record RegisterRequest(string Name, string Email, string Password, string OrganizationName);
