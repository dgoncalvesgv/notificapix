using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NotificaPix.Core.Abstractions.Security;
using NotificaPix.Core.Abstractions.Services;
using NotificaPix.Core.Contracts.Common;
using NotificaPix.Core.Contracts.Responses;
using NotificaPix.Infrastructure.Persistence;

namespace NotificaPix.Api.Endpoints;

public static class QuotaEndpoints
{
    public static IEndpointRouteBuilder MapQuotaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/quota").WithTags("Quota").RequireAuthorization();
        group.MapGet("/usage", GetUsageAsync);
        return app;
    }

    private static async Task<Results<Ok<ApiResponse<UsageResponse>>, NotFound<ApiResponse<UsageResponse>>>> GetUsageAsync(
        ICurrentUserContext currentUser,
        NotificaPixDbContext context,
        IUsageService usageService,
        CancellationToken cancellationToken)
    {
        var organization = await context.Organizations.FirstOrDefaultAsync(o => o.Id == currentUser.OrganizationId, cancellationToken);
        if (organization is null)
        {
            return TypedResults.NotFound(ApiResponse<UsageResponse>.Fail("Organization not found"));
        }

        var response = new UsageResponse(organization.UsageCount, usageService.ResolveQuota(organization), organization.UsageMonth);
        return TypedResults.Ok(ApiResponse<UsageResponse>.Ok(response));
    }
}
