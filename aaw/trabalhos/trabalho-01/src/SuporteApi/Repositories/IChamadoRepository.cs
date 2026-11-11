using SuporteApi.Models;

namespace SuporteApi.Repositories;

public interface IChamadoRepository
{
    Task<PaginaResultado<Chamado>> ListarAsync(int page, int size, string? status, string? prioridade);
    Task<PaginaResultado<Chamado>> ListarPorClienteAsync(string clienteId, int page, int size);
    Task<bool> ExisteParaClienteAsync(string clienteId);
    Task<Chamado?> ObterPorIdAsync(string id);
    Task CriarAsync(Chamado chamado);
    Task<bool> AtualizarAsync(Chamado chamado);
    Task<bool> RemoverAsync(string id);
}
