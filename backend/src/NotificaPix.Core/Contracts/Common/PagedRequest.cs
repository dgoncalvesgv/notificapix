namespace NotificaPix.Core.Contracts.Common;

public record PagedRequest(int Page = 1, int PageSize = 20)
{
    public int PageNormalized => Page < 1 ? 1 : Page;
    public int PageSizeNormalized => Math.Clamp(PageSize, 1, 100);
}
