using System.Collections.Generic;
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
using NotificaPix.Core.Domain.Entities;
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
        group.MapGet("/api/itau", GetItauIntegrationAsync).RequireAuthorization("OrgAdmin");
        group.MapPost("/api/itau", SaveItauIntegrationAsync).RequireAuthorization("OrgAdmin");
        group.MapPost("/api/itau/test", TestItauIntegrationAsync).RequireAuthorization("OrgAdmin");
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

    [Authorize(Policy = "OrgAdmin")]
    private static async Task<Ok<ApiResponse<BankIntegrationStatusDto>>> GetItauIntegrationAsync(
        ICurrentUserContext currentUser,
        NotificaPixDbContext context,
        CancellationToken cancellationToken)
    {
        var integration = await context.BankApiIntegrations.FirstOrDefaultAsync(x =>
            x.OrganizationId == currentUser.OrganizationId && x.Bank == "Itau", cancellationToken);

        var dto = BuildIntegrationDto(integration);

        return TypedResults.Ok(ApiResponse<BankIntegrationStatusDto>.Ok(dto));
    }

    [Authorize(Policy = "OrgAdmin")]
    private static async Task<Ok<ApiResponse<BankIntegrationStatusDto>>> SaveItauIntegrationAsync(
        ItauIntegrationRequest request,
        ICurrentUserContext currentUser,
        NotificaPixDbContext context,
        CancellationToken cancellationToken)
    {
        var integration = await context.BankApiIntegrations.FirstOrDefaultAsync(x =>
            x.OrganizationId == currentUser.OrganizationId && x.Bank == "Itau", cancellationToken);

        if (integration is null)
        {
            integration = new BankApiIntegration
            {
                OrganizationId = currentUser.OrganizationId,
                Bank = "Itau"
            };
            await context.BankApiIntegrations.AddAsync(integration, cancellationToken);
        }

        var hasIncomingCertificate = !string.IsNullOrWhiteSpace(request.CertificateBase64);
        var hasExistingCertificate = !string.IsNullOrWhiteSpace(integration.CertificateBase64);
        if (string.IsNullOrWhiteSpace(request.ServiceUrl))
        {
            throw new ArgumentException("Service URL is required");
        }
        if (string.IsNullOrWhiteSpace(request.ApiKey))
        {
            throw new ArgumentException("API Key é obrigatório");
        }
        if (string.IsNullOrWhiteSpace(request.AccountIdentifier))
        {
            throw new ArgumentException("ID da conta é obrigatório");
        }

        var useProduction = request.ProductionEnabled;

        if (useProduction)
        {
            if (string.IsNullOrWhiteSpace(request.ProductionClientId) || string.IsNullOrWhiteSpace(request.ProductionClientSecret))
            {
                throw new ArgumentException("Dados de produção são obrigatórios quando a produção está habilitada.");
            }

            if (!hasIncomingCertificate && !hasExistingCertificate)
            {
                throw new ArgumentException("Envie o certificado .pfx para usar produção.");
            }
        }

        integration.SandboxClientId = request.SandboxClientId;
        integration.SandboxClientSecret = request.SandboxClientSecret;
        integration.ProductionClientId = request.ProductionClientId;
        integration.ProductionClientSecret = request.ProductionClientSecret;
        integration.CertificatePassword = request.CertificatePassword;
        integration.ServiceUrl = request.ServiceUrl;
        integration.ApiKey = request.ApiKey;
        integration.AccountIdentifier = request.AccountIdentifier;
        integration.ProductionEnabled = useProduction;

        if (hasIncomingCertificate)
        {
            integration.CertificateFileName = request.CertificateFileName;
            integration.CertificateBase64 = request.CertificateBase64;
        }

        integration.IsTested = false;
        integration.LastTestedAt = null;
        integration.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        var dto = BuildIntegrationDto(integration);
        return TypedResults.Ok(ApiResponse<BankIntegrationStatusDto>.Ok(dto));
    }

    [Authorize(Policy = "OrgAdmin")]
    private static async Task<Results<
        Ok<ApiResponse<BankIntegrationStatusDto>>,
        NotFound<ApiResponse<string>>,
        BadRequest<ApiResponse<string>>>> TestItauIntegrationAsync(
            BankIntegrationTestRequest request,
            ICurrentUserContext currentUser,
            NotificaPixDbContext context,
            CancellationToken cancellationToken)
    {
        var integration = await context.BankApiIntegrations.FirstOrDefaultAsync(x =>
            x.OrganizationId == currentUser.OrganizationId && x.Bank == "Itau", cancellationToken);

        if (integration is null)
        {
            return TypedResults.NotFound(ApiResponse<string>.Fail("Integração não encontrada"));
        }

        var useProduction = request.UseProduction ?? integration.ProductionEnabled;
        var missingFields = new List<string>();

        if (useProduction)
        {
            if (string.IsNullOrWhiteSpace(integration.ProductionClientId))
            {
                missingFields.Add("ProductionClientId");
            }
            if (string.IsNullOrWhiteSpace(integration.ProductionClientSecret))
            {
                missingFields.Add("ProductionClientSecret");
            }
            if (string.IsNullOrWhiteSpace(integration.CertificateBase64))
            {
                missingFields.Add("Certificate");
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(integration.SandboxClientId))
            {
                missingFields.Add("SandboxClientId");
            }
            if (string.IsNullOrWhiteSpace(integration.SandboxClientSecret))
            {
                missingFields.Add("SandboxClientSecret");
            }
        }

        if (missingFields.Count > 0)
        {
            var errorMessage = $"Preencha os campos obrigatórios ({string.Join(", ", missingFields)}) antes de testar.";
            return TypedResults.BadRequest(ApiResponse<string>.Fail(errorMessage));
        }

        // Simula chamada à API externa do banco.
        await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);

        integration.ProductionEnabled = useProduction;
        integration.IsTested = true;
        integration.LastTestedAt = DateTime.UtcNow;
        integration.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        var dto = BuildIntegrationDto(integration);
        return TypedResults.Ok(ApiResponse<BankIntegrationStatusDto>.Ok(dto));
    }

    private static BankIntegrationStatusDto BuildIntegrationDto(BankApiIntegration? integration)
    {
        if (integration is null)
        {
            return new BankIntegrationStatusDto(false, null, null, null, null, false, null, null, null, null, null, null, null, null, null, null, false);
        }

        var hasCertificate = !string.IsNullOrWhiteSpace(integration.CertificateBase64);

        return new BankIntegrationStatusDto(
            integration.IsTested,
            integration.UpdatedAt,
            integration.Id,
            integration.Bank,
            integration.CreatedAt,
            integration.ProductionEnabled,
            integration.LastTestedAt,
            integration.ServiceUrl,
            integration.ApiKey,
            integration.AccountIdentifier,
            integration.SandboxClientId,
            integration.SandboxClientSecret,
            integration.ProductionClientId,
            integration.ProductionClientSecret,
            integration.CertificatePassword,
            integration.CertificateFileName,
            hasCertificate);
    }
}
