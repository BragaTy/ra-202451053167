using Microsoft.AspNetCore.Mvc;
using SuporteApi.Dtos;
using SuporteApi.Services;

namespace SuporteApi.Controllers;

// Rotas públicas: não exigem token.
[Route("api/v1/auth")]
public class AuthController(AuthService service) : ApiControllerBase
{
    /// <summary>Registra um novo usuário com perfil "usuario".</summary>
    [HttpPost("registrar")]
    [ProducesResponseType<UsuarioResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registrar(RegistrarRequest request) =>
        StatusCode(StatusCodes.Status201Created, await service.RegistrarAsync(request));

    /// <summary>Autentica e devolve o token JWT (use no botão Authorize).</summary>
    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request) =>
        Ok(await service.LoginAsync(request));
}
