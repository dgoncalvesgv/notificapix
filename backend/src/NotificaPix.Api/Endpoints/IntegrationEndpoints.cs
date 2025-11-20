using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotificaPix.Api.Infrastructure;
using NotificaPix.Core.Domain.Entities;
using NotificaPix.Infrastructure.Persistence;
using NotificaPix.Infrastructure.Services.Itau;

namespace NotificaPix.Api.Endpoints;

public static class IntegrationEndpoints
{
    private static readonly JsonSerializerOptions WebhookSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static IEndpointRouteBuilder MapIntegrationEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/integrations/webhook", ReceiveWebhookAsync).WithTags("Integration");
        app.MapPost("/integrations/itau/webhook", ReceiveItauWebhookAsync).WithTags("Integration");
        return app;
    }

    private static async Task<Ok> ReceiveWebhookAsync(HttpContext context, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("IntegrationWebhook");
        using var reader = new StreamReader(context.Request.Body);
        var payload = await reader.ReadToEndAsync();
        logger.LogInformation("Integration webhook received: {Payload}", payload);
        return TypedResults.Ok();
    }

    private static async Task<IResult> ReceiveItauWebhookAsync(
        HttpContext context,
        NotificaPixDbContext dbContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("ItauWebhook");

        using var reader = new StreamReader(context.Request.Body);
        var payload = await reader.ReadToEndAsync();
        var signature = context.Request.Headers["X-Itau-Signature"].ToString();
        var eventType = context.Request.Headers["X-Itau-Event"].ToString();

        if (string.IsNullOrWhiteSpace(payload))
        {
            logger.LogWarning("Webhook Itaú recebido sem payload.");
            return TypedResults.BadRequest();
        }

        if (!TryParseWebhookPayload(payload, out var webhookPayload))
        {
            logger.LogWarning("Webhook Itaú inválido ou fora do formato esperado.");
            return TypedResults.BadRequest();
        }

        if (string.IsNullOrWhiteSpace(webhookPayload.AccountIdentifier))
        {
            logger.LogWarning("Webhook Itaú não possui id_conta para localizar integração.");
            return TypedResults.Ok();
        }

        var integration = await dbContext.BankApiIntegrations
            .FirstOrDefaultAsync(
                x => x.Bank == "Itau" && x.AccountIdentifier == webhookPayload.AccountIdentifier,
                cancellationToken);

        if (integration is null)
        {
            logger.LogWarning("Webhook Itaú recebido para conta {Account} sem integração correspondente.", webhookPayload.AccountIdentifier);
            return TypedResults.Ok();
        }

        if (!ValidateSignature(signature, payload, integration.ApiKey))
        {
            logger.LogWarning("Assinatura do webhook Itaú inválida para integração {IntegrationId}.", integration.Id);
            return TypedResults.BadRequest();
        }

        var eventId = webhookPayload.EventId ?? signature ?? Guid.NewGuid().ToString("N");
        var eventExists = await dbContext.BankWebhookEvents.AnyAsync(
            x => x.BankApiIntegrationId == integration.Id && x.EventId == eventId,
            cancellationToken);
        if (eventExists)
        {
            logger.LogInformation("Evento Itaú {EventId} já processado anteriormente.", eventId);
            return TypedResults.Ok();
        }

        var transactions = webhookPayload.Lancamentos
            .Where(l => string.Equals(l.TipoOperacao, "credito", StringComparison.OrdinalIgnoreCase))
            .Select(l => ItauPixMapper.MapToTransaction(l, integration.OrganizationId, WebhookSerializerOptions))
            .Where(t => t is not null)
            .Select(t => t!)
            .ToList();

        var upsertResult = await PixTransactionUpserter.UpsertAsync(dbContext, integration.OrganizationId, transactions, cancellationToken);

        var webhookEvent = new BankWebhookEvent
        {
            BankApiIntegrationId = integration.Id,
            OrganizationId = integration.OrganizationId,
            Bank = integration.Bank,
            EventId = eventId,
            EventType = eventType ?? string.Empty,
            Signature = signature ?? string.Empty,
            Payload = payload,
            ReceivedAt = DateTime.UtcNow
        };

        dbContext.BankWebhookEvents.Add(webhookEvent);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Webhook Itaú processado. Novos: {Inserted}, Atualizados: {Updated}", upsertResult.Inserted, upsertResult.Updated);

        return TypedResults.Ok();
    }

    private static bool TryParseWebhookPayload(string payload, out ItauWebhookPayload result)
    {
        result = default!;
        try
        {
            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;
            var accountId = FindStringValue(root, "id_conta", "idConta", "account_identifier", "accountIdentifier");
            var eventId = FindStringValue(root, "id_evento", "eventId", "event_id");
            var lancamentos = ExtractLancamentos(root);

            result = new ItauWebhookPayload(accountId, eventId, lancamentos);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static IReadOnlyCollection<ItauLancamentoDto> ExtractLancamentos(JsonElement root)
    {
        var lancamentos = new List<ItauLancamentoDto>();
        var candidates = new[] { "lancamentos", "lancamentos_pix", "lancamentosPix" };
        foreach (var candidate in candidates)
        {
            if (TryAddLancamentos(root, candidate, lancamentos))
            {
                return lancamentos;
            }
        }

        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("detalhe_pagamento", out _))
        {
            TryAddLancamentoElement(root, lancamentos);
        }

        return lancamentos;
    }

    private static bool TryAddLancamentos(JsonElement root, string propertyName, ICollection<ItauLancamentoDto> target)
    {
        if (!TryGetProperty(root, propertyName, out var element))
        {
            return false;
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                TryAddLancamentoElement(item, target);
            }
        }
        else if (element.ValueKind == JsonValueKind.Object)
        {
            TryAddLancamentoElement(element, target);
        }

        return target.Count > 0;
    }

    private static void TryAddLancamentoElement(JsonElement element, ICollection<ItauLancamentoDto> target)
    {
        try
        {
            var model = JsonSerializer.Deserialize<ItauLancamentoDto>(element.GetRawText(), WebhookSerializerOptions);
            if (model is not null)
            {
                target.Add(model);
            }
        }
        catch (JsonException)
        {
        }
    }

    private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static string? FindStringValue(JsonElement element, params string[] names)
    {
        var queue = new Queue<JsonElement>();
        queue.Enqueue(element);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in current.EnumerateObject())
                {
                    if (names.Any(n => property.Name.Equals(n, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (property.Value.ValueKind == JsonValueKind.String)
                        {
                            return property.Value.GetString();
                        }

                        if (property.Value.ValueKind == JsonValueKind.Number)
                        {
                            return property.Value.GetRawText();
                        }
                    }

                    if (property.Value.ValueKind == JsonValueKind.Object || property.Value.ValueKind == JsonValueKind.Array)
                    {
                        queue.Enqueue(property.Value);
                    }
                }
            }
            else if (current.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in current.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.Object || item.ValueKind == JsonValueKind.Array)
                    {
                        queue.Enqueue(item);
                    }
                }
            }
        }

        return null;
    }

    private static bool ValidateSignature(string? providedSignature, string payload, string? secret)
    {
        if (string.IsNullOrWhiteSpace(providedSignature) || string.IsNullOrWhiteSpace(secret))
        {
            return false;
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var expectedBase64 = Convert.ToBase64String(hash);
        var expectedHex = Convert.ToHexString(hash).ToLowerInvariant();

        providedSignature = providedSignature.Trim();
        return SlowEquals(providedSignature, expectedBase64) || SlowEquals(providedSignature.ToLowerInvariant(), expectedHex);
    }

    private static bool SlowEquals(string left, string right)
    {
        if (left.Length != right.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(left), Encoding.UTF8.GetBytes(right));
    }

    private sealed record ItauWebhookPayload(
        string? AccountIdentifier,
        string? EventId,
        IReadOnlyCollection<ItauLancamentoDto> Lancamentos);
}
