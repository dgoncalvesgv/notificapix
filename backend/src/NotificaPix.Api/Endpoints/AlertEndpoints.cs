using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NotificaPix.Core.Abstractions.Security;
using NotificaPix.Core.Abstractions.Services;
using NotificaPix.Core.Contracts.Common;
using NotificaPix.Core.Contracts.Requests;
using NotificaPix.Core.Contracts.Responses;
using NotificaPix.Infrastructure.Persistence;

namespace NotificaPix.Api.Endpoints;

public static class AlertEndpoints
{
    public static IEndpointRouteBuilder MapAlertEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/alerts").WithTags("Alerts").RequireAuthorization();
        group.MapPost("/list", ListAsync);
        group.MapPost("/test", SendTestAsync).RequireAuthorization("OrgAdmin");
        return app;
    }

    private static async Task<Ok<ApiResponse<PagedResult<AlertDto>>>> ListAsync(
        ListAlertsRequest request,
        ICurrentUserContext currentUser,
        NotificaPixDbContext context,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var query = context.Alerts.Where(a => a.OrganizationId == currentUser.OrganizationId);
        if (request.Status.HasValue)
        {
            query = query.Where(a => a.Status == request.Status);
        }
        if (request.Channel.HasValue)
        {
            query = query.Where(a => a.Channel == request.Channel);
        }

        var total = await query.LongCountAsync(cancellationToken);
        var items = await query.OrderByDescending(a => a.CreatedAt)
            .Skip((request.PageNormalized - 1) * request.PageSizeNormalized)
            .Take(request.PageSizeNormalized)
            .ToListAsync(cancellationToken);

        var dtos = mapper.Map<IEnumerable<AlertDto>>(items);
        var result = new PagedResult<AlertDto>(dtos.ToList(), request.PageNormalized, request.PageSizeNormalized, total);
        return TypedResults.Ok(ApiResponse<PagedResult<AlertDto>>.Ok(result));
    }

    [Authorize(Policy = "OrgAdmin")]
    private static async Task<Ok<ApiResponse<AlertDto>>> SendTestAsync(
        AlertTestRequest request,
        ICurrentUserContext currentUser,
        IAlertService alertService,
        CancellationToken cancellationToken)
    {
        var alert = await alertService.DispatchTestAlertAsync(currentUser.OrganizationId, request, cancellationToken);
        return TypedResults.Ok(ApiResponse<AlertDto>.Ok(alert));
    }
}
