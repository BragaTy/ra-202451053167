using SuporteApi.Models;

namespace SuporteApi.Repositories;

// Contrato do acesso a clientes. O serviço e os testes dependem só disto,
// o que permite trocar o MongoDB por um repositório falso nos testes.
public interface IClienteRepository
{
    Task<PaginaResultado<Cliente>> ListarAsync(int page, int size);
    Task<Cliente?> ObterPorIdAsync(string id);
    Task<Cliente?> ObterPorEmailAsync(string email);
    Task CriarAsync(Cliente cliente);
    Task<bool> AtualizarAsync(Cliente cliente);
    Task<bool> RemoverAsync(string id);
}
