using ServicoProdutos.Models;

namespace ServicoProdutos.Repositorios;

/// <summary>
/// Implementação RELACIONAL (EF Core + SQLite) — COMPLETA, use como referência.
/// Os dados viram LINHAS numa TABELA; a consulta vira SQL gerado pelo EF.
/// Dica: rode com "Persistencia": "Sql" e abra o arquivo produtos.db
/// num visualizador SQLite para ver a tabela.
/// </summary>
public class ProdutoRepositorioSql : IProdutoRepositorio
{
    private readonly ProdutosDbContext _db;

    public ProdutoRepositorioSql(ProdutosDbContext db) => _db = db;

    public List<Produto> ObterTodos() =>
        _db.Produtos.OrderBy(p => p.Id).ToList();

    public Produto? ObterPorId(int id) =>
        _db.Produtos.Find(id);

    public List<Produto> ObterAbaixoDe(decimal precoMaximo) =>
        // Isto vira "SELECT ... WHERE Preco < @max" — o BANCO filtra.
        _db.Produtos.Where(p => p.Preco < precoMaximo).ToList();

    public Produto Criar(Produto produto)
    {
        _db.Produtos.Add(produto);
        _db.SaveChanges();
        return produto;
    }
}
