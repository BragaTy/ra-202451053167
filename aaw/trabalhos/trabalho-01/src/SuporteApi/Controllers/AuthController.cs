using Microsoft.AspNetCore.Mvc;
using SuporteApi.Dtos;

namespace SuporteApi.Controllers;

[Route("api/v1/auth")]
public class AuthController : ApiControllerBase
{
    [HttpPost("registrar")]
    public IActionResult Registrar(RegistrarRequest request) => NaoImplementado();

    [HttpPost("login")]
    public IActionResult Login(LoginRequest request) => NaoImplementado();
}
