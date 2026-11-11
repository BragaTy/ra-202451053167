using MongoDB.Bson;
using MongoDB.Driver;
using SuporteApi.Models;

namespace SuporteApi.Repositories;

public class ClienteRepository : IClienteRepository
{
    public const string Colecao = "clientes";
    private readonly IMongoCollection<Cliente> _clientes;

    public ClienteRepository(IMongoDatabase db) => _clientes = db.GetCollection<Cliente>(Colecao);

    public async Task<PaginaResultado<Cliente>> ListarAsync(int page, int size)
    {
        var filtro = Builders<Cliente>.Filter.Empty;
        var total = await _clientes.CountDocumentsAsync(filtro);
        var itens = await _clientes.Find(filtro)
            .SortBy(c => c.Nome)
            .Skip((page - 1) * size) // pula as páginas anteriores
            .Limit(size)
            .ToListAsync();
        return new PaginaResultado<Cliente>(itens, page, size, total);
    }

    public async Task<Cliente?> ObterPorIdAsync(string id)
    {
        // Id fora do formato ObjectId não existe no banco: evita exceção (500).
        if (!ObjectId.TryParse(id, out _)) return null;
        return await _clientes.Find(c => c.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Cliente?> ObterPorEmailAsync(string email) =>
        await _clientes.Find(c => c.Email == email).FirstOrDefaultAsync();

    public async Task CriarAsync(Cliente cliente)
    {
        cliente.Id = ObjectId.GenerateNewId().ToString();
        await _clientes.InsertOneAsync(cliente);
    }

    // true se encontrou o documento; false vira 404 na camada de cima.
    public async Task<bool> AtualizarAsync(Cliente cliente)
    {
        if (!ObjectId.TryParse(cliente.Id, out _)) return false;
        var resultado = await _clientes.ReplaceOneAsync(c => c.Id == cliente.Id, cliente);
        return resultado.MatchedCount > 0;
    }

    public async Task<bool> RemoverAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _)) return false;
        var resultado = await _clientes.DeleteOneAsync(c => c.Id == id);
        return resultado.DeletedCount > 0;
    }
}
