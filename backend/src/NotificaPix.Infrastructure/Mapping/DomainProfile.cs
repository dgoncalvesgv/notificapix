using AutoMapper;
using NotificaPix.Core.Contracts.Responses;
using NotificaPix.Core.Domain.Entities;

namespace NotificaPix.Infrastructure.Mapping;

public class DomainProfile : Profile
{
    public DomainProfile()
    {
        CreateMap<Organization, OrganizationDto>()
            .ConstructUsing(src => new OrganizationDto(src.Id, src.Name, src.Slug, src.Plan, src.UsageCount, ResolveQuota(src.Plan), src.BillingEmail));

        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Role, opt => opt.Ignore());

        CreateMap<PixTransaction, PixTransactionDto>();
        CreateMap<Alert, AlertDto>();
        CreateMap<BankConnection, BankConnectionDto>();
        CreateMap<NotificationSettings, NotificationSettingsDto>()
            .ConstructUsing(src => new NotificationSettingsDto(
                (src.EmailsCsv ?? string.Empty)
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                src.WebhookUrl,
                src.WebhookSecret,
                src.Enabled));
        CreateMap<Invite, InviteDto>();
        CreateMap<ApiKey, ApiKeyDto>();
        CreateMap<Membership, TeamMemberDto>()
            .ConstructUsing(src => new TeamMemberDto(
                src.Id,
                src.UserId,
                src.User!.Email,
                src.Role,
                src.CreatedAt));
        CreateMap<AuditLog, AuditLogDto>();
    }
    private static int ResolveQuota(NotificaPix.Core.Domain.Enums.PlanType plan) =>
        plan == NotificaPix.Core.Domain.Enums.PlanType.Pro
            ? 1000
            : plan == NotificaPix.Core.Domain.Enums.PlanType.Business
                ? int.MaxValue
                : 100;
}
