using Microsoft.AspNetCore.Mvc;
using SuporteApi.Dtos;

namespace SuporteApi.Controllers;

[Route("api/v1/clientes")]
public class ClientesController : ApiControllerBase
{
    [HttpGet]
    public IActionResult Listar([FromQuery] int page = 1, [FromQuery] int size = 20) => NaoImplementado();

    [HttpGet("{id}")]
    public IActionResult BuscarPorId(string id) => NaoImplementado();

    [HttpGet("{id}/chamados")]
    public IActionResult ListarChamados(string id, [FromQuery] int page = 1, [FromQuery] int size = 20) => NaoImplementado();

    [HttpPost]
    public IActionResult Criar(CriarClienteRequest request) => NaoImplementado();

    [HttpPut("{id}")]
    public IActionResult Atualizar(string id, AtualizarClienteRequest request) => NaoImplementado();

    [HttpDelete("{id}")]
    public IActionResult Remover(string id) => NaoImplementado();
}
