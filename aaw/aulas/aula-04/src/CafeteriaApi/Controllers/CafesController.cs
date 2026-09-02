using CafeteriaApi.Data;
using CafeteriaApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace CafeteriaApi.Controllers;

/// <summary>
/// Recurso "cafés" — o cardápio.
///
/// Tudo o que o PPTX pede está aqui:
///   - URI = substantivo no plural, em kebab-case, sem verbo  (slides 5, 7, 8, 9)
///   - o verbo está no método HTTP                            (slide 9)
///   - versão no path: /api/v1/...                            (slide 13)
///   - coleção sempre paginada, com filtro e ordenação        (slide 15)
///   - ETag + Cache-Control (cacheability)                    (slide 11)
///   - status codes honestos: 200, 201, 204, 304, 400, 404    (slide 2)
/// </summary>
[ApiController]
[Route("api/v1/cafes")]
[Produces("application/json")]
public class CafesController : ControllerBase
{
    private const int TamanhoPadraoDaPagina = 20;
    private const int TamanhoMaximoDaPagina = 100;

    private readonly CafeteriaStore _store;

    public CafesController(CafeteriaStore store)
    {
        _store = store;
    }

    /// <summary>Lista os cafés do cardápio (paginado).</summary>
    /// <remarks>
    /// Nenhuma coleção grande viaja inteira (slide 15). O servidor — e não o cliente —
    /// decide o teto de "size": pedir size=100000 não derruba a API.
    /// Filtros (origem, torra) e ordenação (sort) são query strings DO recurso,
    /// nunca endpoints novos como /buscarPorOrigem.
    /// </remarks>
    [HttpGet(Name = "ListarCafes")]
    [ProducesResponseType(typeof(Pagina<Cafe>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<Pagina<Cafe>> Listar(
        [FromQuery] int page = 1,
        [FromQuery] int size = TamanhoPadraoDaPagina,
        [FromQuery] string? origem = null,
        [FromQuery] string? torra = null,
        [FromQuery] string? sort = null)
    {
        if (page < 1)
        {
            return Problem(
                title: "Parâmetro de paginação inválido.",
                detail: "O parâmetro 'page' começa em 1.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (size < 1 || size > TamanhoMaximoDaPagina)
        {
            return Problem(
                title: "Parâmetro de paginação inválido.",
                detail: $"O parâmetro 'size' deve estar entre 1 e {TamanhoMaximoDaPagina}.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var (itens, total) = _store.ListarCafes(page, size, origem, torra, sort);

        return Ok(new Pagina<Cafe>
        {
            Page = page,
            Size = size,
            Total = total,
            Items = itens
        });
    }

    /// <summary>Obtém um café pelo id.</summary>
    /// <remarks>
    /// Cacheability (slide 11): a resposta leva ETag e Cache-Control.
    /// Se o cliente reenviar o ETag em If-None-Match e nada tiver mudado,
    /// devolvemos 304 Not Modified — sem corpo, sem custo de banda.
    /// </remarks>
    [HttpGet("{id:int}", Name = "ObterCafe")]
    [ProducesResponseType(typeof(Cafe), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Cafe> ObterPorId(int id)
    {
        var cafe = _store.BuscarCafe(id);
        if (cafe is null) return CafeNaoEncontrado(id);

        var etag = $"\"cafe-{cafe.Id}-v{cafe.Versao}\"";

        if (Request.Headers[HeaderNames.IfNoneMatch].ToString().Split(',')
                   .Select(valor => valor.Trim())
                   .Contains(etag))
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }

        Response.Headers[HeaderNames.ETag] = etag;
        Response.Headers[HeaderNames.CacheControl] = "public, max-age=60";

        return Ok(cafe);
    }

    /// <summary>Cadastra um café novo.</summary>
    /// <remarks>
    /// 201 Created + header Location apontando para o recurso criado.
    /// O corpo inválido cai em 400 automaticamente por causa do [ApiController]
    /// (teste enviando um JSON sem "nome").
    /// </remarks>
    [HttpPost(Name = "CriarCafe")]
    [ProducesResponseType(typeof(Cafe), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<Cafe> Criar([FromBody] Cafe cafe)
    {
        var criado = _store.CriarCafe(cafe);

        return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
    }

    /// <summary>Substitui um café inteiro (idempotente).</summary>
    /// <remarks>
    /// PUT é idempotente (slide 2): mandar a MESMA requisição dez vezes deixa
    /// o servidor no MESMO estado que mandar uma vez. Por isso ele substitui,
    /// nunca acumula.
    /// </remarks>
    [HttpPut("{id:int}", Name = "SubstituirCafe")]
    [ProducesResponseType(typeof(Cafe), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Cafe> Substituir(int id, [FromBody] Cafe cafe)
    {
        var atualizado = _store.SubstituirCafe(id, cafe);
        if (atualizado is null) return CafeNaoEncontrado(id);

        return Ok(atualizado);
    }

    /// <summary>Altera parcialmente um café (só os campos enviados).</summary>
    [HttpPatch("{id:int}", Name = "AlterarCafe")]
    [ProducesResponseType(typeof(Cafe), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Cafe> Alterar(int id, [FromBody] CafeParcial mudancas)
    {
        var atualizado = _store.AlterarCafe(id, mudancas);
        if (atualizado is null) return CafeNaoEncontrado(id);

        return Ok(atualizado);
    }

    /// <summary>Remove um café do cardápio.</summary>
    /// <remarks>
    /// 204 No Content na primeira chamada; 404 na segunda — o recurso não existe mais.
    /// O ESTADO do servidor não muda entre a segunda e a terceira chamada:
    /// é isso que torna DELETE idempotente (slide 2).
    /// </remarks>
    [HttpDelete("{id:int}", Name = "RemoverCafe")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Remover(int id)
    {
        if (!_store.RemoverCafe(id)) return CafeNaoEncontrado(id);

        return NoContent();
    }

    /// <summary>404 no formato RFC 7807 (application/problem+json) — erro legível por máquina.</summary>
    private ObjectResult CafeNaoEncontrado(int id) => Problem(
        title: "Café não encontrado.",
        detail: $"Não existe café com id {id}.",
        statusCode: StatusCodes.Status404NotFound,
        instance: $"/api/v1/cafes/{id}");
}
