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

public static class SettingsEndpoints
{
    public static IEndpointRouteBuilder MapSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/settings").WithTags("Settings").RequireAuthorization("OrgAdmin");
        group.MapGet("/notifications", GetNotificationsAsync);
        group.MapPut("/notifications", UpdateNotificationsAsync);
        return app;
    }

    private static async Task<Results<Ok<ApiResponse<NotificationSettingsDto>>, NotFound<ApiResponse<NotificationSettingsDto>>>> GetNotificationsAsync(
        ICurrentUserContext currentUser,
        NotificaPixDbContext context,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var settings = await context.NotificationSettings.FirstOrDefaultAsync(n => n.OrganizationId == currentUser.OrganizationId, cancellationToken);
        if (settings is null)
        {
            return TypedResults.NotFound(ApiResponse<NotificationSettingsDto>.Fail("Settings not found"));
        }

        var dto = mapper.Map<NotificationSettingsDto>(settings);
        return TypedResults.Ok(ApiResponse<NotificationSettingsDto>.Ok(dto));
    }

    private static async Task<Ok<ApiResponse<NotificationSettingsDto>>> UpdateNotificationsAsync(
        NotificationSettingsRequest request,
        ICurrentUserContext currentUser,
        NotificaPixDbContext context,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var settings = await context.NotificationSettings.FirstOrDefaultAsync(n => n.OrganizationId == currentUser.OrganizationId, cancellationToken)
                       ?? new NotificaPix.Core.Domain.Entities.NotificationSettings { OrganizationId = currentUser.OrganizationId };

        settings.EmailsCsv = string.Join(',', request.Emails);
        settings.WebhookUrl = request.WebhookUrl;
        settings.WebhookSecret = request.WebhookSecret;
        settings.Enabled = request.Enabled;

        context.NotificationSettings.Update(settings);
        await context.SaveChangesAsync(cancellationToken);

        var dto = mapper.Map<NotificationSettingsDto>(settings);
        return TypedResults.Ok(ApiResponse<NotificationSettingsDto>.Ok(dto));
    }
}
