using NotificaPix.Core.Contracts.Common;

namespace NotificaPix.Core.Contracts.Requests;

public record ListTransactionsRequest(
    DateTime? From,
    DateTime? To,
    decimal? MinAmount,
    decimal? MaxAmount,
    string? TxId,
    string? PayerKey,
    int Page = 1,
    int PageSize = 20) : PagedRequest(Page, PageSize);
