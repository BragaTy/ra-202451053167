namespace SuporteApi.Auth;

// Seção "Jwt" da configuração. Issuer, Audience e ExpiresInMinutes ficam no
// appsettings.json; a Key é segredo e vem de variável de ambiente (Jwt__Key)
// ou do User Secrets.
public class JwtSettings
{
    public string Key { get; set; } = "";
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public int ExpiresInMinutes { get; set; } = 60;
}
