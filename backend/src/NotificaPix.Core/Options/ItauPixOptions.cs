namespace NotificaPix.Core.Options;

public class ItauPixOptions
{
    public string SandboxBaseUrl { get; set; } = "https://secure.api.itau/pix_recebimentos_conciliacoes/v2";
    public string ProductionBaseUrl { get; set; } = "https://secure.api.itau/pix_recebimentos_conciliacoes/v2";
    public string SandboxOAuthUrl { get; set; } = "https://oauthd.itau/identity/connect/token";
    public string ProductionOAuthUrl { get; set; } = "https://oauth.itau/identity/connect/token";
}
