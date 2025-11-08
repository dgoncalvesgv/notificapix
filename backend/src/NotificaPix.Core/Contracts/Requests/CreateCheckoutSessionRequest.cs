using NotificaPix.Core.Domain.Enums;

namespace NotificaPix.Core.Contracts.Requests;

public record CreateCheckoutSessionRequest(PlanType Plan);
