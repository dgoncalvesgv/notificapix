namespace NotificaPix.Core.Contracts.Responses;

public record AuditLogDto(Guid Id, string Action, string DataJson, DateTime CreatedAt);
