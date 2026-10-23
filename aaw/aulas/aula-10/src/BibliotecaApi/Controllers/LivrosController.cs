using BibliotecaApi.Data;
using BibliotecaApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Livros")]
public class LivrosController : ControllerBase
{
    private readonly BibliotecaRepository _repo;

    public LivrosController(BibliotecaRepository repo) => _repo = repo;

    /// <summary>
    /// Lista os livros do acervo com paginação.
    /// </summary>
    /// <param name="pagina">Número da página (a partir de 1).</param>
    /// <param name="tamanhoPagina">Itens por página (máximo 50).</param>
    /// <response code="200">Página de livros com metadados de paginação.</response>
    [HttpGet]
    [ProducesResponseType(typeof(RespostaPaginada<Livro>), StatusCodes.Status200OK)]
    public IActionResult ObterTodos(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 10)
    {
        tamanhoPagina = Math.Min(tamanhoPagina, 50);
        var todos = _repo.ObterLivros();
        var paginados = todos
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToList();

        var resposta = new RespostaPaginada<Livro>
        {
            Itens = paginados,
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalItens = todos.Count
        };

        return Ok(resposta);
    }

    /// <summary>
    /// Obtém um livro pelo seu ID.
    /// </summary>
    /// <param name="id">ID do livro.</param>
    /// <response code="200">Livro encontrado.</response>
    /// <response code="404">Livro não encontrado.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Livro), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult ObterPorId(int id)
    {
        var livro = _repo.ObterLivroPorId(id);
        if (livro == null)
            return NotFound(new { mensagem = $"Livro com ID {id} não encontrado." });
        return Ok(livro);
    }

    /// <summary>
    /// Busca livros por título, gênero e/ou autor.
    /// </summary>
    /// <param name="titulo">Trecho do título.</param>
    /// <param name="genero">Gênero exato.</param>
    /// <param name="autorId">ID do autor.</param>
    /// <response code="200">Lista (possivelmente vazia) de livros encontrados.</response>
    [HttpGet("buscar")]
    [ProducesResponseType(typeof(List<Livro>), StatusCodes.Status200OK)]
    public IActionResult Buscar(
        [FromQuery] string? titulo,
        [FromQuery] string? genero,
        [FromQuery] int? autorId)
    {
        var resultados = _repo.BuscarLivros(titulo, genero, autorId);
        return Ok(resultados);
    }

    /// <summary>
    /// Cadastra um novo livro no acervo.
    /// </summary>
    /// <remarks>
    /// Exemplo de requisição:
    ///
    ///     POST /api/livros
    ///     {
    ///         "titulo": "Dom Casmurro",
    ///         "genero": "Romance",
    ///         "anoPublicacao": 1899,
    ///         "autorId": 1
    ///     }
    ///
    /// </remarks>
    /// <param name="livro">Dados do livro.</param>
    /// <response code="201">Livro criado com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Livro), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Criar([FromBody] Livro livro)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var criado = _repo.CriarLivro(livro);
        return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
    }

    /// <summary>
    /// Atualiza um livro existente.
    /// </summary>
    /// <param name="id">ID do livro a atualizar.</param>
    /// <param name="livro">Novos dados do livro.</param>
    /// <response code="204">Atualizado com sucesso.</response>
    /// <response code="404">Livro não encontrado.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Atualizar(int id, [FromBody] Livro livro)
    {
        if (!_repo.AtualizarLivro(id, livro))
            return NotFound(new { mensagem = $"Livro com ID {id} não encontrado." });
        return NoContent();
    }

    /// <summary>
    /// Remove um livro do acervo.
    /// </summary>
    /// <param name="id">ID do livro a remover.</param>
    /// <response code="204">Removido com sucesso.</response>
    /// <response code="404">Livro não encontrado.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Remover(int id)
    {
        if (!_repo.RemoverLivro(id))
            return NotFound(new { mensagem = $"Livro com ID {id} não encontrado." });
        return NoContent();
    }
}
