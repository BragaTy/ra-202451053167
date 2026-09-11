using BibliotecaApi.Models;
using BibliotecaApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LivrosController : ControllerBase
{
    private readonly LivroRepository _repository;

    public LivrosController(LivroRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public ActionResult<List<Livro>> GetAll()
    {
        return Ok(_repository.GetAll());
    }

    [HttpGet("{id:int}")]
    public ActionResult<Livro> GetById(int id)
    {
        var livro = _repository.GetById(id);

        if (livro is null)
        {
            return NotFound();
        }

        return Ok(livro);
    }

    [HttpPost]
    public ActionResult<Livro> Create([FromBody] Livro livro)
    {
        var criado = _repository.Create(livro);

        return CreatedAtAction(
            nameof(GetById),
            new { id = criado.Id },
            criado);
    }

    [HttpPut("{id:int}")]
    public ActionResult<Livro> Update(int id, [FromBody] Livro livro)
    {
        var atualizado = _repository.Update(id, livro);

        if (atualizado is null)
        {
            return NotFound();
        }

        return Ok(atualizado);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var removeu = _repository.Delete(id);

        if (!removeu)
        {
            return NotFound();
        }

        return NoContent();
    }
}