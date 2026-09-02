using CafeteriaApi.Data;
using CafeteriaApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace CafeteriaApi.Controllers;

/// <summary>
/// Sub-recurso "itens do pedido" — o exemplo de aninhamento CORRETO (slide 14).
///
/// Um item de pedido não tem vida própria: ele só faz sentido DENTRO de um pedido,
/// por isso a URI é /pedidos/{pedidoId}/itens/{itemId}.
/// É 1 nível de aninhamento — a regra prática do slide é no máximo 2.
/// Compare com o /petshops/1/clientes/5/pets/9/consultas/12/exames/3 da outra API.
/// </summary>
[ApiController]
[Route("api/v1/pedidos/{pedidoId:int}/itens")]
[Produces("application/json")]
public class PedidoItensController : ControllerBase
{
    private readonly CafeteriaStore _store;

    public PedidoItensController(CafeteriaStore store)
    {
        _store = store;
    }

    /// <summary>Lista os itens de um pedido.</summary>
    [HttpGet(Name = "ListarItensDoPedido")]
    [ProducesResponseType(typeof(List<ItemPedido>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<List<ItemPedido>> Listar(int pedidoId)
    {
        if (_store.BuscarPedido(pedidoId) is null) return PedidoNaoEncontrado(pedidoId);

        // Coleção pequena e limitada por natureza (itens de UM pedido): não precisa paginar.
        return Ok(_store.ListarItens(pedidoId));
    }

    /// <summary>Obtém um item específico do pedido.</summary>
    [HttpGet("{itemId:int}", Name = "ObterItemDoPedido")]
    [ProducesResponseType(typeof(ItemPedido), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ItemPedido> ObterPorId(int pedidoId, int itemId)
    {
        if (_store.BuscarPedido(pedidoId) is null) return PedidoNaoEncontrado(pedidoId);

        var item = _store.BuscarItem(pedidoId, itemId);
        if (item is null) return ItemNaoEncontrado(pedidoId, itemId);

        return Ok(item);
    }

    /// <summary>Adiciona um item ao pedido.</summary>
    /// <remarks>201 + Location do item criado; 404 se o pedido ou o café não existir.</remarks>
    [HttpPost(Name = "AdicionarItemAoPedido")]
    [ProducesResponseType(typeof(ItemPedido), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ItemPedido> Adicionar(int pedidoId, [FromBody] ItemPedido item)
    {
        if (_store.BuscarPedido(pedidoId) is null) return PedidoNaoEncontrado(pedidoId);

        var cafe = _store.BuscarCafe(item.CafeId);
        if (cafe is null)
        {
            return Problem(
                title: "Café não encontrado.",
                detail: $"Não existe café com id {item.CafeId} para adicionar ao pedido.",
                statusCode: StatusCodes.Status404NotFound,
                instance: $"/api/v1/cafes/{item.CafeId}");
        }

        var criado = _store.AdicionarItem(pedidoId, item, cafe.Preco);

        return CreatedAtAction(nameof(ObterPorId), new { pedidoId, itemId = criado.Id }, criado);
    }

    /// <summary>Remove um item do pedido.</summary>
    [HttpDelete("{itemId:int}", Name = "RemoverItemDoPedido")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Remover(int pedidoId, int itemId)
    {
        if (_store.BuscarPedido(pedidoId) is null) return PedidoNaoEncontrado(pedidoId);
        if (!_store.RemoverItem(pedidoId, itemId)) return ItemNaoEncontrado(pedidoId, itemId);

        return NoContent();
    }

    private ObjectResult PedidoNaoEncontrado(int pedidoId) => Problem(
        title: "Pedido não encontrado.",
        detail: $"Não existe pedido com id {pedidoId}.",
        statusCode: StatusCodes.Status404NotFound,
        instance: $"/api/v1/pedidos/{pedidoId}");

    private ObjectResult ItemNaoEncontrado(int pedidoId, int itemId) => Problem(
        title: "Item não encontrado.",
        detail: $"O pedido {pedidoId} não tem item com id {itemId}.",
        statusCode: StatusCodes.Status404NotFound,
        instance: $"/api/v1/pedidos/{pedidoId}/itens/{itemId}");
}
