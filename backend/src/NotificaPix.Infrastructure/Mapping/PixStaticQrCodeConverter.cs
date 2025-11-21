using AutoMapper;
using NotificaPix.Core.Contracts.Responses;
using NotificaPix.Core.Domain.Entities;

namespace NotificaPix.Infrastructure.Mapping;

public class PixStaticQrCodeConverter : ITypeConverter<PixStaticQrCode, PixStaticQrCodeDto>
{
    public PixStaticQrCodeDto Convert(PixStaticQrCode source, PixStaticQrCodeDto destination, ResolutionContext context)
    {
        var label = source.PixKey != null ? source.PixKey.Label : string.Empty;
        return new PixStaticQrCodeDto(
            source.Id,
            source.Amount,
            source.Description,
            source.Payload,
            source.TxId,
            label,
            source.CreatedAt);
    }
}
