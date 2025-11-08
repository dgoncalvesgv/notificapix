namespace NotificaPix.Core.Contracts.Requests;

public record ResetPasswordRequest(string Email, string Token, string NewPassword);
