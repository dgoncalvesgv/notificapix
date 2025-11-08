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
using NotificaPix.Core.Domain.Enums;
using NotificaPix.Infrastructure.Persistence;

namespace NotificaPix.Api.Endpoints;

public static class BankEndpoints
{
    public static IEndpointRouteBuilder MapBankEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/bank").WithTags("Bank").RequireAuthorization();
        group.MapPost("/connect/init", InitAsync).RequireAuthorization("OrgAdmin");
        group.MapPost("/connect/callback", CallbackAsync).RequireAuthorization("OrgAdmin");
        group.MapGet("/connections/list", ListAsync).RequireAuthorization("OrgAdmin");
        group.MapPost("/connections/revoke", RevokeAsync).RequireAuthorization("OrgAdmin");
        return app;
    }

    [Authorize(Policy = "OrgAdmin")]
    private static async Task<Ok<ApiResponse<string>>> InitAsync(
        BankConnectInitRequest request,
        IOpenFinanceProvider provider,
        ICurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
        var url = await provider.CreateConsentUrlAsync(currentUser.OrganizationId, cancellationToken);
        return TypedResults.Ok(ApiResponse<string>.Ok(url));
    }

    [Authorize(Policy = "OrgAdmin")]
    private static async Task<Ok<ApiResponse<BankConnectionDto>>> CallbackAsync(
        BankConnectCallbackRequest request,
        IOpenFinanceProvider provider,
        ICurrentUserContext currentUser,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var connection = await provider.CompleteConnectionAsync(currentUser.OrganizationId, request.ConsentId, cancellationToken);
        var dto = mapper.Map<BankConnectionDto>(connection);
        return TypedResults.Ok(ApiResponse<BankConnectionDto>.Ok(dto));
    }

    [Authorize(Policy = "OrgAdmin")]
    private static async Task<Ok<ApiResponse<IEnumerable<BankConnectionDto>>>> ListAsync(
        NotificaPixDbContext context,
        ICurrentUserContext currentUser,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var connections = await context.BankConnections
            .Where(c => c.OrganizationId == currentUser.OrganizationId)
            .ToListAsync(cancellationToken);
        var dtos = mapper.Map<IEnumerable<BankConnectionDto>>(connections);
        return TypedResults.Ok(ApiResponse<IEnumerable<BankConnectionDto>>.Ok(dtos.ToList()));
    }

    [Authorize(Policy = "OrgAdmin")]
    private static async Task<Results<Ok<ApiResponse<string>>, NotFound<ApiResponse<string>>>> RevokeAsync(
        BankConnectionRevokeRequest request,
        NotificaPixDbContext context,
        ICurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
        var connection = await context.BankConnections.FirstOrDefaultAsync(c => c.Id == request.ConnectionId && c.OrganizationId == currentUser.OrganizationId, cancellationToken);
        if (connection is null)
        {
            return TypedResults.NotFound(ApiResponse<string>.Fail("Connection not found"));
        }

        connection.Status = BankConnectionStatus.Revoked;
        await context.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(ApiResponse<string>.Ok("Connection revoked"));
    }
}
