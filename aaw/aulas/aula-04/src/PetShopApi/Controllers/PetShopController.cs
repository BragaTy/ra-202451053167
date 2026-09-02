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

    // Guardado entre requisições, do jeito mais simples possível.
    private static string? _usuarioDaVez;
    private static int _tutorDaVez;

    public PetShopController(PetShopStore store)
    {
        _store = store;
    }

    // ============================================================ ENDPOINT 01
    /// <summary>Lista os 50 primeiros pets para a tela inicial do aplicativo.</summary>
    [HttpGet("api/v1/pets")]
    public IActionResult GetPets()
    {
        var pets = _store.Pets.Take(50).ToList();

        return Ok(pets);
    }

    // ============================================================ ENDPOINT 02
    /// <summary>Exclui um pet do cadastro.</summary>
    /// <remarks>Usado pelo botão "remover" da tela de cadastro.</remarks>
    [HttpDelete("api/v1/pets")]
    public IActionResult DeletarPet([FromQuery] int id)
    {
        var removeu = _store.RemoverPet(id);

        return Ok(new { removido = removeu, id, mensagem = removeu ? "Pet removido." : "Pet nao existia." });
    }

    // ============================================================ ENDPOINT 03
    /// <summary>Consulta a ficha de um pet.</summary>
    [HttpGet("api/v1/pets/{id:int}")]
    public IActionResult FichaDoPet(int id)
    {
        var pet = _store.BuscarPet(id);
        if (pet is null) return NotFound();


        return Ok(pet);
    }

    // ============================================================ ENDPOINT 04
    /// <summary>Lista os atendimentos de banho e tosa.</summary>
    [HttpGet("api/v1/banhos-tosa")]
    public IActionResult BanhosTosa([FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var itens = _store.BanhosETosas.Skip((page - 1) * size).Take(size).ToList();

        return Ok(new { page, size, total = _store.BanhosETosas.Count, items = itens });
    }

    /// <summary>Lista os tutores do programa de fidelidade.</summary>
    [HttpGet("api/v1/tutores-vip")]
    public IActionResult TutoresVip([FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var vips = _store.Tutores.Where(t => t.Vip).ToList();
        var itens = vips.Skip((page - 1) * size).Take(size).ToList();

        return Ok(new { page, size, total = vips.Count, items = itens });
    }

    // ============================================================ ENDPOINT 05
    /// <summary>Cadastra um pet novo.</summary>
    [HttpPut("api/v1/pets")]
    public IActionResult CadastrarPet([FromBody] Pet pet)
    {
        var criado = _store.CriarPet(pet);

        return Ok(criado);
    }

    // ============================================================ ENDPOINT 06
    /// <summary>Consulta um pet pelo id (endpoint usado pelo app mobile).</summary>
    [HttpGet("api/v1/pets/{id:int}")]
    public IActionResult ObterPet(int id)
    {
        var pet = _store.BuscarPet(id);

        if (pet is null)
        {
            return Ok(new { erro = "Pet nao encontrado", id });
        }

        return Ok(pet);
    }

    // ============================================================ ENDPOINT 07
    /// <summary>Lista de pets consumida pelo aplicativo antigo (contrato de 2024).</summary>
    /// <remarks>
    /// O campo "nome" foi renomeado para "nomeDoPet" no último release para
    /// combinar com o vocabulário do time de produto.
    /// </remarks>
    [HttpGet("api/pets")]
    public IActionResult PetsDoAppAntigo([FromQuery] int page = 1, [FromQuery] int size = 20)
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

        return Ok(new { page, size, total = _store.Pets.Count, items = itens });
    }

    // ============================================================ ENDPOINT 08
    /// <summary>Consulta o resultado de um exame.</summary>
    [HttpGet("api/v1/petshops/{petshopId:int}/clientes/{clienteId:int}/pets/{petId:int}/consultas/{consultaId:int}/exames/{exameId:int}")]
    public IActionResult ResultadoDeExame(int petshopId, int clienteId, int petId, int consultaId, int exameId)
    {
        var exame = _store.Exames.FirstOrDefault(e => e.Id == exameId && e.ConsultaId == consultaId);
        if (exame is null) return NotFound();

        return Ok(exame);
    }

    // ============================================================ ENDPOINT 09
    /// <summary>Lista as consultas veterinárias para o relatório da clínica.</summary>
    [HttpGet("api/v1/consultas")]
    public IActionResult Consultas()
    {
        return Ok(_store.Consultas);
    }

    // ============================================================ ENDPOINT 10
    /// <summary>Registra a carteira de vacinação do pet.</summary>
    [HttpPut("api/v1/pets/{id:int}/vacinas")]
    public IActionResult RegistrarVacina(int id, [FromBody] Vacina vacina)
    {
        var pet = _store.BuscarPet(id);
        if (pet is null) return NotFound();

        _store.RegistrarVacina(id, string.IsNullOrWhiteSpace(vacina.Nome) ? "V10" : vacina.Nome);

        return Ok(_store.VacinasDoPet(id));
    }

    // ============================================================ ENDPOINT 11
    /// <summary>Autentica o tutor no aplicativo.</summary>
    [HttpPost("api/v1/sessao")]
    public IActionResult Entrar([FromBody] Credenciais credenciais)
    {
        var tutor = _store.Tutores.FirstOrDefault(t =>
            t.Nome.StartsWith(credenciais.Usuario, StringComparison.OrdinalIgnoreCase));

        if (tutor is null) return Unauthorized();

        _usuarioDaVez = tutor.Nome;
        _tutorDaVez = tutor.Id;

        return Ok(new { mensagem = $"Bem-vindo, {tutor.Nome}!" });
    }

    /// <summary>Lista os pets do tutor autenticado.</summary>
    [HttpGet("api/v1/meus-pets")]
    public IActionResult MeusPets()
    {
        if (_usuarioDaVez is null) return Unauthorized();

        var pets = _store.Pets.Where(p => p.TutorId == _tutorDaVez).ToList();

        return Ok(new { tutor = _usuarioDaVez, items = pets });
    }

    // ============================================================ ENDPOINT 12
    /// <summary>Tabela de preços dos serviços (reajustada uma vez por ano).</summary>
    [HttpGet("api/v1/tabela-de-precos")]
    public IActionResult TabelaDePrecos()
    {
        Response.Headers[HeaderNames.CacheControl] = "no-store, no-cache, must-revalidate";
        Response.Headers[HeaderNames.Pragma] = "no-cache";

        return Ok(_store.TabelaDePrecos);
    }
}
