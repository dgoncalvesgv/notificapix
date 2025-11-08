using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NotificaPix.Core.Abstractions.Security;
using NotificaPix.Core.Contracts.Common;
using NotificaPix.Core.Contracts.Responses;
using NotificaPix.Infrastructure.Persistence;

namespace NotificaPix.Api.Endpoints;

public static class AuditEndpoints
{
    public static IEndpointRouteBuilder MapAuditEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/audit-logs").WithTags("Audit").RequireAuthorization("OrgAdmin");
        group.MapGet("/", ListAsync);
        return app;
    }

    private static async Task<Ok<ApiResponse<IEnumerable<AuditLogDto>>>> ListAsync(
        ICurrentUserContext currentUser,
        NotificaPixDbContext context,
        CancellationToken cancellationToken)
    {
        var logs = await context.AuditLogs
            .Where(l => l.OrganizationId == currentUser.OrganizationId)
            .OrderByDescending(l => l.CreatedAt)
            .Take(200)
            .Select(l => new AuditLogDto(l.Id, l.Action, l.DataJson, l.CreatedAt))
            .ToListAsync(cancellationToken);
        return TypedResults.Ok(ApiResponse<IEnumerable<AuditLogDto>>.Ok(logs));
    }
}
