using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificaPix.Core.Abstractions.Services;
using NotificaPix.Core.Contracts.Common;
using NotificaPix.Core.Domain.Entities;
using NotificaPix.Core.Options;
using NotificaPix.Infrastructure.Services.Itau;

namespace NotificaPix.Infrastructure.Services;

public class ItauPixService : IItauPixService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<ItauPixOptions> _options;
    private readonly ILogger<ItauPixService> _logger;
    private readonly JsonSerializerOptions _serializerOptions;

    public ItauPixService(
        IHttpClientFactory httpClientFactory,
        IOptions<ItauPixOptions> options,
        ILogger<ItauPixService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _logger = logger;
        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    private const string TestDocumento = "65481904594";
    private const string TestQrcodeEmv =
        "00020101021226880014BR.GOV.BCB.PIX2565pix.example.com/qr/v2/25165e3c10614890b5a438f251d954a65204000053039865802BR5925PMDTESTE6009SAOPAULO62070503***6304E2B2";
    private const string TestE2e = "E08561701202208271525CGPXCDAPPNO";

    public async Task<IReadOnlyCollection<PixTransaction>> FetchTransactionsAsync(
        BankApiIntegration integration,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        if (integration is null)
        {
            throw new ArgumentNullException(nameof(integration));
        }

        if (string.IsNullOrWhiteSpace(integration.AccountIdentifier))
        {
            _logger.LogWarning("Integração {IntegrationId} sem id_conta configurado.", integration.Id);
            return Array.Empty<PixTransaction>();
        }

        if (string.IsNullOrWhiteSpace(integration.ApiKey))
        {
            _logger.LogWarning("Integração {IntegrationId} sem API Key configurada.", integration.Id);
            return Array.Empty<PixTransaction>();
        }

        var normalizedFrom = from.ToUniversalTime();
        var normalizedTo = to.ToUniversalTime();
        if (normalizedFrom >= normalizedTo)
        {
            normalizedFrom = normalizedTo.AddHours(-1);
        }

        var useProduction = integration.ProductionEnabled;
        var tokenResult = await RequestAccessTokenAsync(integration, useProduction, cancellationToken);
        if (tokenResult.AccessToken is null)
        {
            return Array.Empty<PixTransaction>();
        }

        try
        {
            using var client = CreateHttpClient(integration, useProduction);
            var requestUrl = BuildLancamentosUrl(integration, useProduction, normalizedFrom, normalizedTo);

            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.AccessToken);
            requestMessage.Headers.TryAddWithoutValidation("x-itau-apikey", integration.ApiKey);
            requestMessage.Headers.TryAddWithoutValidation("x-itau-correlationID", Guid.NewGuid().ToString());

            var response = await client.SendAsync(requestMessage, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Itaú retornou {Status} na consulta de lançamentos: {Body}", response.StatusCode, errorBody);
                return Array.Empty<PixTransaction>();
            }

            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var lancamentos = await JsonSerializer.DeserializeAsync<List<ItauLancamentoDto>>(contentStream, _serializerOptions, cancellationToken);
            if (lancamentos is null || lancamentos.Count == 0)
            {
                return Array.Empty<PixTransaction>();
            }

            var transactions = lancamentos
                .Where(l => string.Equals(l.TipoOperacao, "credito", StringComparison.OrdinalIgnoreCase))
                .Select(l => ItauPixMapper.MapToTransaction(l, integration.OrganizationId, _serializerOptions))
                .Where(t => t is not null)
                .Select(t => t!)
                .ToList();

            return transactions;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consumir lançamentos PIX do Itaú para integração {IntegrationId}", integration.Id);
            return Array.Empty<PixTransaction>();
        }
    }

    public async Task<ItauPixTestResult> TestCredentialsAsync(
        BankApiIntegration integration,
        bool useProduction,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(integration.ApiKey))
        {
            return new ItauPixTestResult(false, "Configure a API Key antes de testar.");
        }

        var tokenResult = await RequestAccessTokenAsync(integration, useProduction, cancellationToken);
        if (tokenResult.AccessToken is null)
        {
            var message = tokenResult.ErrorMessage ?? "Falha ao obter token OAuth do Itaú.";
            return new ItauPixTestResult(false, message);
        }

        try
        {
            using var client = CreateHttpClient(integration, useProduction);
            var baseUrl = ResolveBaseUrl(integration, useProduction).TrimEnd('/');
            var url = $"{baseUrl}/leituras_qrcodes_pix";

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.AccessToken);
            request.Headers.TryAddWithoutValidation("x-itau-apikey", integration.ApiKey);
            request.Headers.TryAddWithoutValidation("x-itau-correlationID", Guid.NewGuid().ToString());

            var payload = BuildTestLeituraPayload();
            var payloadJson = JsonSerializer.Serialize(payload, _serializerOptions);
            request.Content = new StringContent(payloadJson, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode ||
                response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.UnprocessableEntity)
            {
                return new ItauPixTestResult(true);
            }

            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            var message = $"Falha ao testar integração ({(int)response.StatusCode}). {errorBody}";
            return new ItauPixTestResult(false, message);
        }
        catch (InvalidOperationException ex)
        {
            return new ItauPixTestResult(false, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao testar credenciais Itaú para integração {IntegrationId}", integration.Id);
            return new ItauPixTestResult(false, "Erro inesperado ao testar integração com o Itaú.");
        }
    }

    private async Task<OAuthTokenResult> RequestAccessTokenAsync(
        BankApiIntegration integration,
        bool useProduction,
        CancellationToken cancellationToken)
    {
        var clientId = useProduction ? integration.ProductionClientId : integration.SandboxClientId;
        var clientSecret = useProduction ? integration.ProductionClientSecret : integration.SandboxClientSecret;

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            var message = useProduction
                ? "ClientId/ClientSecret de produção não configurados."
                : "ClientId/ClientSecret de sandbox não configurados.";
            _logger.LogWarning("Integração {IntegrationId} sem clientId/clientSecret configurados.", integration.Id);
            return new OAuthTokenResult(null, message);
        }

        var tokenUrl = useProduction ? _options.Value.ProductionOAuthUrl : _options.Value.SandboxOAuthUrl;

        var client = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["scope"] = "pix.recebimentos"
            })
        };

        var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            var message = $"Falha ao obter token OAuth ({(int)response.StatusCode}). {errorBody}";
            _logger.LogWarning("Falha ao obter token OAuth do Itaú ({Status}): {Body}", response.StatusCode, errorBody);
            return new OAuthTokenResult(null, message);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var tokenResponse = await JsonSerializer.DeserializeAsync<OAuthTokenResponse>(stream, _serializerOptions, cancellationToken);
        if (tokenResponse?.AccessToken is null)
        {
            const string message = "Resposta do OAuth Itaú não continha access_token.";
            _logger.LogWarning(message);
            return new OAuthTokenResult(null, message);
        }

        return new OAuthTokenResult(tokenResponse.AccessToken, null);
    }

    private HttpClient CreateHttpClient(BankApiIntegration integration, bool useProduction)
    {
        var baseUrl = ResolveBaseUrl(integration, useProduction);
        var handler = new HttpClientHandler();

        if (useProduction)
        {
            if (string.IsNullOrWhiteSpace(integration.CertificateBase64) || string.IsNullOrWhiteSpace(integration.CertificatePassword))
            {
                throw new InvalidOperationException("Certificado PFX obrigatório para chamadas em produção.");
            }

            try
            {
                var certificateBytes = Convert.FromBase64String(integration.CertificateBase64);
                var certificate = new X509Certificate2(
                    certificateBytes,
                    integration.CertificatePassword,
                    X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
                handler.ClientCertificates.Add(certificate);
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Não foi possível carregar o certificado do Itaú.", ex);
            }
        }

        var client = new HttpClient(handler, disposeHandler: true)
        {
            Timeout = TimeSpan.FromSeconds(40),
            BaseAddress = new Uri(baseUrl, UriKind.Absolute)
        };

        return client;
    }

    private string BuildLancamentosUrl(BankApiIntegration integration, bool useProduction, DateTime from, DateTime to)
    {
        var path = "lancamentos_pix";
        var baseUrl = ResolveBaseUrl(integration, useProduction).TrimEnd('/');
        var builder = new StringBuilder();
        builder.Append(baseUrl).Append('/').Append(path);

        var query = new Dictionary<string, string?>
        {
            ["id_conta"] = integration.AccountIdentifier,
            ["data_criacao_lancamento"] = $"{FormatDate(from)},{FormatDate(to)}"
        };

        var separator = '?';
        foreach (var kvp in query.Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value)))
        {
            builder.Append(separator)
                .Append(Uri.EscapeDataString(kvp.Key))
                .Append('=')
                .Append(Uri.EscapeDataString(kvp.Value!));
            separator = '&';
        }

        return builder.ToString();
    }

    private string ResolveBaseUrl(BankApiIntegration integration, bool useProduction)
    {
        if (!string.IsNullOrWhiteSpace(integration.ServiceUrl))
        {
            return integration.ServiceUrl;
        }

        return useProduction
            ? _options.Value.ProductionBaseUrl
            : _options.Value.SandboxBaseUrl;
    }

    private static string FormatDate(DateTime value) =>
        value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);

    private static ItauLeituraQrcodeRequest BuildTestLeituraPayload() =>
        new(TestDocumento, TestQrcodeEmv, TestE2e);

    private sealed record OAuthTokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("token_type")] string TokenType,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);

    private sealed record OAuthTokenResult(string? AccessToken, string? ErrorMessage);

    private sealed record ItauLeituraQrcodeRequest(
        [property: JsonPropertyName("numero_documento_pagador")] string NumeroDocumentoPagador,
        [property: JsonPropertyName("qrcode_emv")] string QrcodeEmv,
        [property: JsonPropertyName("e2e")] string E2e);
}
