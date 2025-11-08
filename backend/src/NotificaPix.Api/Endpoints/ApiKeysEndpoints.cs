using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NotificaPix.Core.Abstractions.Security;
using NotificaPix.Core.Contracts.Common;
using NotificaPix.Core.Contracts.Requests;
using NotificaPix.Core.Contracts.Responses;
using NotificaPix.Core.Domain.Entities;
using NotificaPix.Infrastructure.Persistence;

namespace NotificaPix.Api.Endpoints;

public static class ApiKeysEndpoints
{
    public static IEndpointRouteBuilder MapApiKeysEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/apikeys").WithTags("API Keys").RequireAuthorization("OrgAdmin");
        group.MapGet("/", ListAsync);
        group.MapPost("/create", CreateAsync);
        group.MapPost("/revoke", RevokeAsync);
        return app;
    }

    private static async Task<Ok<ApiResponse<IEnumerable<ApiKeyDto>>>> ListAsync(
        ICurrentUserContext currentUser,
        NotificaPixDbContext context,
        CancellationToken cancellationToken)
    {
        var keys = await context.ApiKeys
            .Where(k => k.OrganizationId == currentUser.OrganizationId)
            .OrderByDescending(k => k.CreatedAt)
            .Select(k => new ApiKeyDto(k.Id, k.Name, k.IsActive, k.CreatedAt, k.LastUsedAt))
            .ToListAsync(cancellationToken);
        return TypedResults.Ok(ApiResponse<IEnumerable<ApiKeyDto>>.Ok(keys));
    }

    [Authorize(Policy = "OrgAdmin")]
    private static async Task<Ok<ApiResponse<ApiKeyCreatedResponse>>> CreateAsync(
        CreateApiKeyRequest request,
        ICurrentUserContext currentUser,
        NotificaPixDbContext context,
        CancellationToken cancellationToken)
    {
        var secret = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        var apiKey = new ApiKey
        {
            OrganizationId = currentUser.OrganizationId,
            Name = request.Name,
            KeyHash = Hash(secret)
        };

        context.ApiKeys.Add(apiKey);
        await context.SaveChangesAsync(cancellationToken);

        var dto = new ApiKeyDto(apiKey.Id, apiKey.Name, apiKey.IsActive, apiKey.CreatedAt, apiKey.LastUsedAt);
        return TypedResults.Ok(ApiResponse<ApiKeyCreatedResponse>.Ok(new ApiKeyCreatedResponse(dto, secret)));
    }

    [Authorize(Policy = "OrgAdmin")]
    private static async Task<Results<Ok<ApiResponse<string>>, NotFound<ApiResponse<string>>>> RevokeAsync(
        RevokeApiKeyRequest request,
        ICurrentUserContext currentUser,
        NotificaPixDbContext context,
        CancellationToken cancellationToken)
    {
        var apiKey = await context.ApiKeys.FirstOrDefaultAsync(k => k.Id == request.ApiKeyId && k.OrganizationId == currentUser.OrganizationId, cancellationToken);
        if (apiKey is null)
        {
            return TypedResults.NotFound(ApiResponse<string>.Fail("API key not found"));
        }

        apiKey.IsActive = false;
        await context.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(ApiResponse<string>.Ok("Revogado"));
    }

    private static string Hash(string value)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}
