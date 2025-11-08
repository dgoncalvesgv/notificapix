namespace NotificaPix.Core.Contracts.Common;

public record PagedResult<T>(IReadOnlyCollection<T> Items, int Page, int PageSize, long TotalCount);
