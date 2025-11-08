using NotificaPix.Core.Domain.Enums;

namespace NotificaPix.Core.Contracts.Responses;

public record AlertDto(Guid Id, AlertChannel Channel, AlertStatus Status, DateTime? LastAttemptAt, int Attempts, string PayloadJson);
