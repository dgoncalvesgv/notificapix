namespace NotificaPix.Core.Contracts.Responses;

public record ApiKeyDto(Guid Id, string Name, bool IsActive, DateTime CreatedAt, DateTime? LastUsedAt);
