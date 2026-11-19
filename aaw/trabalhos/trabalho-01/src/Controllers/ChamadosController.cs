using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuporteApi.Auth;
using SuporteApi.Dtos;
using SuporteApi.Models;
using SuporteApi.Services;

namespace SuporteApi.Controllers;

// Todos os endpoints exigem login. O DELETE é só do admin; o PUT confere o dono no serviço.
[Authorize]
[Route("api/v1/chamados")]
public class ChamadosController(ChamadoService service) : ApiControllerBase
{
    /// <summary>Lista os chamados, com paginação e filtros opcionais.</summary>
    /// <param name="page">Página, a partir de 1.</param>
    /// <param name="size">Itens por página (1 a 100).</param>
    /// <param name="status">Aberto, EmAtendimento ou Concluido.</param>
    /// <param name="prioridade">Baixa, Media ou Alta.</param>
    [HttpGet]
    [ProducesResponseType<PaginaResultado<Chamado>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Listar(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? prioridade = null) =>
        Ok(await service.ListarAsync(page, size, status, prioridade));

    /// <summary>Busca um chamado pelo id.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType<Chamado>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BuscarPorId(string id) =>
        Ok(await service.ObterAsync(id));

    /// <summary>Abre um chamado; o prazo (SLA) é calculado pela prioridade e pelo plano do cliente.</summary>
    [HttpPost]
    [ProducesResponseType<Chamado>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Criar(CriarChamadoRequest request)
    {
        // O dono vem do token (claim "sub"), não do corpo da requisição.
        var chamado = await service.AbrirAsync(request, User.Id());
        return CreatedAtAction(nameof(BuscarPorId), new { id = chamado.Id }, chamado);
    }

    /// <summary>Atualiza assunto, descrição e status (dono do chamado ou admin).</summary>
    [HttpPut("{id}")]
    [ProducesResponseType<Chamado>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(string id, AtualizarChamadoRequest request) =>
        Ok(await service.AtualizarAsync(id, request, User.Id(), User.EhAdmin()));

    /// <summary>Remove um chamado (apenas admin).</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = Perfis.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(string id)
    {
        await service.RemoverAsync(id);
        return NoContent();
    }
}
