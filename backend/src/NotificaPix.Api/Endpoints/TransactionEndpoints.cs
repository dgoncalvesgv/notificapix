using System.Collections.Generic;
using System.Net.Http.Json;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotificaPix.Core.Abstractions.Security;
using NotificaPix.Core.Contracts.Common;
using NotificaPix.Core.Contracts.Requests;
using NotificaPix.Core.Contracts.Responses;
using NotificaPix.Core.Domain.Entities;
using NotificaPix.Infrastructure.Persistence;

namespace NotificaPix.Api.Endpoints;

public static class TransactionEndpoints
{
    public static IEndpointRouteBuilder MapTransactionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/transactions").WithTags("Transactions").RequireAuthorization();
        group.MapPost("/list", ListAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);
        group.MapPost("/sync-banks", SyncBankIntegrationsAsync).RequireAuthorization("OrgAdmin");
        return app;
    }

    [Authorize]
    private static async Task<Ok<ApiResponse<PagedResult<PixTransactionDto>>>> ListAsync(
        ListTransactionsRequest request,
        ICurrentUserContext currentUser,
        NotificaPixDbContext context,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var query = context.PixTransactions.Where(t => t.OrganizationId == currentUser.OrganizationId);
        if (request.From.HasValue)
        {
            var fromDate = request.From.Value.Date;
            query = query.Where(t => t.OccurredAt >= fromDate);
        }
        if (request.To.HasValue)
        {
            var toDateExclusive = request.To.Value.Date.AddDays(1);
            query = query.Where(t => t.OccurredAt < toDateExclusive);
        }
        if (request.MinAmount.HasValue)
        {
            query = query.Where(t => t.Amount >= request.MinAmount);
        }
        if (request.MaxAmount.HasValue)
        {
            query = query.Where(t => t.Amount <= request.MaxAmount);
        }
        if (!string.IsNullOrWhiteSpace(request.TxId))
        {
            query = query.Where(t => t.TxId.Contains(request.TxId));
        }
        if (!string.IsNullOrWhiteSpace(request.PayerKey))
        {
            query = query.Where(t => t.PayerKey.Contains(request.PayerKey));
        }

        var total = await query.LongCountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(t => t.OccurredAt)
            .Skip((request.PageNormalized - 1) * request.PageSizeNormalized)
            .Take(request.PageSizeNormalized)
            .ToListAsync(cancellationToken);

        var dto = mapper.Map<IEnumerable<PixTransactionDto>>(items);
        var result = new PagedResult<PixTransactionDto>(dto.ToList(), request.PageNormalized, request.PageSizeNormalized, total);
        return TypedResults.Ok(ApiResponse<PagedResult<PixTransactionDto>>.Ok(result));
    }

    [Authorize]
    private static async Task<Results<Ok<ApiResponse<PixTransactionDto>>, NotFound<ApiResponse<PixTransactionDto>>>> GetByIdAsync(
        Guid id,
        ICurrentUserContext currentUser,
        NotificaPixDbContext context,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var transaction = await context.PixTransactions.FirstOrDefaultAsync(t => t.Id == id && t.OrganizationId == currentUser.OrganizationId, cancellationToken);
        if (transaction is null)
        {
            return TypedResults.NotFound(ApiResponse<PixTransactionDto>.Fail("Transaction not found"));
        }

        var dto = mapper.Map<PixTransactionDto>(transaction);
        return TypedResults.Ok(ApiResponse<PixTransactionDto>.Ok(dto));
    }

    [Authorize(Policy = "OrgAdmin")]
    private static async Task<Ok<ApiResponse<BankSyncResultDto>>> SyncBankIntegrationsAsync(
        ICurrentUserContext currentUser,
        NotificaPixDbContext context,
        ILoggerFactory loggerFactory,
        IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("BankManualSync");
        var concludedIntegrations = await context.BankApiIntegrations
            .Where(i => i.OrganizationId == currentUser.OrganizationId && i.IsTested)
            .ToListAsync(cancellationToken);

        if (concludedIntegrations.Count == 0)
        {
            var emptyResult = new BankSyncResultDto(0, 0, "Nenhuma integração concluída para sincronizar.");
            return TypedResults.Ok(ApiResponse<BankSyncResultDto>.Ok(emptyResult));
        }

        var httpClient = httpClientFactory.CreateClient("bank-sync");
        var importedTransactions = 0;
        var processedIntegrations = 0;
        var messages = new List<string>();

        foreach (var integration in concludedIntegrations)
        {
            if (string.IsNullOrWhiteSpace(integration.ServiceUrl))
            {
                logger.LogWarning("Integração {IntegrationId} sem ServiceUrl configurada.", integration.Id);
                messages.Add($"{integration.Bank}: URL do serviço não configurada.");
                continue;
            }

            var executionRequest = BuildExecutionRequest(integration);
            if (executionRequest is null)
            {
                logger.LogWarning(
                    "Integração {IntegrationId} não possui credenciais suficientes para sincronizar.",
                    integration.Id);
                messages.Add($"{integration.Bank}: credenciais incompletas.");
                continue;
            }

            try
            {
                logger.LogInformation(
                    "Disparando sincronização manual para {Bank} (Org {OrgId}) via {Url} ({Environment})",
                    integration.Bank,
                    currentUser.OrganizationId,
                    integration.ServiceUrl,
                    executionRequest.Environment);

                var response = await httpClient.PostAsJsonAsync(
                    integration.ServiceUrl,
                    executionRequest,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    logger.LogWarning("Falha ao sincronizar {Bank}: {Status} {Body}", integration.Bank, response.StatusCode, errorBody);
                    messages.Add($"{integration.Bank}: falha ({(int)response.StatusCode}).");
                    continue;
                }

                var remoteResult = await response.Content.ReadFromJsonAsync<BankSyncResultDto>(cancellationToken: cancellationToken);
                processedIntegrations++;

                if (remoteResult is not null)
                {
                    importedTransactions += remoteResult.TransactionsImported;
                    messages.Add($"{integration.Bank}: {remoteResult.Message}");
                }
                else
                {
                    messages.Add($"{integration.Bank}: sincronização disparada.");
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao sincronizar a integração {IntegrationId}", integration.Id);
                messages.Add($"{integration.Bank}: erro inesperado.");
            }
        }

        var finalMessage = messages.Count == 0
            ? "Nenhuma integração apta para sincronizar."
            : string.Join(" ", messages);

        var result = new BankSyncResultDto(
            processedIntegrations,
            importedTransactions,
            finalMessage);

        return TypedResults.Ok(ApiResponse<BankSyncResultDto>.Ok(result));
    }

    private static IntegrationExecutionRequest? BuildExecutionRequest(BankApiIntegration integration)
    {
        var preferProduction = integration.ProductionEnabled;

        if (preferProduction)
        {
            var productionRequest = TryBuildExecutionRequest(integration, IntegrationEnvironment.Production);
            if (productionRequest is not null)
            {
                return productionRequest;
            }
        }

        return TryBuildExecutionRequest(integration, IntegrationEnvironment.Sandbox);
    }

    private static IntegrationExecutionRequest? TryBuildExecutionRequest(BankApiIntegration integration, IntegrationEnvironment environment)
    {
        var clientId = environment == IntegrationEnvironment.Production
            ? integration.ProductionClientId
            : integration.SandboxClientId;

        var clientSecret = environment == IntegrationEnvironment.Production
            ? integration.ProductionClientSecret
            : integration.SandboxClientSecret;

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(integration.ApiKey) || string.IsNullOrWhiteSpace(integration.AccountIdentifier))
        {
            return null;
        }

        string? certificateBase64 = null;
        string? certificatePassword = null;
        string? certificateFileName = null;

        if (environment == IntegrationEnvironment.Production)
        {
            certificateBase64 = integration.CertificateBase64;
            certificatePassword = integration.CertificatePassword;
            certificateFileName = integration.CertificateFileName;

            if (string.IsNullOrWhiteSpace(certificateBase64) || string.IsNullOrWhiteSpace(certificatePassword))
            {
                return null;
            }
        }

        return new IntegrationExecutionRequest(
            integration.Id,
            integration.OrganizationId,
            integration.Bank,
            environment.ToString(),
            environment == IntegrationEnvironment.Production,
            integration.AccountIdentifier!,
            integration.ApiKey!,
            clientId!,
            clientSecret!,
            certificateBase64,
            certificatePassword,
            certificateFileName);
    }

    private enum IntegrationEnvironment
    {
        Sandbox,
        Production
    }

    private sealed record IntegrationExecutionRequest(
        Guid IntegrationId,
        Guid OrganizationId,
        string Bank,
        string Environment,
        bool UseProduction,
        string AccountIdentifier,
        string ApiKey,
        string ClientId,
        string ClientSecret,
        string? CertificateBase64,
        string? CertificatePassword,
        string? CertificateFileName);
}
