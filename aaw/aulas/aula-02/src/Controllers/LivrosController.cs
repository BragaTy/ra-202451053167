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

    // PASSO 1 (exemplo pronto) — GET api/livros
    // Retorna 200 OK com a lista completa. Teste no Postman antes de continuar.
    [HttpGet]
    public ActionResult<List<Livro>> GetAll()
    {
        return Ok(_repository.GetAll());
    }

    // PASSO 2 — GET api/livros/{id}
    // TODO: buscar pelo id no repositório.
    //   - Se não existir: retornar NotFound()            -> 404
    //   - Se existir:     retornar Ok(livro)             -> 200
    // Teste os DOIS casos no Postman (id 1 e id 999).

    // PASSO 3 — POST api/livros
    // TODO: criar o livro com _repository.Create(livro).
    //   - Retornar CreatedAtAction(nameof(GetById), new { id = criado.Id }, criado)  -> 201 + header Location
    //   - O [ApiController] já devolve 400 automaticamente quando o modelo é inválido:
    //     teste enviando um JSON sem "titulo" e observe o corpo do erro.
    // Dica: o nameof(GetById) exige que o método do Passo 2 se chame GetById.

    // PASSO 4 — PUT api/livros/{id}
    // TODO: atualizar com _repository.Update(id, livro).
    //   - Se não existir: NotFound()                     -> 404
    //   - Se existir:     Ok(atualizado)                 -> 200

    // PASSO 5 — DELETE api/livros/{id}
    // TODO: remover com _repository.Delete(id).
    //   - Se não existia: NotFound()                     -> 404
    //   - Se removeu:     NoContent()                    -> 204
    // Pergunta do exit ticket: o que acontece se você chamar DELETE duas vezes
    // no mesmo id? Qual status deve voltar na segunda chamada — e por quê?
}
