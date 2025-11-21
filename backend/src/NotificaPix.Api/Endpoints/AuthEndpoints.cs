using AutoMapper;
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

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");
        group.MapPost("/register", RegisterAsync);
        group.MapPost("/login", LoginAsync);
        group.MapPost("/forgot", ForgotAsync);
        group.MapPost("/reset", ResetAsync);
        group.MapPost("/change-password", ChangePasswordAsync).RequireAuthorization();
        return app;
    }

    private static async Task<Results<Ok<ApiResponse<AuthResponse>>, Conflict<ApiResponse<AuthResponse>>>> RegisterAsync(
        RegisterRequest request,
        NotificaPixDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        if (await context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
        {
            return TypedResults.Conflict(ApiResponse<AuthResponse>.Fail("E-mail já cadastrado", "email_exists"));
        }

        var organization = new Organization
        {
            Name = request.OrganizationName,
            Slug = await GenerateUniqueSlugAsync(context, request.OrganizationName, cancellationToken),
            BillingEmail = request.Email
        };

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = passwordHasher.Hash(request.Password)
        };

        var membership = new Membership
        {
            Organization = organization,
            User = user,
            Role = MembershipRole.OrgAdmin
        };

        var settings = new NotificationSettings
        {
            Organization = organization,
            EmailsCsv = request.Email
        };

        await context.Memberships.AddAsync(membership, cancellationToken);
        await context.NotificationSettings.AddAsync(settings, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var token = jwtTokenService.Generate(user, organization, MembershipRole.OrgAdmin);
        var userDto = new UserDto(user.Id, user.Name, user.Email, MembershipRole.OrgAdmin);
        var orgDto = mapper.Map<OrganizationDto>(organization);
        return TypedResults.Ok(ApiResponse<AuthResponse>.Ok(new AuthResponse(token, userDto, orgDto)));
    }

    private static async Task<Results<Ok<ApiResponse<AuthResponse>>, UnauthorizedHttpResult>> LoginAsync(
        LoginRequest request,
        NotificaPixDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var membership = await context.Memberships
            .Include(m => m.User)
            .Include(m => m.Organization)
            .FirstOrDefaultAsync(m => m.User!.Email == request.Email, cancellationToken);

        if (membership?.User is null || membership.Organization is null)
        {
            return TypedResults.Unauthorized();
        }

        if (!passwordHasher.Verify(membership.User.PasswordHash, request.Password))
        {
            return TypedResults.Unauthorized();
        }

        var token = jwtTokenService.Generate(membership.User, membership.Organization, membership.Role);
        var userDto = new UserDto(membership.User.Id, membership.User.Name, membership.User.Email, membership.Role);
        var orgDto = mapper.Map<OrganizationDto>(membership.Organization);
        return TypedResults.Ok(ApiResponse<AuthResponse>.Ok(new AuthResponse(token, userDto, orgDto)));
    }

    private static async Task<Ok<ApiResponse<string>>> ForgotAsync(
        ForgotPasswordRequest request,
        IEmailSender emailSender,
        CancellationToken cancellationToken)
    {
        // Mock e-mail sending
        await emailSender.SendAsync(request.Email, "Recuperar senha", "Use o link para resetar sua senha: https://notificapix.dev/reset", cancellationToken);
        return TypedResults.Ok(ApiResponse<string>.Ok("Instructions sent"));
    }

    private static Ok<ApiResponse<string>> ResetAsync(ResetPasswordRequest request) =>
        TypedResults.Ok(ApiResponse<string>.Ok("Reset token accepted (mock)."));

    private static async Task<Results<Ok<ApiResponse<string>>, BadRequest<ApiResponse<string>>, NotFound<ApiResponse<string>>>> ChangePasswordAsync(
        ChangePasswordRequest request,
        ICurrentUserContext currentUser,
        NotificaPixDbContext context,
        IPasswordHasher passwordHasher,
        CancellationToken cancellationToken)
    {
        var membership = await context.Memberships
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.UserId == currentUser.UserId, cancellationToken);

        if (membership?.User is null)
        {
            return TypedResults.NotFound(ApiResponse<string>.Fail("Usuário não encontrado"));
        }

        if (!passwordHasher.Verify(membership.User.PasswordHash, request.CurrentPassword))
        {
            return TypedResults.BadRequest(ApiResponse<string>.Fail("Senha atual incorreta", "invalid_password"));
        }

        membership.User.PasswordHash = passwordHasher.Hash(request.NewPassword);
        await context.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(ApiResponse<string>.Ok("Senha atualizada com sucesso"));
    }

    private static string Slugify(string value) =>
        string.Join('-', value.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    private static async Task<string> GenerateUniqueSlugAsync(NotificaPixDbContext context, string orgName, CancellationToken cancellationToken)
    {
        var baseSlug = Slugify(orgName);
        var slug = baseSlug;
        var suffix = 1;
        while (await context.Organizations.AnyAsync(o => o.Slug == slug, cancellationToken))
        {
            slug = $"{baseSlug}-{suffix++:00}";
        }

        return slug;
    }
}
