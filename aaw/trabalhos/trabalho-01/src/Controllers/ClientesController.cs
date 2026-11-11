using Microsoft.AspNetCore.Mvc;
using SuporteApi.Dtos;
using SuporteApi.Models;
using SuporteApi.Services;

namespace SuporteApi.Controllers;

// O controller só traduz HTTP <-> serviço. Regras ficam no ClienteService,
// e os erros (400, 404, 409) saem pelo TratadorDeErros.
[Route("api/v1/clientes")]
public class ClientesController(ClienteService service) : ApiControllerBase
{
    /// <summary>Lista os clientes, com paginação.</summary>
    [HttpGet]
    [ProducesResponseType<PaginaResultado<Cliente>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Listar([FromQuery] int page = 1, [FromQuery] int size = 20) =>
        Ok(await service.ListarAsync(page, size));

    /// <summary>Busca um cliente pelo id.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType<Cliente>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BuscarPorId(string id) =>
        Ok(await service.ObterAsync(id));

    /// <summary>Lista os chamados de um cliente.</summary>
    [HttpGet("{id}/chamados")]
    [ProducesResponseType<PaginaResultado<Chamado>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarChamados(string id, [FromQuery] int page = 1, [FromQuery] int size = 20) =>
        Ok(await service.ListarChamadosAsync(id, page, size));

    /// <summary>Cadastra um cliente.</summary>
    [HttpPost]
    [ProducesResponseType<Cliente>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Criar(CriarClienteRequest request)
    {
        var cliente = await service.CriarAsync(request);
        // 201 + cabeçalho Location apontando para GET /api/v1/clientes/{id}.
        return CreatedAtAction(nameof(BuscarPorId), new { id = cliente.Id }, cliente);
    }

    /// <summary>Atualiza um cliente (também ativa ou desativa).</summary>
    [HttpPut("{id}")]
    [ProducesResponseType<Cliente>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Atualizar(string id, AtualizarClienteRequest request) =>
        Ok(await service.AtualizarAsync(id, request));

    /// <summary>Remove um cliente sem chamados.</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Remover(string id)
    {
        await service.RemoverAsync(id);
        return NoContent();
    }
}
