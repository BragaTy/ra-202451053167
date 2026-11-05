using Microsoft.AspNetCore.Mvc;
using SuporteApi.Dtos;

namespace SuporteApi.Controllers;

[Route("api/v1/chamados")]
public class ChamadosController : ApiControllerBase
{
    [HttpGet]
    public IActionResult Listar(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? prioridade = null) => NaoImplementado();

    [HttpGet("{id}")]
    public IActionResult BuscarPorId(string id) => NaoImplementado();

    [HttpPost]
    public IActionResult Criar(CriarChamadoRequest request) => NaoImplementado();

    [HttpPut("{id}")]
    public IActionResult Atualizar(string id, AtualizarChamadoRequest request) => NaoImplementado();

    [HttpDelete("{id}")]
    public IActionResult Remover(string id) => NaoImplementado();
}
