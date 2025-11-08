using NotificaPix.Core.Domain.Enums;

namespace NotificaPix.Core.Contracts.Responses;

public record BankConnectionDto(Guid Id, BankProvider Provider, string ConsentId, BankConnectionStatus Status, DateTime? ConnectedAt);
