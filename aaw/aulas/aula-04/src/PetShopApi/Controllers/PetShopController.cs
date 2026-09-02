using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using PetShopApi.Data;
using PetShopApi.Models;

namespace PetShopApi.Controllers;

/// <summary>
/// API do PetHouse.
///
/// ATENÇÃO, ALUNO: esta API FUNCIONA. Toda requisição devolve resposta e nada
/// quebra em produção — e é exatamente por isso que ela passou na revisão de código.
///
/// Só que CADA endpoint aqui viola UMA regra ou diretriz REST vista em aula.
/// Doze endpoints, doze erros diferentes. Sua missão:
///   1. chamar cada um (Postman, curl ou Swagger);
///   2. observar URI, método, status code, headers e corpo da resposta;
///   3. nomear o erro e escrever como você redesenharia.
///
/// Compare sempre com a API do Café Newton (porta 5301), que faz tudo certo.
/// </summary>
[ApiController]
[Produces("application/json")]
public class PetShopController : ControllerBase
{
    private readonly PetShopStore _store;

    public PetShopController(PetShopStore store)
    {
        _store = store;
    }

    // ============================================================ ENDPOINT 01
    /// <summary>Lista os 50 primeiros pets para a tela inicial do aplicativo.</summary>
    [HttpGet("api/v1/pets")]
    public IActionResult GetPets(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20)
    {
        if (page < 1 || size < 1 || size > 100)
            return BadRequest();

        var itens = _store.Pets
            .Skip((page - 1) * size)
            .Take(size)
            .ToList();

        return Ok(new
        {
            page,
            size,
            total = _store.Pets.Count,
            items = itens
        });
    }

    // ============================================================ ENDPOINT 02
    /// <summary>Exclui um pet do cadastro.</summary>
    /// <remarks>Usado pelo botão "remover" da tela de cadastro.</remarks>
    [HttpDelete("api/v1/pets/{id:int}")]
    public IActionResult DeletarPet(int id)
    {
        var removeu = _store.RemoverPet(id);

        if (!removeu)
            return NotFound();

        return NoContent();
    }

    // ============================================================ ENDPOINT 03
    /// <summary>Consulta a ficha de um pet.</summary>

    // ============================================================ ENDPOINT 06
    /// <summary>Consulta um pet pelo id (endpoint usado pelo app mobile).</summary>
    [HttpGet("api/v1/pets/{id:int}", Name = "ObterPet")]
    public IActionResult ObterPet(int id)
    {
        var pet = _store.BuscarPet(id);

        if (pet is null)
        {
            return Problem(
                title: "Pet não encontrado.",
                detail: $"Não existe pet com id {id}.",
                statusCode: StatusCodes.Status404NotFound,
                instance: $"/api/v1/pets/{id}");
        }

        return Ok(pet);
    }

    // ============================================================ ENDPOINT 04
    /// <summary>Lista os atendimentos de banho e tosa.</summary>
    [HttpGet("api/v1/banhos-e-tosas")]
    public IActionResult BanhosTosa(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20)
    {
        var itens = _store.BanhosETosas
            .Skip((page - 1) * size)
            .Take(size)
            .ToList();

        return Ok(new
        {
            page,
            size,
            total = _store.BanhosETosas.Count,
            items = itens
        });
    }

    /// <summary>Lista os tutores do programa de fidelidade.</summary>
    [HttpGet("api/v1/tutores")]
    public IActionResult TutoresVip(
        [FromQuery] bool? vip = null,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20)
    {
        var tutores = vip.HasValue
            ? _store.Tutores.Where(t => t.Vip == vip.Value).ToList()
            : _store.Tutores.ToList();

        var itens = tutores
            .Skip((page - 1) * size)
            .Take(size)
            .ToList();

        return Ok(new
        {
            page,
            size,
            total = tutores.Count,
            items = itens
        });
    }

    // ============================================================ ENDPOINT 05
    /// <summary>Cadastra um pet novo.</summary>
    [HttpPost("api/v1/pets")]
    public IActionResult CadastrarPet([FromBody] Pet pet)
    {
        var criado = _store.CriarPet(pet);

        return CreatedAtAction(
            nameof(ObterPet),
            new { id = criado.Id },
            criado);
    }

    // ============================================================ ENDPOINT 07
    /// <summary>Lista de pets consumida pelo aplicativo antigo (contrato de 2024).</summary>
    /// <remarks>
    /// O campo "nome" foi renomeado para "nomeDoPet" no último release para
    /// combinar com o vocabulário do time de produto.
    /// </remarks>
    [HttpGet("api/v1/pets")]
    public IActionResult PetsDoAppNovo(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20)
    {
        var itens = _store.Pets
            .Skip((page - 1) * size)
            .Take(size)
            .Select(p => new
            {
                p.Id,
                nomeDoPet = p.Nome,
                p.Especie,
                p.Raca,
                p.TutorId
            })
            .ToList();

        return Ok(new
        {
            page,
            size,
            total = _store.Pets.Count,
            items = itens
        });
    }

    // ============================================================ ENDPOINT 08
    /// <summary>Consulta o resultado de um exame.</summary>
    [HttpGet("api/v1/exames/{exameId:int}")]
    public IActionResult ResultadoDeExame(int exameId)
    {
        var exame = _store.Exames.FirstOrDefault(e => e.Id == exameId);

        if (exame is null)
            return NotFound();

        return Ok(exame);
    }

    // ============================================================ ENDPOINT 09
    /// <summary>Lista as consultas veterinárias para o relatório da clínica.</summary>
    [HttpGet("api/v1/consultas")]
    public IActionResult Consultas(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] int? petId = null,
        [FromQuery] string? veterinario = null,
        [FromQuery] string? sort = null)
    {
        if (page < 1 || size < 1 || size > 100)
            return BadRequest();

        IEnumerable<Consulta> consulta = _store.Consultas;

        if (petId.HasValue)
            consulta = consulta.Where(c => c.PetId == petId.Value);

        if (!string.IsNullOrWhiteSpace(veterinario))
        {
            consulta = consulta.Where(c =>
                c.Veterinario.Contains(
                    veterinario,
                    StringComparison.OrdinalIgnoreCase));
        }

        consulta = sort switch
        {
            "data" => consulta.OrderBy(c => c.Data),
            "-data" => consulta.OrderByDescending(c => c.Data),
            _ => consulta.OrderBy(c => c.Id)
        };

        var todas = consulta.ToList();

        var itens = todas
            .Skip((page - 1) * size)
            .Take(size)
            .ToList();

        return Ok(new
        {
            page,
            size,
            total = todas.Count,
            items = itens
        });
    }

    // ============================================================ ENDPOINT 10
    /// <summary>Registra a carteira de vacinação do pet.</summary>
    [HttpPost("api/v1/pets/{id:int}/vacinas")]
    public IActionResult RegistrarVacina(int id, [FromBody] Vacina vacina)
    {
        var pet = _store.BuscarPet(id);

        if (pet is null)
            return NotFound();

        var criada = _store.RegistrarVacina(
            id,
            string.IsNullOrWhiteSpace(vacina.Nome) ? "V10" : vacina.Nome);

        return Created(
            $"/api/v1/pets/{id}/vacinas/{criada.Id}",
            criada);
    }

    // ============================================================ ENDPOINT 11
    /// <summary>Lista os pets do tutor autenticado.</summary>
    [HttpGet("api/v1/tutores/{tutorId:int}/pets")]
    public IActionResult MeusPets(int tutorId)
    {
        var tutor = _store.Tutores.FirstOrDefault(t => t.Id == tutorId);

        if (tutor is null)
            return NotFound();

        var pets = _store.Pets
            .Where(p => p.TutorId == tutorId)
            .ToList();

        return Ok(new
        {
            tutor = tutor.Nome,
            items = pets
        });
    }

    // ============================================================ ENDPOINT 12
    /// <summary>Tabela de preços dos serviços (reajustada uma vez por ano).</summary>
    [HttpGet("api/v1/tabela-de-precos")]
    public IActionResult TabelaDePrecos()
    {
        const string etag = "\"precos-2026-v1\"";

        var etagDoCliente = Request.Headers[HeaderNames.IfNoneMatch]
            .ToString();

        var clientePossuiVersao = etagDoCliente
            .Split(',')
            .Select(valor => valor.Trim())
            .Contains(etag);

        if (clientePossuiVersao)
            return StatusCode(StatusCodes.Status304NotModified);

        Response.Headers[HeaderNames.ETag] = etag;
        Response.Headers[HeaderNames.CacheControl] = "public, max-age=3600";

        return Ok(_store.TabelaDePrecos);
    }
}