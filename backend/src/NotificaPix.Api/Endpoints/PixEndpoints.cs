using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using NotificaPix.Core.Abstractions.Security;
using NotificaPix.Core.Contracts.Common;
using NotificaPix.Core.Contracts.Requests;
using NotificaPix.Core.Contracts.Responses;
using NotificaPix.Core.Domain.Entities;
using NotificaPix.Infrastructure.Persistence;

namespace NotificaPix.Api.Endpoints;

public static class PixEndpoints
{
    public static IEndpointRouteBuilder MapPixEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/pix").WithTags("Pix").RequireAuthorization("OrgAdmin");
        group.MapGet("/keys", GetPixKeysAsync);
        group.MapPost("/keys", CreatePixKeyAsync);
        group.MapPost("/keys/select", SelectPixKeyAsync);
        group.MapDelete("/keys/{pixKeyId:guid}", DeletePixKeyAsync);
        group.MapGet("/qrcodes", ListQrCodesAsync);
        group.MapPost("/qrcodes", CreateQrCodeAsync);
        return app;
    }

    private static async Task<Ok<ApiResponse<PixReceiversResponse>>> GetPixKeysAsync(
        ICurrentUserContext currentUser,
        NotificaPixDbContext context,
        CancellationToken cancellationToken)
    {
        var organization = await context.Organizations.FirstAsync(o => o.Id == currentUser.OrganizationId, cancellationToken);
        var pixKeys = await context.PixKeys
            .Where(k => k.OrganizationId == currentUser.OrganizationId)
            .OrderBy(k => k.Label)
            .ToListAsync(cancellationToken);

        var options = pixKeys
            .Select(k => new PixReceiverDto(
                k.Id,
                k.Label,
                k.KeyType,
                k.KeyValue,
                organization.DefaultPixKeyId == k.Id))
            .ToList();

        var selected = options.FirstOrDefault(o => o.IsDefault);
        return TypedResults.Ok(ApiResponse<PixReceiversResponse>.Ok(new PixReceiversResponse(options, selected)));
    }

    private static async Task<Results<Ok<ApiResponse<PixReceiverDto>>, BadRequest<ApiResponse<string>>>> CreatePixKeyAsync(
        CreatePixKeyRequest request,
        ICurrentUserContext currentUser,
        NotificaPixDbContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Label) || string.IsNullOrWhiteSpace(request.KeyType) || string.IsNullOrWhiteSpace(request.KeyValue))
        {
            return TypedResults.BadRequest(ApiResponse<string>.Fail("Preencha todos os campos da chave Pix."));
        }

        var pixKey = new PixKey
        {
            OrganizationId = currentUser.OrganizationId,
            Label = request.Label.Trim(),
            KeyType = request.KeyType.Trim(),
            KeyValue = request.KeyValue.Trim()
        };
        context.PixKeys.Add(pixKey);
        await context.SaveChangesAsync(cancellationToken);

        var organization = await context.Organizations.FirstAsync(o => o.Id == currentUser.OrganizationId, cancellationToken);
        var dto = new PixReceiverDto(pixKey.Id, pixKey.Label, pixKey.KeyType, pixKey.KeyValue, organization.DefaultPixKeyId == pixKey.Id);
        return TypedResults.Ok(ApiResponse<PixReceiverDto>.Ok(dto));
    }

    private static async Task<Results<Ok<ApiResponse<string>>, BadRequest<ApiResponse<string>>>> DeletePixKeyAsync(
        Guid pixKeyId,
        ICurrentUserContext currentUser,
        NotificaPixDbContext context,
        CancellationToken cancellationToken)
    {
        var pixKey = await context.PixKeys.FirstOrDefaultAsync(k => k.Id == pixKeyId && k.OrganizationId == currentUser.OrganizationId, cancellationToken);
        if (pixKey is null)
        {
            return TypedResults.BadRequest(ApiResponse<string>.Fail("Chave Pix não encontrada."));
        }

        var organization = await context.Organizations.FirstAsync(o => o.Id == currentUser.OrganizationId, cancellationToken);
        if (organization.DefaultPixKeyId == pixKey.Id)
        {
            organization.DefaultPixKeyId = null;
        }

        var qrCodes = await context.PixStaticQrCodes.Where(q => q.PixKeyId == pixKey.Id && q.OrganizationId == currentUser.OrganizationId).ToListAsync(cancellationToken);
        context.PixStaticQrCodes.RemoveRange(qrCodes);
        context.PixKeys.Remove(pixKey);
        await context.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(ApiResponse<string>.Ok("Chave Pix removida."));
    }

    private static async Task<Results<Ok<ApiResponse<string>>, BadRequest<ApiResponse<string>>>> SelectPixKeyAsync(
        SelectPixReceiverRequest request,
        ICurrentUserContext currentUser,
        NotificaPixDbContext context,
        CancellationToken cancellationToken)
    {
        var pixKey = await context.PixKeys.FirstOrDefaultAsync(k => k.Id == request.PixKeyId && k.OrganizationId == currentUser.OrganizationId, cancellationToken);
        if (pixKey is null)
        {
            return TypedResults.BadRequest(ApiResponse<string>.Fail("Chave Pix não encontrada."));
        }

        var organization = await context.Organizations.FirstAsync(o => o.Id == currentUser.OrganizationId, cancellationToken);
        organization.DefaultPixKeyId = pixKey.Id;
        await context.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(ApiResponse<string>.Ok("Chave padrão atualizada."));
    }

    private static async Task<Ok<ApiResponse<IEnumerable<PixStaticQrCodeDto>>>> ListQrCodesAsync(
        ICurrentUserContext currentUser,
        NotificaPixDbContext context,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var qrCodes = await context.PixStaticQrCodes
            .Include(q => q.PixKey)
            .Where(q => q.OrganizationId == currentUser.OrganizationId)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync(cancellationToken);
        var dtos = mapper.Map<IEnumerable<PixStaticQrCodeDto>>(qrCodes);
        return TypedResults.Ok(ApiResponse<IEnumerable<PixStaticQrCodeDto>>.Ok(dtos));
    }

    private static async Task<Results<Ok<ApiResponse<PixStaticQrCodeDto>>, BadRequest<ApiResponse<string>>>> CreateQrCodeAsync(
        CreatePixStaticQrRequest request,
        ICurrentUserContext currentUser,
        NotificaPixDbContext context,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
        {
            return TypedResults.BadRequest(ApiResponse<string>.Fail("Valor inválido."));
        }

        var organization = await context.Organizations.FirstAsync(o => o.Id == currentUser.OrganizationId, cancellationToken);
        var pixKeyId = request.PixKeyId ?? organization.DefaultPixKeyId;
        if (pixKeyId is null)
        {
            return TypedResults.BadRequest(ApiResponse<string>.Fail("Cadastre uma chave Pix e defina como padrão antes de gerar QR codes."));
        }

        var pixKey = await context.PixKeys.FirstOrDefaultAsync(k => k.Id == pixKeyId && k.OrganizationId == currentUser.OrganizationId, cancellationToken);
        if (pixKey is null)
        {
            return TypedResults.BadRequest(ApiResponse<string>.Fail("Chave Pix selecionada não encontrada."));
        }

        var txId = Guid.NewGuid().ToString("N").Substring(0, 25);
        var payload = BuildStaticPayload(pixKey.KeyValue, request.Amount, txId, organization.Name);
        var qrCode = new PixStaticQrCode
        {
            OrganizationId = organization.Id,
            PixKeyId = pixKey.Id,
            Amount = request.Amount,
            Payload = payload,
            TxId = txId
        };

        context.PixStaticQrCodes.Add(qrCode);
        await context.SaveChangesAsync(cancellationToken);

        var dto = mapper.Map<PixStaticQrCodeDto>(qrCode);
        return TypedResults.Ok(ApiResponse<PixStaticQrCodeDto>.Ok(dto));
    }

    private static string BuildStaticPayload(string pixKey, decimal amount, string txId, string organizationName)
    {
        string Segment(string id, string value) => $"{id}{value.Length:D2}{value}";

        var merchantAccount = Segment("00", "BR.GOV.BCB.PIX") + Segment("01", pixKey);
        var merchantName = NormalizeEmvText(organizationName, 25);
        var merchantCity = NormalizeEmvText("SAO PAULO", 15);
        var additionalData = Segment("05", txId);

        var payloadBuilder = new StringBuilder();
        payloadBuilder.Append(Segment("00", "01")); // Payload format indicator
        payloadBuilder.Append(Segment("01", "11")); // Static QR
        payloadBuilder.Append(Segment("26", merchantAccount));
        payloadBuilder.Append(Segment("52", "0000"));
        payloadBuilder.Append(Segment("53", "986"));
        payloadBuilder.Append(Segment("54", amount.ToString("0.00", CultureInfo.InvariantCulture)));
        payloadBuilder.Append(Segment("58", "BR"));
        payloadBuilder.Append(Segment("59", merchantName));
        payloadBuilder.Append(Segment("60", merchantCity));
        payloadBuilder.Append(Segment("62", additionalData));
        payloadBuilder.Append("6304"); // CRC placeholder

        var checksum = CalculateCrc16(payloadBuilder.ToString());
        payloadBuilder.Append(checksum);
        return payloadBuilder.ToString();
    }

    private static string NormalizeEmvText(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "NA";
        }

        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();
        foreach (var ch in normalized)
        {
            if (char.GetUnicodeCategory(ch) == System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                continue;
            }
            if (char.IsLetterOrDigit(ch) || ch == ' ')
            {
                builder.Append(char.ToUpperInvariant(ch));
            }
        }

        var sanitized = builder.ToString().Trim();
        if (sanitized.Length == 0)
        {
            sanitized = "NA";
        }

        return sanitized.Length <= maxLength ? sanitized : sanitized[..maxLength];
    }

    private static string CalculateCrc16(string payload)
    {
        const ushort polynomial = 0x1021;
        ushort crc = 0xFFFF;

        var bytes = Encoding.ASCII.GetBytes(payload);
        foreach (var b in bytes)
        {
            crc ^= (ushort)(b << 8);
            for (int bit = 0; bit < 8; bit++)
            {
                if ((crc & 0x8000) != 0)
                {
                    crc = (ushort)((crc << 1) ^ polynomial);
                }
                else
                {
                    crc <<= 1;
                }
            }
        }

        return crc.ToString("X4");
    }

}
