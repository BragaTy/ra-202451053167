using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SuporteApi.Models;

namespace SuporteApi.Auth;

public class TokenService(JwtSettings settings)
{
    // Gera o JWT assinado com HS256. O perfil vai na claim "role" e é
    // conferido no servidor pelo [Authorize(Roles = ...)].
    public (string Token, DateTime ExpiraEm) Gerar(Usuario usuario)
    {
        var expiraEm = DateTime.UtcNow.AddMinutes(settings.ExpiresInMinutes);
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim("name", usuario.Nome),
            new Claim("role", usuario.Perfil)
        };

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            expires: expiraEm,
            signingCredentials: new SigningCredentials(chave, SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraEm);
    }
}
