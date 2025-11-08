using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NotificaPix.Core.Abstractions.Security;
using NotificaPix.Core.Contracts.Common;
using NotificaPix.Core.Contracts.Requests;
using NotificaPix.Core.Contracts.Responses;
using NotificaPix.Infrastructure.Persistence;

namespace NotificaPix.Api.Endpoints;

public static class TransactionEndpoints
{
    public static IEndpointRouteBuilder MapTransactionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/transactions").WithTags("Transactions").RequireAuthorization();
        group.MapPost("/list", ListAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);
        return app;
    }

    [Authorize]
    private static async Task<Ok<ApiResponse<PagedResult<PixTransactionDto>>>> ListAsync(
        ListTransactionsRequest request,
        ICurrentUserContext currentUser,
        NotificaPixDbContext context,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var query = context.PixTransactions.Where(t => t.OrganizationId == currentUser.OrganizationId);
        if (request.From.HasValue)
        {
            query = query.Where(t => t.OccurredAt >= request.From);
        }
        if (request.To.HasValue)
        {
            query = query.Where(t => t.OccurredAt <= request.To);
        }
        if (request.MinAmount.HasValue)
        {
            query = query.Where(t => t.Amount >= request.MinAmount);
        }
        if (request.MaxAmount.HasValue)
        {
            query = query.Where(t => t.Amount <= request.MaxAmount);
        }
        if (!string.IsNullOrWhiteSpace(request.TxId))
        {
            query = query.Where(t => t.TxId.Contains(request.TxId));
        }
        if (!string.IsNullOrWhiteSpace(request.PayerKey))
        {
            query = query.Where(t => t.PayerKey.Contains(request.PayerKey));
        }

        var total = await query.LongCountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(t => t.OccurredAt)
            .Skip((request.PageNormalized - 1) * request.PageSizeNormalized)
            .Take(request.PageSizeNormalized)
            .ToListAsync(cancellationToken);

        var dto = mapper.Map<IEnumerable<PixTransactionDto>>(items);
        var result = new PagedResult<PixTransactionDto>(dto.ToList(), request.PageNormalized, request.PageSizeNormalized, total);
        return TypedResults.Ok(ApiResponse<PagedResult<PixTransactionDto>>.Ok(result));
    }

    [Authorize]
    private static async Task<Results<Ok<ApiResponse<PixTransactionDto>>, NotFound<ApiResponse<PixTransactionDto>>>> GetByIdAsync(
        Guid id,
        ICurrentUserContext currentUser,
        NotificaPixDbContext context,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var transaction = await context.PixTransactions.FirstOrDefaultAsync(t => t.Id == id && t.OrganizationId == currentUser.OrganizationId, cancellationToken);
        if (transaction is null)
        {
            return TypedResults.NotFound(ApiResponse<PixTransactionDto>.Fail("Transaction not found"));
        }

        var dto = mapper.Map<PixTransactionDto>(transaction);
        return TypedResults.Ok(ApiResponse<PixTransactionDto>.Ok(dto));
    }
}
