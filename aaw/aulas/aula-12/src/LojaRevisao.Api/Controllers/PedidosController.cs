using System.Security.Claims;
using LojaRevisao.Api.Models;
using LojaRevisao.Api.Modulos;
using LojaRevisao.Api.Modulos.Pedidos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LojaRevisao.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/pedidos")]
public class PedidosController : ControllerBase
{
    private readonly PedidoService _service;

    public PedidosController(PedidoService service) => _service = service;

    private int UsuarioLogadoId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // FALHA 09 corrigida (parte B, ABAC): o dono do pedido ou o Admin podem acessar.
    private bool PodeAcessar(Pedido pedido) =>
        pedido.UsuarioId == UsuarioLogadoId || User.IsInRole("Admin");

    /// <summary>Lista os pedidos do usuário logado.</summary>
    [HttpGet]
    public IActionResult Listar() => Ok(_service.ListarDoUsuario(UsuarioLogadoId));

    [HttpGet("{id:int}")]
    public IActionResult Obter(int id)
    {
        var pedido = _service.Obter(id);
        if (pedido is null) return NotFound();
        if (!PodeAcessar(pedido)) return Forbid();
        return Ok(pedido);
    }

    // FALHA 10 corrigida (parte A): o contrato mostra o que o POST realmente devolve.
    [ProducesResponseType(typeof(Pedido), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [HttpPost]
    public async Task<IActionResult> Criar(NovoPedido novo)
    {
        try
        {
            var pedido = await _service.CriarAsync(UsuarioLogadoId, novo);
            return CreatedAtAction(nameof(Obter), new { id = pedido.Id }, pedido);
        }
        catch (RegraDeNegocioException ex)
        {
            return Problem(detail: ex.Message, statusCode: ex.Status, title: ex.Titulo);
        }
    }

    // FALHA 03 corrigida: cancelar é DELETE no recurso e responde 204, sem corpo.
    // A FALHA 09 também vale aqui: ninguém cancela pedido de outra pessoa.
    [HttpDelete("{id:int}")]
    public IActionResult Cancelar(int id)
    {
        var pedido = _service.Obter(id);
        if (pedido is null) return NotFound();
        if (!PodeAcessar(pedido)) return Forbid();

        _service.Cancelar(pedido);
        return NoContent();
    }
}
