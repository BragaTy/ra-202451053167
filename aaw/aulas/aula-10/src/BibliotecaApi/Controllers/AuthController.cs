using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace BibliotecaApi.Controllers;

// PRÉ-PRONTO — NÃO é o foco desta aula (JWT foi tema da Aula 07).
// Existe para que a API tenha endpoints protegidos e o botão "Authorize"
// do Swagger tenha o que autorizar. Credenciais didáticas fixas abaixo.
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Autenticação")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config) => _config = config;

    /// <summary>Credenciais necessárias para obter um token JWT.</summary>
    /// <param name="Usuario">Nome de usuário.</param>
    /// <param name="Senha">Senha da conta.</param>
    public record CredenciaisRequest(string Usuario, string Senha);

    /// <summary>Token JWT e instante em que ele expira.</summary>
    /// <param name="Token">Token JWT para autenticar chamadas protegidas.</param>
    /// <param name="ExpiraEm">Data e hora de expiração do token.</param>
    public record TokenResponse(string Token, DateTime ExpiraEm);

    /// <summary>Autentica o usuário e gera um token JWT.</summary>
    /// <remarks>Credenciais didáticas: bibliotecario / newton123.</remarks>
    /// <param name="credenciais">Usuário e senha.</param>
    /// <response code="200">Token gerado com sucesso.</response>
    /// <response code="401">Usuário ou senha inválidos.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] CredenciaisRequest credenciais)
    {
        // Usuário didático fixo: bibliotecario / newton123
        if (credenciais.Usuario != "bibliotecario" || credenciais.Senha != "newton123")
            return Unauthorized(new { mensagem = "Usuário ou senha inválidos." });

        var chave = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Chave"]!));
        var expiraEm = DateTime.UtcNow.AddHours(2);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Emissor"],
            audience: _config["Jwt:Audiencia"],
            claims: new[]
            {
                new Claim(ClaimTypes.Name, credenciais.Usuario),
                new Claim(ClaimTypes.Role, "Bibliotecario"),
            },
            expires: expiraEm,
            signingCredentials: new SigningCredentials(chave, SecurityAlgorithms.HmacSha256));

        return Ok(new TokenResponse(new JwtSecurityTokenHandler().WriteToken(token), expiraEm));
    }
}
