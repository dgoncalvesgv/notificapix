namespace NotificaPix.Core.Contracts.Requests;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
