using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace SuporteApi.Auth;

public static class AuthSetup
{
    // Configura a validação do JWT em todas as requisições.
    public static IServiceCollection AddAutenticacaoJwt(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();

        // HS256 exige chave de pelo menos 256 bits (32 bytes).
        if (Encoding.UTF8.GetByteCount(settings.Key) < 32)
            throw new InvalidOperationException("Defina a variável de ambiente Jwt__Key com pelo menos 32 caracteres.");

        services.AddSingleton(settings);
        services.AddSingleton<TokenService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                // Mantém os nomes curtos das claims ("sub", "role") como estão no token.
                o.MapInboundClaims = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = settings.Audience,
                    ValidateLifetime = true,             // token vencido = 401
                    ClockSkew = TimeSpan.FromSeconds(30),
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)),
                    RoleClaimType = "role",
                    NameClaimType = "name"
                };
            });

        services.AddAuthorization();
        return services;
    }
}
