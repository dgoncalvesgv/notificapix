namespace NotificaPix.Core.Contracts.Requests;

public record PixQrCodeQuery(
    string? Description,
    DateOnly? CreatedFrom,
    DateOnly? CreatedTo,
    string? SortBy,
    string? SortDirection);
