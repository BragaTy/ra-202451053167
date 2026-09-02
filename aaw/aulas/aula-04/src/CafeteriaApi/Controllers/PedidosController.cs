using CafeteriaApi.Data;
using CafeteriaApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace CafeteriaApi.Controllers;

/// <summary>
/// Recurso "pedidos".
///
/// Repare que NÃO existe endpoint /pedidos/{id}/entregar nem /finalizarPedido:
/// mudar o status é alterar um campo do recurso — PATCH no próprio pedido.
/// Ação vira campo; recurso continua substantivo (slides 5 e 9).
/// </summary>
[ApiController]
[Route("api/v1/pedidos")]
[Produces("application/json")]
public class PedidosController : ControllerBase
{
    private const int TamanhoPadraoDaPagina = 20;
    private const int TamanhoMaximoDaPagina = 100;

    private readonly CafeteriaStore _store;

    public PedidosController(CafeteriaStore store)
    {
        _store = store;
    }

    /// <summary>Lista os pedidos (paginado, com filtro por status e por cliente).</summary>
    [HttpGet(Name = "ListarPedidos")]
    [ProducesResponseType(typeof(Pagina<Pedido>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<Pagina<Pedido>> Listar(
        [FromQuery] int page = 1,
        [FromQuery] int size = TamanhoPadraoDaPagina,
        [FromQuery] string? status = null,
        [FromQuery] string? cliente = null)
    {
        if (page < 1 || size < 1 || size > TamanhoMaximoDaPagina)
        {
            return Problem(
                title: "Parâmetro de paginação inválido.",
                detail: $"'page' começa em 1 e 'size' deve estar entre 1 e {TamanhoMaximoDaPagina}.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var (itens, total) = _store.ListarPedidos(page, size, status, cliente);

        return Ok(new Pagina<Pedido>
        {
            Page = page,
            Size = size,
            Total = total,
            Items = itens
        });
    }

    /// <summary>Obtém um pedido pelo id.</summary>
    [HttpGet("{id:int}", Name = "ObterPedido")]
    [ProducesResponseType(typeof(Pedido), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Pedido> ObterPorId(int id)
    {
        var pedido = _store.BuscarPedido(id);
        if (pedido is null) return PedidoNaoEncontrado(id);

        return Ok(pedido);
    }

    /// <summary>Abre um pedido novo (sem itens).</summary>
    /// <remarks>201 + Location. Os itens entram depois, no sub-recurso /itens.</remarks>
    [HttpPost(Name = "CriarPedido")]
    [ProducesResponseType(typeof(Pedido), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<Pedido> Criar([FromBody] Pedido pedido)
    {
        var criado = _store.CriarPedido(pedido);

        return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
    }

    /// <summary>Muda o status do pedido (recebido | preparando | entregue).</summary>
    /// <remarks>
    /// A "ação de entregar" é modelada como uma alteração parcial do recurso.
    /// É o antídoto do endpoint /pedidos/{id}/entregar.
    /// </remarks>
    [HttpPatch("{id:int}", Name = "AlterarPedido")]
    [ProducesResponseType(typeof(Pedido), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Pedido> AlterarStatus(int id, [FromBody] AlteracaoDeStatus mudanca)
    {
        var pedido = _store.AlterarStatusDoPedido(id, mudanca.Status);
        if (pedido is null) return PedidoNaoEncontrado(id);

        return Ok(pedido);
    }

    private ObjectResult PedidoNaoEncontrado(int id) => Problem(
        title: "Pedido não encontrado.",
        detail: $"Não existe pedido com id {id}.",
        statusCode: StatusCodes.Status404NotFound,
        instance: $"/api/v1/pedidos/{id}");
}
