using MongoDB.Bson;
using SuporteApi.Models;
using SuporteApi.Repositories;

namespace SuporteApi.Tests.Fakes;

// Repositórios falsos, em memória, usados SÓ nos testes unitários:
// isolam o serviço do MongoDB para testar as regras de negócio.

public class ClienteRepositoryFalso : IClienteRepository
{
    public List<Cliente> Itens { get; } = [];

    public Task<PaginaResultado<Cliente>> ListarAsync(int page, int size) =>
        Task.FromResult(new PaginaResultado<Cliente>(
            Itens.Skip((page - 1) * size).Take(size).ToList(), page, size, Itens.Count));

    public Task<Cliente?> ObterPorIdAsync(string id) => Task.FromResult(Itens.FirstOrDefault(c => c.Id == id));

    public Task<Cliente?> ObterPorEmailAsync(string email) =>
        Task.FromResult(Itens.FirstOrDefault(c => c.Email == email));

    public Task CriarAsync(Cliente cliente)
    {
        cliente.Id = ObjectId.GenerateNewId().ToString();
        Itens.Add(cliente);
        return Task.CompletedTask;
    }

    public Task<bool> AtualizarAsync(Cliente cliente) => Task.FromResult(Itens.Any(c => c.Id == cliente.Id));

    public Task<bool> RemoverAsync(string id) => Task.FromResult(Itens.RemoveAll(c => c.Id == id) > 0);
}

public class ChamadoRepositoryFalso : IChamadoRepository
{
    public List<Chamado> Itens { get; } = [];

    public Task<PaginaResultado<Chamado>> ListarAsync(int page, int size, string? status, string? prioridade) =>
        Task.FromResult(new PaginaResultado<Chamado>(Itens, page, size, Itens.Count));

    public Task<PaginaResultado<Chamado>> ListarPorClienteAsync(string clienteId, int page, int size)
    {
        var doCliente = Itens.Where(c => c.ClienteId == clienteId).ToList();
        return Task.FromResult(new PaginaResultado<Chamado>(doCliente, page, size, doCliente.Count));
    }

    public Task<bool> ExisteParaClienteAsync(string clienteId) =>
        Task.FromResult(Itens.Any(c => c.ClienteId == clienteId));

    public Task<Chamado?> ObterPorIdAsync(string id) => Task.FromResult(Itens.FirstOrDefault(c => c.Id == id));

    public Task CriarAsync(Chamado chamado)
    {
        chamado.Id = ObjectId.GenerateNewId().ToString();
        Itens.Add(chamado);
        return Task.CompletedTask;
    }

    public Task<bool> AtualizarAsync(Chamado chamado) => Task.FromResult(Itens.Any(c => c.Id == chamado.Id));

    public Task<bool> RemoverAsync(string id) => Task.FromResult(Itens.RemoveAll(c => c.Id == id) > 0);
}
