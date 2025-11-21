using AutoMapper;
using NotificaPix.Core.Contracts.Responses;
using NotificaPix.Core.Domain.Entities;

namespace NotificaPix.Infrastructure.Mapping;

public class DomainProfile : Profile
{
    public DomainProfile()
    {
        CreateMap<Organization, OrganizationDto>()
            .ConvertUsing<OrganizationToDtoConverter>();

        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Role, opt => opt.Ignore());

        CreateMap<PixStaticQrCode, PixStaticQrCodeDto>()
            .ConvertUsing<PixStaticQrCodeConverter>();

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

}
