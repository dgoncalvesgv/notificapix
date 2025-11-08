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
using NotificaPix.Core.Domain.Entities;
using NotificaPix.Core.Domain.Enums;
using NotificaPix.Infrastructure.Persistence;

namespace NotificaPix.Api.Endpoints;

public static class TeamEndpoints
{
    public static IEndpointRouteBuilder MapTeamEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/team").WithTags("Team");
        group.MapGet("/list", ListMembersAsync).RequireAuthorization();
        group.MapPost("/invite", InviteAsync).RequireAuthorization("OrgAdmin");
        group.MapPost("/accept", AcceptInviteAsync);
        group.MapPost("/remove", RemoveMemberAsync).RequireAuthorization("OrgAdmin");
        return app;
    }

    [Authorize]
    private static async Task<Ok<ApiResponse<IEnumerable<TeamMemberDto>>>> ListMembersAsync(
        ICurrentUserContext currentUser,
        NotificaPixDbContext context,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var members = await context.Memberships
            .Include(m => m.User)
            .Where(m => m.OrganizationId == currentUser.OrganizationId)
            .ToListAsync(cancellationToken);
        var dtos = mapper.Map<IEnumerable<TeamMemberDto>>(members);
        return TypedResults.Ok(ApiResponse<IEnumerable<TeamMemberDto>>.Ok(dtos.ToList()));
    }

    [Authorize(Policy = "OrgAdmin")]
    private static async Task<Ok<ApiResponse<InviteDto>>> InviteAsync(
        TeamInviteRequest request,
        ICurrentUserContext currentUser,
        NotificaPixDbContext context,
        IEmailSender emailSender,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var invite = new Invite
        {
            OrganizationId = currentUser.OrganizationId,
            Email = request.Email,
            Role = request.Role,
            Token = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await context.Invites.AddAsync(invite, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await emailSender.SendAsync(invite.Email, "Convite NotificaPix", $"Use o token {invite.Token} para entrar na organização.", cancellationToken);

        var dto = mapper.Map<InviteDto>(invite);
        return TypedResults.Ok(ApiResponse<InviteDto>.Ok(dto));
    }

    private static async Task<Results<Ok<ApiResponse<string>>, BadRequest<ApiResponse<string>>>> AcceptInviteAsync(
        TeamAcceptRequest request,
        NotificaPixDbContext context,
        IPasswordHasher passwordHasher,
        CancellationToken cancellationToken)
    {
        var invite = await context.Invites.Include(i => i.Organization)
            .FirstOrDefaultAsync(i => i.Token == request.Token && i.ExpiresAt > DateTime.UtcNow, cancellationToken);
        if (invite is null)
        {
            return TypedResults.BadRequest(ApiResponse<string>.Fail("Convite inválido ou expirado"));
        }

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == invite.Email, cancellationToken);
        var isNewUser = user is null;
        user ??= new User { Email = invite.Email };
        user.PasswordHash = passwordHasher.Hash(request.Password);
        user.IsActive = true;

        if (isNewUser)
        {
            await context.Users.AddAsync(user, cancellationToken);
        }

        var membershipExists = await context.Memberships.AnyAsync(m => m.OrganizationId == invite.OrganizationId && m.UserId == user.Id, cancellationToken);
        if (!membershipExists)
        {
            context.Memberships.Add(new Membership
            {
                OrganizationId = invite.OrganizationId,
                UserId = user.Id,
                Role = invite.Role
            });
        }

        invite.AcceptedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(ApiResponse<string>.Ok("Bem-vindo à organização!"));
    }

    [Authorize(Policy = "OrgAdmin")]
    private static async Task<Results<Ok<ApiResponse<string>>, NotFound<ApiResponse<string>>>> RemoveMemberAsync(
        TeamRemoveRequest request,
        ICurrentUserContext currentUser,
        NotificaPixDbContext context,
        CancellationToken cancellationToken)
    {
        var membership = await context.Memberships.FirstOrDefaultAsync(m => m.Id == request.MembershipId && m.OrganizationId == currentUser.OrganizationId, cancellationToken);
        if (membership is null)
        {
            return TypedResults.NotFound(ApiResponse<string>.Fail("Membership not found"));
        }

        context.Memberships.Remove(membership);
        await context.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(ApiResponse<string>.Ok("Membro removido"));
    }
}
