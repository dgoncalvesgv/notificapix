using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NotificaPix.Core.Abstractions.Security;
using NotificaPix.Core.Contracts.Common;
using NotificaPix.Core.Contracts.Requests;
using NotificaPix.Core.Contracts.Responses;
using NotificaPix.Core.Domain.Enums;
using NotificaPix.Infrastructure.Persistence;

namespace NotificaPix.Api.Endpoints;

public static class OrganizationEndpoints
{
    public static IEndpointRouteBuilder MapOrganizationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/org")
            .WithTags("Organization")
            .RequireAuthorization();

        group.MapGet("/current", GetCurrentOrgAsync);
        group.MapPut("/update", UpdateOrgAsync).RequireAuthorization(policyNames: "OrgAdmin");

        return app;
    }

    private static async Task<Results<Ok<ApiResponse<OrganizationDto>>, NotFound<ApiResponse<OrganizationDto>>>> GetCurrentOrgAsync(
        ICurrentUserContext currentUser,
        NotificaPixDbContext context,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var organization = await context.Organizations.FirstOrDefaultAsync(o => o.Id == currentUser.OrganizationId, cancellationToken);
        if (organization is null)
        {
            return TypedResults.NotFound(ApiResponse<OrganizationDto>.Fail("Organization not found"));
        }

        var dto = mapper.Map<OrganizationDto>(organization) with { BillingEmail = organization.BillingEmail };
        return TypedResults.Ok(ApiResponse<OrganizationDto>.Ok(dto));
    }

    [Authorize(Policy = "OrgAdmin")]
    private static async Task<Results<Ok<ApiResponse<OrganizationDto>>, NotFound<ApiResponse<OrganizationDto>>>> UpdateOrgAsync(
        UpdateOrganizationRequest request,
        ICurrentUserContext currentUser,
        NotificaPixDbContext context,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var organization = await context.Organizations.FirstOrDefaultAsync(o => o.Id == currentUser.OrganizationId, cancellationToken);
        if (organization is null)
        {
            return TypedResults.NotFound(ApiResponse<OrganizationDto>.Fail("Organization not found"));
        }

        organization.Name = request.Name;
        organization.BillingEmail = request.BillingEmail;
        await context.SaveChangesAsync(cancellationToken);

        var dto = mapper.Map<OrganizationDto>(organization);
        return TypedResults.Ok(ApiResponse<OrganizationDto>.Ok(dto));
    }
}
