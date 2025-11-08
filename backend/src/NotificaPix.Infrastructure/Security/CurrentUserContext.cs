using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NotificaPix.Core.Abstractions.Security;
using NotificaPix.Core.Domain.Enums;

namespace NotificaPix.Infrastructure.Security;

public class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserContext(IHttpContextAccessor accessor) => _accessor = accessor;

    private ClaimsPrincipal Principal => _accessor.HttpContext?.User ?? throw new InvalidOperationException("No HttpContext available");

    public Guid UserId
    {
        get
        {
            var id = FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? FindFirstValue(ClaimTypes.Name)
                     ?? FindFirstValue("sub");
            return Guid.Parse(id ?? throw new InvalidOperationException("User claim missing"));
        }
    }

    public Guid OrganizationId => Guid.Parse(FindFirstValue("orgId") ?? throw new InvalidOperationException("Organization claim missing"));

    public MembershipRole Role => Enum.TryParse<MembershipRole>(FindFirstValue(ClaimTypes.Role), out var role) ? role : MembershipRole.OrgMember;

    public bool IsInRole(MembershipRole role) => Role == role;

    private string? FindFirstValue(string claimType) => Principal.FindFirst(claimType)?.Value;
}
