using MongoDB.Bson;
using MongoDB.Driver;
using SuporteApi.Models;

namespace SuporteApi.Repositories;

public class ChamadoRepository : IChamadoRepository
{
    public const string Colecao = "chamados";
    private readonly IMongoCollection<Chamado> _chamados;

    public ChamadoRepository(IMongoDatabase db) => _chamados = db.GetCollection<Chamado>(Colecao);

    public Task<PaginaResultado<Chamado>> ListarAsync(int page, int size, string? status, string? prioridade)
    {
        // Os filtros só entram se vierem na query string.
        var f = Builders<Chamado>.Filter;
        var filtro = f.Empty;
        if (!string.IsNullOrWhiteSpace(status)) filtro &= f.Eq(c => c.Status, status);
        if (!string.IsNullOrWhiteSpace(prioridade)) filtro &= f.Eq(c => c.Prioridade, prioridade);
        return PaginarAsync(filtro, page, size);
    }

    public Task<PaginaResultado<Chamado>> ListarPorClienteAsync(string clienteId, int page, int size)
    {
        if (!ObjectId.TryParse(clienteId, out _))
            return Task.FromResult(new PaginaResultado<Chamado>([], page, size, 0));
        return PaginarAsync(Builders<Chamado>.Filter.Eq(c => c.ClienteId, clienteId), page, size);
    }

    // Usado para impedir remover cliente que ainda tem chamados (409).
    public async Task<bool> ExisteParaClienteAsync(string clienteId)
    {
        if (!ObjectId.TryParse(clienteId, out _)) return false;
        return await _chamados.Find(c => c.ClienteId == clienteId).AnyAsync();
    }

    public async Task<Chamado?> ObterPorIdAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _)) return null;
        return await _chamados.Find(c => c.Id == id).FirstOrDefaultAsync();
    }

    public async Task CriarAsync(Chamado chamado)
    {
        chamado.Id = ObjectId.GenerateNewId().ToString();
        await _chamados.InsertOneAsync(chamado);
    }

    public async Task<bool> AtualizarAsync(Chamado chamado)
    {
        if (!ObjectId.TryParse(chamado.Id, out _)) return false;
        var resultado = await _chamados.ReplaceOneAsync(c => c.Id == chamado.Id, chamado);
        return resultado.MatchedCount > 0;
    }

    public async Task<bool> RemoverAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _)) return false;
        var resultado = await _chamados.DeleteOneAsync(c => c.Id == id);
        return resultado.DeletedCount > 0;
    }

    // Mais recentes primeiro.
    private async Task<PaginaResultado<Chamado>> PaginarAsync(FilterDefinition<Chamado> filtro, int page, int size)
    {
        var total = await _chamados.CountDocumentsAsync(filtro);
        var itens = await _chamados.Find(filtro)
            .SortByDescending(c => c.DataAbertura)
            .Skip((page - 1) * size)
            .Limit(size)
            .ToListAsync();
        return new PaginaResultado<Chamado>(itens, page, size, total);
    }
}
