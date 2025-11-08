using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NotificaPix.Core.Abstractions.Security;
using NotificaPix.Core.Contracts.Common;
using NotificaPix.Core.Contracts.Responses;
using NotificaPix.Infrastructure.Persistence;

namespace NotificaPix.Api.Endpoints;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/app").WithTags("Dashboard").RequireAuthorization();
        group.MapGet("/overview", GetOverviewAsync);
        return app;
    }

    private static async Task<Ok<ApiResponse<OverviewMetricsResponse>>> GetOverviewAsync(
        ICurrentUserContext currentUser,
        NotificaPixDbContext context,
        CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var sevenDays = today.AddDays(-7);
        var thirtyDays = today.AddDays(-30);

        var baseQuery = context.PixTransactions.Where(t => t.OrganizationId == currentUser.OrganizationId);

        var todayTotal = await baseQuery.Where(t => t.OccurredAt >= today).SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0;
        var sevenTotal = await baseQuery.Where(t => t.OccurredAt >= sevenDays).SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0;
        var thirtyTotal = await baseQuery.Where(t => t.OccurredAt >= thirtyDays).SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0;

        var recent = await baseQuery.OrderByDescending(t => t.OccurredAt).Take(10)
            .Select(t => new PixTransactionDto(t.Id, t.TxId, t.EndToEndId, t.Amount, t.OccurredAt, t.PayerName, t.PayerKey, t.Description))
            .ToListAsync(cancellationToken);

        var alertsToday = await context.Alerts.CountAsync(a => a.OrganizationId == currentUser.OrganizationId && a.CreatedAt >= today, cancellationToken);
        var connections = await context.BankConnections.CountAsync(b => b.OrganizationId == currentUser.OrganizationId && b.Status == Core.Domain.Enums.BankConnectionStatus.Active, cancellationToken);

        var response = new OverviewMetricsResponse(todayTotal, sevenTotal, thirtyTotal, recent, alertsToday, connections);
        return TypedResults.Ok(ApiResponse<OverviewMetricsResponse>.Ok(response));
    }
}
