using NotificaPix.Core.Contracts.Common;
using NotificaPix.Core.Domain.Enums;

namespace NotificaPix.Core.Contracts.Requests;

public record ListAlertsRequest(
    AlertStatus? Status,
    AlertChannel? Channel,
    int Page = 1,
    int PageSize = 20) : PagedRequest(Page, PageSize);
