using System.Text.Json;
using System.Text.Json.Serialization;
using NotificaPix.Core.Domain.Entities;

namespace NotificaPix.Infrastructure.Services.Itau;

public static class ItauPixMapper
{
    public static PixTransaction? MapToTransaction(ItauLancamentoDto lancamento, Guid organizationId, JsonSerializerOptions serializerOptions)
    {
        var pagamento = lancamento.DetalhePagamento;
        if (pagamento is null)
        {
            return null;
        }

        var rawAmount = pagamento.Valor ?? pagamento.DetalheValor?.Valor;
        if (rawAmount is null || rawAmount == 0)
        {
            return null;
        }

        var occurredAt = TryParseDate(pagamento.Data);
        var txId = pagamento.TxId;
        if (string.IsNullOrWhiteSpace(txId))
        {
            txId = pagamento.IdTransferencia ?? pagamento.IdPagamento ?? lancamento.IdLancamento ?? Guid.NewGuid().ToString("N");
        }

        var endToEndId = pagamento.IdTransferencia ?? pagamento.IdPagamento ?? lancamento.IdLancamento ?? txId!;

        var payerName = pagamento.Debitado?.Nome ?? pagamento.Creditado?.Nome ?? "Pagador PIX";
        var payerKey = pagamento.Debitado?.NumeroDocumento ??
                       pagamento.Debitado?.Conta ??
                       pagamento.Creditado?.ChaveEnderecamento ??
                       pagamento.Creditado?.Conta ??
                       "pix";

        var description = !string.IsNullOrWhiteSpace(lancamento.LiteralLancamento)
            ? lancamento.LiteralLancamento!
            : pagamento.TextoRespostaPagador ?? lancamento.TipoPix ?? "Recebimento PIX";

        var rawJson = JsonSerializer.Serialize(lancamento, serializerOptions);

        return new PixTransaction
        {
            OrganizationId = organizationId,
            TxId = txId!,
            EndToEndId = endToEndId,
            Amount = rawAmount.Value,
            OccurredAt = occurredAt,
            PayerName = payerName,
            PayerKey = payerKey,
            Description = description,
            RawJson = rawJson
        };
    }

    private static DateTime TryParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return DateTime.UtcNow;
        }

        if (DateTime.TryParse(value, out var parsed))
        {
            return parsed.ToUniversalTime();
        }

        return DateTime.UtcNow;
    }
}

public sealed record ItauLancamentoDto(
    [property: JsonPropertyName("id_lancamento")] string? IdLancamento,
    [property: JsonPropertyName("literal_lancamento")] string? LiteralLancamento,
    [property: JsonPropertyName("tipo_lancamento")] string? TipoLancamento,
    [property: JsonPropertyName("tipo_operacao")] string? TipoOperacao,
    [property: JsonPropertyName("tipo_pix")] string? TipoPix,
    [property: JsonPropertyName("detalhe_pagamento")] ItauPagamentoDto? DetalhePagamento);

public sealed record ItauPagamentoDto(
    [property: JsonPropertyName("id_pagamento")] string? IdPagamento,
    [property: JsonPropertyName("id_transferencia")] string? IdTransferencia,
    [property: JsonPropertyName("txid")] string? TxId,
    [property: JsonPropertyName("valor")] decimal? Valor,
    [property: JsonPropertyName("detalhe_valor")] ItauPagamentoDetalheValorDto? DetalheValor,
    [property: JsonPropertyName("data")] string? Data,
    [property: JsonPropertyName("texto_resposta_pagador")] string? TextoRespostaPagador,
    [property: JsonPropertyName("debitado")] ItauContaBancariaDto? Debitado,
    [property: JsonPropertyName("creditado")] ItauContaBancariaDto? Creditado);

public sealed record ItauPagamentoDetalheValorDto(
    [property: JsonPropertyName("valor")] decimal? Valor,
    [property: JsonPropertyName("saque")] decimal? Saque,
    [property: JsonPropertyName("troco")] decimal? Troco,
    [property: JsonPropertyName("compra")] decimal? Compra);

public sealed record ItauContaBancariaDto(
    [property: JsonPropertyName("nome")] string? Nome,
    [property: JsonPropertyName("numero_documento")] string? NumeroDocumento,
    [property: JsonPropertyName("conta")] string? Conta,
    [property: JsonPropertyName("agencia")] string? Agencia,
    [property: JsonPropertyName("chave_enderecamento")] string? ChaveEnderecamento);
