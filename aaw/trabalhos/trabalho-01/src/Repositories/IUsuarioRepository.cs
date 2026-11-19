using SuporteApi.Models;

namespace SuporteApi.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorEmailAsync(string email);
    Task CriarAsync(Usuario usuario);
}
