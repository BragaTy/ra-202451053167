using MongoDB.Bson;
using MongoDB.Driver;
using SuporteApi.Models;

namespace SuporteApi.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    public const string Colecao = "usuarios";
    private readonly IMongoCollection<Usuario> _usuarios;

    public UsuarioRepository(IMongoDatabase db) => _usuarios = db.GetCollection<Usuario>(Colecao);

    public async Task<Usuario?> ObterPorEmailAsync(string email) =>
        await _usuarios.Find(u => u.Email == email).FirstOrDefaultAsync();

    public async Task CriarAsync(Usuario usuario)
    {
        usuario.Id = ObjectId.GenerateNewId().ToString();
        await _usuarios.InsertOneAsync(usuario);
    }
}
